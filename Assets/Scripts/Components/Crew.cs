using UnityEngine;

/// <summary>
/// Componente para tripulación que maneja el indicador visual de víctima cargada
/// </summary>
public class Crew : MonoBehaviour
{
    [Header("Indicador de Víctima")]
    [Tooltip("GameObject hijo que aparece cuando carga una víctima (puede ser un icono, sprite, o mesh)")]
    public GameObject indicadorVictima;
    
    [Header("Configuración Visual")]
    [Tooltip("Altura sobre el crew donde aparece el indicador (opcional)")]
    public float alturaIndicador = 1.5f;
    
    [Tooltip("Escala del indicador cuando está visible")]
    public float escalaIndicador = 0.5f;
    
    [Header("Knockdown Materials")]
    [Tooltip("Material ROJO para primer knockdown")]
    public Material materialKnockdown1;
    
    [Tooltip("Material NEGRO para segundo knockdown (muerte)")]
    public Material materialKnockdown2;
    
    private bool cargandoVictima = false;
    private Renderer rendererCrew;
    private Material materialOriginal;
    private int knockdownCount = 0;
    private Vector3 escalaOriginal;
    
    void Start()
    {
        // Obtener renderer del astronauta
        rendererCrew = GetComponent<Renderer>();
        if (rendererCrew == null)
        {
            rendererCrew = GetComponentInChildren<Renderer>();
        }
        
        // Guardar material y escala original
        escalaOriginal = transform.localScale;
        if (rendererCrew != null)
        {
            materialOriginal = rendererCrew.material;
        }
        
        // Cargar materiales knockdown si no están asignados
        if (materialKnockdown1 == null)
        {
            materialKnockdown1 = Resources.Load<Material>("Materials/Crew_Knockdown1");
            if (materialKnockdown1 == null)
            {
                Debug.LogWarning($"⚠️ No se encontró material Crew_Knockdown1 en Resources");
            }
        }
        
        if (materialKnockdown2 == null)
        {
            materialKnockdown2 = Resources.Load<Material>("Materials/Crew_Knockdown2");
            if (materialKnockdown2 == null)
            {
                Debug.LogWarning($"⚠️ No se encontró material Crew_Knockdown2 en Resources");
            }
        }
        
        // Si no hay indicador asignado, intentar crear uno simple
        if (indicadorVictima == null)
        {
            CrearIndicadorPorDefecto();
        }
        
        // Ocultar indicador al inicio
        if (indicadorVictima != null)
        {
            indicadorVictima.SetActive(false);
        }
    }
    
    /// <summary>
    /// Crea un indicador visual simple si no hay uno asignado
    /// </summary>
    private void CrearIndicadorPorDefecto()
    {
        // Crear esfera pequeña como indicador
        GameObject indicador = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicador.name = "IndicadorVictima";
        indicador.transform.SetParent(transform);
        indicador.transform.localPosition = new Vector3(0, alturaIndicador, 0);
        indicador.transform.localScale = Vector3.one * escalaIndicador;
        
        // Color distintivo (rojo para víctima)
        Renderer renderer = indicador.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
        
        // Eliminar collider para que no interfiera
        Collider col = indicador.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }
        
        indicadorVictima = indicador;
    }
    
    /// <summary>
    /// Muestra el indicador cuando el crew recoge una víctima
    /// </summary>
    public void CargarVictima()
    {
        cargandoVictima = true;
        
        // Cambiar astronauta a VERDE
        if (rendererCrew != null)
        {
            rendererCrew.material.color = Color.green;
        }
        
        if (indicadorVictima != null)
        {
            indicadorVictima.SetActive(true);
            StartCoroutine(AnimarCargarVictima());
        }
        
        Debug.Log($"🚀 {gameObject.name} ahora CARGA una víctima (cambiando a VERDE)");
    }
    
    /// <summary>
    /// Oculta el indicador cuando el crew deposita la víctima
    /// </summary>
    public void DepositarVictima()
    {
        cargandoVictima = false;
        
        // Restaurar color (pero mantener color de knockdown si aplica) - crear instancia de material
        if (rendererCrew != null)
        {
            if (knockdownCount == 0)
            {
                // Restaurar material original si no tiene knockdowns
                rendererCrew.material = materialOriginal;
            }
            else if (knockdownCount == 1)
            {
                // Mantener material rojo si tiene 1 knockdown
                if (materialKnockdown1 != null)
                    rendererCrew.material = materialKnockdown1;
            }
            else
            {
                // Mantener material negro si está muerto
                if (materialKnockdown2 != null)
                    rendererCrew.material = materialKnockdown2;
            }
        }
        
        if (indicadorVictima != null)
        {
            StartCoroutine(AnimarDepositarVictima());
        }
        
        Debug.Log($"🚀 {gameObject.name} depositó la víctima (restaurando color)");
    }
    
    /// <summary>
    /// Animación al cargar víctima: aparece con escalado
    /// </summary>
    private System.Collections.IEnumerator AnimarCargarVictima()
    {
        if (indicadorVictima == null) yield break;
        
        // Animar aparición desde escala 0
        float duracion = 0.3f;
        float tiempo = 0;
        Vector3 escalaFinal = Vector3.one * escalaIndicador;
        
        while (tiempo < duracion)
        {
            indicadorVictima.transform.localScale = Vector3.Lerp(Vector3.zero, escalaFinal, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        indicadorVictima.transform.localScale = escalaFinal;
    }
    
    /// <summary>
    /// Animación al depositar víctima: desaparece con escalado
    /// </summary>
    private System.Collections.IEnumerator AnimarDepositarVictima()
    {
        if (indicadorVictima == null) yield break;
        
        // Animar desaparición hacia escala 0
        float duracion = 0.3f;
        float tiempo = 0;
        Vector3 escalaInicial = indicadorVictima.transform.localScale;
        
        while (tiempo < duracion)
        {
            indicadorVictima.transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        indicadorVictima.SetActive(false);
        indicadorVictima.transform.localScale = Vector3.one * escalaIndicador;
    }
    
    /// <summary>
    /// Obtiene el estado actual de carga
    /// </summary>
    public bool EstaCargandoVictima()
    {
        return cargandoVictima;
    }
    
    /// <summary>
    /// Aplica un knockdown al tripulante (cambio visual)
    /// </summary>
    public void AplicarKnockdown()
    {
        knockdownCount++;
        
        if (knockdownCount == 1)
        {
            // Primer knockdown - Material ROJO + 80% tamaño
            if (rendererCrew != null && materialKnockdown1 != null)
            {
                rendererCrew.material = materialKnockdown1;
                Debug.Log($"🎨 {gameObject.name} → Material ROJO aplicado");
            }
            else if (rendererCrew != null)
            {
                // Fallback: usar color directo si no hay material
                Material matInstancia = new Material(rendererCrew.material);
                matInstancia.color = Color.red;
                rendererCrew.material = matInstancia;
                Debug.Log($"🎨 {gameObject.name} → Color ROJO aplicado (fallback)");
            }
            else
            {
                Debug.LogWarning($"⚠️ {gameObject.name} no tiene Renderer para aplicar color");
            }
            transform.localScale = escalaOriginal * 0.8f;
            Debug.Log($"⚠️ {gameObject.name} recibe PRIMER KNOCKDOWN (1/2) - Color ROJO, escala 80%");
        }
        else if (knockdownCount >= 2)
        {
            // Segundo knockdown - Material NEGRO + muy pequeño
            if (rendererCrew != null && materialKnockdown2 != null)
            {
                rendererCrew.material = materialKnockdown2;
                Debug.Log($"🎨 {gameObject.name} → Material NEGRO aplicado");
            }
            else if (rendererCrew != null)
            {
                // Fallback: usar color directo si no hay material
                Material matInstancia = new Material(rendererCrew.material);
                matInstancia.color = Color.black;
                rendererCrew.material = matInstancia;
                Debug.Log($"🎨 {gameObject.name} → Color NEGRO aplicado (fallback)");
            }
            else
            {
                Debug.LogWarning($"⚠️ {gameObject.name} no tiene Renderer para aplicar color");
            }
            transform.localScale = escalaOriginal * 0.5f;
            Debug.Log($"💀 {gameObject.name} ELIMINADO (2/2 knockdowns) - Color NEGRO, escala 50%");
        }
    }
    
    /// <summary>
    /// Verifica si el tripulante está muerto (2+ knockdowns)
    /// </summary>
    public bool EstaMuerto()
    {
        return knockdownCount >= 2;
    }
    
    /// <summary>
    /// Obtiene el contador de knockdowns actual
    /// </summary>
    public int GetKnockdownCount()
    {
        return knockdownCount;
    }
}
