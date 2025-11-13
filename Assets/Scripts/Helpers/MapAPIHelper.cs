using UnityEngine;
using System.Net;
using System.IO;
using System;

public static class MapAPIHelper
{
    private static string baseURL = "http://localhost:5000/api"; // Cambia por tu URL del backend
    
    // Obtener el estado actual del mapa desde el backend
    public static GameMap GetCurrentMap()
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{baseURL}/map");
            request.Method = "GET";
            request.ContentType = "application/json";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string json = reader.ReadToEnd();

            return JsonUtility.FromJson<GameMap>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error obteniendo mapa: {e.Message}");
            return CreateDefaultMap(); // Retornar mapa por defecto en caso de error
        }
    }
    
    // Enviar el estado actualizado del mapa al backend
    public static bool SendMapUpdate(GameMap map)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{baseURL}/map");
            request.Method = "POST";
            request.ContentType = "application/json";

            string json = JsonUtility.ToJson(map);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
            
            request.ContentLength = data.Length;
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error enviando actualización de mapa: {e.Message}");
            return false;
        }
    }
    
    // Obtener movimientos de agentes desde el backend
    public static AgentMovement[] GetAgentMovements()
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{baseURL}/agents/movements");
            request.Method = "GET";
            request.ContentType = "application/json";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string json = reader.ReadToEnd();

            AgentMovementsWrapper wrapper = JsonUtility.FromJson<AgentMovementsWrapper>(json);
            return wrapper.movements;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error obteniendo movimientos de agentes: {e.Message}");
            return new AgentMovement[0];
        }
    }
    
    // Cargar mapa desde archivo JSON local (para testing)
    public static GameMap LoadMapFromFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<GameMap>(json);
            }
            else
            {
                Debug.LogWarning($"Archivo no encontrado: {filePath}");
                return CreateDefaultMap();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error cargando mapa desde archivo: {e.Message}");
            return CreateDefaultMap();
        }
    }
    
    // Guardar mapa en archivo JSON local
    public static bool SaveMapToFile(GameMap map, string filePath)
    {
        try
        {
            string json = JsonUtility.ToJson(map, true);
            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error guardando mapa en archivo: {e.Message}");
            return false;
        }
    }
    
    // Crear mapa por defecto basado en tu ejemplo
    private static GameMap CreateDefaultMap()
    {
        GameMap defaultMap = new GameMap();
        defaultMap.width = 8;
        defaultMap.height = 6;
        
        // Tu matriz de ejemplo
        defaultMap.walls = new int[][]
        {
            new int[] { 0x9, 0x8, 0xC, 0x9, 0xC, 0x9, 0x8, 0xC }, // 1001 1000 1100 1001 1100 1001 1000 1100
            new int[] { 0x1, 0x0, 0x6, 0x3, 0x6, 0x3, 0x2, 0x6 }, // 0001 0000 0110 0011 0110 0011 0010 0110
            new int[] { 0x1, 0x4, 0x9, 0x8, 0x8, 0xC, 0x9, 0xC }, // 0001 0100 1001 1000 1000 1100 1001 1100
            new int[] { 0x3, 0x6, 0x3, 0x2, 0x2, 0x6, 0x3, 0x6 }, // 0011 0110 0011 0010 0010 0110 0011 0110
            new int[] { 0x9, 0x8, 0x8, 0x8, 0xC, 0x9, 0xC, 0xD }, // 1001 1000 1000 1000 1100 1001 1100 1101
            new int[] { 0x3, 0x2, 0x2, 0x2, 0x6, 0x3, 0x6, 0x7 }  // 0011 0010 0010 0010 0110 0011 0110 0111
        };
        
        // Posiciones de elementos basadas en tu ejemplo
        defaultMap.victims = new Vector2Int[]
        {
            new Vector2Int(2, 4), // v
            new Vector2Int(5, 8)  // v
        };
        
        defaultMap.fires = new Vector2Int[]
        {
            new Vector2Int(5, 1) // f
        };
        
        // Otras posiciones que mencionaste
        defaultMap.doors = new Vector2Int[]
        {
            new Vector2Int(1, 6),
            new Vector2Int(3, 1),
            new Vector2Int(4, 8),
            new Vector2Int(6, 3)
        };
        
        return defaultMap;
    }
}

// Clase auxiliar para deserializar arrays de movimientos
[System.Serializable]
public class AgentMovementsWrapper
{
    public AgentMovement[] movements;
}

[System.Serializable]
public class AgentMovement
{
    public int agentId;
    public Vector2Int from;
    public Vector2Int to;
    public string action; // "move", "rescue", "extinguish", etc.
}