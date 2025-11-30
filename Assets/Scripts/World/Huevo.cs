using UnityEngine;

/// <summary>
/// Componente para el prefab de Huevo (peligro latente que eclosiona en araña)
/// Los huevos eclosionan después de 2 turnos y crean una araña
/// </summary>
public class Huevo : MonoBehaviour 
{
    [Header("Posición en Tablero")]
    public int fila;
    public int columna;
    
    [Header("Eclosión")]
    [Tooltip("Turnos antes de eclosionar (normalmente 2)")]
    public int turnosParaEclosionar = 2;
    private int turnosRestantes;
    
    [Header("Referencias")]
    [Tooltip("Prefab de araña que se creará al eclosionar")]
    public GameObject aranaPrefab;
    [Tooltip("Prefab de efecto de explosión")]
    public GameObject efectoExplosion;
    
    [Header("Efectos Visuales")]
    public ParticleSystem particulasPulso;
    public Material materialNormal;
    public Material materialAdvertencia; // Material que parpadea cuando está por eclosionar
    
    [Header("Audio")]
    public AudioClip sonidoAdvertencia;
    public AudioClip sonidoExplosion;
    
    private MeshRenderer meshRenderer;
    private AudioSource audioSource;
    private bool estaParpadeo = false;
    
    void Start() 
    {
        turnosRestantes = turnosParaEclosionar;
        meshRenderer = GetComponent<MeshRenderer>();
        
        // Auto-detectar partículas
        if (particulasPulso == null)
            particulasPulso = GetComponentInChildren<ParticleSystem>();
            
        // Configurar audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D sound
        
        name = $"Huevo_{fila}_{columna}";
        
        Debug.Log($"🥚 Huevo creado en ({fila},{columna}) - Eclosiona en {turnosParaEclosionar} turnos");
    }
    
    /// <summary>
    /// Reduce contador de turnos (llamado por SistemaHuevos.cs cada turno)
    /// </summary>
    public void ActualizarTurno()
    {
        turnosRestantes--;
        
        Debug.Log($"🥚 Huevo en ({fila},{columna}) - Turnos restantes: {turnosRestantes}");
        
        if (turnosRestantes <= 0)
        {
            Eclosionar();
        }
        else if (turnosRestantes == 1)
        {
            // Advertencia visual: el huevo está por eclosionar
            MostrarAdvertencia();
        }
        else
        {
            // Efecto visual de crecimiento
            CreceGradualmente();
        }
    }
    
    /// <summary>
    /// Efecto visual de advertencia cuando queda 1 turno
    /// </summary>
    void MostrarAdvertencia()
    {
        Debug.Log($"⚠️ Huevo en ({fila},{columna}) está por eclosionar!");
        
        // Sonido de advertencia
        if (sonidoAdvertencia != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoAdvertencia);
        }
        
        // Parpadeo rojo
        if (!estaParpadeo)
        {
            estaParpadeo = true;
            StartCoroutine(ParpadeoAdvertencia());
        }
        
        // Aumentar emisión de partículas
        if (particulasPulso != null)
        {
            var emission = particulasPulso.emission;
            emission.rateOverTime = 20f; // Más partículas
        }
    }
    
    /// <summary>
    /// Efecto de parpadeo rojo de advertencia
    /// </summary>
    System.Collections.IEnumerator ParpadeoAdvertencia()
    {
        while (turnosRestantes > 0 && turnosRestantes <= 1)
        {
            if (meshRenderer != null && materialAdvertencia != null)
            {
                meshRenderer.material = materialAdvertencia;
                yield return new WaitForSeconds(0.3f);
                
                if (materialNormal != null)
                {
                    meshRenderer.material = materialNormal;
                }
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                yield break;
            }
        }
    }
    
    /// <summary>
    /// Efecto de crecimiento gradual cada turno
    /// </summary>
    void CreceGradualmente()
    {
        // Aumentar tamaño ligeramente (10% por turno)
        float factorCrecimiento = 1.1f;
        transform.localScale *= factorCrecimiento;
        
        // Efecto visual de pulso
        StartCoroutine(EfectoPulso());
    }
    
    /// <summary>
    /// Efecto de pulso al crecer
    /// </summary>
    System.Collections.IEnumerator EfectoPulso()
    {
        Vector3 escalaOriginal = transform.localScale;
        float duracion = 0.5f;
        float tiempo = 0;
        
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;
            float escala = 1f + Mathf.Sin(progreso * Mathf.PI) * 0.2f;
            transform.localScale = escalaOriginal * escala;
            yield return null;
        }
        
        transform.localScale = escalaOriginal;
    }
    
    /// <summary>
    /// Eclosiona y crea una araña en su lugar
    /// </summary>
    void Eclosionar()
    {
        Debug.Log($"💥 Huevo en ({fila},{columna}) ECLOSIONANDO!");
        
        // Sonido de explosión
        if (sonidoExplosion != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoExplosion);
        }
        
        // Crear efecto de explosión
        if (efectoExplosion != null)
        {
            GameObject explosion = Instantiate(efectoExplosion, transform.position, Quaternion.identity);
            Destroy(explosion, 2f); // Destruir efecto después de 2 segundos
        }
        
        // Crear araña en la misma posición
        if (aranaPrefab != null)
        {
            Vector3 posicionArana = transform.position;
            GameObject nuevaArana = Instantiate(aranaPrefab, posicionArana, Quaternion.identity);
            
            // Configurar la araña
            Arana scriptArana = nuevaArana.GetComponent<Arana>();
            if (scriptArana != null)
            {
                scriptArana.fila = fila;
                scriptArana.columna = columna;
                Debug.Log($"🕷️ Araña creada desde huevo en ({fila},{columna})");
            }
            else
            {
                Debug.LogError($"❌ El prefab de araña no tiene el script Arana.cs");
            }
        }
        else
        {
            Debug.LogError($"❌ No hay aranaPrefab asignado en Huevo ({fila},{columna})");
        }
        
        // Destruir el huevo
        Destroy(gameObject, 0.5f); // Pequeño delay para que se escuche el sonido
    }
    
    /// <summary>
    /// Método público para verificar si está listo para eclosionar
    /// </summary>
    public bool EstaListoParaEclosionar()
    {
        return turnosRestantes <= 0;
    }
    
    /// <summary>
    /// Obtener turnos restantes
    /// </summary>
    public int ObtenerTurnosRestantes()
    {
        return turnosRestantes;
    }
    
    void OnDestroy()
    {
        // Cleanup
        StopAllCoroutines();
    }
}
