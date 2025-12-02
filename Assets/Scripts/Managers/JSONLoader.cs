using UnityEngine;

public static class JSONLoader
{
    /// <summary>
    /// Carga el escenario desde Resources/[nombreArchivo].json
    /// </summary>
    /// <param name="nombreArchivo">Nombre del archivo sin extensión (ej: "escenario" o "simulacion_completa")</param>
    public static EscenarioData CargarEscenario(string nombreArchivo = "escenario")
    {
        // Modo Local: Lee desde Resources
        TextAsset jsonFile = Resources.Load<TextAsset>(nombreArchivo);
        
        if (jsonFile == null)
        {
            Debug.LogError($"❌ No se pudo cargar Resources/{nombreArchivo}.json");
            return null;
        }
        
        Debug.Log($"📄 Archivo cargado: {nombreArchivo}.json ({jsonFile.text.Length} caracteres)");
        
        // Deserializa JSON a objeto C#
        return ParsearJSON(jsonFile.text);
    }
    
    /// <summary>
    /// Parsea un string JSON a EscenarioData
    /// Usado cuando se reciben datos del servidor Python
    /// </summary>
    public static EscenarioData ParsearJSON(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
        {
            Debug.LogError("❌ JSON vacío o nulo");
            return null;
        }
        
        try
        {
            EscenarioData escenario = JsonUtility.FromJson<EscenarioData>(jsonData);
            
            if (escenario == null)
            {
                Debug.LogError("❌ Error al deserializar el JSON del escenario");
                return null;
            }
            
            Debug.Log($"✅ Escenario parseado desde servidor: {escenario.turnos.Length} turnos");
            return escenario;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Excepción al parsear JSON: {e.Message}");
            return null;
        }
    }
}
