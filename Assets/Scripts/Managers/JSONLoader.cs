using UnityEngine;

public static class JSONLoader
{
    /// <summary>
    /// Carga el escenario desde Assets/Scripts/Domain/[nombreArchivo].json
    /// </summary>
    /// <param name="nombreArchivo">Nombre del archivo sin extensión (ej: "simulacion")</param>
    public static EscenarioData CargarEscenario(string nombreArchivo = "simulacion")
    {
        // Ruta al archivo JSON en Assets/Scripts/Domain/
        string rutaCompleta = System.IO.Path.Combine(Application.dataPath, "Scripts", "Domain", nombreArchivo + ".json");
        
        if (!System.IO.File.Exists(rutaCompleta))
        {
            Debug.LogError($"❌ No se pudo encontrar el archivo: {rutaCompleta}");
            return null;
        }
        
        string jsonContent = System.IO.File.ReadAllText(rutaCompleta);
        Debug.Log($"📄 Archivo cargado: {nombreArchivo}.json ({jsonContent.Length} caracteres)");
        
        // Deserializa JSON a objeto C#
        return ParsearJSON(jsonContent);
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
