using UnityEngine;

/// <summary>
/// Sistema de efectos visuales para paredes
/// Maneja partículas, luces y otros efectos
/// </summary>
public class WallEffects : MonoBehaviour
{
    [Header("Prefabs de Efectos")]
    [SerializeField] private GameObject efectoPolvo;
    [SerializeField] private GameObject efectoEscombros;
    [SerializeField] private GameObject efectoChispas;
    
    [Header("Luces")]
    [SerializeField] private Light luzImpacto;
    [SerializeField] private Color colorLuzDano = new Color(1f, 0.6f, 0f); // Naranja
    [SerializeField] private Color colorLuzDestruccion = new Color(1f, 0.2f, 0f); // Rojo
    
    [Header("Configuración")]
    [SerializeField] private float duracionFlashLuz = 0.1f;
    [SerializeField] private float intensidadLuz = 3f;
    [SerializeField] private bool usarShakeCamera = true;
    
    void Start()
    {
        // NO CREAR LUZ AUTOMÁTICAMENTE - El usuario no quiere "luz impacto" en escena
        // La luz debe ser asignada manualmente desde el inspector si se desea
        if (luzImpacto != null)
        {
            luzImpacto.enabled = false;
        }
    }
    
    /// <summary>
    /// Efecto al recibir daño (polvo, luz, sonido)
    /// </summary>
    public void EfectoDano(int cantidad)
    {
        // Partículas de polvo
        if (efectoPolvo != null)
        {
            InstanciarEfecto(efectoPolvo, 2f);
        }
        
        // Flash de luz
        if (luzImpacto != null)
        {
            StartCoroutine(FlashLuz(colorLuzDano));
        }
        
        // Shake de cámara (leve o medio según daño)
        if (usarShakeCamera && CameraShake.Instance != null)
        {
            if (cantidad == 1)
                CameraShake.Instance.ShakeLeve();
            else
                CameraShake.Instance.ShakeMedio();
        }
        
        // Chispas (opcional, estilo espacial)
        if (efectoChispas != null && Random.value > 0.7f) // 30% chance
        {
            InstanciarEfecto(efectoChispas, 1.5f);
        }
    }
    
    /// <summary>
    /// Efecto al destruir (escombros, explosión, luz fuerte)
    /// </summary>
    public void EfectoDestruccion()
    {
        // Escombros grandes
        if (efectoEscombros != null)
        {
            InstanciarEfecto(efectoEscombros, 3f);
        }
        
        // Polvo extra (más cantidad)
        if (efectoPolvo != null)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector3 offset = Random.insideUnitSphere * 0.5f;
                InstanciarEfecto(efectoPolvo, 2.5f, offset);
            }
        }
        
        // Flash de luz fuerte
        if (luzImpacto != null)
        {
            StartCoroutine(FlashLuz(colorLuzDestruccion, duracionFlashLuz * 2));
        }
        
        // Shake de cámara fuerte
        if (usarShakeCamera && CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeFuerte();
        }
    }
    
    /// <summary>
    /// Instancia un efecto de partículas en la posición del objeto
    /// </summary>
    void InstanciarEfecto(GameObject prefabEfecto, float duracion, Vector3 offset = default)
    {
        GameObject efecto = Instantiate(prefabEfecto, transform.position + offset, Quaternion.identity);
        Destroy(efecto, duracion);
    }
    
    /// <summary>
    /// Flash de luz con fade out
    /// </summary>
    System.Collections.IEnumerator FlashLuz(Color color, float duracion = -1)
    {
        if (duracion < 0)
            duracion = duracionFlashLuz;
        
        luzImpacto.color = color;
        luzImpacto.intensity = intensidadLuz;
        luzImpacto.enabled = true;
        
        float tiempoTranscurrido = 0f;
        
        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido += Time.deltaTime;
            luzImpacto.intensity = Mathf.Lerp(intensidadLuz, 0, tiempoTranscurrido / duracion);
            yield return null;
        }
        
        luzImpacto.enabled = false;
    }
}
