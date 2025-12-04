using UnityEngine;
using FireRescue.Networking;

/// <summary>
/// Tipo de fuente de datos para la simulación
/// </summary>
public enum FuenteDatos
{
    Servidor,      // Cargar simulacion_completa.json desde servidor Python (localhost:8585)
    Local,         // Cargar escenario.json desde Resources/ (JSON local original)
    LocalPython    // Cargar simulacion.json generado por multiagentes.py desde raíz del proyecto
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
            
            case FuenteDatos.LocalPython:
                Debug.Log("🐍 Modo LocalPython: cargando simulacion.json de multiagentes.py");
                IniciarDesdePythonLocal();
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
    
    void IniciarDesdePythonLocal()
    {
        // Cargar simulacion.json desde la raíz del proyecto Unity
        string rutaArchivo = System.IO.Path.Combine(Application.dataPath, "..", "simulacion.json");
        rutaArchivo = System.IO.Path.GetFullPath(rutaArchivo); // Normalizar ruta
        
        Debug.Log($"📂 Buscando archivo: {rutaArchivo}");
        
        if (!System.IO.File.Exists(rutaArchivo))
        {
            Debug.LogError($"❌ No se encontró simulacion.json en: {rutaArchivo}");
            Debug.LogWarning("💡 Ejecuta 'python Assets/python/simulation/multiagentes.py' primero");
            Debug.Log("📂 Fallback: usando escenario.json local");
            IniciarDesdeArchivoLocal("escenario");
            return;
        }
        
        try
        {
            string jsonData = System.IO.File.ReadAllText(rutaArchivo);
            Debug.Log($"✅ simulacion.json leído ({jsonData.Length} caracteres)");
            
            EscenarioData escenario = JSONLoader.ParsearJSON(jsonData);
            if (escenario != null)
            {
                Debug.Log("✅ JSON parseado correctamente desde multiagentes.py");
                ConstruirYSimular(escenario);
            }
            else
            {
                Debug.LogError("❌ Error parseando simulacion.json");
                IniciarDesdeArchivoLocal("escenario");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error leyendo simulacion.json: {ex.Message}");
            IniciarDesdeArchivoLocal("escenario");
        }
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
    
    [ContextMenu("🐍 Cargar Python Local (simulacion.json)")]
    public void CargarPythonLocalContext()
    {
        fuenteDatos = FuenteDatos.LocalPython;
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
