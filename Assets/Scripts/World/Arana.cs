using UnityEngine;

/// <summary>
/// Componente para el prefab de Araña (representa fuego en el juego)
/// Las arañas se propagan a celdas adyacentes cada turno
/// </summary>
public class Arana : MonoBehaviour 
{
    [Header("Posición en Tablero")]
    public int fila;
    public int columna;
    
    [Header("Propagación")]
    [Tooltip("Turnos que deben pasar antes de propagarse (normalmente 1)")]
    public int turnosParaPropagacion = 1;
    private int turnosRestantes;
    
    [Header("Efectos Visuales")]
    public ParticleSystem particulasFuego;
    public float intensidadLuz = 2f;
    public Color colorLuz = new Color(1f, 0.4f, 0f); // Naranja fuego
    
    [Header("Audio")]
    public AudioClip sonidoFuego;
    
    private Light luzFuego;
    private AudioSource audioSource;
    
    void Start() 
    {
        turnosRestantes = turnosParaPropagacion;
        
        // Auto-detectar partículas
        if (particulasFuego == null)
            particulasFuego = GetComponentInChildren<ParticleSystem>();
            
        // Agregar luz dinámica para efecto de fuego
        luzFuego = gameObject.AddComponent<Light>();
        luzFuego.type = LightType.Point;
        luzFuego.color = colorLuz;
        luzFuego.intensity = intensidadLuz;
        luzFuego.range = 4f;
        luzFuego.shadows = LightShadows.None; // Optimización
        
        // Efecto de parpadeo de luz (simulando fuego)
        StartCoroutine(ParpadeoLuz());
        
        // Audio ambiente de fuego
        if (sonidoFuego != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = sonidoFuego;
            audioSource.loop = true;
            audioSource.volume = 0.3f;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.Play();
        }
        
        name = $"Araña_{fila}_{columna}";
        
        Debug.Log($"🕷️ Araña creada en ({fila},{columna})");
    }
    
    /// <summary>
    /// Efecto de parpadeo de luz para simular fuego realista
    /// </summary>
    System.Collections.IEnumerator ParpadeoLuz()
    {
        while (true)
        {
            if (luzFuego != null)
            {
                luzFuego.intensity = intensidadLuz + Random.Range(-0.3f, 0.3f);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    /// <summary>
    /// Actualiza el contador de turnos (llamado por sistema de propagación)
    /// </summary>
    public void ActualizarTurno()
    {
        turnosRestantes--;
        
        if (turnosRestantes <= 0)
        {
            // Marcar como lista para propagarse
            turnosRestantes = turnosParaPropagacion; // Reset para siguiente propagación
        }
    }
    
    /// <summary>
    /// Propaga fuego a celdas adyacentes (llamado por PropagacionFuego.cs)
    /// Retorna true si puede propagarse
    /// </summary>
    public bool PuedePropagarse()
    {
        return turnosRestantes <= 0;
    }
    
    /// <summary>
    /// Efecto visual de propagación
    /// </summary>
    public void MostrarEfectoPropagacion()
    {
        // Aumentar tamaño temporalmente
        StartCoroutine(EfectoPulso());
        
        // Aumentar intensidad de luz
        if (luzFuego != null)
        {
            luzFuego.intensity = intensidadLuz * 1.5f;
        }
    }
    
    /// <summary>
    /// Efecto de pulso al propagarse
    /// </summary>
    System.Collections.IEnumerator EfectoPulso()
    {
        Vector3 escalaOriginal = transform.localScale;
        float duracion = 0.3f;
        float tiempo = 0;
        
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;
            float escala = 1f + Mathf.Sin(progreso * Mathf.PI) * 0.3f;
            transform.localScale = escalaOriginal * escala;
            yield return null;
        }
        
        transform.localScale = escalaOriginal;
    }
    
    /// <summary>
    /// Destruye la araña con efecto visual
    /// </summary>
    public void Extinguir()
    {
        Debug.Log($"💧 Araña en ({fila},{columna}) extinguida");
        
        // Efecto de extinción (humo, vapor)
        if (particulasFuego != null)
        {
            var emission = particulasFuego.emission;
            emission.enabled = false;
        }
        
        // Fade out de luz
        if (luzFuego != null)
        {
            StartCoroutine(FadeOutLuz());
        }
        
        Destroy(gameObject, 1f); // Destruir después de 1 segundo
    }
    
    /// <summary>
    /// Fade out de la luz al extinguirse
    /// </summary>
    System.Collections.IEnumerator FadeOutLuz()
    {
        float duracion = 0.5f;
        float tiempo = 0;
        float intensidadInicial = luzFuego.intensity;
        
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            luzFuego.intensity = Mathf.Lerp(intensidadInicial, 0, tiempo / duracion);
            yield return null;
        }
    }
    
    void OnDestroy()
    {
        // Cleanup
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
