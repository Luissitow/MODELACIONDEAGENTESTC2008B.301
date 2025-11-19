using System;

/// <summary>
/// Clase principal que mapea el JSON completo del escenario
/// Contiene toda la información del tablero, víctimas, arañas, etc.
/// </summary>
[Serializable]
public class EscenarioData
{
    public int fila;                    // Número de filas del tablero (6)
    public int columna;                 // Número de columnas del tablero (8)
    public string[] celdas;             // Array de configuraciones de paredes "1100", "1000", etc.
    public VictimaData[] victimas;      // Array de víctimas y falsas alarmas
    public AranaData[] arañas;          // Array de posiciones de arañas
    public HuevoData[] huevos;          // Array de posiciones de huevos
    public PuertaData[] puertas;        // Array de puertas entre celdas
    public EntradaData[] entradas;      // Array de puntos de entrada
    
    /// <summary>
    /// Obtiene la configuración de paredes de una celda específica
    /// </summary>
    /// <param name="fila">Fila de la celda (0-5)</param>
    /// <param name="columna">Columna de la celda (0-7)</param>
    /// <returns>String de 4 bits representando las paredes "1100"</returns>
    public string ObtenerParedes(int fila, int columna)
    {
        if (fila < 0 || fila >= this.fila || columna < 0 || columna >= this.columna)
            return "0000";
            
        int indice = fila * this.columna + columna;
        return celdas[indice];
    }
}
