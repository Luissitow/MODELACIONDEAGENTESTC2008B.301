// TC2008B Modelación de Sistemas Multiagentes con gráficas computacionales
// C# client to interact with Python server via POST
// Adaptado para FireRescue2

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace FireRescue.Networking
{
    /// <summary>
    /// Cliente para comunicarse con el servidor Python de Fire Rescue
    /// Compatible con el servidor HTTP del laboratorio
    /// </summary>
    public class APIClient : MonoBehaviour
    {
        [Header("Configuración del Servidor")]
        [Tooltip("URL del servidor Python")]
        public string serverURL = "http://localhost:8585";
        
        [Header("Estado")]
        public bool serverConnected = false;
        
        [Header("Debug")]
        public bool logResponses = true;
        
        /// <summary>
        /// Obtiene los datos de simulación completa del servidor via GET
        /// Endpoint: GET /simulation_data
        /// </summary>
        public IEnumerator ObtenerSimulacion(System.Action<string> onSuccess, System.Action<string> onError)
        {
            string url = $"{serverURL}/simulation_data";
            
            if (logResponses) Debug.Log($"📡 GET {url}");
            
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || 
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    string error = $"❌ Error: {www.error}";
                    Debug.LogError(error);
                    serverConnected = false;
                    onError?.Invoke(error);
                }
                else
                {
                    string text = www.downloadHandler.text;
                    
                    if (logResponses)
                    {
                        Debug.Log($"✅ Recibidos {www.downloadHandler.data.Length} bytes");
                        int previewLen = Mathf.Min(300, text.Length);
                        Debug.Log($"📄 JSON: {text.Substring(0, previewLen)}{(text.Length > previewLen ? "..." : "")}");
                    }
                    
                    serverConnected = true;
                    onSuccess?.Invoke(text);
                }
            }
        }
        
        /// <summary>
        /// Obtiene los datos via POST (compatible con laboratorio TC2008B)
        /// Endpoint: POST / (raíz del servidor)
        /// </summary>
        public IEnumerator ObtenerSimulacionPOST(System.Action<string> onSuccess, System.Action<string> onError)
        {
            string url = serverURL;
            
            if (logResponses) Debug.Log($"📡 POST {url}");
            
            WWWForm form = new WWWForm();
            
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
                
                yield return www.SendWebRequest();
                
                if (www.result == UnityWebRequest.Result.ConnectionError || 
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    string error = $"❌ Error: {www.error}";
                    Debug.LogError(error);
                    serverConnected = false;
                    onError?.Invoke(error);
                }
                else
                {
                    string text = www.downloadHandler.text;
                    
                    if (logResponses)
                    {
                        Debug.Log($"✅ Recibidos {www.downloadHandler.data.Length} bytes");
                        int previewLen = Mathf.Min(300, text.Length);
                        Debug.Log($"📄 JSON: {text.Substring(0, previewLen)}{(text.Length > previewLen ? "..." : "")}");
                    }
                    
                    serverConnected = true;
                    onSuccess?.Invoke(text);
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
