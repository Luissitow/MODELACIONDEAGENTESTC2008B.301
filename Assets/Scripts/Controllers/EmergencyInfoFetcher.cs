using UnityEngine;
using TMPro;

public class EmergencyInfoFetcher : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI emergencyInfoText;
    public UnityEngine.UI.Button fetchInfoButton;
    
    void Start()
    {
        // Configurar el botón siguiendo el patrón del laboratorio Chuck Norris
        if (fetchInfoButton != null)
            fetchInfoButton.onClick.AddListener(GetNewEmergencyInfo);
        
        // Texto inicial
        if (emergencyInfoText != null)
            emergencyInfoText.text = "Presiona el botón para obtener información de emergencia";
    }
    
    // Función principal que obtiene nueva información (como NewJoke() en Chuck Norris)
    public void GetNewEmergencyInfo()
    {
        // Simular obtener información de emergencia desde una API
        EmergencyInfo info = EmergencyAPIHelper.GetRandomEmergency();
        
        if (emergencyInfoText != null && info != null)
        {
            // Formatear la información para mostrar (como se hacía con el chiste)
            emergencyInfoText.text = FormatEmergencyInfo(info);
        }
    }
    
    // Formatear la información de emergencia para mostrar
    private string FormatEmergencyInfo(EmergencyInfo info)
    {
        string formattedText = $"<size=16><b>🚨 EMERGENCIA ACTIVA</b></size>\n\n";
        formattedText += $"<b>Tipo:</b> {info.type}\n";
        formattedText += $"<b>Ubicación:</b> ({info.location.x}, {info.location.y})\n";
        formattedText += $"<b>Prioridad:</b> {info.priority}\n";
        formattedText += $"<b>Víctimas:</b> {info.victims}\n";
        formattedText += $"<b>Descripción:</b>\n{info.description}";
        
        return formattedText;
    }
    
    void OnDestroy()
    {
        // Limpiar listener del botón
        if (fetchInfoButton != null)
            fetchInfoButton.onClick.RemoveListener(GetNewEmergencyInfo);
    }
}

// Clase helper para obtener información de emergencias (como APIHelper en Chuck Norris)
public static class EmergencyAPIHelper
{
    // Obtener información de emergencia aleatoria (simulada o desde API real)
    public static EmergencyInfo GetRandomEmergency()
    {
        // Por ahora simulo datos, pero esto podría conectar a una API real
        return GenerateRandomEmergency();
    }
    
    // Generar emergencia aleatoria para demo
    private static EmergencyInfo GenerateRandomEmergency()
    {
        string[] emergencyTypes = { "Incendio", "Rescate", "Explosión", "Accidente", "Derrumbe" };
        string[] priorities = { "BAJA", "MEDIA", "ALTA", "CRÍTICA" };
        
        string[] descriptions = {
            "Fuego en edificio residencial, múltiples víctimas atrapadas",
            "Persona atrapada en escombros, acceso limitado",
            "Explosión de gas reportada, evacuación necesaria",
            "Accidente vehicular con heridos, vía bloqueada",
            "Derrumbe estructural, posibles supervivientes"
        };
        
        EmergencyInfo emergency = new EmergencyInfo();
        emergency.type = emergencyTypes[Random.Range(0, emergencyTypes.Length)];
        emergency.priority = priorities[Random.Range(0, priorities.Length)];
        emergency.description = descriptions[Random.Range(0, descriptions.Length)];
        emergency.location = new Vector2Int(Random.Range(0, 8), Random.Range(0, 6));
        emergency.victims = Random.Range(1, 6);
        emergency.timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        
        return emergency;
    }
}

// Modelo de datos para información de emergencia (como Joke en Chuck Norris)
[System.Serializable]
public class EmergencyInfo
{
    public string type;
    public string priority;
    public string description;
    public Vector2Int location;
    public int victims;
    public string timestamp;
    public string status = "ACTIVA";
}