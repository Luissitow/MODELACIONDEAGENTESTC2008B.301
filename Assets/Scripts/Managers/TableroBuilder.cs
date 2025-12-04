using UnityEngine;
using System.Collections.Generic;

public class TableroBuilder : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject pisoPrefab;
    public GameObject paredPrefab;
    public GameObject puertaPrefab;
    public GameObject crewPrefab;
    public GameObject spiderPrefab;
    public GameObject eggPrefab;
    public GameObject poiPrefab;
    public GameObject entradaPrefab;
    
    // Diccionarios para guardar referencias a objetos creados
    private static Dictionary<string, GameObject> paredes = new Dictionary<string, GameObject>();
    private static Dictionary<string, GameObject> puertas = new Dictionary<string, GameObject>();
    private static Dictionary<int, GameObject> tripulacion = new Dictionary<int, GameObject>();
    private static Dictionary<int, GameObject> puntosInteres = new Dictionary<int, GameObject>();
    private static Dictionary<string, GameObject> spiders = new Dictionary<string, GameObject>();
    private static Dictionary<string, GameObject> huevos = new Dictionary<string, GameObject>();
    
    // Instancia singleton para acceso a prefabs desde métodos estáticos
    private static TableroBuilder instancia;
    
    // HashSet para trackear posiciones con puertas/entradas
    private HashSet<string> posicionesBloqueadas = new HashSet<string>();
    
    /// <summary>
    /// Construye el tablero completo basado en el escenario
    /// </summary>
    public void Construir(EscenarioData escenario)
    {
        if (escenario == null)
        {
            Debug.LogError("Escenario nulo, no se puede construir");
            return;
        }
        
        instancia = this;
        LimpiarDiccionarios();
        posicionesBloqueadas.Clear();
        
        Debug.Log("🏗️ Iniciando construcción del tablero...");
        
        // 1. Registrar posiciones de puertas y entradas
        RegistrarPosicionesBloqueadas(escenario.estado_inicial);
        
        // 2. Crear pisos y paredes (evitando puertas/entradas)
        ConstruirTablero(escenario.tablero);
        
        // 3. Crear estado inicial
        ConstruirEstadoInicial(escenario.estado_inicial);
        
        Debug.Log("✅ Tablero construido exitosamente");
        
        // 4. Inicializar cámaras después de crear agentes
        CameraManager cameraManager = FindAnyObjectByType<CameraManager>();
        if (cameraManager != null && tripulacion.Count > 0)
        {
            Debug.Log($"📹 Inicializando cámaras para {tripulacion.Count} agentes");
        }
    }
    
    private void ConstruirTablero(TableroConfig tablero)
    {
        int index = 0;
        for (int fila = 1; fila <= tablero.fila; fila++)
        {
            for (int columna = 1; columna <= tablero.columna; columna++)
            {
                if (index >= tablero.celdas.Length) break;
                
                Vector3 posicion = CoordenadasHelper.JSONaPosicionUnity(fila, columna);
                
                // Crear piso
                if (pisoPrefab != null)
                {
                    Instantiate(pisoPrefab, posicion, Quaternion.identity, transform);
                }
                
                // Crear paredes según el encoding "1100" (NOSE)
                CrearParedes(fila, columna, tablero.celdas[index]);
                
                index++;
            }
        }
    }
    
    private void CrearParedes(int fila, int columna, string encoding)
    {
        if (string.IsNullOrEmpty(encoding) || encoding.Length != 4) return;
        
        string[] direcciones = { "norte", "oeste", "sur", "este" };
        
        for (int i = 0; i < 4; i++)
        {
            if (encoding[i] == '1')
            {
                CrearPared(fila, columna, direcciones[i]);
            }
        }
    }
    
    private void CrearPared(int fila, int columna, string direccion)
    {
        if (paredPrefab == null) return;
        
        // Verificar si hay puerta o entrada en esta posición
        string key = CoordenadasHelper.GenerarKey(fila, columna, direccion);
        if (posicionesBloqueadas.Contains(key))
        {
            Debug.Log($"⛔ BLOQUEADA - No se crea pared en {key}");
            return;
        }
        
        // Verificar si la pared ya existe (puede haber sido creada desde la celda adyacente)
        if (paredes.ContainsKey(key))
        {
            return; // La pared ya existe, no duplicar
        }
        
        Vector3 posBase = CoordenadasHelper.JSONaPosicionUnity(fila, columna);
        Vector3 offset = CoordenadasHelper.ObtenerOffsetPared(direccion);
        Quaternion rotacion = CoordenadasHelper.ObtenerRotacionDireccion(direccion);
        
        // Subir la pared 1.5 unidades para que quede sobre el piso
        Vector3 posicionPared = posBase + offset + new Vector3(0, 1.5f, 0);
        
        GameObject pared = Instantiate(paredPrefab, posicionPared, rotacion, transform);
        pared.name = $"Pared_{fila}_{columna}_{direccion}";
        
        // Guardar referencia con la clave actual
        paredes[key] = pared;
        
        // TAMBIÉN guardar con la clave del lado opuesto (misma pared, 2 referencias)
        var (filaOpuesta, colOpuesta, dirOpuesta) = ObtenerLadoOpuesto(fila, columna, direccion);
        string keyOpuesta = CoordenadasHelper.GenerarKey(filaOpuesta, colOpuesta, dirOpuesta);
        paredes[keyOpuesta] = pared; // Misma pared, accesible desde ambos lados
    }
    
    private void RegistrarPosicionesBloqueadas(EstadoInicial estado)
    {
        // Registrar puertas (ambos lados)
        foreach (var puerta in estado.puertas)
        {
            string key = CoordenadasHelper.GenerarKey(puerta.fila, puerta.columna, puerta.direccion);
            posicionesBloqueadas.Add(key);
            
            // Bloquear también el lado opuesto
            var ladoOpuesto = ObtenerLadoOpuesto(puerta.fila, puerta.columna, puerta.direccion);
            if (ladoOpuesto.Item1 > 0) // Validar que la celda opuesta exista
            {
                string keyOpuesto = CoordenadasHelper.GenerarKey(ladoOpuesto.Item1, ladoOpuesto.Item2, ladoOpuesto.Item3);
                posicionesBloqueadas.Add(keyOpuesto);
            }
        }
        
        // Registrar entradas (ambos lados)
        foreach (var entrada in estado.entradas)
        {
            string key = CoordenadasHelper.GenerarKey(entrada.fila, entrada.columna, entrada.direccion);
            posicionesBloqueadas.Add(key);
            
            // Bloquear también el lado opuesto
            var ladoOpuesto = ObtenerLadoOpuesto(entrada.fila, entrada.columna, entrada.direccion);
            if (ladoOpuesto.Item1 > 0) // Validar que la celda opuesta exista
            {
                string keyOpuesto = CoordenadasHelper.GenerarKey(ladoOpuesto.Item1, ladoOpuesto.Item2, ladoOpuesto.Item3);
                posicionesBloqueadas.Add(keyOpuesto);
            }
        }
        
        Debug.Log($"✅ {posicionesBloqueadas.Count} posiciones bloqueadas (puertas+entradas, ambos lados)");
    }
    
    /// <summary>
    /// Obtiene la celda y dirección opuesta a una pared
    /// Ejemplo: (2,8,sur) -> (3,8,norte)
    /// </summary>
    private (int, int, string) ObtenerLadoOpuesto(int fila, int columna, string direccion)
    {
        switch (direccion.ToLower())
        {
            case "norte":
                return (fila - 1, columna, "sur");
            case "sur":
                return (fila + 1, columna, "norte");
            case "este":
                return (fila, columna + 1, "oeste");
            case "oeste":
                return (fila, columna - 1, "este");
            default:
                return (0, 0, ""); // Inválido
        }
    }
    
    private void ConstruirEstadoInicial(EstadoInicial estado)
    {
        // Crear tripulación
        foreach (var crew in estado.tripulacion)
        {
            CrearTripulacion(crew);
        }
        
        // Crear POIs
        foreach (var poi in estado.puntosInteres)
        {
            CrearPOI(poi);
        }
         
        // Crear spiders
        Debug.Log($"📋 Leyendo {estado.arañas.Length} arañas del JSON...");
        foreach (var spider in estado.arañas)
        {
            CrearSpider(spider.fila, spider.columna);
        }
        
        // Crear huevos
        foreach (var huevo in estado.huevos)
        {
            CrearHuevo(huevo.fila, huevo.columna);
        }
        
        // Crear puertas
        foreach (var puerta in estado.puertas)
        {
            CrearPuerta(puerta);
        }
        
        // Crear entradas
        foreach (var entrada in estado.entradas)
        {
            CrearEntrada(entrada);
        }
    }
    
    private void CrearTripulacion(TripulacionData crew)
    {
        if (crewPrefab == null) return;
        
        Vector3 posicion = CoordenadasHelper.JSONaPosicionUnity(crew.fila, crew.columna);
        posicion.y = 1f; // Altura para que se vea sobre el piso
        
        GameObject crewObj = Instantiate(crewPrefab, posicion, Quaternion.identity, transform);
        crewObj.name = $"Crew_{crew.id}_{crew.tipo}";
        
        // Agregar componente Crew.cs si no existe
        if (crewObj.GetComponent<Crew>() == null)
        {
            crewObj.AddComponent<Crew>();
            Debug.Log($"✅ Componente Crew.cs agregado a {crewObj.name}");
        }
        
        tripulacion[crew.id] = crewObj;  
        
        // Notificar al CameraManager sobre el nuevo agente
        CameraManager cameraManager = FindAnyObjectByType<CameraManager>();
        if (cameraManager != null)
        {
            cameraManager.AgregarAgente(crewObj);
        }
    }
    
    private void CrearPOI(PuntoInteresData poi)
    {
        if (poiPrefab == null) return;
        
        Vector3 posicion = CoordenadasHelper.JSONaPosicionUnity(poi.fila, poi.columna);
        posicion.y = 0.5f;
        
        GameObject poiObj = Instantiate(poiPrefab, posicion, Quaternion.identity, transform);
        poiObj.name = $"POI_{poi.id}";
        
        puntosInteres[poi.id] = poiObj;
    }
    
    private void CrearSpider(int fila, int columna)
    {
        if (spiderPrefab == null) 
        {
            Debug.LogError($"❌ CrearSpider({fila},{columna}): spiderPrefab es NULL!");
            return;
        }
        
        Vector3 posicion = CoordenadasHelper.JSONaPosicionUnity(fila, columna);
        posicion.y = 1f;
        
        GameObject spider = Instantiate(spiderPrefab, posicion, Quaternion.identity, transform);
        spider.name = $"Spider_{fila}_{columna}";
        
        string key = $"{fila},{columna}";
        spiders[key] = spider;
        
        Debug.Log($"🕷️ Creada araña F{fila}C{columna} en posición Unity ({posicion.x:F1}, {posicion.y:F1}, {posicion.z:F1})");
    }
    
    private void CrearHuevo(int fila, int columna)
    {
        if (eggPrefab == null) return;
        
        Vector3 posicion = CoordenadasHelper.JSONaPosicionUnity(fila, columna);
        posicion.y = 0.3f;
        
        GameObject huevo = Instantiate(eggPrefab, posicion, Quaternion.identity, transform);
        huevo.name = $"Egg_{fila}_{columna}";
        
        string key = $"{fila},{columna}";
        huevos[key] = huevo;
    }
    
    private void CrearPuerta(PuertaData puerta)
    {
        if (puertaPrefab == null) return;
        
        Vector3 posBase = CoordenadasHelper.JSONaPosicionUnity(puerta.fila, puerta.columna);
        Vector3 offset = CoordenadasHelper.ObtenerOffsetPared(puerta.direccion);
        Quaternion rotacion = CoordenadasHelper.ObtenerRotacionDireccion(puerta.direccion);
        
        // Subir la puerta 1.5 unidades como las paredes
        Vector3 posicionPuerta = posBase + offset + new Vector3(0, 1.5f, 0);
        
        GameObject puertaObj = Instantiate(puertaPrefab, posicionPuerta, rotacion, transform);
        puertaObj.name = $"Puerta_{puerta.fila}_{puerta.columna}_{puerta.direccion}";
        
        string key = CoordenadasHelper.GenerarKey(puerta.fila, puerta.columna, puerta.direccion);
        puertas[key] = puertaObj;
    }
    
    private void CrearEntrada(EntradaData entrada)
    {
        if (entradaPrefab == null) return;
        
        Vector3 posBase = CoordenadasHelper.JSONaPosicionUnity(entrada.fila, entrada.columna);
        Vector3 offset = CoordenadasHelper.ObtenerOffsetPared(entrada.direccion);
        Quaternion rotacion = CoordenadasHelper.ObtenerRotacionDireccion(entrada.direccion);
        
        // Subir la entrada 1.5 unidades como las paredes
        Vector3 posicionEntrada = posBase + offset + new Vector3(0, 1.5f, 0);
        
        GameObject entradaObj = Instantiate(entradaPrefab, posicionEntrada, rotacion, transform);
        entradaObj.name = $"Entrada_{entrada.fila}_{entrada.columna}_{entrada.direccion}";
    }
    
    // Métodos públicos para acceder a los objetos
    public static GameObject ObtenerPared(int fila, int columna, string direccion)
    {
        string key = CoordenadasHelper.GenerarKey(fila, columna, direccion);
        return paredes.ContainsKey(key) ? paredes[key] : null;
    }
    
    public static GameObject ObtenerPuerta(int fila, int columna, string direccion)
    {
        string key = CoordenadasHelper.GenerarKey(fila, columna, direccion);
        return puertas.ContainsKey(key) ? puertas[key] : null;
    }
    
    public static GameObject ObtenerTripulacion(int id)
    {
        return tripulacion.ContainsKey(id) ? tripulacion[id] : null;
    }
    
    // Alias para compatibilidad
    public static GameObject ObtenerCrew(int id)
    {
        return ObtenerTripulacion(id);
    }
    
    public static GameObject ObtenerPOI(int id)
    {
        return puntosInteres.ContainsKey(id) ? puntosInteres[id] : null;
    }
    
    public static GameObject ObtenerPOIPorPosicion(int fila, int columna)
    {
        // Buscar POI en la posición especificada
        foreach (var kvp in puntosInteres)
        {
            GameObject poi = kvp.Value;
            if (poi != null)
            {
                Vector3 posEsperada = CoordenadasHelper.JSONaPosicionUnity(fila, columna);
                posEsperada.y = 0.5f;
                
                // Comparar posiciones (con tolerancia para flotantes)
                if (Vector3.Distance(poi.transform.position, posEsperada) < 0.1f)
                {
                    return poi;
                }
            }
        }
        return null;
    }
    
    public static GameObject ObtenerSpider(int fila, int columna)
    {
        string key = $"{fila},{columna}";
        return spiders.ContainsKey(key) ? spiders[key] : null;
    }
    
    public static GameObject ObtenerHuevo(int fila, int columna)
    {
        string key = $"{fila},{columna}";
        return huevos.ContainsKey(key) ? huevos[key] : null;
    }
    
    // Métodos públicos para crear dinámicamente durante la simulación
    public GameObject CrearHuevoDinamico(int fila, int columna)
    {
        if (eggPrefab == null)
        {
            Debug.LogWarning("⚠️ eggPrefab no asignado en TableroBuilder");
            return null;
        }
        
        Vector3 posicion = CoordenadasHelper.JSONaPosicionUnity(fila, columna);
        posicion.y = 0.3f;
        
        GameObject huevo = Instantiate(eggPrefab, posicion, Quaternion.identity, transform);
        huevo.name = $"Egg_{fila}_{columna}_dinamico";
        
        string key = $"{fila},{columna}";
        huevos[key] = huevo;
        
        Debug.Log($"🥚 Huevo creado dinámicamente en ({fila},{columna})");
        return huevo;
    }
    
    public GameObject CrearSpiderDinamica(int fila, int columna)
    {
        if (spiderPrefab == null)
        {
            Debug.LogWarning("⚠️ spiderPrefab no asignado en TableroBuilder");
            return null;
        }
        
        Vector3 posicion = CoordenadasHelper.JSONaPosicionUnity(fila, columna);
        posicion.y = 1f;
        
        GameObject spider = Instantiate(spiderPrefab, posicion, Quaternion.identity, transform);
        spider.name = $"Spider_{fila}_{columna}_dinamico";
        
        string key = $"{fila},{columna}";
        spiders[key] = spider;
        
        Debug.Log($"🕷️ Araña creada dinámicamente en ({fila},{columna})");
        return spider;
    }
    
    public static GameObject CrearPOIDinamico(int fila, int columna)
    {
        if (instancia == null || instancia.poiPrefab == null)
        {
            Debug.LogWarning("⚠️ No se puede crear POI: instancia o poiPrefab no disponible");
            return null;
        }
        
        Vector3 posicion = CoordenadasHelper.JSONaPosicionUnity(fila, columna);
        posicion.y = 0.5f;
        
        GameObject poiObj = Instantiate(instancia.poiPrefab, posicion, Quaternion.identity, instancia.transform);
        poiObj.name = $"POI_{fila}_{columna}_dinamico";
        
        // Asignar un ID único basado en la posición
        int uniqueId = fila * 100 + columna;
        puntosInteres[uniqueId] = poiObj;
        
        Debug.Log($"🆕 POI creado dinámicamente en ({fila},{columna}) con ID temporal {uniqueId}");
        return poiObj;
    }
    
    public static void RemoverHuevoDelDiccionario(int fila, int columna)
    {
        string key = $"{fila},{columna}";
        if (huevos.ContainsKey(key))
        {
            huevos.Remove(key);
        }
    }
    
    public static void RemoverSpiderDelDiccionario(int fila, int columna)
    {
        string key = $"{fila},{columna}";
        if (spiders.ContainsKey(key))
        {
            spiders.Remove(key);
        }
    }
    
    private void LimpiarDiccionarios()
    {
        paredes.Clear();
        puertas.Clear();
        tripulacion.Clear();
        puntosInteres.Clear();
        spiders.Clear();
        huevos.Clear();
    }
}
