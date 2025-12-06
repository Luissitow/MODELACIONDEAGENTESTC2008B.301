using UnityEngine;
using System.Collections;

/// <summary>
/// Componente para controlar huevos de araña
/// Maneja: aparición, evolución a araña, y eliminación
/// </summary>
public class Egg : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float duracionAparicion = 0.8f;
    [SerializeField] private float duracionEvolucion = 1.5f;
    [SerializeField] private float duracionEliminacion = 0.5f;
    [SerializeField] private AnimationCurve curvaAparicion = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private string estadoActual = "normal";
    private bool estaEliminado = false;
    
    private void Start()
    {
        // Animar aparición al instanciarse
        StartCoroutine(AnimarAparicion());
    }
    
    /// <summary>
    /// Anima la aparición del huevo desde el suelo
    /// </summary>
    private IEnumerator AnimarAparicion()
    {
        Vector3 escalaOriginal = transform.localScale;
        transform.localScale = Vector3.zero;
        
        float tiempoTranscurrido = 0f;
        
        while (tiempoTranscurrido < duracionAparicion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracionAparicion;
            float factorEscala = curvaAparicion.Evaluate(progreso);
            
            transform.localScale = escalaOriginal * factorEscala;
            
            yield return null;
        }
        
        transform.localScale = escalaOriginal;
        Debug.Log($"🥚 Huevo apareció: {gameObject.name}");
    }
    
    /// <summary>
    /// Evoluciona el huevo a una araña
    /// Retorna la posición donde debe instanciarse la araña
    /// </summary>
    public IEnumerator Evolucionar()
    {
        if (estaEliminado)
        {
            Debug.LogWarning($"⚠️ Huevo {gameObject.name} ya fue eliminado");
            yield break;
        }
        
        estaEliminado = true;
        estadoActual = "evolucionando";
        
        Debug.Log($"🥚➡️🕷️ Huevo evolucionando: {gameObject.name}");
        
        // Animación de pulsación/vibración (sin cambio de material)
        float tiempoTranscurrido = 0f;
        Vector3 escalaOriginal = transform.localScale;
        
        while (tiempoTranscurrido < duracionEvolucion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracionEvolucion;
            
            // Pulsación: crece y se encoge rápidamente
            float pulso = 1f + Mathf.Sin(progreso * Mathf.PI * 6) * 0.2f;
            transform.localScale = escalaOriginal * pulso;
            
            // Rotación errática
            transform.Rotate(Vector3.up, Time.deltaTime * 360f);
            
            yield return null;
        }
        
        Debug.Log($"✅ Huevo evolucionó completamente: {gameObject.name}");
        
        // El GameObject se destruye, la araña se instancia en TableroBuilder
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Elimina el huevo (cuando un jugador lo destruye)
    /// </summary>
    public void Eliminar()
    {
        if (estaEliminado)
        {
            Debug.LogWarning($"⚠️ Huevo {gameObject.name} ya fue eliminado");
            return;
        }
        
        estaEliminado = true;
        StartCoroutine(AnimarEliminacion());
    }
    
    /// <summary>
    /// Anima la eliminación del huevo (aplastado/destruido)
    /// </summary>
    private IEnumerator AnimarEliminacion()
    {
        Debug.Log($"🥚💥 Eliminando huevo: {gameObject.name}");
        
        estadoActual = "destruido";
        
        float tiempoTranscurrido = 0f;
        Vector3 escalaOriginal = transform.localScale;
        
        while (tiempoTranscurrido < duracionEliminacion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracionEliminacion;
            
            // Aplastar (reducir Y, expandir X y Z)
            float factorY = 1f - progreso;
            float factorXZ = 1f + progreso * 0.5f;
            
            transform.localScale = new Vector3(
                escalaOriginal.x * factorXZ,
                escalaOriginal.y * factorY,
                escalaOriginal.z * factorXZ
            );
            
            yield return null;
        }
        
        Debug.Log($"✅ Huevo eliminado completamente: {gameObject.name}");
        
        // Destruir el GameObject
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Obtiene el estado actual del huevo
    /// </summary>
    public string ObtenerEstado()
    {
        return estadoActual;
    }
    
    /// <summary>
    /// Verifica si el huevo ya fue eliminado o está evolucionando
    /// </summary>
    public bool EstaEliminado()
    {
        return estaEliminado;
    }
}
