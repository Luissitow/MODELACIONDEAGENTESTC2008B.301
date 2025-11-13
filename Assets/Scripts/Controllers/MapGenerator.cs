using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Prefabs de Elementos")]
    public GameObject floorPrefab;          // Prefab del piso (separado)
    public GameObject roomPrefab;           // Tu prefab "CuartoSinTecho" (solo 4 paredes)
    public GameObject victimPrefab;
    public GameObject firePrefab;
    public GameObject agentPrefab;
    public GameObject doorPrefab;
    public GameObject explosionPrefab;
    
    [Header("Configuración")]
    public float cellSize = 1.0f;
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
        for (int y = 0; y < currentMap.height; y++)
        {
            for (int x = 0; x < currentMap.width; x++)
            {
                int wallValue = currentMap.walls[y][x];
                Vector3 basePosition = new Vector3(x * cellSize, 0, y * cellSize);
                
                // Si hay alguna pared en esta celda, generar el cuarto completo
                if (wallValue > 0)
                {
                    GenerateRoomAtPosition(x, y, wallValue, basePosition);
                }
            }
        }
    }
    
    // Generar cuarto completo y activar solo las paredes necesarias
    private void GenerateRoomAtPosition(int x, int y, int wallValue, Vector3 basePosition)
    {
        if (roomPrefab == null) return;
        
        // Instanciar el cuarto completo
        GameObject room = Instantiate(roomPrefab, basePosition, Quaternion.identity, mapParent);
        room.name = $"Room_{x}_{y}";
        
        // Buscar las 4 paredes hijas y configurarlas
        WallElement[] wallElements = room.GetComponentsInChildren<WallElement>();
        
        foreach (WallElement wallElement in wallElements)
        {
            // Calcular ID único para esta pared
            int wallId = CalculateWallId(x, y, (int)wallElement.direction);
            
            // Configurar propiedades de la pared
            wallElement.wallId = wallId;
            wallElement.gridPosition = new Vector2Int(x, y);
            wallElement.bitPosition = (int)wallElement.direction;
            
            // Verificar si esta pared debe estar activa según el valor binario
            bool shouldBeActive = (wallValue & (1 << (int)wallElement.direction)) != 0;
            
            // Activar/desactivar la pared según corresponde
            wallElement.gameObject.SetActive(shouldBeActive);
            
            if (shouldBeActive)
            {
                // Registrar pared activa
                string wallKey = $"Wall_{x}_{y}_{wallElement.direction}";
                wallObjects[wallKey] = wallElement.gameObject;
                
                Debug.Log($"Pared activa: {wallElement.direction} en ({x},{y}) - ID: {wallId}");
            }
        }
    }
    
    // Generar una pared individual en una dirección específica
    private void GenerateWall(int x, int y, WallDirection direction, Vector3 basePosition)
    {
        Vector3 wallPosition = basePosition;
        Quaternion wallRotation = Quaternion.identity;
        GameObject wallPrefabToUse = null;
        
        switch (direction)
        {
            case WallDirection.North: // Arriba
                wallPosition += new Vector3(0, 0, cellSize * 0.5f);
                wallRotation = Quaternion.Euler(0, 0, 0);
                wallPrefabToUse = wallNorthPrefab;
                break;
            case WallDirection.East: // Derecha
                wallPosition += new Vector3(cellSize * 0.5f, 0, 0);
                wallRotation = Quaternion.Euler(0, 90, 0);
                wallPrefabToUse = wallEastPrefab;
                break;
            case WallDirection.South: // Abajo
                wallPosition += new Vector3(0, 0, -cellSize * 0.5f);
                wallRotation = Quaternion.Euler(0, 180, 0);
                wallPrefabToUse = wallSouthPrefab;
                break;
            case WallDirection.West: // Izquierda
                wallPosition += new Vector3(-cellSize * 0.5f, 0, 0);
                wallRotation = Quaternion.Euler(0, 270, 0);
                wallPrefabToUse = wallWestPrefab;
                break;
        }
        
        if (wallPrefabToUse != null)
        {
            string wallKey = $"Wall_{x}_{y}_{direction}";
            GameObject wall = Instantiate(wallPrefabToUse, wallPosition, wallRotation, mapParent);
            wall.name = wallKey;
            
            // Configurar el WallElement con el ID correcto
            WallElement wallElement = wall.GetComponent<WallElement>();
            if (wallElement != null)
            {
                wallElement.wallId = CalculateWallId(x, y, (int)direction);
                wallElement.gridPosition = new Vector2Int(x, y);
                wallElement.direction = direction;
                wallElement.bitPosition = (int)direction;
            }
            
            wallObjects[wallKey] = wall;
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
        // Generar víctimas
        foreach (var victim in currentMap.victims)
        {
            SpawnElement(victim, victimPrefab, "Victim");
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
        
        // Generar puertas
        foreach (var door in currentMap.doors)
        {
            SpawnElement(door, doorPrefab, "Door");
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
        
        Vector3 worldPos = new Vector3(gridPos.x * cellSize, 0.1f, gridPos.y * cellSize);
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