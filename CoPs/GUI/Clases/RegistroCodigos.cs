using GUI.Clases;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;

public class RegistroCodigos
{
    private string filePath;
    public List<EntradaRegistro> Codigos { get; private set; }
    private Dictionary<string, int> CodigoUso;

    public RegistroCodigos(string path)
    {
        filePath = path;
        CodigoUso = new Dictionary<string, int>();
        CargarDesdeJSON();
    }

    private void CargarDesdeJSON()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Codigos = JsonConvert.DeserializeObject<List<EntradaRegistro>>(json) ?? new List<EntradaRegistro>();

            // Cargar el diccionario de conteo de forma eficiente
            CodigoUso = Codigos
                .GroupBy(c => c.CodigoValor)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        else
        {
            Codigos = new List<EntradaRegistro>();
            CodigoUso = new Dictionary<string, int>();
        }
    }

    public void AgregarCodigo(string codigo)
    {
        var fecha = DateTime.Now;
        Codigos.Add(new EntradaRegistro(codigo, fecha));

        // Actualizar el diccionario de conteo de uso
        if (CodigoUso.ContainsKey(codigo))
            CodigoUso[codigo]++;
        else
            CodigoUso[codigo] = 1;
    }

    public int ObtenerVecesUsado(string codigo)
    {
        return CodigoUso.ContainsKey(codigo) ? CodigoUso[codigo] : 0;
    }

    public void GuardarEnJSON()
    {
        string json = JsonConvert.SerializeObject(Codigos, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
}
