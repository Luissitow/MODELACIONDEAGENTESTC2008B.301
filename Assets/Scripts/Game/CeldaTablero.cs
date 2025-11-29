using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente que va en el PREFAB de cada celda del tablero
/// Maneja el estado visual de la celda (fuego, humo, víctimas, items)
/// </summary>
public class CeldaTablero : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Posición en Tablero")]
    public int fila;
    public int columna;

    [Header("Estado de Celda")]
    public bool tieneFuego = false;
    public bool tieneHumo = false;
    public bool tieneVictima = false;
    public TipoVictima tipoVictima = TipoVictima.Falsa;
    public bool tieneHuevecillos = false; // Hazmat - explota si pisas
    public bool tieneArana = false; // Araña en esta celda

    [Header("Referencias Visuales")]
    [SerializeField] private GameObject efectoFuego;
    [SerializeField] private GameObject efectoHumo;
    [SerializeField] private GameObject iconoVictima;
    [SerializeField] private GameObject iconoHuevecillos; // Hazmat visual
    [SerializeField] private GameObject iconoArana; // Araña visual
    [SerializeField] private GameObject efectoExplosion; // Particle System explosión
    [SerializeField] private GameObject indicadorHover;
    [SerializeField] private MeshRenderer renderPiso;

    [Header("Materiales")]
    [SerializeField] private Material materialNormal;
    [SerializeField] private Material materialHover;
    [SerializeField] private Material materialSeleccionado;
    [SerializeField] private Material materialPeligro; // Para hazmat

    [Header("Items en Celda")]
    public bool tieneHacha = false;
    public bool tieneExtintor = false;
    
    private bool seleccionada = false;
    private Color colorOriginal;

    void Start()
    {
        // Inicializar estado visual
        ActualizarEstadoVisual();

        // Guardar color original
        if (renderPiso != null)
        {
            colorOriginal = renderPiso.material.color;
        }

        // Desactivar indicador hover
        if (indicadorHover != null)
            indicadorHover.SetActive(false);
    }

    /// <summary>
    /// Actualiza el estado visual de la celda según sus propiedades
    /// </summary>
    public void ActualizarEstadoVisual()
    {
        // Fuego
        if (efectoFuego != null)
            efectoFuego.SetActive(tieneFuego);

        // Humo
        if (efectoHumo != null)
            efectoHumo.SetActive(tieneHumo);

        // Víctima
        if (iconoVictima != null)
        {
            iconoVictima.SetActive(tieneVictima);
            
            // Cambiar color según tipo de víctima
            if (tieneVictima)
            {
                var renderer = iconoVictima.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    switch (tipoVictima)
                    {
                        case TipoVictima.Verdadera:
                            renderer.material.color = Color.green;
                            break;
                        case TipoVictima.Falsa:
                            renderer.material.color = Color.red;
                            break;
                    }
                }
            }
        }

        // Huevecillos (Hazmat)
        if (iconoHuevecillos != null)
            iconoHuevecillos.SetActive(tieneHuevecillos);

        // Araña
        if (iconoArana != null)
            iconoArana.SetActive(tieneArana);

        // Cambiar color del piso si hay peligro
        if (renderPiso != null && materialPeligro != null && tieneHuevecillos)
        {
            renderPiso.material = materialPeligro;
        }
    }

    /// <summary>
    /// Establece el estado de fuego en la celda
    /// </summary>
    public void SetFuego(bool activar)
    {
        tieneFuego = activar;
        ActualizarEstadoVisual();

        if (activar)
            Debug.Log($"🔥 Fuego activado en celda ({fila},{columna})");
        else
            Debug.Log($"💧 Fuego apagado en celda ({fila},{columna})");
    }

    /// <summary>
    /// Establece el estado de humo en la celda
    /// </summary>
    public void SetHumo(bool activar)
    {
        tieneHumo = activar;
        ActualizarEstadoVisual();

        if (activar)
            Debug.Log($"💨 Humo activado en celda ({fila},{columna})");
    }

    /// <summary>
    /// Coloca una víctima en la celda
    /// </summary>
    public void ColocarVictima(TipoVictima tipo)
    {
        tieneVictima = true;
        tipoVictima = tipo;
        ActualizarEstadoVisual();

        Debug.Log($"👤 Víctima {tipo} colocada en celda ({fila},{columna})");
    }

    /// <summary>
    /// Rescata la víctima de la celda
    /// </summary>
    public TipoVictima RescatarVictima()
    {
        if (!tieneVictima)
        {
            Debug.LogWarning($"⚠️ No hay víctima en celda ({fila},{columna})");
            return TipoVictima.Falsa;
        }

        TipoVictima tipo = tipoVictima;
        tieneVictima = false;
        ActualizarEstadoVisual();

        Debug.Log($"✅ Víctima {tipo} rescatada de celda ({fila},{columna})");
        return tipo;
    }

    /// <summary>
    /// Propagación de fuego a celda adyacente
    /// </summary>
    public void PropagacionFuego()
    {
        if (tieneHumo && !tieneFuego)
        {
            SetFuego(true);
            SetHumo(false);
            Debug.Log($"🔥 Fuego propagado a celda ({fila},{columna}) desde humo");
        }
        else if (!tieneFuego && !tieneHumo)
        {
            SetHumo(true);
            Debug.Log($"💨 Humo propagado a celda ({fila},{columna})");
        }
    }

    /// <summary>
    /// Selecciona o deselecciona la celda
    /// </summary>
    public void SetSeleccionada(bool seleccionar)
    {
        seleccionada = seleccionar;
        
        if (renderPiso != null)
        {
            if (seleccionar && materialSeleccionado != null)
                renderPiso.material = materialSeleccionado;
            else if (materialNormal != null)
                renderPiso.material = materialNormal;
        }
    }

    /// <summary>
    /// Resalta la celda al pasar el mouse
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!seleccionada)
        {
            if (indicadorHover != null)
                indicadorHover.SetActive(true);

            if (renderPiso != null && materialHover != null)
                renderPiso.material = materialHover;
        }
    }

    /// <summary>
    /// Quita el resaltado al salir el mouse
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!seleccionada)
        {
            if (indicadorHover != null)
                indicadorHover.SetActive(false);

            if (renderPiso != null && materialNormal != null)
                renderPiso.material = materialNormal;
        }
    }

    /// <summary>
    /// Maneja el click en la celda
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"🖱️ Click en celda ({fila},{columna}) - Fuego: {tieneFuego}, Humo: {tieneHumo}, Víctima: {tieneVictima}");
        
        // Aquí puedes agregar lógica para seleccionar celdas, mostrar información, etc.
    }

    /// <summary>
    /// Coloca huevecillos (Hazmat) en la celda
    /// </summary>
    public void ColocarHuevecillos()
    {
        tieneHuevecillos = true;
        ActualizarEstadoVisual();
        Debug.Log($"🥚 Huevecillos colocados en celda ({fila},{columna}) ¡PELIGRO!");
    }

    /// <summary>
    /// Coloca una araña en la celda
    /// </summary>
    public void ColocarArana()
    {
        tieneArana = true;
        ActualizarEstadoVisual();
        Debug.Log($"🕷️ Araña colocada en celda ({fila},{columna})");
    }

    /// <summary>
    /// Causa una explosión en la celda
    /// </summary>
    public void Explotar()
    {
        Debug.Log($"💥 ¡EXPLOSIÓN en celda ({fila},{columna})!");

        // Activar efecto visual de explosión
        if (efectoExplosion != null)
        {
            StartCoroutine(MostrarExplosion());
        }

        // La explosión destruye todo en la celda
        tieneFuego = false;
        tieneHumo = false;
        tieneHuevecillos = false;
        tieneArana = false;
        tieneVictima = false; // ¡La víctima muere!

        // Propagar daño (esto debería afectar paredes adyacentes)
        ActualizarEstadoVisual();
    }

    /// <summary>
    /// Muestra el efecto de explosión temporalmente
    /// </summary>
    System.Collections.IEnumerator MostrarExplosion()
    {
        efectoExplosion.SetActive(true);
        yield return new WaitForSeconds(1f);
        efectoExplosion.SetActive(false);
    }

    /// <summary>
    /// Un astronauta pisa la celda - verifica si hay huevecillos
    /// </summary>
    public bool PisarCelda()
    {
        if (tieneHuevecillos)
        {
            Debug.LogWarning($"💥 ¡Astronauta pisó huevecillos en ({fila},{columna})!");
            Explotar();
            return true; // Hubo explosión
        }
        return false; // No hubo explosión
    }

    /// <summary>
    /// Ataca la araña en esta celda (apagar fuego = atacar araña)
    /// </summary>
    public void AtacarArana()
    {
        if (!tieneArana)
        {
            Debug.LogWarning($"⚠️ No hay araña en celda ({fila},{columna})");
            return;
        }

        tieneArana = false;
        ActualizarEstadoVisual();
        Debug.Log($"⚔️ Araña eliminada en celda ({fila},{columna})");
    }

    /// <summary>
    /// Obtiene información de la celda como string
    /// </summary>
    public string ObtenerInfo()
    {
        string info = $"Celda ({fila},{columna})\n";
        
        if (tieneFuego)
            info += "🔥 Fuego\n";
        
        if (tieneHumo)
            info += "💨 Humo\n";
        
        if (tieneVictima)
            info += $"👤 Víctima {tipoVictima}\n";

        if (tieneHuevecillos)
            info += "🥚 Huevecillos ¡PELIGRO!\n";

        if (tieneArana)
            info += "🕷️ Araña\n";

        if (tieneHacha)
            info += "🪓 Hacha\n";

        if (tieneExtintor)
            info += "🧯 Extintor\n";

        return info;
    }

    /// <summary>
    /// Visualización en el editor
    /// </summary>
    void OnDrawGizmos()
    {
        // Dibuja el número de celda en el editor
        #if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 10;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.1f, $"({fila},{columna})", style);
        #endif
    }
}

/// <summary>
/// Tipos de víctimas en el juego
/// </summary>
public enum TipoVictima
{
    Verdadera, // Cuenta para victoria
    Falsa      // No cuenta para victoria
}
