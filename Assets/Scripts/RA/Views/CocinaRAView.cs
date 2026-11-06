using System.Collections;
using UnityEngine;

namespace Assets.Scripts.RA
{
    public sealed class CocinaRAView : MonoBehaviour, IAccionEscenarioRA
    {
        [SerializeField] private Transform leche;
        [SerializeField] private Transform vaso;
        [SerializeField] private Transform plato;
        [SerializeField] private Transform puntoPlato;
        [SerializeField] private Transform puntoOllaMesa;
        [SerializeField] private Transform lecheVaso;
        [SerializeField] private Transform arrozCrudo;
        [SerializeField] private Transform arrozCocido;
        [SerializeField] private Transform arrozPlato;
        [SerializeField] private GameObject suciedad;
        [SerializeField] private LineRenderer chorro;
        [SerializeField] private Transform tapaOlla;
        [SerializeField] private Transform tapaAbierta;
        [SerializeField] private Transform bordeVasija;
        [SerializeField] private Transform centroOlla;
        [SerializeField] private Transform origenVasija;
        [SerializeField] private Transform rutaArroz;
        [SerializeField] private Transform picoJarra;
        [SerializeField] private Transform centroVaso;
        [SerializeField] private Transform rutaLeche;

        public void ConfigurarVertidos(Transform tapa, Transform abierta, Transform borde, Transform entrada,
            Transform retorno, Transform recorridoArroz, Transform pico, Transform entradaVaso, Transform recorridoLeche)
        {
            tapaOlla = tapa;
            tapaAbierta = abierta;
            bordeVasija = borde;
            centroOlla = entrada;
            origenVasija = retorno;
            rutaArroz = recorridoArroz;
            picoJarra = pico;
            centroVaso = entradaVaso;
            rutaLeche = recorridoLeche;
        }
        private float nivelLeche;
        private bool bebiendo;
        private bool vertiendo;

        public void Configurar(Transform jarra, Transform recipiente, Transform vajilla, Transform destino,
            Transform liquido, Transform crudo, Transform cocido, Transform servido, GameObject mancha, LineRenderer flujo)
        {
            leche = jarra; vaso = recipiente; plato = vajilla; puntoPlato = destino;
            lecheVaso = liquido; arrozCrudo = crudo; arrozCocido = cocido;
            arrozPlato = servido; suciedad = mancha; chorro = flujo;
        }

        public bool Contiene(string reaccion) => reaccion == "olla_limpiar" || reaccion == "arroz_cocinar" ||
            reaccion == "plato_colocar" || reaccion == "leche_servir" || reaccion == "arroz_servir" || reaccion == "nino_beber";
        public bool SustituyeColocacion(string objetoId) => objetoId == "obj_leche" || objetoId == "obj_olla" || objetoId == "obj_arroz";

        public void ConfigurarPuntoOlla(Transform punto) => puntoOllaMesa = punto;

        public IEnumerator Ejecutar(string reaccion)
        {
            switch (reaccion)
            {
                case "nino_beber":
                    bebiendo = true;
                    break;
                case "olla_limpiar":
                    suciedad.SetActive(false);
                    break;
                case "arroz_cocinar":
                    yield return CocinarArroz();
                    break;
                case "plato_colocar":
                    yield return Mover(plato, puntoPlato.position, puntoPlato.rotation, 0.6f);
                    break;
                case "leche_servir":
                    yield return ServirLeche();
                    break;
                case "arroz_servir":
                    var olla = arrozCocido.GetComponentInParent<ObjetoRAView>().transform;
                    if (puntoOllaMesa != null)
                        yield return Mover(olla, puntoOllaMesa.position, puntoOllaMesa.rotation, 0.6f);
                    Vector3 cerrada = tapaOlla.localPosition;
                    Quaternion giroTapa = tapaOlla.localRotation;
                    yield return Mover(tapaOlla, tapaAbierta.position, tapaOlla.rotation, 0.3f);
                    yield return Esperar(0.4f);
                    arrozPlato.localScale = Vector3.one;
                    arrozCocido.localScale = Vector3.zero;
                    yield return Mover(tapaOlla, tapaOlla.parent.TransformPoint(cerrada),
                        tapaOlla.parent.rotation * giroTapa, 0.3f);
                    break;
            }
        }

        private IEnumerator CocinarArroz()
        {
            var vasija = arrozCrudo.GetComponentInParent<ObjetoRAView>().transform;
            Quaternion giro = origenVasija.rotation;
            Vector3 cerrada = tapaOlla.localPosition;
            Quaternion giroTapa = tapaOlla.localRotation;
            yield return Mover(tapaOlla, tapaAbierta.position, tapaOlla.rotation, 0.35f);

            Vector3 alto = AAltura(vasija.position, rutaArroz.position);
            yield return Mover(vasija, alto, giro, 0.35f);
            Quaternion inclinado = Quaternion.AngleAxis(65f, transform.forward) * giro;
            Vector3 destino = PosicionParaAlinear(vasija, bordeVasija, centroOlla.position, inclinado);
            Vector3 sobreOlla = AAltura(destino, rutaArroz.position);
            yield return Mover(vasija, sobreOlla, giro, 0.6f);
            yield return Mover(vasija, destino, inclinado, 0.4f);
            yield return Esperar(0.4f);
            arrozCrudo.localScale = Vector3.zero;
            arrozCocido.localScale = Vector3.one;
            yield return Mover(vasija, sobreOlla, giro, 0.4f);
            yield return Mover(vasija, AAltura(origenVasija.position, rutaArroz.position), giro, 0.6f);
            yield return Mover(vasija, origenVasija.position, giro, 0.35f);
            yield return Mover(tapaOlla, tapaOlla.parent.TransformPoint(cerrada),
                tapaOlla.parent.rotation * giroTapa, 0.35f);
        }

        private IEnumerator ServirLeche()
        {
            Vector3 inicio = leche.position;
            Quaternion giro = leche.rotation;
            Vector3 alto = AAltura(inicio, rutaLeche.position);
            yield return Mover(leche, alto, giro, 0.4f);
            yield return Mover(leche, rutaLeche.position, giro, 0.6f);
            Quaternion inclinado = Quaternion.AngleAxis(-50f, transform.forward) * giro;
            Vector3 entrada = AAltura(centroVaso.position, rutaLeche.position);
            Vector3 destino = PosicionParaAlinear(leche, picoJarra, entrada, inclinado);
            yield return Mover(leche, destino, inclinado, 0.7f);

            vertiendo = true;
            chorro.enabled = true;
            float tiempo = 0f;
            while (tiempo < 0.8f)
            {
                if (gameObject.activeInHierarchy) tiempo += Time.deltaTime;
                chorro.SetPosition(0, picoJarra.position);
                chorro.SetPosition(1, centroVaso.position);
                chorro.startWidth = chorro.endWidth = 0.012f * Mathf.Abs(leche.lossyScale.x);
                nivelLeche = Mathf.Clamp01(tiempo / 0.8f);
                lecheVaso.localScale = Vector3.one * nivelLeche;
                yield return null;
            }
            vertiendo = false;
            chorro.enabled = false;
            yield return Mover(leche, alto, giro, 0.7f);
            yield return Mover(leche, inicio, giro, 0.4f);
        }

        private Vector3 AAltura(Vector3 posicion, Vector3 referencia)
            => posicion + transform.up * Vector3.Dot(referencia - posicion, transform.up);

        private static Vector3 PosicionParaAlinear(Transform objeto, Transform punto, Vector3 destino, Quaternion giro)
        {
            Vector3 offset = Quaternion.Inverse(objeto.rotation) * (punto.position - objeto.position);
            return destino - giro * offset;
        }


        private IEnumerator Mover(Transform objeto, Vector3 destino, Quaternion giro, float duracion)
        {
            Vector3 inicio = objeto.position;
            Quaternion rotacion = objeto.rotation;
            float tiempo = 0f;
            while (tiempo < duracion)
            {
                if (gameObject.activeInHierarchy) tiempo += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / duracion));
                objeto.SetPositionAndRotation(Vector3.Lerp(inicio, destino, t), Quaternion.Slerp(rotacion, giro, t));
                yield return null;
            }
            objeto.SetPositionAndRotation(destino, giro);
        }

        private IEnumerator Esperar(float duracion)
        {
            float tiempo = 0f;
            while (tiempo < duracion)
            {
                if (gameObject.activeInHierarchy) tiempo += Time.deltaTime;
                yield return null;
            }
        }

        private void OnEnable() { if (chorro != null) chorro.enabled = vertiendo; }
        private void OnDisable() { if (chorro != null) chorro.enabled = false; }

        private void LateUpdate()
        {
            // El rig Generic también escribe la pose de sus nodos sin curvas.
            // La cocina conserva el llenado hasta entregar el vaso a la animación de beber.
            if (!bebiendo && lecheVaso != null) lecheVaso.localScale = Vector3.one * nivelLeche;
        }

        public void Reiniciar()
        {
            nivelLeche = 0f;
            bebiendo = false;
            vertiendo = false;
            if (chorro != null) chorro.enabled = false;
        }
    }
}
