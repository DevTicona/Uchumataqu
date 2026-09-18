using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Scripts.Editor.Tests
{
    public class MvpDependencyTests
    {
        private static string CodigoRuntime(string archivo)
        {
            string codigo = File.ReadAllText(archivo);
            codigo = Regex.Replace(codigo, @"#if UNITY_EDITOR\b.*?#endif", "", RegexOptions.Singleline);
            return Regex.Replace(codigo, @"/\*.*?\*/|//[^\r\n]*", "", RegexOptions.Singleline);
        }

        [Test]
        public void Views_NoAccedenAFuentesDeDatosNiRepositorios()
        {
            string directorio = Path.Combine(Application.dataPath, "Scripts", "UI");
            foreach (string archivo in Directory.GetFiles(directorio, "*View.cs", SearchOption.AllDirectories))
            {
                string codigo = CodigoRuntime(archivo);
                Assert.IsFalse(Regex.IsMatch(codigo,
                    @"\b\w*Repository\s*\.|\b(?:File|Directory|Microphone|Resources|Addressables)\s*\.|\bApplication\s*\.\s*Quit\s*\("),
                    $"La Vista contiene acceso a datos o acciones de aplicación: {archivo}");
            }
        }

        [Test]
        public void Motores_NoDependenDeUIRepositoriosServiciosOUnity()
        {
            string directorio = Path.Combine(Application.dataPath, "Scripts", "PLN", "Domain");
            foreach (string archivo in Directory.GetFiles(directorio, "*.cs"))
                Assert.IsFalse(Regex.IsMatch(CodigoRuntime(archivo),
                    @"\bUnityEngine\b|\b\w*(?:View|Presenter|Repository|Service)\b|\b(?:File|Directory)\s*\."), archivo);
        }

        [Test]
        public void RepositoriosYServiciosTecnicos_NoDependenDeViewsOPresenters()
        {
            foreach (string archivo in Directory.GetFiles(Path.Combine(Application.dataPath, "Scripts"), "*.cs", SearchOption.AllDirectories))
            {
                string nombre = Path.GetFileNameWithoutExtension(archivo);
                if (!nombre.EndsWith("Repository") && !nombre.EndsWith("Service")) continue;
                Assert.IsFalse(Regex.IsMatch(CodigoRuntime(archivo), @"\b\w+(?:View|Presenter)\b"), archivo);
            }
        }

        [Test]
        public void Persistencia_NoDeclaraPresentersDePantalla()
        {
            string directorio = Path.Combine(Application.dataPath, "Scripts", "Data", "Persistencia");
            foreach (string archivo in Directory.GetFiles(directorio, "*.cs", SearchOption.AllDirectories))
                Assert.IsFalse(Regex.IsMatch(CodigoRuntime(archivo), @"\bclass\s+\w*Presenter\b"), archivo);
        }
    }
}
