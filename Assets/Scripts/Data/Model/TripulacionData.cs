using System;

/// <summary>
/// Representa un miembro de la tripulación (personajes jugadores/controlables)
/// </summary>
[Serializable]
public class TripulacionData
{
    public int row;      // Fila del miembro de la tripulación
    public int col;      // Columna del miembro de la tripulación
    public string tipo;  // Tipo de tripulante (ej: "bombero", "paramedico", "especialista")
    public int id;       // ID único del tripulante
}
