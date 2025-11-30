using System;

/// <summary>
/// Representa un movimiento o acción de la tripulación
/// </summary>
[Serializable]
public class MovimientoData
{
    public string tipo; // "mover", "abrir_puerta", "danar_pared", etc.
    public DestinoData destino; // Solo para tipo "mover"
    public string direccion; // Para abrir puerta o dañar pared
}

/// <summary>
/// Representa el destino de un movimiento de tipo "mover"
/// </summary>
[Serializable]
public class DestinoData
{
    public int row;
    public int col;
}

/// <summary>
/// Representa un miembro de la tripulación (personajes jugadores/controlables)
/// </summary>
[Serializable]
public class TripulacionData
{
    public int row;      // Fila del miembro de la tripulación
    public int col;      // Columna del miembro de la tripulación
    public string tipo;  // Tipo de tripulante (ej: "astronauta")
    public int id;       // ID único del tripulante
    public bool esJugador; // true = controlado por humano, false = controlado por IA/Python
    public MovimientoData[] movimientos; // Array de movimientos/acciones (solo para NPCs)
}
