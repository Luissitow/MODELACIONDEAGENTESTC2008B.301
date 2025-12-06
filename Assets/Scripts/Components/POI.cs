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
        Debug.Log($"🔧 POI.Awake() en {gameObject.name}");
        
        rendererPOI = GetComponent<Renderer>();
        if (rendererPOI == null)
        {
            Debug.Log($"   No hay Renderer en objeto raíz, buscando en hijos...");
            rendererPOI = GetComponentInChildren<Renderer>();
        }
        
        if (rendererPOI == null)
        {
            Debug.LogError($"❌ POI {gameObject.name}: NO SE ENCONTRÓ RENDERER en el objeto ni en sus hijos!");
            Debug.LogError($"   El prefab necesita tener un objeto hijo con MeshRenderer o SpriteRenderer");
            return;
        }
        
        Debug.Log($"✅ Renderer encontrado en: {rendererPOI.gameObject.name}");
        Debug.Log($"   Tipo: {rendererPOI.GetType().Name}");
        
        // Inicializar con material oculto
        if (materialOculto != null)
        {
            rendererPOI.material = materialOculto;
            Debug.Log($"✅ Material oculto '{materialOculto.name}' aplicado");
        }
        else
        {
            Debug.LogWarning($"⚠️ Material oculto es NULL");
        }
    }
    
    /// <summary>
    /// Revela el contenido del punto de interés
    /// </summary>
    /// <param name="tipo">"victima" o "falsa_alarma"</param>
    public void Revelar(string tipo)
    {
        Debug.Log($"🔍 Revelar llamado en {gameObject.name} con tipo: '{tipo}'");
        
        if (rendererPOI == null)
        {
            Debug.LogWarning($"⚠️ POI en {gameObject.name} no tiene Renderer");
            return;
        }
        
        Debug.Log($"🎨 Renderer encontrado. Material actual: {rendererPOI.material?.name ?? "NULL"}");
        Debug.Log($"🎨 Material víctima disponible: {materialVictima != null}");
        Debug.Log($"🎨 Material falsa alarma disponible: {materialFalsaAlarma != null}");
        
        switch (tipo.ToLower())
        {
            case "victima":
            case "víctima":
                // Cambiar a material de víctima
                if (materialVictima != null)
                {
                    // Crear una nueva instancia del material para este objeto
                    Material nuevoMaterial = new Material(materialVictima);
                    rendererPOI.material = nuevoMaterial;
                    Debug.Log($"✅ POI {gameObject.name} → Material víctima aplicado: {materialVictima.name}");
                    Debug.Log($"   Material actual del renderer: {rendererPOI.material.name}");
                }
                else
                {
                    // Fallback: crear material con color verde
                    Material materialFallback = new Material(Shader.Find("Standard"));
                    materialFallback.color = Color.green;
                    rendererPOI.material = materialFallback;
                    Debug.LogWarning($"⚠️ Material víctima NULL, usando material verde como fallback");
                }
                estadoActual = "victima";
                Debug.Log($"🆘 POI {gameObject.name} → ¡Víctima encontrada!");
                break;
                
            case "falsa_alarma":
            case "falsa alarma":
                // Cambiar a material de falsa alarma
                if (materialFalsaAlarma != null)
                {
                    // Crear una nueva instancia del material para este objeto
                    Material nuevoMaterial = new Material(materialFalsaAlarma);
                    rendererPOI.material = nuevoMaterial;
                    Debug.Log($"✅ POI {gameObject.name} → Material falsa alarma aplicado: {materialFalsaAlarma.name}");
                    Debug.Log($"   Material actual del renderer: {rendererPOI.material.name}");
                }
                else
                {
                    // Fallback: crear material con color negro
                    Material materialFallback = new Material(Shader.Find("Standard"));
                    materialFallback.color = Color.black;
                    rendererPOI.material = materialFallback;
                    Debug.LogWarning($"⚠️ Material falsa alarma NULL, usando material negro como fallback");
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
