using UnityEngine;

/// <summary>
/// Componente para Puntos de Interés (víctimas/falsas alarmas)
/// </summary>
public class POI : MonoBehaviour
{
    [Header("Materiales")]
    [SerializeField] private Material materialOculto;
    [SerializeField] private Material materialVictima;
    [SerializeField] private Material materialFalsaAlarma;
    
    private Renderer rendererPOI;
    private string estadoActual = "oculto";
    
    private void Awake()
    {
        rendererPOI = GetComponent<Renderer>();
        if (rendererPOI == null)
        {
            rendererPOI = GetComponentInChildren<Renderer>();
        }
        
        // Inicializar con material oculto
        if (rendererPOI != null && materialOculto != null)
        {
            rendererPOI.material = materialOculto;
        }
    }
    
    /// <summary>
    /// Revela el contenido del punto de interés
    /// </summary>
    /// <param name="tipo">"victima" o "falsa_alarma"</param>
    public void Revelar(string tipo)
    {
        if (rendererPOI == null)
        {
            Debug.LogWarning($"⚠️ POI en {gameObject.name} no tiene Renderer");
            return;
        }
        
        switch (tipo.ToLower())
        {
            case "victima":
            case "víctima":
                // Cambiar a material de víctima
                if (materialVictima != null)
                {
                    rendererPOI.material = materialVictima;
                }
                else
                {
                    // Fallback: color verde
                    rendererPOI.material.color = Color.green;
                }
                estadoActual = "victima";
                Debug.Log($"🆘 POI {gameObject.name} → ¡Víctima encontrada!");
                break;
                
            case "falsa_alarma":
            case "falsa alarma":
                // Cambiar a material de falsa alarma
                if (materialFalsaAlarma != null)
                {
                    rendererPOI.material = materialFalsaAlarma;
                }
                else
                {
                    // Fallback: color negro
                    rendererPOI.material.color = Color.black;
                }
                estadoActual = "falsa_alarma";
                Debug.Log($"❌ POI {gameObject.name} → Falsa alarma");
                
                // Destruir después de la animación
                StartCoroutine(DestruirDespuesDePausa());
                break;
                
            default:
                Debug.LogWarning($"⚠️ Tipo de POI desconocido: {tipo}");
                break;
        }
    }
    
    /// <summary>
    /// Oculta el POI (vuelve al estado inicial)
    /// </summary>
    public void Ocultar()
    {
        if (rendererPOI != null && materialOculto != null)
        {
            rendererPOI.material = materialOculto;
            estadoActual = "oculto";
            Debug.Log($"🔒 POI {gameObject.name} → Oculto");
        }
    }
    
    /// <summary>
    /// Obtiene el estado actual del POI
    /// </summary>
    public string ObtenerEstado()
    {
        return estadoActual;
    }
    
    /// <summary>
    /// Destruye el POI después de una pequeña pausa (para falsas alarmas)
    /// </summary>
    private System.Collections.IEnumerator DestruirDespuesDePausa()
    {
        yield return new UnityEngine.WaitForSeconds(1.5f);
        
        Debug.Log($"🗑️ Eliminando falsa alarma: {gameObject.name}");
        Destroy(gameObject);
    }
}
