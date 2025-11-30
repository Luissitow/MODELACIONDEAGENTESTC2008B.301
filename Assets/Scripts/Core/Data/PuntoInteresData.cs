using System;

/// <summary>
/// Representa un punto de interés en el mapa (objetivos, ubicaciones especiales, etc.)
/// </summary>
[Serializable]
public class PuntoInteresData
{
    public int row;      // Fila del punto de interés
    public int col;      // Columna del punto de interés
    public string tipo;  // Tipo de punto de interés (ej: "objetivo", "hazmat", "explosivo")
}
