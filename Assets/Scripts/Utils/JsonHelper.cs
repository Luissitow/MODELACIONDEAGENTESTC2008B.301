using UnityEngine;
using System.Text.RegularExpressions;

/// <summary>
/// Helper para deserializar JSON con snake_case a objetos C# con camelCase
/// Compatible con Unity's JsonUtility (no requiere Newtonsoft.Json)
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// Convierte snake_case a camelCase en todo el JSON
    /// Ejemplos: 
    /// "astronauta_id" -> "astronautaID" (ID en mayúsculas)
    /// "duracion_total" -> "duracionTotal"
    /// "estado_despues" -> "estadoDespues"
    /// </summary>
    public static string ConvertSnakeCaseToCamelCase(string json)
    {
        // Patrón mejorado para manejar múltiples underscores y casos especiales
        // Busca: "palabra_palabra_palabra": 
        var pattern = @"""([a-z_]+)""(\s*:)";
        
        json = Regex.Replace(json, pattern, match =>
        {
            string snakeCase = match.Groups[1].Value;
            string colon = match.Groups[2].Value;
            
            // Si no tiene underscore, dejar como está
            if (!snakeCase.Contains("_"))
                return match.Value;
            
            // Convertir a camelCase
            string camelCase = SnakeToCamel(snakeCase);
            
            return $"\"{camelCase}\"{colon}";
        });
        
        return json;
    }
    
    /// <summary>
    /// Convierte una cadena snake_case a camelCase
    /// </summary>
    private static string SnakeToCamel(string snakeCase)
    {
        string[] parts = snakeCase.Split('_');
        
        if (parts.Length == 0)
            return snakeCase;
        
        // Primera parte en minúsculas
        string result = parts[0];
        
        // Resto de partes con primera letra en mayúsculas
        for (int i = 1; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
            {
                // Casos especiales: "id" -> "ID"
                if (parts[i] == "id")
                {
                    result += "ID";
                }
                else
                {
                    result += char.ToUpper(parts[i][0]) + parts[i].Substring(1);
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Deserializa JSON snake_case a objeto C# con JsonUtility
    /// </summary>
    public static T Deserialize<T>(string json)
    {
        // Convertir snake_case a camelCase
        string convertedJson = ConvertSnakeCaseToCamelCase(json);
        
        // DEBUG: Guardar JSON convertido para inspección
        #if UNITY_EDITOR
        if (convertedJson.Length < 5000) // Solo para JSONs pequeños
        {
            Debug.Log($"[JsonHelper] JSON convertido (primeros 500 chars): {convertedJson.Substring(0, Mathf.Min(500, convertedJson.Length))}...");
        }
        #endif
        
        // Deserializar con JsonUtility de Unity
        return JsonUtility.FromJson<T>(convertedJson);
    }
}
