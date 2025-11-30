using UnityEngine;

/// <summary>
/// Asegura que solo haya UN AudioListener activo en la escena
/// Se ejecuta en Awake para desactivar listeners duplicados inmediatamente
/// </summary>
public class AudioListenerManager : MonoBehaviour
{
    void Awake()
    {
        // Buscar TODOS los AudioListeners en la escena
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        
        if (listeners.Length > 1)
        {
            Debug.LogWarning($"⚠️ Se encontraron {listeners.Length} AudioListeners en la escena. Solo debe haber 1.");
            
            // Determinar cuál mantener activo
            // Prioridad: Main Camera > otros
            AudioListener listenerPrincipal = null;
            
            foreach (AudioListener listener in listeners)
            {
                if (listener.gameObject.name.Contains("Main Camera") || 
                    listener.gameObject.name.Contains("MainCamera"))
                {
                    listenerPrincipal = listener;
                    break;
                }
            }
            
            // Si no hay Main Camera, usar el primero
            if (listenerPrincipal == null)
                listenerPrincipal = listeners[0];
            
            // Desactivar todos menos el principal
            foreach (AudioListener listener in listeners)
            {
                if (listener != listenerPrincipal)
                {
                    Debug.Log($"🔇 Desactivando AudioListener en: {listener.gameObject.name}");
                    listener.enabled = false;
                }
                else
                {
                    Debug.Log($"🔊 AudioListener activo en: {listener.gameObject.name}");
                }
            }
        }
        else if (listeners.Length == 1)
        {
            Debug.Log($"✅ AudioListener correcto: {listeners[0].gameObject.name}");
        }
        else
        {
            Debug.LogError("❌ NO se encontró ningún AudioListener en la escena. Debería haber 1.");
        }
    }
}
