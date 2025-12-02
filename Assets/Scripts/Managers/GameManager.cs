using UnityEngine;
using FireRescue.Networking;

/// <summary>
/// Tipo de fuente de datos para la simulación
/// </summary>
public enum FuenteDatos
{
    Servidor,    // Cargar simulacion_completa.json desde servidor Python (localhost:8585)
    Local        // Cargar escenario.json desde Resources/ (JSON local original)
}

/// <summary>
/// Manager principal que orquesta la carga y simulación del juego
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Referencias")]
    public TableroBuilder tableroBuilder;
    public SimulacionRunner simulacionRunner;
    public APIClient apiClient;
    
    [Header("Configuración")]
    [Tooltip("Selecciona de dónde cargar los datos de la simulación")]
    public FuenteDatos fuenteDatos = FuenteDatos.Servidor;
    public bool iniciarAutomaticamente = true;
    
    void Start()
    {
        // Auto-crear APIClient si no está asignado y se necesita para servidor
        if (fuenteDatos == FuenteDatos.Servidor && apiClient == null)
        {
            apiClient = GetComponent<APIClient>();
            if (apiClient == null)
            {
                apiClient = gameObject.AddComponent<APIClient>();
                Debug.Log("🔧 APIClient creado automáticamente");
            }
        }
        
        if (iniciarAutomaticamente)
        {
            IniciarJuego();
        }
    }
    
    public void IniciarJuego()
    {
        switch (fuenteDatos)
        {
            case FuenteDatos.Servidor:
                Debug.Log("🌐 Modo Servidor: descargando simulacion_completa.json desde Python");
                IniciarDesdeServidor();
                break;
            
            case FuenteDatos.Local:
                Debug.Log("📂 Modo Local: cargando escenario.json desde Resources/");
                IniciarDesdeArchivoLocal("escenario");
                break;
        }
    }
    
    void IniciarDesdeServidor()
    {
        if (apiClient == null)
        {
            Debug.LogError("❌ APIClient no disponible. Cambiando a escenario.json local.");
            IniciarDesdeArchivoLocal("escenario");
            return;
        }
        
        Debug.Log("🌐 Cargando escenario desde servidor Python (localhost:8585)...");
        StartCoroutine(apiClient.ObtenerSimulacion(
            onSuccess: (jsonData) => {
                Debug.Log($"🛰️ JSON recibido del servidor ({jsonData?.Length ?? 0} caracteres)");
                EscenarioData escenario = JSONLoader.ParsearJSON(jsonData);
                if (escenario != null)
                {
                    Debug.Log("✅ JSON parseado correctamente desde servidor");
                    ConstruirYSimular(escenario);
                }
                else
                {
                    Debug.LogError("❌ Error parseando JSON del servidor");
                    Debug.Log("📂 Fallback: usando escenario.json local");
                    IniciarDesdeArchivoLocal("escenario");
                }
            },
            onError: (error) => {
                Debug.LogWarning($"⚠️ Error conectando al servidor: {error}");
                Debug.Log("📂 Fallback: usando escenario.json local");
                IniciarDesdeArchivoLocal("escenario");
            }
        ));
    }

    // Atajos en el menú contextual del Inspector
    [ContextMenu("🌐 Cargar desde Servidor (simulacion_completa.json)")]
    public void CargarDesdeServidorContext()
    {
        fuenteDatos = FuenteDatos.Servidor;
        IniciarJuego();
    }
    
    [ContextMenu("📄 Cargar Local (escenario.json)")]
    public void CargarLocalContext()
    {
        fuenteDatos = FuenteDatos.Local;
        IniciarJuego();
    }
    
    void IniciarDesdeArchivoLocal(string nombreArchivo)
    {
        // Cargar JSON local desde Resources
        EscenarioData escenario = JSONLoader.CargarEscenario(nombreArchivo);
        
        if (escenario == null)
        {
            Debug.LogError($"❌ No se pudo cargar {nombreArchivo}.json desde Resources/");
            return;
        }
        
        Debug.Log($"✅ {nombreArchivo}.json cargado correctamente");
        ConstruirYSimular(escenario);
    }
    
    void ConstruirYSimular(EscenarioData escenario)
    {
        // 2. Construir tablero
        if (tableroBuilder != null)
        {
            tableroBuilder.Construir(escenario);
        }
        else
        {
            Debug.LogError("❌ TableroBuilder no asignado en GameManager");
            return;
        }
        
        // 3. Iniciar simulación
        if (simulacionRunner != null)
        {
            simulacionRunner.IniciarSimulacion(escenario);
        }
        else
        {
            Debug.LogError("❌ SimulacionRunner no asignado en GameManager");
        }
    }
}
