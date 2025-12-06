using UnityEngine;
using System.Collections;

/// <summary>
/// Componente para controlar la animación de apertura de puertas
/// </summary>
public class Door : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float velocidadApertura = 2f;
    
    private bool estaAbierta = false;
    private Vector3 posicionOriginal;
    private Vector3 posicionAbierta;
    
    private void Awake()
    {
        posicionOriginal = transform.position;
        // La puerta se mueve hacia arriba cuando se abre
        posicionAbierta = posicionOriginal + Vector3.up * 3f;
    }
    
    /// <summary>
    /// Abre la puerta con animación
    /// </summary>
    public void Abrir()
    {
        if (!estaAbierta)
        {
            StartCoroutine(AnimarApertura());
        }
    }
    
    /// <summary>
    /// Cierra la puerta con animación
    /// </summary>
    public void Cerrar()
    {
        if (estaAbierta)
        {
            StartCoroutine(AnimarCierre());
        }
    }
    
    private IEnumerator AnimarApertura()
    {
        Debug.Log($"🚪 Abriendo puerta: {gameObject.name}");
        
        float tiempoTranscurrido = 0f;
        Vector3 posicionInicio = transform.position;
        
        while (tiempoTranscurrido < 1f)
        {
            tiempoTranscurrido += Time.deltaTime * velocidadApertura;
            transform.position = Vector3.Lerp(posicionInicio, posicionAbierta, tiempoTranscurrido);
            yield return null;
        }
        
        transform.position = posicionAbierta;
        estaAbierta = true;
        Debug.Log($"✅ Puerta abierta: {gameObject.name}");
    }
    
    private IEnumerator AnimarCierre()
    {
        Debug.Log($"🚪 Cerrando puerta: {gameObject.name}");
        
        float tiempoTranscurrido = 0f;
        Vector3 posicionInicio = transform.position;
        
        while (tiempoTranscurrido < 1f)
        {
            tiempoTranscurrido += Time.deltaTime * velocidadApertura;
            transform.position = Vector3.Lerp(posicionInicio, posicionOriginal, tiempoTranscurrido);
            yield return null;
        }
        
        transform.position = posicionOriginal;
        estaAbierta = false;
        Debug.Log($"✅ Puerta cerrada: {gameObject.name}");
    }
}
