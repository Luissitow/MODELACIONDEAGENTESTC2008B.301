using UnityEngine;
using FireRescue.Networking;

/// <summary>
/// Script de prueba para verificar la conexión con el servidor
/// </summary>
public class TestConexionServidor : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🧪 Probando conexión al servidor...");
        
        // Crear APIClient temporal
        APIClient client = gameObject.AddComponent<APIClient>();
        
        // Intentar obtener simulación
        StartCoroutine(client.ObtenerSimulacion(
            onSuccess: (jsonData) => {
                Debug.Log($"✅ ÉXITO: Recibidos {jsonData.Length} caracteres del servidor");
                Debug.Log($"📄 Primeros 500 caracteres:\n{jsonData.Substring(0, Mathf.Min(500, jsonData.Length))}");
            },
            onError: (error) => {
                Debug.LogError($"❌ ERROR: {error}");
                Debug.LogError("🔍 Verifica que el notebook esté ejecutándose en el puerto 8585");
            }
        ));
    }
}
