// TC2008B Modelación de Sistemas Multiagentes con gráficas computacionales
// C# client to interact with Python server via POST
// Adaptado para FireRescue2

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace FireRescue.Networking
{
    /// <summary>
    /// Cliente para comunicarse con el servidor Python
    /// </summary>
    public class APIClient : MonoBehaviour
    {
        [Header("Configuración del Servidor")]
        [Tooltip("URL del servidor Python")]
        public string serverURL = "http://localhost:8585";
        
        [Header("Estado")]
        public bool serverConnected = false;
        
        /// <summary>
        /// Obtiene los datos de simulación completa del servidor
        /// </summary>
        public IEnumerator ObtenerSimulacion(System.Action<string> onSuccess, System.Action<string> onError)
        {
            string url = $"{serverURL}/simulation_data";
            
            Debug.Log($"📡 Solicitando datos de simulación desde: {url}");
            
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || 
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    string error = $"❌ Error de conexión: {www.error}";
                    Debug.LogError(error);
                    serverConnected = false;
                    onError?.Invoke(error);
                }
                else
                {
                    Debug.Log($"✅ Datos recibidos del servidor ({www.downloadHandler.data.Length} bytes)");
                    // Muestra un preview del JSON para diagnosticar errores de parseo
                    string text = www.downloadHandler.text;
                    int previewLen = Mathf.Min(300, text.Length);
                    Debug.Log($"📄 Preview JSON: {text.Substring(0, previewLen)}{(text.Length>previewLen?"...":"")}");
                    serverConnected = true;
                    onSuccess?.Invoke(text);
                }
            }
        }
        
        /// <summary>
        /// Alternativa usando POST (por compatibilidad con el servidor)
        /// </summary>
        public IEnumerator ObtenerSimulacionPOST(System.Action<string> onSuccess, System.Action<string> onError)
        {
            string url = serverURL;
            
            Debug.Log($"📡 Solicitando datos via POST desde: {url}");
            
            WWWForm form = new WWWForm();
            
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                // El servidor responde con JSON directamente
                www.downloadHandler = new DownloadHandlerBuffer();
                
                yield return www.SendWebRequest();
                
                if (www.result == UnityWebRequest.Result.ConnectionError || 
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    string error = $"❌ Error de conexión: {www.error}";
                    Debug.LogError(error);
                    serverConnected = false;
                    onError?.Invoke(error);
                }
                else
                {
                    Debug.Log($"✅ Datos recibidos del servidor ({www.downloadHandler.data.Length} bytes)");
                    Debug.Log($"📄 Preview: {www.downloadHandler.text.Substring(0, Mathf.Min(200, www.downloadHandler.text.Length))}...");
                    serverConnected = true;
                    onSuccess?.Invoke(www.downloadHandler.text);
                }
            }
        }
        
        /// <summary>
        /// Verifica si el servidor está disponible
        /// </summary>
        public IEnumerator VerificarConexion(System.Action<bool> callback)
        {
            string url = $"{serverURL}/simulation_data";
            
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.timeout = 3; // 3 segundos de timeout
                yield return www.SendWebRequest();
                
                bool connected = www.result != UnityWebRequest.Result.ConnectionError && 
                                 www.result != UnityWebRequest.Result.ProtocolError;
                
                serverConnected = connected;
                
                if (connected)
                {
                    Debug.Log("✅ Servidor Python conectado");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Servidor Python no disponible: {www.error}");
                }
                
                callback?.Invoke(connected);
            }
        }
    }
}
