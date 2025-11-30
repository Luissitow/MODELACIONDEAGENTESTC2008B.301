using UnityEngine;

/// <summary>
/// Sistema de shake de cámara para efectos de impacto
/// Agregar a Main Camera
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    
    private Vector3 posicionOriginal;
    private bool estaSacudiendo = false;
    
    void Awake()
    {
        // Singleton para acceso global
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        posicionOriginal = transform.localPosition;
    }
    
    /// <summary>
    /// Sacude la cámara con magnitud y duración específicas
    /// </summary>
    /// <param name="duracion">Duración en segundos</param>
    /// <param name="magnitud">Intensidad del shake (0.1 = sutil, 0.5 = fuerte)</param>
    public void Shake(float duracion, float magnitud)
    {
        if (!estaSacudiendo)
        {
            StartCoroutine(ShakeCoroutine(duracion, magnitud));
        }
    }
    
    /// <summary>
    /// Sacude la cámara con configuración predefinida: Leve
    /// </summary>
    public void ShakeLeve()
    {
        Shake(0.2f, 0.05f);
    }
    
    /// <summary>
    /// Sacude la cámara con configuración predefinida: Medio
    /// </summary>
    public void ShakeMedio()
    {
        Shake(0.3f, 0.15f);
    }
    
    /// <summary>
    /// Sacude la cámara con configuración predefinida: Fuerte
    /// </summary>
    public void ShakeFuerte()
    {
        Shake(0.5f, 0.3f);
    }
    
    System.Collections.IEnumerator ShakeCoroutine(float duracion, float magnitud)
    {
        estaSacudiendo = true;
        float tiempoTranscurrido = 0f;
        
        while (tiempoTranscurrido < duracion)
        {
            // Generar offset random en X y Y
            float offsetX = Random.Range(-1f, 1f) * magnitud;
            float offsetY = Random.Range(-1f, 1f) * magnitud;
            
            // Aplicar offset
            transform.localPosition = posicionOriginal + new Vector3(offsetX, offsetY, 0);
            
            tiempoTranscurrido += Time.deltaTime;
            
            // Reducir magnitud progresivamente (fade out)
            magnitud = Mathf.Lerp(magnitud, 0, tiempoTranscurrido / duracion);
            
            yield return null;
        }
        
        // Restaurar posición original
        transform.localPosition = posicionOriginal;
        estaSacudiendo = false;
    }
    
    /// <summary>
    /// Detiene el shake inmediatamente
    /// </summary>
    public void DetenerShake()
    {
        StopAllCoroutines();
        transform.localPosition = posicionOriginal;
        estaSacudiendo = false;
    }
}
