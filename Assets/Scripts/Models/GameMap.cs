using System;

[System.Serializable]
public class GameMap
{
    public int width;
    public int height;
    public int[][] walls;           // Matriz de paredes en binario
    public Vector2Int[] victims;    // Posiciones de víctimas (v)
    public Vector2Int[] fires;      // Posiciones de fuegos (f)
    public Vector2Int[] doors;      // Posiciones de puertas
    public Vector2Int[] agents;     // Posiciones de agentes
    public Vector2Int[] explosions; // Posiciones de explosiones
    public WallInfo[] wallsInfo;    // Información detallada de paredes individuales
    
    // Constructor
    public GameMap()
    {
        victims = new Vector2Int[0];
        fires = new Vector2Int[0];
        doors = new Vector2Int[0];
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
        
        // Verificar si hay puerta
        foreach (var door in doors)
        {
            if (door.x == x && door.y == y)
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