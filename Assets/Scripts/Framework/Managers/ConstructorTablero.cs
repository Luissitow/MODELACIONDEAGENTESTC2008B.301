using System.Collections;
using System.IO;
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
    public GameObject aranaPrefab;  // Público para JSONSyncManager (fuego en el juego)
    public GameObject huevoPrefab;  // Público para JSONSyncManager (hazmat)
    public GameObject tripulantePrefab;  // Tripulante VIVO en el piso (rescatable)
    public GameObject falsaAlarmaPrefab;  // Falsa alarma en el piso (no rescatable)
    public GameObject puntoInteresPrefab;  // Marcador "?" flotante ARRIBA (vista aérea)
    [SerializeField] private GameObject puertaPrefab;
    [SerializeField] private GameObject paredDanadaPrefab;
    [SerializeField] private GameObject paredDestruidaPrefab;
    [SerializeField] private GameObject player1Prefab; // Prefab del astronauta jugador 1
    [SerializeField] private GameObject player2Prefab; // Prefab del astronauta jugador 2
    
    [Header("Configuración")]
    public float tamanioCelda = 3f;  // Tamaño de cada celda - CAMBIADO A 3 UNIDADES (público para ActionExecutor)
    ///[SerializeField] private float alturaBase = -2f;   // Altura Y base del tablero (para elevarlo)
    
    private EscenarioData datosActuales;

    /// <summary>
    /// Construye el mapa completo a partir de los datos del escenario (ESTADO INICIAL)
    /// Llamado por SimulacionPlayer al inicio
    /// </summary>
    public void ConstruirMapaInicial(EscenarioData datos)
    {
        datosActuales = datos;
        ConstruirMapa(datos);
    }

    /// <summary>
    /// Construye el mapa completo a partir de los datos del escenario
    /// </summary>
    public void ConstruirMapa(EscenarioData datos)
    {
        CrearPisos(datos);
        CrearParedes(datos);
        CrearPuertas(datos);
        // Entradas: no se crean objetos, simplemente son celdas sin paredes
        CrearAranas(datos);
        CrearHuevos(datos);
        CrearTripulantes(datos); // Tripulantes rescatables en el piso
        CrearFalsasAlarmas(datos); // Falsas alarmas en el piso
        CrearPuntosInteres(datos); // Marcadores "?" flotantes ARRIBA (vista aérea)
        CrearJugadores(datos); // Astronautas jugadores (los 2 principales)
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
                // Piso: escala (2.8, 0.1, 2.8) para caber en celda de 3x3 con separación
                Vector3 posicion = new Vector3(col * tamanioCelda, -0.4f, (datos.fila - 1 - fila) * tamanioCelda);
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
                // Saltar paredes si hay entrada en esta celda
                if (HayEntradaEnCelda(datos, fila, col))
                    continue;

                string paredes = datos.ObtenerParedes(fila, col);
                // Convertir de grid a Unity: invertir Z
                Vector3 posicionCelda = new Vector3(col * tamanioCelda, 0, (datos.fila - 1 - fila) * tamanioCelda);
                
                // Norte (bit 0) - hacia Z+ (arriba en grid)
                if (paredes[0] == '1')
                {
                    Vector3 pos = posicionCelda + new Vector3(0, 0.7f, 0.5f * tamanioCelda);
                    InstanciarPared(datos, fila, col, "norte", pos, Quaternion.identity);
                }
                
                // Oeste (bit 1) - hacia X- (izquierda)
                if (paredes[1] == '1')
                {
                    Vector3 pos = posicionCelda + new Vector3(-0.5f * tamanioCelda, 0.7f, 0);
                    Quaternion rot = Quaternion.Euler(0, 90, 0);
                    InstanciarPared(datos, fila, col, "oeste", pos, rot);
                }
                
                // Sur (bit 2) - hacia Z- (abajo en grid)
                if (paredes[2] == '1')
                {
                    Vector3 pos = posicionCelda + new Vector3(0, 0.7f, -0.5f * tamanioCelda);
                    InstanciarPared(datos, fila, col, "sur", pos, Quaternion.identity);
                }
                
                // Este (bit 3) - hacia X+ (derecha)
                if (paredes[3] == '1')
                {
                    Vector3 pos = posicionCelda + new Vector3(0.5f * tamanioCelda, 0.7f, 0);
                    Quaternion rot = Quaternion.Euler(0, 90, 0);
                    InstanciarPared(datos, fila, col, "este", pos, rot);
                }
            }
        }
    }
    
    /// <summary>
    /// Instancia una pared, considerando si es especial (puerta, dañada)
    /// </summary>
    void InstanciarPared(EscenarioData datos, int fila, int col, string direccion, Vector3 posicion, Quaternion rotacion)
    {
        ParedEspecialData especial = ObtenerParedEspecial(datos, fila, col, direccion);
        
        GameObject prefabAUsar = paredPrefab; // Por defecto
        bool esPuerta = false;
        int estadoInicial = 0;
        
        if (especial != null)
        {
            if (especial.tipo == "puerta")
            {
                prefabAUsar = puertaPrefab; // Asumir que puertaPrefab existe
                esPuerta = true;
            }
            estadoInicial = especial.estado;
        }
        
        // Instanciar
        GameObject paredObj = Instantiate(prefabAUsar, posicion, rotacion, transform);
        paredObj.name = $"Pared_{fila}_{col}_{direccion}";
        
        // Obtener o agregar script Wall
        Wall wallScript = paredObj.GetComponent<Wall>();
        if (wallScript == null)
        {
            wallScript = paredObj.AddComponent<Wall>();
        }
        
        // Configurar propiedades
        wallScript.tipo = esPuerta ? TipoPared.Puerta : TipoPared.Madera;
        wallScript.fila = fila;
        wallScript.columna = col;
        wallScript.direccion = direccion;
        wallScript.prefabDanado = paredDanadaPrefab;
        wallScript.prefabDestruido = paredDestruidaPrefab;
        
        // Aplicar estado inicial (daño) - SOLO A PAREDES, NO PUERTAS
        if (estadoInicial > 0 && !esPuerta)
        {
            wallScript.RecibirDano(estadoInicial);
        }
    }
    
    /// <summary>
    /// Busca si hay una pared especial en la posición y dirección dadas
    /// fila y col son 0-indexed (Unity), especial.fila/col pueden ser 0-indexed o 1-indexed según JSON
    /// NOTA: paredesEspeciales en el JSON actual usa 0-indexed
    /// </summary>
    ParedEspecialData ObtenerParedEspecial(EscenarioData datos, int fila, int col, string direccion)
    {
        if (datos.paredesEspeciales == null) return null;
        
        foreach (var especial in datos.paredesEspeciales)
        {
            // Asumiendo que paredesEspeciales usa 0-indexed (como en tu JSON actual)
            if (especial.fila == fila && especial.col == col && especial.direccion == direccion)
            {
                return especial;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Verifica si hay una entrada en la celda especificada
    /// fila y col son 0-indexed (Unity), entrada.row/col son 1-indexed (JSON)
    /// </summary>
    bool HayEntradaEnCelda(EscenarioData datos, int fila, int col)
    {
        if (datos.entradas == null) return false;
        
        foreach (var entrada in datos.entradas)
        {
            // Convertir entrada de 1-indexed a 0-indexed para comparar
            if (entrada.row - 1 == fila && entrada.col - 1 == col)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Crea las arañas en las posiciones especificadas
    /// JSON usa 1-indexed (row: 1-6, col: 1-8)
    /// Unity usa 0-indexed, por eso restamos 1
    /// </summary>
    void CrearAranas(EscenarioData datos)
    {
        if (aranaPrefab == null || datos.arañas == null) return;
        
        foreach (var arana in datos.arañas)
        {
            // Convertir de 1-indexed (JSON) a 0-indexed (Unity)
            // col->X, row invertido para Z (fila 1 arriba = Z mayor)
            int filaUnity = arana.row - 1; // 1-6 -> 0-5
            int colUnity = arana.col - 1;  // 1-8 -> 0-7
            
            Vector3 posicion = new Vector3(colUnity * tamanioCelda, -0.2f, (datos.fila - 1 - filaUnity) * tamanioCelda);
            GameObject aranaObj = Instantiate(aranaPrefab, posicion, Quaternion.identity, transform);
            aranaObj.name = $"Arana_{arana.row}_{arana.col}";
        }
    }
    
    /// <summary>
    /// Crea los huevos en las posiciones especificadas
    /// JSON usa 1-indexed (row: 1-6, col: 1-8)
    /// </summary>
    void CrearHuevos(EscenarioData datos)
    {
        if (huevoPrefab == null || datos.huevos == null) return;
        
        foreach (var huevo in datos.huevos)
        {
            // Convertir de 1-indexed (JSON) a 0-indexed (Unity)
            int filaUnity = huevo.row - 1;
            int colUnity = huevo.col - 1;
            
            Vector3 posicion = new Vector3(colUnity * tamanioCelda, -0.3f, (datos.fila - 1 - filaUnity) * tamanioCelda);
            GameObject huevoObj = Instantiate(huevoPrefab, posicion, Quaternion.identity, transform);
            huevoObj.name = $"Huevo_{huevo.row}_{huevo.col}";
        }
    }
    
    /// <summary>
    /// Crea los tripulantes rescatables en el piso
    /// JSON usa 1-indexed (row: 1-6, col: 1-8)
    /// Estos están en el piso (Y=-0.35) y son RESCATABLES
    /// </summary>
    void CrearTripulantes(EscenarioData datos)
    {
        if (tripulantePrefab == null || datos.victimas == null) return;
        
        foreach (var victima in datos.victimas)
        {
            // Convertir de 1-indexed (JSON) a 0-indexed (Unity)
            int filaUnity = victima.row - 1;
            int colUnity = victima.col - 1;
            
            Vector3 posicion = new Vector3(colUnity * tamanioCelda, -0.35f, (datos.fila - 1 - filaUnity) * tamanioCelda);
            GameObject tripulanteObj = Instantiate(tripulantePrefab, posicion, Quaternion.identity, transform);
            tripulanteObj.name = $"Tripulante_{victima.row}_{victima.col}";
        }
    }
    
    /// <summary>
    /// Crea las falsas alarmas en el piso
    /// JSON usa 1-indexed (row: 1-6, col: 1-8)
    /// Estos están en el piso (Y=-0.35) y NO son rescatables
    /// </summary>
    void CrearFalsasAlarmas(EscenarioData datos)
    {
        if (falsaAlarmaPrefab == null || datos.falsasAlarmas == null) return;
        
        foreach (var falsa in datos.falsasAlarmas)
        {
            // Convertir de 1-indexed (JSON) a 0-indexed (Unity)
            int filaUnity = falsa.row - 1;
            int colUnity = falsa.col - 1;
            
            Vector3 posicion = new Vector3(colUnity * tamanioCelda, -0.35f, (datos.fila - 1 - filaUnity) * tamanioCelda);
            GameObject falsaObj = Instantiate(falsaAlarmaPrefab, posicion, Quaternion.identity, transform);
            falsaObj.name = $"FalsaAlarma_{falsa.row}_{falsa.col}";
        }
    }
    
    /// <summary>
    /// Instancia los jugadores astronautas en el tablero
    /// JSON usa 1-indexed (row: 1-6, col: 1-8)
    /// </summary>
    void CrearJugadores(EscenarioData datos)
    {
        // Player 1
        if (player1Prefab != null && datos.tripulacion != null && datos.tripulacion.Length > 0)
        {
            var jugador1 = datos.tripulacion[0];
            int filaUnity = jugador1.row - 1;
            int colUnity = jugador1.col - 1;
            
            Vector3 pos1 = new Vector3(colUnity * tamanioCelda, -0.35f, (datos.fila - 1 - filaUnity) * tamanioCelda);
            GameObject player1Obj = Instantiate(player1Prefab, pos1, Quaternion.identity, transform);
            player1Obj.name = "Player1_Astronauta";
            var controller1 = player1Obj.GetComponent<AstronautController>();
            if (controller1 != null)
            {
                controller1.astronautaID = 1;
                controller1.filaActual = jugador1.row;  // Mantener 1-indexed en el controller
                controller1.columnaActual = jugador1.col;
            }
        }
        // Player 2
        if (player2Prefab != null && datos.tripulacion != null && datos.tripulacion.Length > 1)
        {
            var jugador2 = datos.tripulacion[1];
            int filaUnity = jugador2.row - 1;
            int colUnity = jugador2.col - 1;
            
            Vector3 pos2 = new Vector3(colUnity * tamanioCelda, -0.35f, (datos.fila - 1 - filaUnity) * tamanioCelda);
            GameObject player2Obj = Instantiate(player2Prefab, pos2, Quaternion.identity, transform);
            player2Obj.name = "Player2_Astronauta";
            var controller2 = player2Obj.GetComponent<AstronautController>();
            if (controller2 != null)
            {
                controller2.astronautaID = 2;
                controller2.filaActual = jugador2.row;  // Mantener 1-indexed en el controller
                controller2.columnaActual = jugador2.col;
            }
        }
    }

    // ELIMINADO: CrearTripulacion()
    // Ya no se necesita porque los tripulantes rescatables se crean con CrearTripulantes()
    // Solo existen 2 jugadores principales (Player1 y Player2)
    
    /// <summary>
    /// Crea las puertas entre dos celdas
    /// Las puertas conectan dos celdas adyacentes y se posicionan en el muro entre ellas
    /// JSON usa 1-indexed (r1/r2: 1-6, c1/c2: 1-8)
    /// </summary>
    void CrearPuertas(EscenarioData datos)
    {
        if (puertaPrefab == null || datos.puertas == null) return;
        
        foreach (var puerta in datos.puertas)
        {
            // Convertir de 1-indexed (JSON) a 0-indexed (Unity)
            int fila1Unity = puerta.r1 - 1;
            int col1Unity = puerta.c1 - 1;
            int fila2Unity = puerta.r2 - 1;
            int col2Unity = puerta.c2 - 1;
            
            Vector3 pos1 = new Vector3(col1Unity * tamanioCelda, 0, (datos.fila - 1 - fila1Unity) * tamanioCelda);
            Vector3 pos2 = new Vector3(col2Unity * tamanioCelda, 0, (datos.fila - 1 - fila2Unity) * tamanioCelda);
            
            // Calcular posición central entre las dos celdas
            Vector3 posicionPuerta = (pos1 + pos2) / 2f;
            posicionPuerta.y = 0.7f; // Altura de la puerta (misma que paredes)
            
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
    
    // ELIMINADO: CrearEntradas()
    // Las entradas son simplemente celdas sin paredes, no necesitan objetos
    // La lógica de entradas se maneja en HayEntradaEnCelda() al crear paredes
    
    /// <summary>
    /// Crea marcadores "?" flotantes ARRIBA de las celdas con tripulantes o falsas alarmas
    /// Estos marcadores están a altura Y=2.0 para vista aérea
    /// El jugador NO puede ver desde arriba qué hay en el piso hasta que llega a la celda
    /// JSON usa 1-indexed (row: 1-6, col: 1-8)
    /// </summary>
    void CrearPuntosInteres(EscenarioData datos)
    {
        if (puntoInteresPrefab == null) return;
        
        // Marcador "?" sobre tripulantes rescatables
        if (datos.victimas != null)
        {
            foreach (var victima in datos.victimas)
            {
                int filaUnity = victima.row - 1;
                int colUnity = victima.col - 1;
                
                // Altura elevada para vista aérea (no tapa a los astronautas)
                Vector3 posicion = new Vector3(colUnity * tamanioCelda, 2.0f, (datos.fila - 1 - filaUnity) * tamanioCelda);
                GameObject puntoObj = Instantiate(puntoInteresPrefab, posicion, Quaternion.identity, transform);
                puntoObj.name = $"Marcador_?_{victima.row}_{victima.col}";
            }
        }
        
        // Marcador "?" sobre falsas alarmas
        if (datos.falsasAlarmas != null)
        {
            foreach (var falsa in datos.falsasAlarmas)
            {
                int filaUnity = falsa.row - 1;
                int colUnity = falsa.col - 1;
                
                Vector3 posicion = new Vector3(colUnity * tamanioCelda, 2.0f, (datos.fila - 1 - filaUnity) * tamanioCelda);
                GameObject puntoObj = Instantiate(puntoInteresPrefab, posicion, Quaternion.identity, transform);
                puntoObj.name = $"Marcador_?_{falsa.row}_{falsa.col}";
            }
        }
        
        // Puntos de interés adicionales del JSON (si existen)
        if (datos.puntosInteres != null)
        {
            foreach (var punto in datos.puntosInteres)
            {
                int filaUnity = punto.row - 1;
                int colUnity = punto.col - 1;
                
                Vector3 posicion = new Vector3(colUnity * tamanioCelda, 2.0f, (datos.fila - 1 - filaUnity) * tamanioCelda);
                GameObject puntoObj = Instantiate(puntoInteresPrefab, posicion, Quaternion.identity, transform);
                puntoObj.name = $"Marcador_{punto.tipo}_{punto.row}_{punto.col}";
            }
        }
    }

    // ConstructorTablero solo construye el tablero inicial
    // SimulacionPlayer se encarga de reproducir la simulación

    // Método auxiliar para buscar un objeto (puerta o pared) en una dirección desde una posición
    GameObject BuscarObjetoEnDireccion(Vector3 origen, string direccion, string tipo)
    {
        Vector3 offset = Vector3.zero;
        switch (direccion.ToLower())
        {
            case "norte": offset = new Vector3(0, 0, tamanioCelda / 2); break;
            case "sur": offset = new Vector3(0, 0, -tamanioCelda / 2); break;
            case "este": offset = new Vector3(tamanioCelda / 2, 0, 0); break;
            case "oeste": offset = new Vector3(-tamanioCelda / 2, 0, 0); break;
        }
        Vector3 posBusqueda = origen + offset;
        foreach (Transform child in transform)
        {
            if (child.name.Contains(tipo) && Vector3.Distance(child.position, posBusqueda) < tamanioCelda / 2)
                return child.gameObject;
        }
        return null;
    }
}
