using System;

/// <summary>
/// Representa una puerta que conecta dos celdas del mapa
/// </summary>
[Serializable]
public class PuertaData
{
    public int r1;  // Fila de la primera celda
    public int c1;  // Columna de la primera celda
    public int r2;  // Fila de la segunda celda
    public int c2;  // Columna de la segunda celda
}
