using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Reproduce una simulación completa del juego
/// Lee TODO el JSON con todos los turnos y los ejecuta secuencialmente
/// </summary>
public class SimulacionPlayer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ConstructorTablero constructorTablero;
    [SerializeField] private ActionExecutor actionExecutor;
    [SerializeField] private GameManager gameManager;

    [Header("Configuración de Reproducción")]
    [SerializeField] private string archivoSimulacion = "simulacion_completa.json";
    [SerializeField] private float velocidadReproduccion = 1f; // 1 = normal, 2 = 2x, 0.5 = slow-mo
    [SerializeField] private bool reproducirAutomaticamente = true;
    [SerializeField] private bool pausarEntreTurnos = false;
    [SerializeField] private float tiempoEntreTurnos = 1f;

    [Header("Estado de Reproducción")]
    public bool estaReproduciendo = false;
    public bool estaPausado = false;
    public int turnoActual = 0;
    public int turnosTotales = 0;

    [Header("Debug")]
    [SerializeField] private bool mostrarDebugLogs = true;

    // Datos de la simulación
    private SimulacionData simulacionData;
    private Coroutine coroutineReproduccion;

    void Start()
    {
        if (reproducirAutomaticamente)
        {
            // Esperar 1 segundo para que ControladorJuego construya el tablero primero
            StartCoroutine(IniciarConRetraso());
        }
    }

    IEnumerator IniciarConRetraso()
    {
        // Esperar a que el tablero se construya completamente
        yield return new WaitForSeconds(1f);
        CargarYReproducirSimulacion();
    }

    /// <summary>
    /// Carga el JSON completo y comienza la reproducción
    /// </summary>
    public void CargarYReproducirSimulacion()
    {
        if (estaReproduciendo)
        {
            Debug.LogWarning("⚠️ Ya hay una simulación en reproducción");
            return;
        }

        // Cargar JSON
        string rutaArchivo = Path.Combine(Application.dataPath, "Resources", archivoSimulacion);
        
        if (!File.Exists(rutaArchivo))
        {
            Debug.LogError($"❌ No se encontró el archivo: {rutaArchivo}");
            return;
        }

        string jsonContent = File.ReadAllText(rutaArchivo);
        
        try
        {
            simulacionData = JsonUtility.FromJson<SimulacionData>(jsonContent);
            
            if (simulacionData == null || simulacionData.turnos == null)
            {
                Debug.LogError("❌ Error al parsear JSON de simulación");
                return;
            }

            turnosTotales = simulacionData.turnos.Count;
            
            if (mostrarDebugLogs)
                Debug.Log($"📹 Simulación cargada: {turnosTotales} turnos");

            // Construir estado inicial y esperar a que termine
            StartCoroutine(PrepararYReproducir());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al cargar simulación: {e.Message}");
        }
    }

    /// <summary>
    /// Construye el tablero con el estado inicial
    /// </summary>
    void ConstruirEstadoInicial()
    {
        if (constructorTablero == null)
        {
            Debug.LogWarning("⚠️ No hay ConstructorTablero asignado");
            return;
        }

        // Si el JSON tiene estado inicial con escenario, usarlo
        if (simulacionData.estado_inicial != null && simulacionData.estado_inicial.escenario != null)
        {
            constructorTablero.ConstruirMapaInicial(simulacionData.estado_inicial.escenario);
            if (mostrarDebugLogs)
                Debug.Log("🏗️ Tablero inicial construido desde JSON de simulación");
        }
        else
        {
            // Si no hay escenario en simulacion_completa.json, cargar desde escenario.json
            CargarEscenarioPorDefecto();
        }

        // Esperar un frame adicional para asegurar que los astronautas estén creados
        StartCoroutine(ReinicializarCacheConRetraso());
    }

    /// <summary>
    /// Carga el escenario por defecto desde escenario.json
    /// </summary>
    void CargarEscenarioPorDefecto()
    {
        string rutaEscenario = Path.Combine(Application.dataPath, "Resources", "escenario.json");
        
        if (!File.Exists(rutaEscenario))
        {
            Debug.LogError("❌ No se encontró escenario.json en Resources");
            return;
        }

        string jsonEscenario = File.ReadAllText(rutaEscenario);
        
        try
        {
            EscenarioData escenario = JsonUtility.FromJson<EscenarioData>(jsonEscenario);
            
            if (escenario != null)
            {
                constructorTablero.ConstruirMapaInicial(escenario);
                if (mostrarDebugLogs)
                    Debug.Log("🏗️ Tablero construido desde escenario.json");
            }
            else
            {
                Debug.LogError("❌ Error al parsear escenario.json");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al cargar escenario.json: {e.Message}");
        }
    }

    /// <summary>
    /// Prepara el tablero y espera antes de iniciar la reproducción
    /// </summary>
    IEnumerator PrepararYReproducir()
    {
        // Construir estado inicial
        if (simulacionData.estado_inicial != null)
        {
            ConstruirEstadoInicial();
        }

        // Esperar 2 frames para que todos los astronautas se creen
        yield return null;
        yield return null;
        
        // Reinicializar cache de ActionExecutor
        if (actionExecutor != null)
        {
            actionExecutor.ReinicializarCache();
        }

        if (mostrarDebugLogs)
            Debug.Log("🏗️ Tablero listo para simulación");

        // Ahora sí iniciar reproducción
        IniciarReproduccion();
    }

    IEnumerator ReinicializarCacheConRetraso()
    {
        yield return null; // Esperar 1 frame
        
        // Reinicializar cache de ActionExecutor después de crear los astronautas
        if (actionExecutor != null)
        {
            actionExecutor.ReinicializarCache();
        }

        if (mostrarDebugLogs)
            Debug.Log("🏗️ Tablero inicial construido desde JSON");
    }

    /// <summary>
    /// Inicia la reproducción de la simulación
    /// </summary>
    public void IniciarReproduccion()
    {
        if (estaReproduciendo)
        {
            Debug.LogWarning("⚠️ La simulación ya está reproduciéndose");
            return;
        }

        if (simulacionData == null || turnosTotales == 0)
        {
            Debug.LogError("❌ No hay simulación cargada");
            return;
        }

        // Validar referencias necesarias
        if (actionExecutor == null)
        {
            Debug.LogError("❌ ActionExecutor no está asignado en SimulacionPlayer. Asígnalo en el Inspector de Unity.");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogWarning("⚠️ GameManager no está asignado en SimulacionPlayer");
        }

        estaReproduciendo = true;
        estaPausado = false;
        turnoActual = 0;

        if (mostrarDebugLogs)
            Debug.Log("▶️ Iniciando reproducción de simulación");

        coroutineReproduccion = StartCoroutine(ReproducirSimulacion());
    }

    /// <summary>
    /// Coroutine que reproduce todos los turnos
    /// </summary>
    IEnumerator ReproducirSimulacion()
    {
        for (int i = 0; i < turnosTotales; i++)
        {
            // Verificar pausa
            while (estaPausado)
            {
                yield return null;
            }

            turnoActual = i + 1;
            TurnoData turno = simulacionData.turnos[i];

            if (mostrarDebugLogs)
                Debug.Log($"🎬 === TURNO {turnoActual}/{turnosTotales} ===");

            // Ejecutar acciones del turno
            if (turno.acciones != null && turno.acciones.Count > 0)
            {
                if (actionExecutor == null)
                {
                    Debug.LogError("❌ ActionExecutor no está asignado en SimulacionPlayer");
                    Detener();
                    yield break;
                }
                
                yield return actionExecutor.EjecutarAcciones(turno.acciones);
            }

            // Actualizar estado después del turno
            if (turno.estado_despues != null)
            {
                ActualizarEstado(turno.estado_despues);
            }

            // Esperar entre turnos (solo si no es el último turno)
            if (i < turnosTotales - 1)
            {
                float tiempoEspera = tiempoEntreTurnos / velocidadReproduccion;
                
                if (pausarEntreTurnos)
                {
                    if (mostrarDebugLogs)
                        Debug.Log($"⏸️ Pausa entre turnos ({tiempoEspera}s)");
                }
                
                yield return new WaitForSeconds(tiempoEspera);
            }
        }

        // Simulación terminada
        FinalizarReproduccion();
    }

    /// <summary>
    /// Actualiza el estado del juego después de un turno
    /// </summary>
    void ActualizarEstado(EstadoDespuesData estado)
    {
        if (actionExecutor != null && estado.escenario != null)
        {
            actionExecutor.ActualizarEstadoCompleto(estado.escenario);
        }

        // Actualizar GameManager
        if (gameManager != null && estado.estado_juego != null)
        {
            gameManager.victimasRescatadas = estado.estado_juego.victimas_rescatadas;
            gameManager.victimasPerdidas = estado.estado_juego.victimas_perdidas;
            gameManager.puntosDanio = estado.estado_juego.puntos_dano;
        }
    }

    /// <summary>
    /// Finaliza la reproducción
    /// </summary>
    void FinalizarReproduccion()
    {
        estaReproduciendo = false;
        estaPausado = false;

        if (mostrarDebugLogs)
            Debug.Log("✅ Simulación completada");

        // Mostrar resultado final
        if (simulacionData.resultado_final != null)
        {
            MostrarResultadoFinal();
        }
    }

    /// <summary>
    /// Muestra el resultado final de la simulación
    /// </summary>
    void MostrarResultadoFinal()
    {
        ResultadoFinalData resultado = simulacionData.resultado_final;

        string mensaje = resultado.victoria ? "🎉 ¡VICTORIA!" : "💀 DERROTA";
        mensaje += $"\n📊 Víctimas rescatadas: {resultado.victimas_rescatadas}";
        mensaje += $"\n💔 Víctimas perdidas: {resultado.victimas_perdidas}";
        mensaje += $"\n🔥 Puntos de daño: {resultado.puntos_dano}";

        Debug.Log(mensaje);
    }

    // ========== CONTROLES DE REPRODUCCIÓN ==========

    /// <summary>
    /// Pausa la reproducción
    /// </summary>
    public void Pausar()
    {
        if (!estaReproduciendo)
        {
            Debug.LogWarning("⚠️ No hay reproducción activa para pausar");
            return;
        }

        estaPausado = true;
        if (mostrarDebugLogs)
            Debug.Log("⏸️ Reproducción pausada");
    }

    /// <summary>
    /// Reanuda la reproducción
    /// </summary>
    public void Reanudar()
    {
        if (!estaReproduciendo)
        {
            Debug.LogWarning("⚠️ No hay reproducción activa para reanudar");
            return;
        }

        estaPausado = false;
        if (mostrarDebugLogs)
            Debug.Log("▶️ Reproducción reanudada");
    }

    /// <summary>
    /// Detiene la reproducción completamente
    /// </summary>
    public void Detener()
    {
        if (coroutineReproduccion != null)
        {
            StopCoroutine(coroutineReproduccion);
            coroutineReproduccion = null;
        }

        estaReproduciendo = false;
        estaPausado = false;
        turnoActual = 0;

        if (mostrarDebugLogs)
            Debug.Log("⏹️ Reproducción detenida");
    }

    /// <summary>
    /// Cambia la velocidad de reproducción
    /// </summary>
    public void CambiarVelocidad(float nuevaVelocidad)
    {
        velocidadReproduccion = Mathf.Clamp(nuevaVelocidad, 0.1f, 5f);
        
        if (mostrarDebugLogs)
            Debug.Log($"⚡ Velocidad: {velocidadReproduccion}x");
    }

    /// <summary>
    /// Reinicia la simulación desde el inicio
    /// </summary>
    public void Reiniciar()
    {
        Detener();
        CargarYReproducirSimulacion();
    }

    void OnDestroy()
    {
        Detener();
    }
}

// ========== CLASES DE DATOS PARA JSON ==========

[System.Serializable]
public class SimulacionData
{
    public int duracion_total;
    public List<TurnoData> turnos;
    public EstadoInicialData estado_inicial;
    public ResultadoFinalData resultado_final;
}

[System.Serializable]
public class TurnoData
{
    public int turno;
    public string timestamp;
    public List<AccionData> acciones;
    public EstadoDespuesData estado_despues;
}

[System.Serializable]
public class EstadoInicialData
{
    public EscenarioData escenario; // Usar la estructura de EscenarioData que ya existe
}

[System.Serializable]
public class EstadoDespuesData
{
    public EscenarioData escenario;
    public EstadoJuegoData estado_juego;
}

[System.Serializable]
public class ResultadoFinalData
{
    public bool victoria;
    public int victimas_rescatadas;
    public int victimas_perdidas;
    public int puntos_dano;
    public int turnos_totales;
}

[System.Serializable]
public class MapaData
{
    public int filas;
    public int columnas;
    public List<CeldaData> celdas;
    public List<ParedData> paredes;
}

[System.Serializable]
public class CeldaData
{
    public int fila;
    public int columna;
    public string tipo;
    public bool fuego;
    public bool humo;
    public VictimaData victima;
    public List<string> items;
}

// VictimaData y ParedData ya existen en Assets/Scripts/Data/Model/

[System.Serializable]
public class JugadorData
{
    public int id;
    public string nombre;
    public int fila;
    public int columna;
    public string orientacion;
    public int puntos_accion;
    public bool vivo;
    public bool en_zona_exterior;
    public bool tiene_victima;
    public List<string> items;
}

[System.Serializable]
public class EstadoJuegoData
{
    public int victimas_rescatadas;
    public int victimas_perdidas;
    public int puntos_dano;
    public int turno_actual;
    public bool juego_terminado;
    public string resultado;
}

// EscenarioData ya existe en Assets/Scripts/Data/Model/EscenarioData.cs
