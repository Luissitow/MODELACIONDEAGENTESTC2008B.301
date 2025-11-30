using System;

/// <summary>
/// Representa una víctima o falsa alarma en el mapa
/// </summary>
[Serializable]
public class VictimaData
{
    public int row;           // Fila de la víctima
    public int col;           // Columna de la víctima
    public string type;       // "victima" o "falsaalarma"
    
    public bool EsVictima => type == "victima";
    public bool EsFalsaAlarma => type == "falsaalarma";
}
