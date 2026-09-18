using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor.Tests
{
    public static class TestRunnerHelper
    {
        [MenuItem("Uchumataqu/Ejecutar Todas las Pruebas Unitarias")]
        public static void EjecutarTodasLasPruebas() => EjecutarSuite();

        public static int EjecutarSuite()
        {
            var resultados = new List<string>();
            int aprobadas = 0, fallidas = 0;
            using (var scope = new PersistenciaTestScope())
            {
                var tipos = typeof(TestRunnerHelper).Assembly.GetTypes()
                    .Where(tipo => tipo.Namespace == typeof(TestRunnerHelper).Namespace)
                    .OrderBy(tipo => tipo.Name);
                foreach (Type tipo in tipos)
                {
                    foreach (var prueba in tipo.GetMethods()
                        .Where(metodo => metodo.IsDefined(typeof(TestAttribute), true))
                        .OrderBy(metodo => metodo.Name))
                    {
                        object instancia = Activator.CreateInstance(tipo);
                        string nombre = tipo.Name + "." + prueba.Name;
                        Exception fallo = null;
                        try
                        {
                            if (prueba.GetParameters().Length != 0 || prueba.ReturnType != typeof(void))
                                throw new NotSupportedException("El ejecutor requiere pruebas sin parámetros y síncronas; utiliza Test Runner para otros tipos.");
                            foreach (var setup in tipo.GetMethods().Where(m => m.IsDefined(typeof(SetUpAttribute), true)))
                                setup.Invoke(instancia, null);
                            prueba.Invoke(instancia, null);
                        }
                        catch (Exception ex) { fallo = ex.InnerException ?? ex; }
                        finally
                        {
                            try
                            {
                                foreach (var teardown in tipo.GetMethods().Where(m => m.IsDefined(typeof(TearDownAttribute), true)))
                                    teardown.Invoke(instancia, null);
                            }
                            catch (Exception ex) { fallo = ex.InnerException ?? ex; }
                        }
                        if (fallo == null)
                        {
                            aprobadas++;
                            resultados.Add("PASS " + nombre);
                        }
                        else
                        {
                            fallidas++;
                            resultados.Add("FAIL " + nombre + ": " + fallo.Message);
                            Debug.LogError("[Tests] " + nombre + ": " + fallo);
                        }
                    }
                }
            }
            string resumen = $"TOTAL {aprobadas} PASS, {fallidas} FAIL";
            resultados.Add(resumen);
            const string directorio = "Docs/Reportes/Estabilizacion";
            Directory.CreateDirectory(directorio);
            File.WriteAllLines(Path.Combine(directorio, "pruebas-post-auditoria.txt"), resultados);
            Debug.Log("[Tests] " + resumen + "; persistencia aislada en Temp.");
            return fallidas;
        }
    }
}