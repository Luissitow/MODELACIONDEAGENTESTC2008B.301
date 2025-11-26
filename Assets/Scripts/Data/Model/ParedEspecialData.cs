using System;

/// <summary>
/// Datos para paredes especiales (puertas, dañadas, etc.)
/// </summary>
[Serializable]
public class ParedEspecialData
{
    public int fila;        // Fila de la celda (0-5)
    public int col;         // Columna de la celda (0-7)
    public string direccion; // "norte", "oeste", "sur", "este"
    public string tipo;     // "pared" (normal), "puerta" (abre/cierra)
    public int estado;      // 0: normal, 1: dañado, 2: destruido
}