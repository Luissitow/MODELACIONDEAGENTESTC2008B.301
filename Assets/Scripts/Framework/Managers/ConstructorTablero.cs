    using UnityEngine;

/// <summary>
/// Clase responsable de construir el tablero 3D a partir de los datos del escenario
/// Lee el JSON y crea todos los GameObjects (pisos, paredes, arañas, víctimas)
/// </summary>
public class ConstructorTablero : MonoBehaviour
{
    [Header("Prefabs de construcción")]
    [SerializeField] private GameObject pisoPrefab;
    [SerializeField] private GameObject paredPrefab;
    [SerializeField] private GameObject aranaPrefab;
    [SerializeField] private GameObject huevoPrefab;
    [SerializeField] private GameObject victimaPrefab;
    
    [Header("Configuración")]
    [SerializeField] private float tamanoCelda = 1f;  // Tamaño de cada celda (1 unidad)
    
    /// <summary>
    /// Construye el mapa completo a partir de los datos del escenario
    /// </summary>
    public void ConstruirMapa(EscenarioData datos)
    {
        CrearPisos(datos);
        CrearParedes(datos);
        CrearAranas(datos);
        CrearHuevos(datos);
        CrearVictimas(datos);
        // CrearPuertas(datos);  // Por implementar
        // CrearEntradas(datos); // Por implementar
    }
    
    /// <summary>
    /// Crea todos los pisos del tablero (6 filas × 8 columnas = 48 pisos)
    /// </summary>
    void CrearPisos(EscenarioData datos)
    {
        for (int fila = 0; fila < datos.fila; fila++)
        {
            for (int col = 0; col < datos.columna; col++)
            {
                // Convertir de grid (row,col) a Unity (X,Z)
                // row=0 debe estar arriba (Z mayor), row=5 abajo (Z menor)
                Vector3 posicion = new Vector3(col * tamanoCelda, 0, (datos.fila - 1 - fila) * tamanoCelda);
                GameObject piso = Instantiate(pisoPrefab, posicion, Quaternion.identity, transform);
                piso.name = $"Piso_{fila}_{col}";
            }
        }
    }
    
    /// <summary>
    /// Crea las paredes según la configuración de bits de cada celda
    /// Bits: [0]=Norte, [1]=Oeste, [2]=Sur, [3]=Este
    /// </summary>
    void CrearParedes(EscenarioData datos)
    {
        for (int fila = 0; fila < datos.fila; fila++)
        {
            for (int col = 0; col < datos.columna; col++)
            {
                string paredes = datos.ObtenerParedes(fila, col);
                // Convertir de grid a Unity: invertir Z
                Vector3 posicionCelda = new Vector3(col * tamanoCelda, 0, (datos.fila - 1 - fila) * tamanoCelda);
                
                // Norte (bit 0) - hacia Z+ (arriba en grid)
                if (paredes[0] == '1')
                {
                    Vector3 pos = posicionCelda + new Vector3(0, 0.5f, 0.5f * tamanoCelda);
                    Instantiate(paredPrefab, pos, Quaternion.identity, transform);
                }
                
                // Oeste (bit 1) - hacia X- (izquierda)
                if (paredes[1] == '1')
                {
                    Vector3 pos = posicionCelda + new Vector3(-0.5f * tamanoCelda, 0.5f, 0);
                    Quaternion rot = Quaternion.Euler(0, 90, 0);
                    Instantiate(paredPrefab, pos, rot, transform);
                }
                
                // Sur (bit 2) - hacia Z- (abajo en grid)
                if (paredes[2] == '1')
                {
                    Vector3 pos = posicionCelda + new Vector3(0, 0.5f, -0.5f * tamanoCelda);
                    Instantiate(paredPrefab, pos, Quaternion.identity, transform);
                }
                
                // Este (bit 3) - hacia X+ (derecha)
                if (paredes[3] == '1')
                {
                    Vector3 pos = posicionCelda + new Vector3(0.5f * tamanoCelda, 0.5f, 0);
                    Quaternion rot = Quaternion.Euler(0, 90, 0);
                    Instantiate(paredPrefab, pos, rot, transform);
                }
            }
        }
    }
    
    /// <summary>
    /// Crea las arañas en las posiciones especificadas
    /// </summary>
    void CrearAranas(EscenarioData datos)
    {
        if (aranaPrefab == null || datos.arañas == null) return;
        
        foreach (var arana in datos.arañas)
        {
            // Convertir: col->X, row->Z invertido
            Vector3 posicion = new Vector3((arana.col - 1) * tamanoCelda, 0.2f, (datos.fila - arana.row) * tamanoCelda);
            GameObject aranaObj = Instantiate(aranaPrefab, posicion, Quaternion.identity, transform);
            aranaObj.name = $"Arana_{arana.row}_{arana.col}";
        }
    }
    
    /// <summary>
    /// Crea los huevos en las posiciones especificadas
    /// </summary>
    void CrearHuevos(EscenarioData datos)
    {
        if (huevoPrefab == null || datos.huevos == null) return;
        
        foreach (var huevo in datos.huevos)
        {
            // Convertir: col->X, row->Z invertido
            Vector3 posicion = new Vector3((huevo.col - 1) * tamanoCelda, 0.1f, (datos.fila - huevo.row) * tamanoCelda);
            GameObject huevoObj = Instantiate(huevoPrefab, posicion, Quaternion.identity, transform);
            huevoObj.name = $"Huevo_{huevo.row}_{huevo.col}";
        }
    }
    
    /// <summary>
    /// Crea las víctimas en las posiciones especificadas
    /// </summary>
    void CrearVictimas(EscenarioData datos)
    {
        if (victimaPrefab == null || datos.victimas == null) return;
        
        foreach (var victima in datos.victimas)
        {
            // Convertir: col->X, row->Z invertido  
            Vector3 posicion = new Vector3((victima.col - 1) * tamanoCelda, 0.3f, (datos.fila - victima.row) * tamanoCelda);
            GameObject victimaObj = Instantiate(victimaPrefab, posicion, Quaternion.identity, transform);
            victimaObj.name = $"Victima_{victima.row}_{victima.col}_{victima.type}";
        }
    }
}
