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
    [SerializeField] private GameObject falsaAlarmaPrefab;
    [SerializeField] private GameObject tripulacionPrefab;
    [SerializeField] private GameObject puertaPrefab;
    [SerializeField] private GameObject entradaPrefab;
    [SerializeField] private GameObject puntoInteresPrefab;
    
    [Header("Configuración")]
    [SerializeField] private float tamanoCelda = 4f;  // Tamaño de cada celda (1 unidad)
    [SerializeField] private float alturaBase = 4f;   // Altura Y base del tablero (para elevarlo)
    
    /// <summary>
    /// Construye el mapa completo a partir de los datos del escenario
    /// </summary>
    public void ConstruirMapa(EscenarioData datos)
    {
        CrearPisos(datos);
        CrearParedes(datos);
        CrearPuertas(datos);
        CrearEntradas(datos);
        CrearAranas(datos);
        CrearHuevos(datos);
        CrearVictimas(datos);
        CrearFalsasAlarmas(datos);
        CrearTripulacion(datos);
        CrearPuntosInteres(datos);
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
    /// Crea las víctimas reales en las posiciones especificadas
    /// </summary>
    void CrearVictimas(EscenarioData datos)
    {
        if (victimaPrefab == null || datos.victimas == null) return;
        
        foreach (var victima in datos.victimas)
        {
            // Convertir: col->X, row->Z invertido  
            Vector3 posicion = new Vector3((victima.col - 1) * tamanoCelda, 0.05f, (datos.fila - victima.row) * tamanoCelda);
            GameObject victimaObj = Instantiate(victimaPrefab, posicion, Quaternion.identity, transform);
            victimaObj.name = $"Victima_{victima.row}_{victima.col}";
        }
    }
    
    /// <summary>
    /// Crea las falsas alarmas en las posiciones especificadas
    /// </summary>
    void CrearFalsasAlarmas(EscenarioData datos)
    {
        if (falsaAlarmaPrefab == null || datos.falsasAlarmas == null) return;
        
        foreach (var falsa in datos.falsasAlarmas)
        {
            // Convertir: col->X, row->Z invertido  
            Vector3 posicion = new Vector3((falsa.col - 1) * tamanoCelda, 0.05f, (datos.fila - falsa.row) * tamanoCelda);
            GameObject falsaObj = Instantiate(falsaAlarmaPrefab, posicion, Quaternion.identity, transform);
            falsaObj.name = $"FalsaAlarma_{falsa.row}_{falsa.col}";
        }
    }
    
    /// <summary>
    /// Crea los miembros de la tripulación en las posiciones especificadas
    /// </summary>
    void CrearTripulacion(EscenarioData datos)
    {
        if (tripulacionPrefab == null || datos.tripulacion == null) return;
        
        foreach (var miembro in datos.tripulacion)
        {
            // Convertir: col->X, row->Z invertido
            Vector3 posicion = new Vector3((miembro.col - 1) * tamanoCelda, 0.05f, (datos.fila - miembro.row) * tamanoCelda);
            GameObject tripulanteObj = Instantiate(tripulacionPrefab, posicion, Quaternion.identity, transform);
            tripulanteObj.name = $"Tripulante_{miembro.id}_{miembro.tipo}";
        }
    }
    
    /// <summary>
    /// Crea las puertas entre dos celdas
    /// Las puertas conectan dos celdas adyacentes y se posicionan en el muro entre ellas
    /// </summary>
    void CrearPuertas(EscenarioData datos)
    {
        if (puertaPrefab == null || datos.puertas == null) return;
        
        foreach (var puerta in datos.puertas)
        {
            // Convertir coordenadas de grid a Unity
            Vector3 pos1 = new Vector3((puerta.c1 - 1) * tamanoCelda, 0, (datos.fila - puerta.r1) * tamanoCelda);
            Vector3 pos2 = new Vector3((puerta.c2 - 1) * tamanoCelda, 0, (datos.fila - puerta.r2) * tamanoCelda);
            
            // Calcular posición central entre las dos celdas
            Vector3 posicionPuerta = (pos1 + pos2) / 2f;
            posicionPuerta.y = 0.5f; // Altura de la puerta (misma que paredes)
            
            // Determinar orientación de la puerta (horizontal o vertical)
            Quaternion rotacion = Quaternion.identity;
            
            // Si la diferencia es en columnas -> puerta vertical (Este-Oeste)
            if (puerta.c1 != puerta.c2)
            {
                rotacion = Quaternion.Euler(0, 90, 0);
            }
            // Si la diferencia es en filas -> puerta horizontal (Norte-Sur)
            // (rotación por defecto = 0)
            
            GameObject puertaObj = Instantiate(puertaPrefab, posicionPuerta, rotacion, transform);
            puertaObj.name = $"Puerta_({puerta.r1},{puerta.c1})-({puerta.r2},{puerta.c2})";
        }
    }
    
    /// <summary>
    /// Crea los puntos de entrada al edificio
    /// Los puntos de entrada son marcadores especiales en el borde del mapa
    /// </summary>
    void CrearEntradas(EscenarioData datos)
    {
        if (entradaPrefab == null || datos.entradas == null) return;
        
        foreach (var entrada in datos.entradas)
        {
            // Convertir: col->X, row->Z invertido
            Vector3 posicion = new Vector3((entrada.col - 1) * tamanoCelda, 0.05f, (datos.fila - entrada.row) * tamanoCelda);
            GameObject entradaObj = Instantiate(entradaPrefab, posicion, Quaternion.identity, transform);
            entradaObj.name = $"Entrada_{entrada.row}_{entrada.col}";
            
            // Opcional: Escalar el prefab para que sea un marcador visible
            entradaObj.transform.localScale = new Vector3(0.6f, 0.1f, 0.6f);
        }
    }
    
    /// <summary>
    /// Crea los puntos de interés en el mapa
    /// Los puntos de interés se colocan automáticamente donde hay víctimas y falsas alarmas
    /// Altura elevada (Y=1.5) para que sean visibles desde vista aérea sin tapar personajes
    /// </summary>
    void CrearPuntosInteres(EscenarioData datos)
    {
        if (puntoInteresPrefab == null) return;
        
        // Crear puntos de interés en las posiciones de víctimas
        if (datos.victimas != null)
        {
            foreach (var victima in datos.victimas)
            {
                Vector3 posicion = new Vector3((victima.col - 1) * tamanoCelda, 2.0f, (datos.fila - victima.row) * tamanoCelda);
                GameObject puntoObj = Instantiate(puntoInteresPrefab, posicion, Quaternion.identity, transform);
                puntoObj.name = $"PuntoInteres_Victima_{victima.row}_{victima.col}";
            }
        }
        
        // Crear puntos de interés en las posiciones de falsas alarmas
        if (datos.falsasAlarmas != null)
        {
            foreach (var falsa in datos.falsasAlarmas)
            {
                Vector3 posicion = new Vector3((falsa.col - 1) * tamanoCelda, 2.0f, (datos.fila - falsa.row) * tamanoCelda);
                GameObject puntoObj = Instantiate(puntoInteresPrefab, posicion, Quaternion.identity, transform);
                puntoObj.name = $"PuntoInteres_FalsaAlarma_{falsa.row}_{falsa.col}";
            }
        }
        
        // Crear puntos de interés adicionales si están definidos en el JSON
        if (datos.puntosInteres != null)
        {
            foreach (var punto in datos.puntosInteres)
            {
                Vector3 posicion = new Vector3((punto.col - 1) * tamanoCelda, 2.0f, (datos.fila - punto.row) * tamanoCelda);
                GameObject puntoObj = Instantiate(puntoInteresPrefab, posicion, Quaternion.identity, transform);
                puntoObj.name = $"PuntoInteres_{punto.tipo}_{punto.row}_{punto.col}";
            }
        }
    }
}
