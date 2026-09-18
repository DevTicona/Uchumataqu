using System;
using UnityEngine;

namespace Assets.Scripts.ASR
{
    public sealed class GrabacionVozService : IDisposable
    {
        private AudioClip _grabacion;
        public bool EstaGrabando { get; private set; }
        public bool TieneDispositivo => Application.isMobilePlatform || Microphone.devices.Length > 0;

        public bool Iniciar()
        {
            Dispose();
            _grabacion = Microphone.Start(null, false, 6, 16000);
            return EstaGrabando = _grabacion != null;
        }

        public AudioClip Finalizar()
        {
            if (!EstaGrabando || _grabacion == null) return null;
            int muestras = Microphone.GetPosition(null);
            Microphone.End(null);
            EstaGrabando = false;
            if (muestras <= 0) { Dispose(); return null; }

            var datos = new float[muestras * _grabacion.channels];
            if (!_grabacion.GetData(datos, 0)) { Dispose(); return null; }
            AudioClip recortado = AudioClip.Create("pronunciacion", muestras, _grabacion.channels, _grabacion.frequency, false);
            recortado.SetData(datos, 0);
            Dispose();
            return recortado;
        }

        public void Dispose()
        {
            if (EstaGrabando) Microphone.End(null);
            EstaGrabando = false;
            if (_grabacion != null) UnityEngine.Object.Destroy(_grabacion);
            _grabacion = null;
        }
    }
}
