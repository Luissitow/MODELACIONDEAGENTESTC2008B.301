using UnityEngine;

/// <summary>
/// InteractionSystem - Maneja las interacciones del astronauta con víctimas, falsas alarmas, y entradas
/// Se coloca en los astronautas para detectar cercanía con objetos interactuables
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float radioDeteccion = 2f; // Radio para detectar objetos cercanos
    [SerializeField] private KeyCode teclaInteraccion = KeyCode.E; // Tecla para interactuar
    [SerializeField] private LayerMask capasInteractuables;

    private AstronautController controller;
    private GameObject objetoCercano;
    private bool puedeInteractuar = false;
 
    void Start()
    {
        controller = GetComponent<AstronautController>();
        if (controller == null)
        {
            Debug.LogError("❌ InteractionSystem requiere AstronautController en el mismo GameObject");
        }
    }

    void Update()
    {
        if (controller == null) return;

        // Detectar objetos cercanos
        DetectarObjetosCercanos();

        // Interactuar al presionar tecla
        if (puedeInteractuar && Input.GetKeyDown(teclaInteraccion))
        {
            Interactuar();
        }
    }

    /// <summary>
    /// Detecta objetos interactuables cercanos (víctimas, falsas alarmas, entradas)
    /// </summary>
    void DetectarObjetosCercanos()
    {
        objetoCercano = null;
        puedeInteractuar = false;

        // Buscar víctimas cercanas
        GameObject[] victimas = GameObject.FindGameObjectsWithTag("Victima");
        foreach (var victima in victimas)
        {
            if (Vector3.Distance(transform.position, victima.transform.position) <= radioDeteccion)
            {
                objetoCercano = victima;
                puedeInteractuar = true;
                MostrarIndicadorInteraccion("Presiona E para recoger víctima");
                return;
            }
        }

        // Buscar falsas alarmas cercanas
        GameObject[] falsasAlarmas = GameObject.FindGameObjectsWithTag("FalsaAlarma");
        foreach (var falsa in falsasAlarmas)
        {
            if (Vector3.Distance(transform.position, falsa.transform.position) <= radioDeteccion)
            {
                objetoCercano = falsa;
                puedeInteractuar = true;
                MostrarIndicadorInteraccion("Presiona E para revisar");
                return;
            }
        }

        // Buscar entradas cercanas (para dejar víctimas)
        if (GameManager.Instance != null && GameManager.Instance.EstaCargandoVictima(controller.id))
        {
            GameObject[] entradas = GameObject.FindGameObjectsWithTag("Entrada");
            foreach (var entrada in entradas)
            {
                if (Vector3.Distance(transform.position, entrada.transform.position) <= radioDeteccion)
                {
                    objetoCercano = entrada;
                    puedeInteractuar = true;
                    MostrarIndicadorInteraccion("Presiona E para dejar víctima");
                    return;
                }
            }
        }

        OcultarIndicadorInteraccion();
    }

    /// <summary>
    /// Ejecuta la interacción con el objeto cercano
    /// </summary>
    void Interactuar()
    {
        if (objetoCercano == null) return;

        if (objetoCercano.CompareTag("Victima"))
        {
            InteractuarConVictima(objetoCercano);
        }
        else if (objetoCercano.CompareTag("FalsaAlarma"))
        {
            InteractuarConFalsaAlarma(objetoCercano);
        }
        else if (objetoCercano.CompareTag("Entrada"))
        {
            InteractuarConEntrada(objetoCercano);
        }
    }

    /// <summary>
    /// Interactúa con una víctima (recogerla)
    /// </summary>
    void InteractuarConVictima(GameObject victima)
    {
        // Verificar si ya está cargando una víctima
        if (GameManager.Instance != null && GameManager.Instance.EstaCargandoVictima(controller.id))
        {
            Debug.LogWarning($"⚠️ Astronauta {controller.id} ya está cargando una víctima");
            return;
        }

        Debug.Log($"👤 Astronauta {controller.id} recogió una víctima");

        // Eliminar punto de interés asociado
        EliminarPuntoInteresEnPosicion(victima.transform.position);

        // Destruir víctima
        Destroy(victima);

        // Notificar al GameManager
        if (GameManager.Instance != null)
        {
            VictimaData victimaData = new VictimaData
            {
                row = controller.row,
                col = controller.col,
                type = "victima"
            };
            GameManager.Instance.CargarVictima(controller.id, victimaData);
        }

        // Opcional: Crear indicador visual de que está cargando víctima
        CrearIndicadorVictimaCargada();
    }

    /// <summary>
    /// Interactúa con una falsa alarma (revelarla)
    /// </summary>
    void InteractuarConFalsaAlarma(GameObject falsaAlarma)
    {
        Debug.Log($"🚫 Astronauta {controller.id} descubrió una falsa alarma");

        // Eliminar punto de interés asociado
        EliminarPuntoInteresEnPosicion(falsaAlarma.transform.position);

        // Destruir falsa alarma
        Destroy(falsaAlarma);

        // Notificar al GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RevelarFalsaAlarma(falsaAlarma.transform.position);
        }
    }

    /// <summary>
    /// Interactúa con una entrada (dejar víctima)
    /// </summary>
    void InteractuarConEntrada(GameObject entrada)
    {
        // Verificar si está cargando víctima
        if (GameManager.Instance == null || !GameManager.Instance.EstaCargandoVictima(controller.id))
        {
            Debug.LogWarning($"⚠️ Astronauta {controller.id} no está cargando ninguna víctima");
            return;
        }

        Debug.Log($"🏠 Astronauta {controller.id} dejó víctima en entrada - ¡Rescatada!");

        // Notificar al GameManager
        GameManager.Instance.DescargarVictima(controller.id);
        GameManager.Instance.RescatarVictima(controller.id, transform.position);

        // Eliminar indicador visual de víctima cargada
        EliminarIndicadorVictimaCargada();
    }

    /// <summary>
    /// Elimina el punto de interés en una posición (cuando se revela víctima o falsa alarma)
    /// </summary>
    void EliminarPuntoInteresEnPosicion(Vector3 posicion)
    {
        GameObject[] puntosInteres = GameObject.FindGameObjectsWithTag("PuntoInteres");
        foreach (var punto in puntosInteres)
        {
            if (Vector3.Distance(punto.transform.position, posicion) < 1f)
            {
                Destroy(punto);
                Debug.Log("❌ Punto de interés eliminado");
                break;
            }
        }
    }

    /// <summary>
    /// Crea un indicador visual de que el astronauta está cargando una víctima
    /// </summary>
    void CrearIndicadorVictimaCargada()
    {
        // TODO: Crear un objeto visual encima del astronauta
        // Por ejemplo, un pequeño sprite o modelo 3D de víctima
        GameObject indicador = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicador.name = "IndicadorVictima";
        indicador.transform.SetParent(transform);
        indicador.transform.localPosition = new Vector3(0, 2f, 0);
        indicador.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // Hacer el indicador verde brillante
        var renderer = indicador.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }

        Debug.Log("✅ Indicador de víctima cargada creado");
    }

    /// <summary>
    /// Elimina el indicador visual de víctima cargada
    /// </summary>
    void EliminarIndicadorVictimaCargada()
    {
        Transform indicador = transform.Find("IndicadorVictima");
        if (indicador != null)
        {
            Destroy(indicador.gameObject);
            Debug.Log("❌ Indicador de víctima cargada eliminado");
        }
    }

    /// <summary>
    /// Muestra un indicador de interacción (UI placeholder)
    /// </summary>
    void MostrarIndicadorInteraccion(string texto)
    {
        // TODO: Implementar UI real
        // Por ahora solo en consola
        // Debug.Log(texto);
    }

    /// <summary>
    /// Oculta el indicador de interacción
    /// </summary>
    void OcultarIndicadorInteraccion()
    {
        // TODO: Implementar UI real
    }

    /// <summary>
    /// Dibuja el radio de detección en el editor (para debugging)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}
