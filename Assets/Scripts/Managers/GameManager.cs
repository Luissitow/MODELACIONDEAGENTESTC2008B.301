using UnityEngine;

/// <summary>
/// Manager principal que orquesta la carga y simulación del juego
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Referencias")]
    public TableroBuilder tableroBuilder;
    public SimulacionRunner simulacionRunner;
    
    [Header("Configuración")]
    public bool iniciarAutomaticamente = true;
    
    void Start()
    {
        if (iniciarAutomaticamente)
        {
            IniciarJuego();
        }
    }
    
    public void IniciarJuego()
    {
        // 1. Cargar JSON
        EscenarioData escenario = JSONLoader.CargarEscenario();
        
        if (escenario == null)
        {
            Debug.LogError("❌ No se pudo cargar el escenario");
            return;
        }
        
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
