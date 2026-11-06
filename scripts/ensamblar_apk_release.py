"""Reconstruye un APK desde partes verificadas sin guardar binarios en Git.

El modo local permite comprobar el ensamblado antes de usar GitHub Actions.
El modo remoto solo modifica un release en borrador y retira las partes
después de verificar el tamaño y el digest del APK adjunto.
"""

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import tempfile
from urllib.parse import quote, urlsplit
from urllib.request import HTTPRedirectHandler, Request, build_opener


class RedireccionSinCredenciales(HTTPRedirectHandler):
    def redirect_request(self, request, response, code, message, headers, new_url):
        if urlsplit(new_url).scheme != "https":
            raise ValueError("GitHub redirigió a un destino sin HTTPS.")
        redirected = super().redirect_request(
            request, response, code, message, headers, new_url
        )
        if urlsplit(new_url).hostname != urlsplit(request.full_url).hostname:
            redirected.remove_header("Authorization")
        return redirected


class GitHubApi:
    def __init__(self, repository, token):
        if not re.fullmatch(r"[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+", repository):
            raise ValueError("Repositorio de GitHub no válido.")
        if not token:
            raise ValueError("Falta GITHUB_TOKEN.")
        self.repository = repository
        self.opener = build_opener(RedireccionSinCredenciales())
        self.headers = {
            "Authorization": f"Bearer {token}",
            "Accept": "application/vnd.github+json",
            "X-GitHub-Api-Version": "2022-11-28",
            "User-Agent": "Uchumataqu-release",
        }

    def abrir(self, path, method="GET", data=None, extra_headers=None):
        headers = {**self.headers, **(extra_headers or {})}
        request = Request(
            f"https://api.github.com/repos/{self.repository}/{path}",
            data=data,
            headers=headers,
            method=method,
        )
        return self.opener.open(request, timeout=180)

    def leer(self, path):
        with self.abrir(path) as response:
            return json.load(response)

    def assets(self, release_id):
        result = []
        page = 1
        while True:
            batch = self.leer(
                f"releases/{release_id}/assets?per_page=100&page={page}"
            )
            result.extend(batch)
            if len(batch) < 100:
                return result
            page += 1

    def descargar(self, asset):
        return self.abrir(
            f"releases/assets/{int(asset['id'])}",
            extra_headers={"Accept": "application/octet-stream"},
        )

    def eliminar(self, asset):
        with self.abrir(f"releases/assets/{int(asset['id'])}", method="DELETE"):
            pass

    def adjuntar(self, release, archivo):
        url = release["upload_url"].split("{", 1)[0]
        expected_path = (
            f"/repos/{self.repository}/releases/{int(release['id'])}/assets"
        )
        parsed = urlsplit(url)
        if (parsed.scheme, parsed.hostname, parsed.path) != (
            "https", "uploads.github.com", expected_path
        ):
            raise ValueError("URL de carga de GitHub inesperada.")
        headers = {
            **self.headers,
            "Content-Type": "application/vnd.android.package-archive",
            "Content-Length": str(archivo.stat().st_size),
        }
        with archivo.open("rb") as source:
            request = Request(
                f"{url}?name={quote(archivo.name, safe='')}",
                data=source,
                headers=headers,
                method="POST",
            )
            with self.opener.open(request, timeout=180) as response:
                return json.load(response)


def ordenar_partes(assets, nombre):
    prefix = nombre + ".part-"
    partes = sorted(
        (asset for asset in assets if asset["name"].startswith(prefix)),
        key=lambda asset: asset["name"],
    )
    if not partes or len(partes) > 999:
        raise ValueError("No hay una colección válida de partes.")
    for index, parte in enumerate(partes, 1):
        if parte["name"] != f"{prefix}{index:03d}":
            raise ValueError("Las partes no forman una secuencia completa.")
        if parte.get("state", "uploaded") != "uploaded" or parte["size"] <= 0:
            raise ValueError(f"Parte incompleta: {parte['name']}.")
    return partes


def ensamblar(partes, abrir_parte, destino, tamano, sha256):
    if sum(parte["size"] for parte in partes) != tamano:
        raise ValueError("La suma de las partes no coincide con el tamaño esperado.")
    digest = hashlib.sha256()
    with destino.open("xb") as output:
        for parte in partes:
            digest_parte = hashlib.sha256()
            bytes_parte = 0
            with abrir_parte(parte) as source:
                while chunk := source.read(1024 * 1024):
                    output.write(chunk)
                    digest.update(chunk)
                    digest_parte.update(chunk)
                    bytes_parte += len(chunk)
            if bytes_parte != parte["size"]:
                raise ValueError(f"Tamaño incorrecto: {parte['name']}.")
            esperado = parte.get("digest")
            if esperado and esperado != "sha256:" + digest_parte.hexdigest():
                raise ValueError(f"SHA-256 incorrecto: {parte['name']}.")
    if destino.stat().st_size != tamano or digest.hexdigest() != sha256:
        raise ValueError("El APK ensamblado no coincide con el archivo original.")


def asset_verificado(asset, tamano, sha256):
    return (
        asset.get("state") == "uploaded"
        and asset.get("size") == tamano
        and asset.get("digest") == "sha256:" + sha256
    )


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--asset-name", default=os.getenv("ASSET_NAME"))
    parser.add_argument("--size", type=int, default=os.getenv("EXPECTED_SIZE"))
    parser.add_argument("--sha256", default=os.getenv("EXPECTED_SHA256"))
    parser.add_argument("--release-id", type=int, default=os.getenv("RELEASE_ID"))
    parser.add_argument("--local-parts", type=Path)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    args.sha256 = (args.sha256 or "").lower()
    nombre = args.asset_name or ""
    if (
        not nombre.endswith(".apk")
        or len(nombre) > 180
        or "/" in nombre
        or "\\" in nombre
        or any(ord(char) < 32 for char in nombre)
    ):
        parser.error("Se requiere un nombre de APK sin directorios.")
    if not args.size or args.size <= 0 or not re.fullmatch(
        r"[a-f0-9]{64}", args.sha256 or ""
    ):
        parser.error("Se requieren tamaño y SHA-256 válidos.")

    if args.local_parts:
        if not args.output:
            parser.error("El modo local requiere --output; el archivo no debe existir.")
        partes = ordenar_partes(
            [
                {"name": path.name, "size": path.stat().st_size, "path": path}
                for path in args.local_parts.glob(nombre + ".part-*")
                if path.is_file()
            ],
            nombre,
        )
        ensamblar(
            partes, lambda parte: parte["path"].open("rb"), args.output,
            args.size, args.sha256,
        )
        print(f"APK local verificado: {args.output}; SHA-256 {args.sha256}")
        return

    if not args.release_id or args.release_id <= 0:
        parser.error("Se requiere un identificador de release positivo.")
    api = GitHubApi(os.getenv("GITHUB_REPOSITORY", ""), os.getenv("GITHUB_TOKEN"))
    release = api.leer(f"releases/{args.release_id}")
    if not release.get("draft"):
        raise ValueError("Solo se permite ensamblar assets en releases en borrador.")
    assets = api.assets(args.release_id)
    final = next((asset for asset in assets if asset["name"] == nombre), None)
    if final and not asset_verificado(final, args.size, args.sha256):
        if final.get("state") == "starter" and final.get("size") == 0:
            api.eliminar(final)
            final = None
        else:
            raise ValueError("El APK existente no coincide; no se sobrescribió.")
    if not final:
        partes = ordenar_partes(assets, nombre)
        with tempfile.TemporaryDirectory(
            prefix="uchumataqu-release-", dir=os.getenv("RUNNER_TEMP")
        ) as temporary:
            archivo = Path(temporary) / nombre
            ensamblar(partes, api.descargar, archivo, args.size, args.sha256)
            final = api.adjuntar(release, archivo)
    if not asset_verificado(final, args.size, args.sha256):
        raise ValueError("GitHub no confirmó el digest del APK; se conservan las partes.")
    for asset in assets:
        if re.fullmatch(re.escape(nombre) + r"\.part-[0-9]{3}", asset["name"]):
            api.eliminar(asset)
    print(f"APK adjunto y verificado: {final['browser_download_url']}")


if __name__ == "__main__":
    main()
