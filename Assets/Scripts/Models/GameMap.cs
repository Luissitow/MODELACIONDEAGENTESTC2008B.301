using System;

[System.Serializable]
public class GameMap
{
    public int width;
    public int height;
    public int[][] walls;           // Matriz de paredes en binario
    public VictimInfo[] victims;    // Víctimas con información de falsa alarma
    public Vector2Int[] fires;      // Posiciones de fuegos
    public DoorInfo[] doors;        // Puertas que conectan dos celdas
    public Vector2Int[] entryPoints; // Puntos de entrada
    public Vector2Int[] agents;     // Posiciones de agentes
    public Vector2Int[] explosions; // Posiciones de explosiones
    public WallInfo[] wallsInfo;    // Información detallada de paredes individuales
    
    // Constructor
    public GameMap()
    {
        victims = new VictimInfo[0];
        fires = new Vector2Int[0];
        doors = new DoorInfo[0];
        entryPoints = new Vector2Int[0];
        agents = new Vector2Int[0];
        explosions = new Vector2Int[0];
        wallsInfo = new WallInfo[0];
    }
    
    // Obtener el tipo de celda en una posición específica
    public CellType GetCellType(int x, int y)
    {
        // Verificar si hay víctimas
        foreach (var victim in victims)
        {
            if (victim.x == x && victim.y == y)
                return CellType.Victim;
        }
        
        // Verificar si hay fuego
        foreach (var fire in fires)
        {
            if (fire.x == x && fire.y == y)
                return CellType.Fire;
        }
        
        // Verificar si hay agente
        foreach (var agent in agents)
        {
            if (agent.x == x && agent.y == y)
                return CellType.Agent;
        }
        
        // Verificar si hay explosión
        foreach (var explosion in explosions)
        {
            if (explosion.x == x && explosion.y == y)
                return CellType.Explosion;
        }
        
        // Verificar si hay puerta (cualquiera de las dos celdas)
        foreach (var door in doors)
        {
            if ((door.cell1.x == x && door.cell1.y == y) ||
                (door.cell2.x == x && door.cell2.y == y))
                return CellType.Door;
        }
        
        // Si no hay nada especial, es piso libre
        return CellType.Floor;
    }
    
    // Verificar si hay pared en una dirección específica
    public bool HasWall(int x, int y, WallDirection direction)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return true; // Los bordes del mapa siempre tienen paredes
            
        int wallValue = walls[y][x];
        return (wallValue & (1 << (int)direction)) != 0;
    }
}

[System.Serializable]
public struct Vector2Int
{
    public int x;
    public int y;
    
    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public enum CellType
{
    Floor = 0,      // Piso libre
    Victim = 1,     // Víctima
    Fire = 2,       // Fuego
    Agent = 3,      // Agente
    Door = 4,       // Puerta
    Explosion = 5   // Explosión
}

public enum WallDirection
{
    North = 0,  // Arriba
    East = 1,   // Derecha
    South = 2,  // Abajo
    West = 3    // Izquierda
}

// Información de víctima
[System.Serializable]
public struct VictimInfo
{
    public int x;
    public int y;
    public bool isFake; // true = falsa alarma, false = víctima real
    
    public VictimInfo(int x, int y, bool isFake)
    {
        this.x = x;
        this.y = y;
        this.isFake = isFake;
    }
}

// Información de puerta (conecta dos celdas)
[System.Serializable]
public struct DoorInfo
{
    public Vector2Int cell1;
    public Vector2Int cell2;
    
    public DoorInfo(Vector2Int cell1, Vector2Int cell2)
    {
        this.cell1 = cell1;
        this.cell2 = cell2;
    }
}