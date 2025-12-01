using UnityEngine;

public static class JSONLoader
{
    private const string RUTA_JSON = "escenario";
    
    /// <summary>
    /// Carga el escenario desde Resources/escenario.json
    /// NOTA: Preparado para cambiar a API en el futuro
    /// </summary>
    public static EscenarioData CargarEscenario()
    {
        // Modo Local: Lee desde Resources
        TextAsset jsonFile = Resources.Load<TextAsset>(RUTA_JSON);
        
        if (jsonFile == null)
        {
            Debug.LogError($"No se pudo cargar el archivo {RUTA_JSON}.json desde Resources");
            return null;
        }
        
        // Deserializa JSON a objeto C#
        EscenarioData escenario = JsonUtility.FromJson<EscenarioData>(jsonFile.text);
        
        if (escenario == null)
        {
            Debug.LogError("Error al deserializar el JSON del escenario");
            return null;
        }
        
        Debug.Log($"✅ Escenario cargado: {escenario.turnos.Length} turnos");
        return escenario;
    }
    
    // TODO: Para conectar con API en el futuro
    /*
    public static EscenarioData CargarDesdeAPI(string url)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string json = reader.ReadToEnd();
        
        return JsonUtility.FromJson<EscenarioData>(json);
    }
    */
}
