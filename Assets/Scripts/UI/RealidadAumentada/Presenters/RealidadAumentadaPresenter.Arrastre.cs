using System.Collections;
using Assets.Scripts.RA;
using UnityEngine;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public partial class RealidadAumentadaPresenter
    {
        private bool PuedeArrastrarObjetoGuiado(ObjetoRAView objeto)
        {
            if (AyudaBloqueaInteraccion) return false;
            var paso = mision?.PasoActual;
            return PuedeInteractuarGuiado && inicioGuiado && aciertoPendienteGuiado == null &&
                !animandoColocacion && !procesandoVoz && !iniciandoMicrofono && !grabacionVoz.EstaGrabando &&
                paso?.tipo == "arrastrar" && !paso.UsaTarjeta && objeto.ObjetoId == paso.objetivo_objeto_id;
        }

        private DestinoRA ResolverDestinoArrastreGuiado(ObjetoRAView objeto, Vector2 posicion)
        {
            var paso = mision?.PasoActual;
            if (paso?.tipo != "arrastrar" || paso.UsaTarjeta || objeto.ObjetoId != paso.objetivo_objeto_id ||
                !destinos.TryGetValue(paso.destino_id, out var esperado)) return null;
            if (interaccion.ResolverImpacto(interaccion.Camara.ScreenPointToRay(posicion), out var impacto))
            {
                var destino = impacto.collider.GetComponentInParent<DestinoRA>();
                if (destino != null) return destino == esperado ? destino : null;
                var receptor = impacto.collider.GetComponentInParent<ObjetoRAView>();
                if (receptor != null && receptor != esperado.ObjetoReceptor) return null;
                if (receptor == esperado.ObjetoReceptor && receptor != null) return esperado;
            }
            return esperado.ContieneEnPantalla(posicion, interaccion.Camara) ? esperado : null;
        }

        private IEnumerator AnimarColocacionObjeto(ObjetoRAView actor, DestinoRA destino, bool vertical)
        {
            var objeto = actor.transform;
            var punto = destino.PuntoPara(actor.ObjetoId);
            var prenda = actor.GetComponent<PrendaRAView>();
            Vector3 inicio = objeto.position;
            Quaternion giro = objeto.rotation;
            Vector3 arriba = vertical ? root.transform.up : punto.up;
            Quaternion final = vertical ? GeometriaColocacionRA.RotacionVertical(objeto, punto, arriba) : punto.rotation;
            if (vertical) GeometriaColocacionRA.AlinearPiezasParaColocacionVertical(objeto, final, arriba);
            bool tieneLimites = GeometriaColocacionRA.ObtenerLimitesLocales(objeto, out var limites, vertical);
            Vector3 centro = vertical && tieneLimites ? limites.center : Vector3.zero;
            Vector3 offset = final * Vector3.Scale(centro, objeto.lossyScale);
            float duracion = vertical ? 0.85f : 0.55f;
            float tiempo = 0f;
            while (tiempo < duracion)
            {
                if (!colocador.TieneSeguimiento || (UsaFooterGuiado && !PuedeInteractuarGuiado)) { yield return null; continue; }
                tiempo += Time.deltaTime;
                float t = Mathf.Clamp01(tiempo / duracion);
                float rotacion = vertical ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.45f)) : Mathf.SmoothStep(0f, 1f, t);
                float avance = vertical ? Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.3f, 1f, t)) : rotacion;
                float elevacion = vertical ? 0.08f * root.transform.lossyScale.x : prenda != null ? prenda.Elevacion : 0f;
                objeto.SetPositionAndRotation(Vector3.Lerp(inicio, punto.position - offset, avance) +
                    arriba * (elevacion * Mathf.Sin(Mathf.PI * t)), Quaternion.Slerp(giro, final, rotacion));
                yield return null;
            }
            objeto.SetParent(punto, true);
            objeto.SetPositionAndRotation(punto.position - offset, final);
            prenda?.Equipar();
            if (vertical && destino.OcultarAlColocar)
            {
                // Conserva el ancla activa para mostrar la palabra después de guardar el útil.
                foreach (var renderer in actor.GetComponentsInChildren<Renderer>())
                    renderer.gameObject.SetActive(false);
                if (destino.ObjetoReceptor != null)
                    actor.AnclaDialogo.position = destino.ObjetoReceptor.AnclaDialogo.position;
            }
            else objeto.gameObject.SetActive(!destino.OcultarAlColocar);
        }
    }
}
