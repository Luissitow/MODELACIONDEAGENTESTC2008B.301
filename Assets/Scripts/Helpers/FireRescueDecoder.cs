using UnityEngine;

public static class FireRescueDecoder
{
    // Decodificar matriz binaria exactamente como Fire Rescue
    public static GameMap DecodeFireRescueFormat(string[] binaryRows, string[] elements)
    {
        GameMap map = new GameMap();
        map.width = binaryRows[0].Split(' ').Length;
        map.height = binaryRows.Length;
        
        // Inicializar matriz de paredes
        map.walls = new int[map.height][];
        for (int i = 0; i < map.height; i++)
        {
            string[] rowValues = binaryRows[i].Split(' ');
            map.walls[i] = new int[map.width];
            
            for (int j = 0; j < map.width; j++)
            {
                // Convertir binario a decimal
                map.walls[i][j] = System.Convert.ToInt32(rowValues[j], 2);
            }
        }
        
        // Decodificar elementos especiales
        DecodeElements(map, elements);
        
        return map;
    }
    
    // Decodificar elementos especiales (víctimas, fuegos, agentes, puertas)
    private static void DecodeElements(GameMap map, string[] elements)
    {
        var victims = new System.Collections.Generic.List<Vector2Int>();
        var fires = new System.Collections.Generic.List<Vector2Int>();
        var agents = new System.Collections.Generic.List<Vector2Int>();
        var doors = new System.Collections.Generic.List<Vector2Int>();
        
        foreach (string element in elements)
        {
            string[] parts = element.Trim().Split(' ');
            
            if (parts.Length >= 3 && parts[2] == "v")
            {
                // Víctima: "2 4 v"
                victims.Add(new Vector2Int(int.Parse(parts[0]), int.Parse(parts[1])));
            }
            else if (parts.Length >= 3 && parts[2] == "f")
            {
                // Fuego: "5 1 f"
                fires.Add(new Vector2Int(int.Parse(parts[0]), int.Parse(parts[1])));
            }
            else if (parts.Length == 2)
            {
                // Agente o puerta: "2 2" o "1 6"
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                
                // Determinar si es puerta o agente basado en posición
                // (Este es un ejemplo, ajustar según tu lógica específica)
                if (IsAtMapBorder(x, y, map.width, map.height))
                {
                    doors.Add(new Vector2Int(x, y));
                }
                else
                {
                    agents.Add(new Vector2Int(x, y));
                }
            }
        }
        
        map.victims = victims.ToArray();
        map.fires = fires.ToArray();
        map.agents = agents.ToArray();
        map.doors = doors.ToArray();
    }
    
    // Verificar si una posición está en el borde del mapa (probable puerta)
    private static bool IsAtMapBorder(int x, int y, int width, int height)
    {
        return x == 0 || x == width - 1 || y == 0 || y == height - 1;
    }
    
    // Generar IDs únicos para paredes como Fire Rescue
    public static WallInfo[] GenerateWallIds(GameMap map)
    {
        var wallsList = new System.Collections.Generic.List<WallInfo>();
        
        for (int y = 0; y < map.height; y++)
        {
            for (int x = 0; x < map.width; x++)
            {
                int wallValue = map.walls[y][x];
                
                // Verificar cada dirección de pared
                for (int direction = 0; direction < 4; direction++)
                {
                    if ((wallValue & (1 << direction)) != 0)
                    {
                        WallInfo wallInfo = new WallInfo
                        {
                            wallId = CalculateUniqueWallId(x, y, direction, map.width),
                            gridPosition = new Vector2Int(x, y),
                            direction = (WallDirection)direction,
                            bitPosition = direction,
                            state = WallState.Normal,
                            health = 100,
                            maxHealth = 100
                        };
                        
                        wallsList.Add(wallInfo);
                    }
                }
            }
        }
        
        return wallsList.ToArray();
    }
    
    // Calcular ID único para pared (como en Fire Rescue)
    private static int CalculateUniqueWallId(int x, int y, int direction, int mapWidth)
    {
        // Fórmula Fire Rescue: (fila * ancho * 4) + (columna * 4) + dirección
        return (y * mapWidth * 4) + (x * 4) + direction + 1000; // +1000 para evitar conflictos
    }
    
    // Obtener todas las paredes que deberían existir en una celda específica
    public static bool[] GetWallsForCell(int binaryValue)
    {
        bool[] walls = new bool[4];
        
        walls[0] = (binaryValue & (1 << 0)) != 0; // Norte
        walls[1] = (binaryValue & (1 << 1)) != 0; // Este  
        walls[2] = (binaryValue & (1 << 2)) != 0; // Sur
        walls[3] = (binaryValue & (1 << 3)) != 0; // Oeste
        
        return walls;
    }
    
    // Debug: Mostrar información de una celda
    public static string DebugCellInfo(int x, int y, int binaryValue)
    {
        bool[] walls = GetWallsForCell(binaryValue);
        
        string info = $"Celda ({x}, {y}) - Valor: {binaryValue} (binario: {System.Convert.ToString(binaryValue, 2).PadLeft(4, '0')})\n";
        info += $"Norte: {(walls[0] ? "✅" : "❌")} ";
        info += $"Este: {(walls[1] ? "✅" : "❌")} ";
        info += $"Sur: {(walls[2] ? "✅" : "❌")} ";
        info += $"Oeste: {(walls[3] ? "✅" : "❌")}";
        
        return info;
    }
}