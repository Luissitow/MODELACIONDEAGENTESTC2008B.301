using UnityEngine;

/// <summary>
/// Componente para prefabs de Tripulación y Falsa Alarma
/// Maneja la revelación, diferenciación y rescate de víctimas
/// </summary>
public class Victima : MonoBehaviour 
{
    [Header("Posición en Tablero")]
    public int fila;
    public int columna;
    
    [Header("Tipo")]
    [Tooltip("true = víctima real rescatable, false = falsa alarma")]
    public bool esVictima = true;
    public bool estaRevelada = false;
    public bool estaRescatada = false;
    
    [Header("Materiales Visuales")]
    [Tooltip("Material antes de ser revelada (marcador '?')")]
    public Material materialOculto;
    [Tooltip("Material cuando es víctima real (humano vivo - verde/azul)")]
    public Material materialVictima;
    [Tooltip("Material cuando es falsa alarma (objeto inanimado - gris)")]
    public Material materialFalsaAlarma;
    
    [Header("Efectos")]
    public ParticleSystem efectoRevelacion;
    public GameObject indicadorSalvable; // Icono flotante que indica "rescatable"
    
    [Header("Audio")]
    public AudioClip sonidoRevelacion;
    public AudioClip sonidoRescate;
    public AudioClip sonidoFalsaAlarma;
    
    private MeshRenderer meshRenderer;
    private AudioSource audioSource;
    private GameObject astronautaTransportador;
    
    void Start() 
    {
        meshRenderer = GetComponent<MeshRenderer>();
        
        // Configurar audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D sound
        
        // Estado inicial: oculta con "?"
        if (!estaRevelada && materialOculto != null && meshRenderer != null)
        {
            meshRenderer.material = materialOculto;
        }
        
        // Ocultar indicador hasta que se revele
        if (indicadorSalvable != null)
        {
            indicadorSalvable.SetActive(false);
        }
        
        name = esVictima ? $"Victima_{fila}_{columna}" : $"FalsaAlarma_{fila}_{columna}";
        
        Debug.Log($"👤 {(esVictima ? "Víctima" : "Falsa Alarma")} creada en ({fila},{columna}) - Oculta: {!estaRevelada}");
    }
    
    /// <summary>
    /// Revela si es víctima real o falsa alarma
    /// Llamado cuando un astronauta llega a la celda
    /// </summary>
    public void Revelar()
    {
        if (estaRevelada)
        {
            Debug.LogWarning($"⚠️ La víctima en ({fila},{columna}) ya estaba revelada");
            return;
        }
        
        estaRevelada = true;
        
        // Efecto de revelación
        if (efectoRevelacion != null)
        {
            Instantiate(efectoRevelacion, transform.position, Quaternion.identity);
        }
        
        if (esVictima)
        {
            // ES VÍCTIMA REAL
            if (meshRenderer != null && materialVictima != null)
            {
                meshRenderer.material = materialVictima;
            }
            
            // Mostrar indicador de salvable
            if (indicadorSalvable != null)
            {
                indicadorSalvable.SetActive(true);
            }
            
            // Sonido de víctima encontrada
            if (sonidoRescate != null && audioSource != null)
            {
                audioSource.PlayOneShot(sonidoRescate);
            }
            
            Debug.Log($"✅ VÍCTIMA REAL revelada en ({fila},{columna})");
        }
        else
        {
            // ES FALSA ALARMA
            if (meshRenderer != null && materialFalsaAlarma != null)
            {
                meshRenderer.material = materialFalsaAlarma;
            }
            
            // Sonido de decepción
            if (sonidoFalsaAlarma != null && audioSource != null)
            {
                audioSource.PlayOneShot(sonidoFalsaAlarma);
            }
            
            Debug.Log($"❌ FALSA ALARMA revelada en ({fila},{columna})");
            
            // Las falsas alarmas se pueden destruir después de revelarse
            Destroy(gameObject, 3f);
        }
    }
    
    /// <summary>
    /// Astronauta recoge la víctima para transportarla
    /// </summary>
    /// <param name="astronauta">GameObject del astronauta que recoge</param>
    public void SerRecogida(GameObject astronauta)
    {
        if (!esVictima)
        {
            Debug.LogWarning($"⚠️ No se puede recoger falsa alarma en ({fila},{columna})");
            return;
        }
        
        if (!estaRevelada)
        {
            Debug.LogWarning($"⚠️ No se puede recoger víctima en ({fila},{columna}) - No está revelada");
            // Auto-revelar al intentar recoger
            Revelar();
        }
        
        if (estaRescatada)
        {
            Debug.LogWarning($"⚠️ Víctima en ({fila},{columna}) ya está siendo transportada");
            return;
        }
        
        estaRescatada = true;
        astronautaTransportador = astronauta;
        
        // Parenting: hacerse hijo del astronauta
        transform.SetParent(astronauta.transform);
        transform.localPosition = new Vector3(0, 1.5f, 0); // Encima del astronauta
        transform.localRotation = Quaternion.identity;
        
        // Desactivar collider para que no interfiera
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Ocultar indicador
        if (indicadorSalvable != null)
        {
            indicadorSalvable.SetActive(false);
        }
        
        // Efecto visual de ser recogida
        StartCoroutine(EfectoRecogida());
        
        Debug.Log($"🚀 Víctima en ({fila},{columna}) recogida por astronauta {astronauta.name}");
    }
    
    /// <summary>
    /// Efecto de elevación al ser recogida
    /// </summary>
    System.Collections.IEnumerator EfectoRecogida()
    {
        Vector3 posicionInicial = transform.localPosition;
        Vector3 posicionFinal = new Vector3(0, 1.5f, 0);
        float duracion = 0.5f;
        float tiempo = 0;
        
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;
            transform.localPosition = Vector3.Lerp(posicionInicial, posicionFinal, progreso);
            yield return null;
        }
        
        transform.localPosition = posicionFinal;
    }
    
    /// <summary>
    /// Astronauta suelta la víctima (en punto de salida o por daño)
    /// </summary>
    /// <param name="posicion">Posición donde soltar (opcional)</param>
    public void SerSoltada(Vector3? posicion = null)
    {
        if (!estaRescatada)
        {
            Debug.LogWarning($"⚠️ La víctima en ({fila},{columna}) no estaba siendo transportada");
            return;
        }
        
        estaRescatada = false;
        
        // Deshacer parenting
        transform.SetParent(null);
        
        if (posicion.HasValue)
        {
            transform.position = posicion.Value;
        }
        
        // Reactivar collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
        
        // Mostrar indicador nuevamente
        if (indicadorSalvable != null && esVictima)
        {
            indicadorSalvable.SetActive(true);
        }
        
        astronautaTransportador = null;
        
        Debug.Log($"📍 Víctima soltada en nueva posición");
    }
    
    /// <summary>
    /// Víctima es rescatada exitosamente (llegó a punto de salida)
    /// </summary>
    public void RescateExitoso()
    {
        if (!esVictima)
        {
            Debug.LogWarning($"⚠️ No se puede rescatar falsa alarma");
            return;
        }
        
        Debug.Log($"🎉 VÍCTIMA RESCATADA EXITOSAMENTE desde ({fila},{columna})!");
        
        // Efecto de celebración
        if (efectoRevelacion != null)
        {
            ParticleSystem efecto = Instantiate(efectoRevelacion, transform.position, Quaternion.identity);
            Destroy(efecto.gameObject, 2f);
        }
        
        // Sonido de rescate exitoso
        if (sonidoRescate != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoRescate);
        }
        
        // Notificar al GameManager (incrementar contador de víctimas salvadas)
        // TODO: Implementar en Fase 7
        
        // Destruir víctima después de rescate
        Destroy(gameObject, 1f);
    }
    
    /// <summary>
    /// Víctima muere (por fuego, explosión, etc.)
    /// </summary>
    public void Morir()
    {
        if (!esVictima)
        {
            return; // Falsas alarmas no pueden morir
        }
        
        Debug.Log($"💀 Víctima en ({fila},{columna}) ha MUERTO");
        
        // Cambiar material a "muerto" (gris oscuro)
        if (meshRenderer != null && materialFalsaAlarma != null)
        {
            meshRenderer.material = materialFalsaAlarma;
        }
        
        // Si estaba siendo transportada, notificar al astronauta
        if (astronautaTransportador != null)
        {
            AstronautController astronauta = astronautaTransportador.GetComponent<AstronautController>();
            if (astronauta != null)
            {
                astronauta.SoltarVictima();
            }
        }
        
        // Notificar al GameManager (incrementar contador de víctimas perdidas)
        // TODO: Implementar en Fase 7
        
        // Destruir después de un momento
        Destroy(gameObject, 2f);
    }
    
    /// <summary>
    /// Verifica si puede ser recogida
    /// </summary>
    public bool PuedeSerRecogida()
    {
        return esVictima && estaRevelada && !estaRescatada;
    }
    
    /// <summary>
    /// Obtiene el astronauta que la está transportando
    /// </summary>
    public GameObject ObtenerAstronautaTransportador()
    {
        return astronautaTransportador;
    }
}
