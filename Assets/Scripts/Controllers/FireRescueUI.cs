using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FireRescueUI : MonoBehaviour
{
    [Header("Text Components")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI victimsText;
    public TextMeshProUGUI firesText;
    public TextMeshProUGUI agentsText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;
    
    [Header("Buttons")]
    public Button updateMapButton;
    public Button toggleAutoUpdateButton;
    public Button saveMapButton;
    
    [Header("Game Manager")]
    public FireRescueGameManager gameManager;
    
    private float gameTime;
    private int totalScore;
    private bool isAutoUpdating = true;
    
    void Start()
    {
        // Configurar botones siguiendo el patrón del laboratorio Chuck Norris
        if (updateMapButton != null)
            updateMapButton.onClick.AddListener(UpdateMapFromAPI);
            
        if (toggleAutoUpdateButton != null)
            toggleAutoUpdateButton.onClick.AddListener(ToggleAutoUpdate);
            
        if (saveMapButton != null)
            saveMapButton.onClick.AddListener(SaveCurrentMap);
        
        // Encontrar el Game Manager si no está asignado
        if (gameManager == null)
            gameManager = FindObjectOfType<FireRescueGameManager>();
        
        // Inicializar textos
        InitializeUI();
        
        // Actualizar UI cada segundo
        InvokeRepeating(nameof(UpdateUI), 1f, 1f);
    }
    
    void Update()
    {
        // Actualizar el tiempo de juego
        gameTime += Time.deltaTime;
    }
    
    // Inicializar la interfaz
    private void InitializeUI()
    {
        if (statusText != null)
            statusText.text = "Estado: Iniciando...";
            
        if (victimsText != null)
            victimsText.text = "Víctimas: --";
            
        if (firesText != null)
            firesText.text = "Fuegos: --";
            
        if (agentsText != null)
            agentsText.text = "Agentes: --";
            
        if (timeText != null)
            timeText.text = "Tiempo: 00:00";
            
        if (scoreText != null)
            scoreText.text = "Puntuación: 0";
    }
    
    // Actualizar la interfaz con información actual
    public void UpdateUI()
    {
        if (gameManager == null || gameManager.CurrentMap == null)
        {
            if (statusText != null)
                statusText.text = "Estado: Sin mapa cargado";
            return;
        }
        
        var currentMap = gameManager.CurrentMap;
        
        // Actualizar estado
        if (statusText != null)
        {
            string status = isAutoUpdating ? "Actualizando automáticamente" : "Actualización manual";
            statusText.text = $"Estado: {status}";
        }
        
        // Actualizar contadores de elementos
        if (victimsText != null)
        {
            int victimsCount = currentMap.victims != null ? currentMap.victims.Length : 0;
            victimsText.text = $"Víctimas: {victimsCount}";
        }
        
        if (firesText != null)
        {
            int firesCount = currentMap.fires != null ? currentMap.fires.Length : 0;
            firesText.text = $"Fuegos: {firesCount}";
        }
        
        if (agentsText != null)
        {
            int agentsCount = currentMap.agents != null ? currentMap.agents.Length : 0;
            agentsText.text = $"Agentes: {agentsCount}";
        }
        
        // Actualizar tiempo
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            timeText.text = $"Tiempo: {minutes:00}:{seconds:00}";
        }
        
        // Actualizar puntuación
        if (scoreText != null)
        {
            CalculateScore(currentMap);
            scoreText.text = $"Puntuación: {totalScore}";
        }
    }
    
    // Función para el botón de actualizar mapa (siguiendo patrón Chuck Norris)
    public void UpdateMapFromAPI()
    {
        if (gameManager != null)
        {
            if (statusText != null)
                statusText.text = "Estado: Actualizando desde API...";
            
            gameManager.ForceUpdate();
        }
    }
    
    // Función para alternar actualización automática
    public void ToggleAutoUpdate()
    {
        isAutoUpdating = !isAutoUpdating;
        
        if (gameManager != null)
            gameManager.ToggleAutoUpdate();
        
        // Actualizar texto del botón
        if (toggleAutoUpdateButton != null)
        {
            TextMeshProUGUI buttonText = toggleAutoUpdateButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isAutoUpdating ? "Pausar Auto-Update" : "Iniciar Auto-Update";
            }
        }
    }
    
    // Función para guardar mapa
    public void SaveCurrentMap()
    {
        if (gameManager != null)
        {
            gameManager.SaveCurrentMapToFile();
            
            if (statusText != null)
            {
                statusText.text = "Estado: Mapa guardado exitosamente";
                // Restaurar estado después de 2 segundos
                Invoke(nameof(RestoreStatusText), 2f);
            }
        }
    }
    
    private void RestoreStatusText()
    {
        if (statusText != null)
        {
            string status = isAutoUpdating ? "Actualizando automáticamente" : "Actualización manual";
            statusText.text = $"Estado: {status}";
        }
    }
    
    // Calcular puntuación basada en objetivos de rescate
    private void CalculateScore(GameMap map)
    {
        totalScore = 0;
        
        // Puntos por víctimas rescatadas (comparar con mapa inicial)
        // Este es un ejemplo simple, puedes ajustar la lógica
        int initialVictims = 10; // Número inicial de víctimas (configurar según tu juego)
        int currentVictims = map.victims != null ? map.victims.Length : 0;
        int victimsRescued = Mathf.Max(0, initialVictims - currentVictims);
        totalScore += victimsRescued * 100;
        
        // Puntos por fuegos extinguidos
        int initialFires = 8; // Número inicial de fuegos (configurar según tu juego)
        int currentFires = map.fires != null ? map.fires.Length : 0;
        int firesExtinguished = Mathf.Max(0, initialFires - currentFires);
        totalScore += firesExtinguished * 50;
        
        // Penalización por tiempo
        int timePenalty = Mathf.FloorToInt(gameTime / 10); // -1 punto cada 10 segundos
        totalScore = Mathf.Max(0, totalScore - timePenalty);
    }
    
    // Mostrar información de un elemento específico (para debug)
    public void ShowElementInfo(Vector2Int position, string elementType)
    {
        if (statusText != null)
        {
            statusText.text = $"Estado: {elementType} en ({position.x}, {position.y})";
            Invoke(nameof(RestoreStatusText), 3f);
        }
    }
    
    // Mostrar mensaje temporal
    public void ShowMessage(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"Estado: {message}";
            Invoke(nameof(RestoreStatusText), 2f);
        }
    }
    
    void OnDestroy()
    {
        // Limpiar listeners
        if (updateMapButton != null)
            updateMapButton.onClick.RemoveListener(UpdateMapFromAPI);
            
        if (toggleAutoUpdateButton != null)
            toggleAutoUpdateButton.onClick.RemoveListener(ToggleAutoUpdate);
            
        if (saveMapButton != null)
            saveMapButton.onClick.RemoveListener(SaveCurrentMap);
    }
}