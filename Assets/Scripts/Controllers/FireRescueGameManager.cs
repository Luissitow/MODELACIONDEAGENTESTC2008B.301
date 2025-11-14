using UnityEngine;
using System.Collections;

public class FireRescueGameManager : MonoBehaviour
{
    [Header("Componentes")]
    public MapGenerator mapGenerator;
    
    [Header("Configuración")]
    public float updateInterval = 1.0f; // Intervalo de actualización en segundos
    public bool autoUpdate = true;
    public string mapJsonPath = "Assets/Data/map.json"; // Ruta del archivo JSON local
    
    [Header("Debug")]
    public bool useLocalFile = true; // Si usar archivo local en lugar de API
    public bool showDebugLogs = true;
    
    private GameMap currentMap;
    private bool isUpdating = false;
    
    // Propiedad pública para acceder al mapa actual desde la UI
    public GameMap CurrentMap => currentMap;
    
    void Start()
    {
        // Inicializar el sistema
        InitializeGame();
        
        // Comenzar la actualización automática si está habilitada
        if (autoUpdate)
        {
            StartCoroutine(UpdateLoop());
        }
    }
    
    // Inicializar el juego
    public void InitializeGame()
    {
        if (mapGenerator == null)
        {
            mapGenerator = FindObjectOfType<MapGenerator>();
            if (mapGenerator == null)
            {
                Debug.LogError("MapGenerator no encontrado en la escena!");
                return;
            }
        }
        
        // Cargar y generar el mapa inicial
        LoadAndGenerateMap();
    }
    
    // Cargar el mapa y generarlo
    public void LoadAndGenerateMap()
    {
        if (showDebugLogs)
            Debug.Log("Cargando mapa...");
            
        if (useLocalFile)
        {
            // Cargar desde archivo JSON local
            currentMap = MapAPIHelper.LoadMapFromFile(mapJsonPath);
        }
        else
        {
            // Cargar desde API del backend
            currentMap = MapAPIHelper.GetCurrentMap();
        }
        
        if (currentMap != null)
        {
            mapGenerator.GenerateMap(currentMap);
            if (showDebugLogs)
                Debug.Log($"Mapa generado: {currentMap.width}x{currentMap.height}");
        }
        else
        {
            Debug.LogError("No se pudo cargar el mapa!");
        }
    }
    
    // Loop de actualización automática
    private IEnumerator UpdateLoop()
    {
        while (autoUpdate)
        {
            yield return new WaitForSeconds(updateInterval);
            
            if (!isUpdating)
            {
                StartCoroutine(UpdateMapFromBackend());
            }
        }
    }
    
    // Actualizar el mapa desde el backend
    public IEnumerator UpdateMapFromBackend()
    {
        isUpdating = true;
        
        if (showDebugLogs)
            Debug.Log("Actualizando estado del mapa...");
        
        // Obtener movimientos de agentes
        AgentMovement[] movements = MapAPIHelper.GetAgentMovements();
        
        if (movements.Length > 0)
        {
            ProcessAgentMovements(movements);
        }
        
        // Obtener estado actualizado del mapa si no usamos archivo local
        if (!useLocalFile)
        {
            GameMap updatedMap = MapAPIHelper.GetCurrentMap();
            if (updatedMap != null)
            {
                UpdateMapDifferences(updatedMap);
                currentMap = updatedMap;
            }
        }
        else
        {
            // Recargar desde archivo local para simular actualizaciones
            GameMap fileMap = MapAPIHelper.LoadMapFromFile(mapJsonPath);
            if (fileMap != null && !CompareMaps(currentMap, fileMap))
            {
                UpdateMapDifferences(fileMap);
                currentMap = fileMap;
            }
        }
        
        isUpdating = false;
        yield return null;
    }
    
    // Procesar movimientos de agentes
    private void ProcessAgentMovements(AgentMovement[] movements)
    {
        foreach (var movement in movements)
        {
            if (showDebugLogs)
                Debug.Log($"Agente {movement.agentId}: {movement.action} de {movement.from.x},{movement.from.y} a {movement.to.x},{movement.to.y}");
            
            // Remover agente de posición anterior
            mapGenerator.UpdateElement(movement.from, CellType.Floor);
            
            // Colocar agente en nueva posición
            mapGenerator.UpdateElement(movement.to, CellType.Agent);
            
            // Procesar acciones especiales
            ProcessAgentAction(movement);
        }
        
        // Actualizar posiciones de agentes en el mapa actual
        UpdateAgentPositions(movements);
    }
    
    // Procesar acciones especiales de agentes
    private void ProcessAgentAction(AgentMovement movement)
    {
        switch (movement.action.ToLower())
        {
            case "rescue":
                // Remover víctima si el agente está en su posición
                mapGenerator.UpdateElement(movement.to, CellType.Agent);
                RemoveVictimAt(movement.to);
                if (showDebugLogs)
                    Debug.Log($"Víctima rescatada en {movement.to.x},{movement.to.y}");
                break;
                
            case "extinguish":
                // Extinguir fuego si el agente está adyacente
                mapGenerator.UpdateElement(movement.to, CellType.Floor);
                RemoveFireAt(movement.to);
                if (showDebugLogs)
                    Debug.Log($"Fuego extinguido en {movement.to.x},{movement.to.y}");
                break;
                
            case "explosion":
                // Crear explosión
                mapGenerator.UpdateElement(movement.to, CellType.Explosion);
                AddExplosionAt(movement.to);
                if (showDebugLogs)
                    Debug.Log($"Explosión en {movement.to.x},{movement.to.y}");
                break;
        }
    }
    
    // Actualizar diferencias entre mapas
    private void UpdateMapDifferences(GameMap newMap)
    {
        if (currentMap == null) return;
        
        // Comparar y actualizar víctimas (con conversión de VictimInfo a Vector2Int)
        Vector2Int[] oldVictimPositions = System.Array.ConvertAll(currentMap.victims, v => new Vector2Int(v.x, v.y));
        Vector2Int[] newVictimPositions = System.Array.ConvertAll(newMap.victims, v => new Vector2Int(v.x, v.y));
        CompareAndUpdateElements(oldVictimPositions, newVictimPositions, CellType.Victim, "Víctimas");
        
        // Comparar y actualizar fuegos
        CompareAndUpdateElements(currentMap.fires, newMap.fires, CellType.Fire, "Fuegos");
        
        // Comparar y actualizar agentes
        CompareAndUpdateElements(currentMap.agents, newMap.agents, CellType.Agent, "Agentes");
        
        // Comparar y actualizar explosiones
        CompareAndUpdateElements(currentMap.explosions, newMap.explosions, CellType.Explosion, "Explosiones");
    }
    
    // Comparar y actualizar elementos específicos
    private void CompareAndUpdateElements(Vector2Int[] oldElements, Vector2Int[] newElements, CellType elementType, string elementName)
    {
        // Remover elementos que ya no existen
        foreach (var oldElement in oldElements)
        {
            bool stillExists = false;
            foreach (var newElement in newElements)
            {
                if (oldElement.x == newElement.x && oldElement.y == newElement.y)
                {
                    stillExists = true;
                    break;
                }
            }
            
            if (!stillExists)
            {
                mapGenerator.UpdateElement(oldElement, CellType.Floor);
                if (showDebugLogs)
                    Debug.Log($"{elementName} removido en {oldElement.x},{oldElement.y}");
            }
        }
        
        // Agregar nuevos elementos
        foreach (var newElement in newElements)
        {
            bool isNew = true;
            foreach (var oldElement in oldElements)
            {
                if (oldElement.x == newElement.x && oldElement.y == newElement.y)
                {
                    isNew = false;
                    break;
                }
            }
            
            if (isNew)
            {
                mapGenerator.UpdateElement(newElement, elementType);
                if (showDebugLogs)
                    Debug.Log($"{elementName} agregado en {newElement.x},{newElement.y}");
            }
        }
    }
    
    // Comparar dos mapas para ver si son diferentes
    private bool CompareMaps(GameMap map1, GameMap map2)
    {
        if (map1 == null || map2 == null) return false;
        if (map1.width != map2.width || map1.height != map2.height) return false;
        
        // Comparar elementos (simplificado)
        return map1.victims.Length == map2.victims.Length &&
               map1.fires.Length == map2.fires.Length &&
               map1.agents.Length == map2.agents.Length &&
               map1.explosions.Length == map2.explosions.Length;
    }
    
    // Métodos auxiliares para manejar elementos específicos
    private void UpdateAgentPositions(AgentMovement[] movements)
    {
        // Actualizar posiciones de agentes en currentMap
        foreach (var movement in movements)
        {
            for (int i = 0; i < currentMap.agents.Length; i++)
            {
                if (currentMap.agents[i].x == movement.from.x && 
                    currentMap.agents[i].y == movement.from.y)
                {
                    currentMap.agents[i] = movement.to;
                    break;
                }
            }
        }
    }
    
    private void RemoveVictimAt(Vector2Int position)
    {
        var victimsList = new System.Collections.Generic.List<VictimInfo>(currentMap.victims);
        victimsList.RemoveAll(v => v.x == position.x && v.y == position.y);
        currentMap.victims = victimsList.ToArray();
    }
    
    private void RemoveFireAt(Vector2Int position)
    {
        var firesList = new System.Collections.Generic.List<Vector2Int>(currentMap.fires);
        firesList.RemoveAll(f => f.x == position.x && f.y == position.y);
        currentMap.fires = firesList.ToArray();
    }
    
    private void AddExplosionAt(Vector2Int position)
    {
        var explosionsList = new System.Collections.Generic.List<Vector2Int>(currentMap.explosions);
        explosionsList.Add(position);
        currentMap.explosions = explosionsList.ToArray();
    }
    
    // Métodos públicos para control manual
    public void ForceUpdate()
    {
        StartCoroutine(UpdateMapFromBackend());
    }
    
    public void ToggleAutoUpdate()
    {
        autoUpdate = !autoUpdate;
        if (autoUpdate)
        {
            StartCoroutine(UpdateLoop());
        }
    }
    
    public void SaveCurrentMapToFile()
    {
        if (currentMap != null)
        {
            MapAPIHelper.SaveMapToFile(currentMap, mapJsonPath);
            Debug.Log($"Mapa guardado en {mapJsonPath}");
        }
    }
}