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
    [SerializeField] private Material materialDestruida; // Material vacío/transparente
    
    [Header("Configuración")]
    [SerializeField] private float duracionTransicion = 0.5f;
    
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
            // Destruir pared (cambiar a material vacío)
            CambiarEstado("destruida");
            return true;
        }
        else if (nivelDano == 1)
        {
            // Dañar visualmente (grietas)
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
                if (materialDestruida != null)
                {
                    StartCoroutine(AnimarDestruccion());
                }
                else
                {
                    // Si no hay material vacío, hacer transparente
                    if (rendererPared.material != null)
                    {
                        Color color = rendererPared.material.color;
                        color.a = 0f; // Completamente transparente
                        rendererPared.material.color = color;
                    }
                    Debug.Log($"💀 Pared {gameObject.name} → Destruida (invisible)");
                }
                break;
                
            default:
                Debug.LogWarning($"⚠️ Estado desconocido para pared: {estado}");
                break;
        }
    }
    
    /// <summary>
    /// Anima la transición a pared destruida (material vacío)
    /// </summary>
    private IEnumerator AnimarDestruccion()
    {
        Debug.Log($"💥🧱 Pared {gameObject.name} → ¡DESTRUIDA! (desvaneciendo)");
        
        Material materialOriginal = rendererPared.material;
        float tiempoTranscurrido = 0f;
        
        // Transición gradual al material vacío
        while (tiempoTranscurrido < duracionTransicion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracionTransicion;
            
            // Desvanecer el material actual solo si tiene la propiedad _Color
            if (materialOriginal != null && materialOriginal.HasProperty("_Color"))
            {
                Color color = materialOriginal.color;
                color.a = Mathf.Lerp(1f, 0f, progreso);
                materialOriginal.color = color;
            }
            
            yield return null;
        }
        
        // Cambiar al material vacío/destruido
        if (materialDestruida != null)
        {
            rendererPared.material = materialDestruida;
            Debug.Log($"✅ Pared {gameObject.name} → Material vacío aplicado");
        }
        else
        {
            Debug.LogWarning($"⚠️ Pared {gameObject.name} no tiene Material Destruida asignado en el Inspector");
        }
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
