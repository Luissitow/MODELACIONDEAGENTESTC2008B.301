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
    
    private bool cargandoVictima = false;
    private Renderer rendererCrew;
    private Color colorOriginal;
    
    void Start()
    {
        // Obtener renderer del astronauta
        rendererCrew = GetComponent<Renderer>();
        if (rendererCrew == null)
        {
            rendererCrew = GetComponentInChildren<Renderer>();
        }
        
        // Guardar color original
        if (rendererCrew != null)
        {
            colorOriginal = rendererCrew.material.color;
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
        
        // Restaurar color original
        if (rendererCrew != null)
        {
            rendererCrew.material.color = colorOriginal;
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
}
