using UnityEngine;

/// <summary>
/// Controlador principal del juego
/// Coordina la carga del escenario y la construcción del tablero

/// </summary>
public class ControladorJuego : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ConstructorTablero constructorTablero;
    
    [Header("Configuración")]
    [SerializeField] private string nombreArchivoJSON = "escenario";
    
    private EscenarioData datosEscenario;
    
    void Start()
    {
        // Si no está asignado, buscar en la escena
        if (constructorTablero == null)
        {
            constructorTablero = FindFirstObjectByType<ConstructorTablero>();
            
            if (constructorTablero == null)
            {
                Debug.LogError("❌ No se encontró ConstructorTablero en la escena. Asegúrate de tener un GameObject con ese componente.");
                return;
            }
            
            Debug.Log("✅ ConstructorTablero encontrado automáticamente");
        }
        
        CargarEscenario();
        ConstruirTablero();
    }
    


    /// <summary>
    /// Carga el archivo JSON del escenario desde Resources
    /// </summary>
    /// 
    void CargarEscenario()
    {
        TextAsset archivoJSON = Resources.Load<TextAsset>(nombreArchivoJSON);
        
        if (archivoJSON == null)
        {
            Debug.LogError($"No se encontró el archivo {nombreArchivoJSON}.json en Resources/");
            return;
        }
        
        datosEscenario = JsonUtility.FromJson<EscenarioData>(archivoJSON.text);
        Debug.Log($"Escenario cargado: {datosEscenario.fila}x{datosEscenario.columna} celdas");
    }
    
    /// <summary>
    /// Construye el tablero 3D usando los datos cargados
    /// </summary>
    void ConstruirTablero()
    {
        if (datosEscenario == null)
        {
            Debug.LogError("No se puede construir el tablero sin datos del escenario");
            return;
        }
        
        if (constructorTablero == null)
        {
            Debug.LogError("Falta asignar el ConstructorTablero en el Inspector");
            return;
        }
        
        constructorTablero.ConstruirMapa(datosEscenario);
        Debug.Log("Tablero construido exitosamente");
    }
}
