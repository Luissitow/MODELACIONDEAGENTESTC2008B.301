using UnityEngine;
using System.Collections;

/// <summary>
/// Componente para controlar arañas alienígenas
/// Maneja: animación de eliminación y muerte
/// </summary>
public class Spider : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float duracionMuerte = 1f;
    [SerializeField] private AnimationCurve curvaDesaparicion = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    private string estadoActual = "viva";
    private bool estaEliminada = false;
    
    /// <summary>
    /// Elimina la araña con animación de muerte
    /// </summary>
    public void Eliminar()
    {
        if (estaEliminada)
        {
            Debug.LogWarning($"⚠️ Spider {gameObject.name} ya fue eliminada");
            return;
        }
        
        estaEliminada = true;
        StartCoroutine(AnimarEliminacion());
    }
    
    /// <summary>
    /// Anima la eliminación de la araña (cambio de material + desvanecimiento)
    /// </summary>
    private IEnumerator AnimarEliminacion()
    {
        Debug.Log($"🕷️💀 Eliminando araña: {gameObject.name}");
        
        estadoActual = "muerta";
        
        // Animación de desvanecimiento (solo escala, sin cambio de material)
        float tiempoTranscurrido = 0f;
        Vector3 escalaOriginal = transform.localScale;
        
        while (tiempoTranscurrido < duracionMuerte)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracionMuerte;
            float factorEscala = curvaDesaparicion.Evaluate(progreso);
            
            // Reducir escala gradualmente
            transform.localScale = escalaOriginal * factorEscala;
            
            yield return null;
        }
        
        Debug.Log($"✅ Araña eliminada completamente: {gameObject.name}");
        
        // Destruir el GameObject
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Obtiene el estado actual de la araña
    /// </summary>
    public string ObtenerEstado()
    {
        return estadoActual;
    }
    
    /// <summary>
    /// Verifica si la araña ya fue eliminada
    /// </summary>
    public bool EstaEliminada()
    {
        return estaEliminada;
    }
}
