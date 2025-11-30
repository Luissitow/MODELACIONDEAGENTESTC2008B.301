using UnityEngine;

/// <summary>
/// Script de puerta con soporte para animaciones y daño
/// Este script es opcional y complementa a Wall.cs
/// Usa Animator si está disponible, sino usa el sistema de Wall.cs
/// </summary>
public class Door : MonoBehaviour
{
    [Header("Componentes")]
    public bool abierta = false;
    public Animator animator;
    private Wall wallScript;

    [Header("Animación Manual (si no hay Animator)")]
    [SerializeField] private float alturaAbrir = 3f;
    [SerializeField] private float velocidadAbrir = 2f;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        
        wallScript = GetComponent<Wall>();
    }

    public void AbrirPuerta()
    {
        abierta = true;

        // Prioridad 1: Usar Wall.cs si existe
        if (wallScript != null)
        {
            wallScript.AbrirPuerta();
            return;
        }

        // Prioridad 2: Usar Animator si existe
        if (animator != null)
        {
            animator.SetTrigger("Abrir");
            animator.SetBool("Abierta", true);
        }
        else
        {
            // Prioridad 3: Animación manual simple
            StartCoroutine(AnimarApertura());
        }

        Debug.Log($"🚪 Door.AbrirPuerta() ejecutado");
    }

    public void CerrarPuerta()
    {
        abierta = false;
        
        if (animator != null)
        {
            animator.SetTrigger("Cerrar");
            animator.SetBool("Abierta", false);
        }
        
        Debug.Log($"🚪 Door.CerrarPuerta() ejecutado");
    }

    /// <summary>
    /// Animación simple de apertura moviendo hacia arriba
    /// </summary>
    System.Collections.IEnumerator AnimarApertura()
    {
        Vector3 posInicial = transform.position;
        Vector3 posFinal = posInicial + Vector3.up * alturaAbrir;
        float tiempo = 0f;
        float duracion = 1f / velocidadAbrir;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            transform.position = Vector3.Lerp(posInicial, posFinal, tiempo / duracion);
            yield return null;
        }

        transform.position = posFinal;
    }

    /// <summary>
    /// Método para recibir daño (delega a Wall.cs)
    /// </summary>
    public void RecibirDano(int cantidad = 1)
    {
        if (wallScript != null)
        {
            wallScript.RecibirDano(cantidad);
        }
        else
        {
            Debug.LogWarning($"⚠️ Door sin Wall.cs no puede recibir daño");
        }
    }
}
