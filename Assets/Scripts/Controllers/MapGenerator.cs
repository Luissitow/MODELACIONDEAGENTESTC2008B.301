using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Prefabs de Elementos")]
    public GameObject floorPrefab;          // Prefab del piso
    public GameObject wallNorthPrefab;      // Prefab pared Norte
    public GameObject wallEastPrefab;       // Prefab pared Este
    public GameObject wallSouthPrefab;      // Prefab pared Sur
    public GameObject wallWestPrefab;       // Prefab pared Oeste
    public GameObject victimPrefab;
    public GameObject firePrefab;
    public GameObject agentPrefab;
    public GameObject doorPrefab;
    public GameObject explosionPrefab;
    
    [Header("Configuración")]
    public float cellSize = 2.0f;
    public Transform mapParent;
    
    private GameMap currentMap;
    private Dictionary<Vector2Int, GameObject> spawnedObjects;
    private Dictionary<string, GameObject> wallObjects;
    private Dictionary<int, WallElement> registeredWalls; // Nuevas paredes registradas
    
    void Start()
    {
        spawnedObjects = new Dictionary<Vector2Int, GameObject>();
        wallObjects = new Dictionary<string, GameObject>();
        registeredWalls = new Dictionary<int, WallElement>();
        
        if (mapParent == null)
        {
            GameObject mapContainer = new GameObject("Map");
            mapParent = mapContainer.transform;
        }
    }
    
    // Generar el mapa completo desde una instancia de GameMap
    public void GenerateMap(GameMap map)
    {
        currentMap = map;
        ClearCurrentMap();
        
        // Generar suelo base
        GenerateFloor();
        
        // Generar paredes basadas en la matriz binaria
        GenerateWalls();
        
        // Generar elementos especiales
        GenerateElements();
    }
    
    // Limpiar el mapa actual
    public void ClearCurrentMap()
    {
        foreach (var obj in spawnedObjects.Values)
        {
            if (obj != null)
                DestroyImmediate(obj);
        }
        
        foreach (var wall in wallObjects.Values)
        {
            if (wall != null)
                DestroyImmediate(wall);
        }
        
        spawnedObjects.Clear();
        wallObjects.Clear();
    }
    
    // Generar el suelo base del mapa
    private void GenerateFloor()
    {
        for (int y = 0; y < currentMap.height; y++)
        {
            for (int x = 0; x < currentMap.width; x++)
            {
                Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);
                GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity, mapParent);
                floor.name = $"Floor_{x}_{y}";
            }
        }
    }
    
    // Generar paredes basadas en la matriz binaria
    private void GenerateWalls()
    {
        // Validar que exista la matriz de paredes
        if (currentMap.walls == null)
        {
            Debug.LogError("¡La matriz de paredes (walls) es null!");
            return;
        }
        
        for (int y = 0; y < currentMap.height; y++)
        {
            // Validar que la fila exista
            if (currentMap.walls[y] == null)
            {
                Debug.LogError($"La fila {y} de la matriz de paredes es null!");
                continue;
            }
            
            for (int x = 0; x < currentMap.width; x++)
            {
                int wallValue = currentMap.walls[y][x];
                Vector3 basePosition = new Vector3(x * cellSize, 0, y * cellSize);
                
                // Generar paredes individuales según el valor binario
                if (wallValue > 0)
                {
                    GenerateWallsAtPosition(x, y, wallValue, basePosition);
                }
            }
        }
    }
    
    // Generar paredes individuales en una celda
    private void GenerateWallsAtPosition(int x, int y, int wallValue, Vector3 basePosition)
    {
        // Verificar cada dirección (bits 0-3)
        GenerateWallIfNeeded(x, y, wallValue, WallDirection.North, wallNorthPrefab, basePosition);
        GenerateWallIfNeeded(x, y, wallValue, WallDirection.East, wallEastPrefab, basePosition);
        GenerateWallIfNeeded(x, y, wallValue, WallDirection.South, wallSouthPrefab, basePosition);
        GenerateWallIfNeeded(x, y, wallValue, WallDirection.West, wallWestPrefab, basePosition);
    }
    
    // Generar una pared si está activa en el binario
    private void GenerateWallIfNeeded(int x, int y, int wallValue, WallDirection direction, GameObject wallPrefab, Vector3 basePosition)
    {
        // Verificar si esta dirección está activa (bit = 1)
        bool shouldExist = (wallValue & (1 << (int)direction)) != 0;
        
        if (!shouldExist || wallPrefab == null)
            return;
        
        // Instanciar la pared
        GameObject wall = Instantiate(wallPrefab, basePosition, Quaternion.identity, mapParent);
        string wallKey = $"Wall_{x}_{y}_{direction}";
        wall.name = wallKey;
        
        // Configurar el WallElement
        WallElement wallElement = wall.GetComponent<WallElement>();
        if (wallElement != null)
        {
            int wallId = CalculateWallId(x, y, (int)direction);
            wallElement.wallId = wallId;
            wallElement.gridPosition = new Vector2Int(x, y);
            wallElement.direction = direction;
            wallElement.bitPosition = (int)direction;
            
            // Registrar la pared
            registeredWalls[wallId] = wallElement;
            wallObjects[wallKey] = wall;
            
            Debug.Log($"Pared generada: {direction} en ({x},{y}) - ID: {wallId}");
        }
        else
        {
            Debug.LogWarning($"Pared {wallKey} no tiene componente WallElement!");
        }
    }
    
    // Calcular ID único de pared usando la fórmula de Fire Rescue
    private int CalculateWallId(int x, int y, int direction)
    {
        return (y * currentMap.width * 4) + (x * 4) + direction + 1000;
    }
    
    // Generar elementos especiales (víctimas, fuego, agentes, etc.)
    private void GenerateElements()
    {
        // Generar víctimas (con información de falsa alarma)
        if (victimPrefab != null)
        {
            foreach (var victim in currentMap.victims)
            {
                Vector3 position = new Vector3(victim.x * cellSize, 0.5f, victim.y * cellSize);
                GameObject victimObj = Instantiate(victimPrefab, position, Quaternion.identity, mapParent);
                victimObj.name = victim.isFake ? $"FalseAlarm_{victim.x}_{victim.y}" : $"Victim_{victim.x}_{victim.y}";
                spawnedObjects[new Vector2Int(victim.x, victim.y)] = victimObj;
            }
        }
        
        // Generar fuego
        foreach (var fire in currentMap.fires)
        {
            SpawnElement(fire, firePrefab, "Fire");
        }
        
        // Generar agentes
        foreach (var agent in currentMap.agents)
        {
            SpawnElement(agent, agentPrefab, "Agent");
        }
        
        // Generar puertas (entre dos celdas)
        if (doorPrefab != null)
        {
            foreach (var door in currentMap.doors)
            {
                // Calcular posición entre las dos celdas
                Vector3 pos1 = new Vector3(door.cell1.x * cellSize, 0.5f, door.cell1.y * cellSize);
                Vector3 pos2 = new Vector3(door.cell2.x * cellSize, 0.5f, door.cell2.y * cellSize);
                Vector3 doorPosition = (pos1 + pos2) / 2f;
                
                GameObject doorObj = Instantiate(doorPrefab, doorPosition, Quaternion.identity, mapParent);
                doorObj.name = $"Door_{door.cell1.x}_{door.cell1.y}_to_{door.cell2.x}_{door.cell2.y}";
            }
        }
        
        // Generar explosiones
        foreach (var explosion in currentMap.explosions)
        {
            SpawnElement(explosion, explosionPrefab, "Explosion");
        }
    }
    
    // Instanciar un elemento específico en una posición
    private void SpawnElement(Vector2Int gridPos, GameObject prefab, string elementType)
    {
        if (prefab == null) return;
        
        Vector3 worldPos = new Vector3(gridPos.x * cellSize, 0.5f, gridPos.y * cellSize);
        GameObject element = Instantiate(prefab, worldPos, Quaternion.identity, mapParent);
        element.name = $"{elementType}_{gridPos.x}_{gridPos.y}";
        
        spawnedObjects[gridPos] = element;
    }
    
    // Actualizar un elemento específico en el mapa
    public void UpdateElement(Vector2Int position, CellType newType)
    {
        // Remover elemento existente si existe
        if (spawnedObjects.ContainsKey(position))
        {
            DestroyImmediate(spawnedObjects[position]);
            spawnedObjects.Remove(position);
        }
        
        // Instanciar nuevo elemento basado en el tipo
        GameObject prefabToSpawn = GetPrefabForType(newType);
        if (prefabToSpawn != null)
        {
            SpawnElement(position, prefabToSpawn, newType.ToString());
        }
    }
    
    // Obtener el prefab correspondiente a un tipo de celda
    private GameObject GetPrefabForType(CellType type)
    {
        switch (type)
        {
            case CellType.Victim: return victimPrefab;
            case CellType.Fire: return firePrefab;
            case CellType.Agent: return agentPrefab;
            case CellType.Door: return doorPrefab;
            case CellType.Explosion: return explosionPrefab;
            case CellType.Floor: return null; // El suelo ya está generado
            default: return null;
        }
    }
    
    // Convertir posición del mundo a coordenadas de grid
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / cellSize);
        int z = Mathf.RoundToInt(worldPosition.z / cellSize);
        return new Vector2Int(x, z);
    }
    
    // Convertir coordenadas de grid a posición del mundo
    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * cellSize, 0, gridPosition.y * cellSize);
    }
    
    // Verificar si una posición es válida y caminable
    public bool IsWalkable(Vector2Int gridPosition)
    {
        if (gridPosition.x < 0 || gridPosition.x >= currentMap.width ||
            gridPosition.y < 0 || gridPosition.y >= currentMap.height)
            return false;
            
        CellType cellType = currentMap.GetCellType(gridPosition.x, gridPosition.y);
        return cellType == CellType.Floor || cellType == CellType.Door || cellType == CellType.Agent;
    }
    
    // Registrar una pared individual para control por ID
    public void RegisterWallElement(WallElement wallElement)
    {
        if (wallElement != null && wallElement.wallId != 0)
        {
            registeredWalls[wallElement.wallId] = wallElement;
            Debug.Log($"Pared registrada: ID {wallElement.wallId} en posición {wallElement.gridPosition}");
        }
    }
    
    // Actualizar una pared específica por ID
    public void UpdateWallById(int wallId, WallState newState)
    {
        if (registeredWalls.ContainsKey(wallId))
        {
            registeredWalls[wallId].SetWallState(newState);
            Debug.Log($"Pared {wallId} actualizada a estado: {newState}");
        }
    }
    
    // Dañar una pared específica por ID
    public void DamageWallById(int wallId, int damage)
    {
        if (registeredWalls.ContainsKey(wallId))
        {
            registeredWalls[wallId].DamageWall(damage);
            Debug.Log($"Pared {wallId} dañada con {damage} puntos");
        }
    }
    
    // Obtener información de todas las paredes registradas
    public WallInfo[] GetAllWallsInfo()
    {
        WallInfo[] wallsInfo = new WallInfo[registeredWalls.Count];
        int index = 0;
        
        foreach (var wall in registeredWalls.Values)
        {
            wallsInfo[index] = wall.GetWallInfo();
            index++;
        }
        
        return wallsInfo;
    }
    
    // Aplicar cambios a todas las paredes desde JSON
    public void ApplyWallsFromJson(WallInfo[] wallsInfo)
    {
        foreach (var wallInfo in wallsInfo)
        {
            if (registeredWalls.ContainsKey(wallInfo.wallId))
            {
                registeredWalls[wallInfo.wallId].ApplyWallInfo(wallInfo);
            }
        }
    }
}