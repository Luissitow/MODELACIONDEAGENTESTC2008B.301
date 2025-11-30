using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameManager - Controla la lógica principal del juego Flash Point adaptado a astronautas
/// Maneja condiciones de victoria/derrota, contadores, y el flujo del juego
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ConstructorTablero constructorTablero;
    [SerializeField] private ControladorJuego controladorJuego;

    [Header("Contadores del Juego")]
    [SerializeField] public int victimasRescatadas = 0;
    [SerializeField] public int victimasPerdidas = 0;
    [SerializeField] private int puntosDanioEdificio = 0;
    [SerializeField] private int turnoActual = 0;
    
    // Propiedad pública para puntos de daño
    public int puntosDanio
    {
        get => puntosDanioEdificio;
        set => puntosDanioEdificio = value;
    }

    [Header("Configuración del Juego")]
    [SerializeField] private int victimasParaGanar = 7;
    [SerializeField] private int victimasPerdidasMaximas = 4;
    [SerializeField] private int danioMaximoEdificio = 24; // 25 o más = colapso
    // Nota: numeroAstronautas se calcula dinámicamente del JSON
    // [SerializeField] private int numeroAstronautas = 6; // Según las reglas: 6 bomberos/astronautas

    [Header("Estado del Juego")]
    [SerializeField] private EstadoJuego estadoActual = EstadoJuego.EnProgreso;
    [SerializeField] private bool juegoTerminado = false;

    // Tracking de entidades
    private HashSet<string> victimasEnMapa = new HashSet<string>();
    private HashSet<string> falsasAlarmasEnMapa = new HashSet<string>();
    private Dictionary<int, VictimaData> victimasCargadas = new Dictionary<int, VictimaData>(); // astronautaID -> victima

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InicializarJuego();
    }

    void Update()
    {
        if (juegoTerminado) return;

        // Verificar condiciones de victoria/derrota cada frame
        VerificarCondicionesJuego();
    }

    /// <summary>
    /// Inicializa el juego con valores por defecto
    /// </summary>
    void InicializarJuego()
    {
        victimasRescatadas = 0;
        victimasPerdidas = 0;
        puntosDanioEdificio = 0;
        turnoActual = 0;
        estadoActual = EstadoJuego.EnProgreso;
        juegoTerminado = false;

        Debug.Log("🎮 GameManager: Juego inicializado");
        Debug.Log($"📊 Objetivo: Rescatar {victimasParaGanar} víctimas");
        Debug.Log($"⚠️ Límites: Máximo {victimasPerdidasMaximas} víctimas perdidas, {danioMaximoEdificio} puntos de daño");
    }

    /// <summary>
    /// Verifica las condiciones de victoria o derrota
    /// </summary>
    void VerificarCondicionesJuego()
    {
        // CONDICIÓN DE VICTORIA: 7 víctimas rescatadas
        if (victimasRescatadas >= victimasParaGanar)
        {
            TerminarJuego(EstadoJuego.Victoria);
            return;
        }

        // CONDICIÓN DE DERROTA: 4 víctimas perdidas
        if (victimasPerdidas >= victimasPerdidasMaximas)
        {
            TerminarJuego(EstadoJuego.DerrotaVictimasPerdidas);
            return;
        }

        // CONDICIÓN DE DERROTA: 25 o más puntos de daño (colapso del edificio)
        if (puntosDanioEdificio >= danioMaximoEdificio + 1)
        {
            TerminarJuego(EstadoJuego.DerrotaColapso);
            return;
        }
    }

    /// <summary>
    /// Termina el juego con el estado especificado
    /// </summary>
    void TerminarJuego(EstadoJuego estado)
    {
        if (juegoTerminado) return;

        juegoTerminado = true;
        estadoActual = estado;

        switch (estado)
        {
            case EstadoJuego.Victoria:
                Debug.Log($"🎉 ¡VICTORIA! Has rescatado {victimasRescatadas} víctimas");
                MostrarPantallaVictoria();
                break;

            case EstadoJuego.DerrotaVictimasPerdidas:
                Debug.Log($"💀 DERROTA: Se perdieron {victimasPerdidas} víctimas");
                MostrarPantallaDerrota("Demasiadas víctimas perdidas");
                break;

            case EstadoJuego.DerrotaColapso:
                Debug.Log($"🏚️ DERROTA: El edificio colapsó con {puntosDanioEdificio} puntos de daño");
                MostrarPantallaDerrota("El edificio colapsó");
                break;
        }
    }

    /// <summary>
    /// Registra el rescate de una víctima
    /// </summary>
    public void RescatarVictima(int astronautaID, Vector3 posicion)
    {
        victimasRescatadas++;
        Debug.Log($"✅ Víctima rescatada por astronauta {astronautaID}! Total: {victimasRescatadas}/{victimasParaGanar}");

        // Eliminar del tracking
        string key = $"{posicion.x}_{posicion.z}";
        victimasEnMapa.Remove(key);
    }

    /// <summary>
    /// Registra la pérdida de una víctima (por fuego/arañas)
    /// </summary>
    public void PerderVictima(Vector3 posicion)
    {
        victimasPerdidas++;
        Debug.Log($"❌ Víctima perdida! Total: {victimasPerdidas}/{victimasPerdidasMaximas}");

        // Eliminar del tracking
        string key = $"{posicion.x}_{posicion.z}";
        victimasEnMapa.Remove(key);
    }

    /// <summary>
    /// Registra el descubrimiento de una falsa alarma
    /// </summary>
    public void RevelarFalsaAlarma(Vector3 posicion)
    {
        Debug.Log($"🚫 Falsa alarma descubierta en {posicion}");

        // Eliminar del tracking
        string key = $"{posicion.x}_{posicion.z}";
        falsasAlarmasEnMapa.Remove(key);
    }

    /// <summary>
    /// Añade daño al edificio (paredes destruidas, explosiones)
    /// </summary>
    public void AnadirDanioEdificio(int cantidad)
    {
        puntosDanioEdificio += cantidad;
        Debug.Log($"💥 Daño al edificio: +{cantidad} (Total: {puntosDanioEdificio}/{danioMaximoEdificio})");

        if (puntosDanioEdificio >= danioMaximoEdificio + 1)
        {
            Debug.LogWarning("⚠️ ¡El edificio está a punto de colapsar!");
        }
    }

    /// <summary>
    /// Avanza al siguiente turno
    /// </summary>
    public void AvanzarTurno()
    {
        turnoActual++;
        Debug.Log($"🔄 Turno {turnoActual} iniciado");
    }

    /// <summary>
    /// Marca que un astronauta está cargando una víctima
    /// </summary>
    public void CargarVictima(int astronautaID, VictimaData victima)
    {
        if (!victimasCargadas.ContainsKey(astronautaID))
        {
            victimasCargadas[astronautaID] = victima;
            Debug.Log($"🎒 Astronauta {astronautaID} cargó una víctima");
        }
        else
        {
            Debug.LogWarning($"⚠️ Astronauta {astronautaID} ya está cargando una víctima");
        }
    }

    /// <summary>
    /// Descarga la víctima que lleva un astronauta (al llegar a entrada)
    /// </summary>
    public void DescargarVictima(int astronautaID)
    {
        if (victimasCargadas.ContainsKey(astronautaID))
        {
            victimasCargadas.Remove(astronautaID);
            // Nota: RescatarVictima() se llama desde otro lugar para incrementar contador
        }
    }

    /// <summary>
    /// Verifica si un astronauta está cargando una víctima
    /// </summary>
    public bool EstaCargandoVictima(int astronautaID)
    {
        return victimasCargadas.ContainsKey(astronautaID);
    }

    /// <summary>
    /// Registra una víctima en el mapa (tracking)
    /// </summary>
    public void RegistrarVictima(Vector3 posicion)
    {
        string key = $"{posicion.x}_{posicion.z}";
        victimasEnMapa.Add(key);
    }

    /// <summary>
    /// Registra una falsa alarma en el mapa (tracking)
    /// </summary>
    public void RegistrarFalsaAlarma(Vector3 posicion)
    {
        string key = $"{posicion.x}_{posicion.z}";
        falsasAlarmasEnMapa.Add(key);
    }

    /// <summary>
    /// Muestra pantalla de victoria (UI placeholder)
    /// </summary>
    void MostrarPantallaVictoria()
    {
        // TODO: Implementar UI de victoria
        Debug.Log("=== PANTALLA DE VICTORIA ===");
        Debug.Log($"Víctimas rescatadas: {victimasRescatadas}");
        Debug.Log($"Turnos jugados: {turnoActual}");
        Debug.Log($"Daño del edificio: {puntosDanioEdificio}");
    }

    /// <summary>
    /// Muestra pantalla de derrota (UI placeholder)
    /// </summary>
    void MostrarPantallaDerrota(string razon)
    {
        // TODO: Implementar UI de derrota
        Debug.Log("=== PANTALLA DE DERROTA ===");
        Debug.Log($"Razón: {razon}");
        Debug.Log($"Víctimas rescatadas: {victimasRescatadas}");
        Debug.Log($"Víctimas perdidas: {victimasPerdidas}");
        Debug.Log($"Daño del edificio: {puntosDanioEdificio}");
        Debug.Log($"Turnos jugados: {turnoActual}");
    }

    // Getters públicos
    public int GetVictimasRescatadas() => victimasRescatadas;
    public int GetVictimasPerdidas() => victimasPerdidas;
    public int GetPuntosDanioEdificio() => puntosDanioEdificio;
    public int GetTurnoActual() => turnoActual;
    public EstadoJuego GetEstadoJuego() => estadoActual;
    public bool EstaJuegoTerminado() => juegoTerminado;
}

/// <summary>
/// Estados posibles del juego
/// </summary>
public enum EstadoJuego
{
    EnProgreso,
    Victoria,
    DerrotaVictimasPerdidas,
    DerrotaColapso
}
