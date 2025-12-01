using UnityEngine;
using System.Collections;

/// <summary>
/// Componente para controlar el estado visual de una pared con sistema de daño acumulativo
/// 0 daño = normal, 1 daño = dañada, 2+ daños = destruida
/// </summary>
public class Wall : MonoBehaviour
{
    [Header("Materiales (Opcional)")]
    [SerializeField] private Material materialNormal;
    [SerializeField] private Material materialDanada;
    
    [Header("Configuración")]
    [SerializeField] private float duracionDestruccion = 1f;
    
    private Renderer rendererPared;
    private int nivelDano = 0;
    private string estadoActual = "normal";
    
    private void Awake()
    {
        rendererPared = GetComponent<Renderer>();
        if (rendererPared == null)
        {
            rendererPared = GetComponentInChildren<Renderer>();
        }
    }
    
    /// <summary>
    /// Aplica daño a la pared. Retorna true si la pared fue destruida
    /// </summary>
    public bool AplicarDano(int cantidad = 1)
    {
        nivelDano += cantidad;
        
        Debug.Log($"🧱 Pared {gameObject.name} recibe {cantidad} daño (total: {nivelDano})");
        
        if (nivelDano >= 2)
        {
            // Destruir pared
            StartCoroutine(AnimarDestruccion());
            return true;
        }
        else if (nivelDano == 1)
        {
            // Dañar visualmente
            CambiarEstado("dañada");
            return false;
        }
        
        return false;
    }
    
    /// <summary>
    /// Cambia el estado visual de la pared
    /// </summary>
    /// <param name="estado">"normal", "dañada" o "destruida"</param>
    public void CambiarEstado(string estado)
    {
        estadoActual = estado.ToLower();
        
        if (rendererPared == null)
        {
            Debug.LogWarning($"⚠️ Wall en {gameObject.name} no tiene Renderer");
            return;
        }
        
        switch (estadoActual)
        {
            case "normal":
                if (materialNormal != null)
                {
                    rendererPared.material = materialNormal;
                }
                nivelDano = 0;
                Debug.Log($"🧱 Pared {gameObject.name} → Normal");
                break;
                
            case "dañada":
            case "danada":
                if (materialDanada != null)
                {
                    rendererPared.material = materialDanada;
                }
                else
                {
                    // Si no hay material, oscurecer el actual
                    if (rendererPared.material != null)
                    {
                        Color color = rendererPared.material.color;
                        color *= 0.7f; // Oscurecer 30%
                        rendererPared.material.color = color;
                    }
                }
                Debug.Log($"💥 Pared {gameObject.name} → Dañada (grietas)");
                break;
                
            case "destruida":
                StartCoroutine(AnimarDestruccion());
                break;
                
            default:
                Debug.LogWarning($"⚠️ Estado desconocido para pared: {estado}");
                break;
        }
    }
    
    /// <summary>
    /// Anima el colapso y destrucción de la pared
    /// </summary>
    private IEnumerator AnimarDestruccion()
    {
        estadoActual = "destruida";
        Debug.Log($"💥🧱 Pared {gameObject.name} → ¡DESTRUIDA! (colapsando)");
        
        float tiempoTranscurrido = 0f;
        Vector3 escalaOriginal = transform.localScale;
        Vector3 posicionOriginal = transform.position;
        
        while (tiempoTranscurrido < duracionDestruccion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracionDestruccion;
            
            // Caer hacia abajo
            transform.position = posicionOriginal + Vector3.down * progreso * 2f;
            
            // Encogerse
            transform.localScale = Vector3.Lerp(escalaOriginal, Vector3.zero, progreso);
            
            // Rotar aleatoriamente
            transform.Rotate(Vector3.forward, Time.deltaTime * 360f);
            
            yield return null;
        }
        
        Debug.Log($"✅ Pared destruida completamente: {gameObject.name}");
        
        // Destruir el GameObject
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Obtiene el nivel de daño actual (0 = normal, 1 = dañada, 2+ = destruida)
    /// </summary>
    public int ObtenerNivelDano()
    {
        return nivelDano;
    }
    
    /// <summary>
    /// Obtiene el estado actual de la pared
    /// </summary>
    public string ObtenerEstado()
    {
        return estadoActual;
    }
}
