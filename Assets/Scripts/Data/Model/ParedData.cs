using System;

/// <summary>
/// Representa una pared o puerta en el tablero, según el array 'paredes' del JSON
/// </summary>
[Serializable]
public class ParedData
{
    public int row;           // Fila de la celda
    public int col;           // Columna de la celda
    public string orientacion; // "N", "S", "E", "O"
    public int daño;          // Nivel de daño
    public string tipo;       // "puerta" o null
    public string estado;     // "abierta", "cerrada" (solo para puertas)
}
