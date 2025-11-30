using UnityEngine;

/// <summary>
/// Componente para el prefab de Punto de Interés (marcador "?" flotante)
/// Indica la posición de víctimas/falsas alarmas sin revelar
/// Visible desde vista aérea superior
/// </summary>
public class PuntoInteres : MonoBehaviour 
{
    [Header("Posición en Tablero")]
    public int fila;
    public int columna;
    
    [Header("Referencias")]
    [Tooltip("Víctima asociada a este punto de interés")]
    public GameObject victimaAsociada;
    
    [Header("Animación")]
    [Tooltip("Velocidad de rotación en grados por segundo")]
    public float velocidadRotacion = 30f;
    [Tooltip("Amplitud del movimiento de flotación")]
    public float amplitudFlotacion = 0.3f;
    [Tooltip("Velocidad del movimiento de flotación")]
    public float velocidadFlotacion = 2f;
    
    [Header("Efectos Visuales")]
    public ParticleSystem particulasBrillo;
    public Light luzIndicador;
    public float intensidadLuzBase = 1.5f;
    
    [Header("Audio")]
    public AudioClip sonidoPulso;
    
    private Vector3 posicionInicial;
    private AudioSource audioSource;
    private MeshRenderer meshRenderer;
    private float tiempoVida = 0f;
    
    void Start() 
    {
        posicionInicial = transform.position;
        meshRenderer = GetComponent<MeshRenderer>();
        
        // Auto-detectar partículas
        if (particulasBrillo == null)
        {
            particulasBrillo = GetComponentInChildren<ParticleSystem>();
        }
        
        // Configurar luz indicadora
        if (luzIndicador == null)
        {
            luzIndicador = gameObject.AddComponent<Light>();
            luzIndicador.type = LightType.Point;
            luzIndicador.color = Color.yellow;
            luzIndicador.intensity = intensidadLuzBase;
            luzIndicador.range = 5f;
            luzIndicador.shadows = LightShadows.None;
        }
        
        // Configurar audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.volume = 0.2f;
        
        // Pulso de sonido periódico
        if (sonidoPulso != null)
        {
            InvokeRepeating(nameof(EmitirPulso), 2f, 3f); // Cada 3 segundos
        }
        
        name = $"PuntoInteres_{fila}_{columna}";
        
        Debug.Log($"❓ Punto de interés creado en ({fila},{columna})");
    }
    
    void Update() 
    {
        tiempoVida += Time.deltaTime;
        
        // Rotación constante
        transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime, Space.World);
        
        // Flotación suave arriba y abajo
        float offset = Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        transform.position = posicionInicial + new Vector3(0, offset, 0);
        
        // Pulso de luz
        if (luzIndicador != null)
        {
            float pulso = Mathf.Sin(Time.time * 3f) * 0.3f + 1f;
            luzIndicador.intensity = intensidadLuzBase * pulso;
        }
        
        // Pulso de escala
        float escalaPulso = 1f + Mathf.Sin(Time.time * 2f) * 0.1f;
        transform.localScale = Vector3.one * escalaPulso;
    }
    
    /// <summary>
    /// Emite un pulso de sonido periódico
    /// </summary>
    void EmitirPulso()
    {
        if (sonidoPulso != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoPulso, 0.2f);
        }
        
        // Efecto visual de pulso
        if (particulasBrillo != null)
        {
            particulasBrillo.Emit(10);
        }
    }
    
    /// <summary>
    /// Revela el punto de interés (la víctima fue descubierta)
    /// Destruye el marcador con efecto visual
    /// </summary>
    public void Revelar()
    {
        Debug.Log($"✅ Punto de interés en ({fila},{columna}) revelado");
        
        // Efecto de revelación
        StartCoroutine(EfectoRevelacion());
    }
    
    /// <summary>
    /// Efecto visual de revelación antes de desaparecer
    /// </summary>
    System.Collections.IEnumerator EfectoRevelacion()
    {
        // Aumentar velocidad de rotación
        velocidadRotacion *= 3f;
        
        // Explosión de partículas
        if (particulasBrillo != null)
        {
            var emission = particulasBrillo.emission;
            emission.rateOverTime = 100f;
        }
        
        // Fade out de luz
        float duracion = 0.5f;
        float tiempo = 0;
        float intensidadInicial = luzIndicador != null ? luzIndicador.intensity : 0;
        
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;
            
            // Fade out de luz
            if (luzIndicador != null)
            {
                luzIndicador.intensity = Mathf.Lerp(intensidadInicial, 0, progreso);
            }
            
            // Fade out de material
            if (meshRenderer != null)
            {
                Color color = meshRenderer.material.color;
                color.a = Mathf.Lerp(1f, 0f, progreso);
                meshRenderer.material.color = color;
            }
            
            // Aumentar escala
            transform.localScale = Vector3.one * (1f + progreso * 0.5f);
            
            yield return null;
        }
        
        // Destruir el marcador
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Asocia una víctima a este punto de interés
    /// </summary>
    public void AsociarVictima(GameObject victima)
    {
        victimaAsociada = victima;
        
        // Obtener la posición de la víctima
        Victima scriptVictima = victima.GetComponent<Victima>();
        if (scriptVictima != null)
        {
            fila = scriptVictima.fila;
            columna = scriptVictima.columna;
        }
    }
    
    /// <summary>
    /// Verifica si la víctima asociada fue revelada
    /// </summary>
    void LateUpdate()
    {
        if (victimaAsociada != null)
        {
            Victima scriptVictima = victimaAsociada.GetComponent<Victima>();
            if (scriptVictima != null && scriptVictima.estaRevelada)
            {
                // La víctima fue revelada, destruir este marcador
                Revelar();
            }
        }
    }
    
    /// <summary>
    /// Cambiar color del marcador según urgencia
    /// </summary>
    public void CambiarColorUrgencia(Color nuevoColor)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = nuevoColor;
        }
        
        if (luzIndicador != null)
        {
            luzIndicador.color = nuevoColor;
        }
    }
    
    void OnDestroy()
    {
        // Cleanup
        CancelInvoke();
        StopAllCoroutines();
    }
}
