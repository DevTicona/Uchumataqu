using System.Collections.Generic;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Activities;

public class ActividadData
{
    // Palabras que representan la respuesta correcta
    public List<PalabraData> palabrasCorrectas;

    // Palabras distractoras
    public List<PalabraData> palabrasIncorrectas;

    // Dirección (Uchumataqu → Español, etc.)
    public DireccionPregunta direccion;
}
