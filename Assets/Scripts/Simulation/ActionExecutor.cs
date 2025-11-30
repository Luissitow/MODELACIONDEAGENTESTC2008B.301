using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ejecuta las acciones del JSON sobre la escena de Unity
/// Este es el "cerebro" que interpreta comandos y los ejecuta visualmente
/// </summary>
public class ActionExecutor : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ConstructorTablero constructorTablero;
    [SerializeField] private GameManager gameManager;
    
    [Header("Configuración")]
    #pragma warning disable 0414 // Campo asignado pero no usado (reservado para futuras animaciones)
    [SerializeField] private float tiempoAnimacionMovimiento = 0.5f;
    #pragma warning restore 0414
    [SerializeField] private bool mostrarDebugLogs = true;

    // Cache de referencias para acceso rápido
    private Dictionary<int, GameObject> astronautasCache = new Dictionary<int, GameObject>();
    private Dictionary<string, GameObject> paredesCache = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> celdasCache = new Dictionary<string, GameObject>();

    void Start()
    {
        // No inicializar aquí, esperar a que SimulacionPlayer construya el tablero
    }

    /// <summary>
    /// Inicializa el cache de referencias a GameObjects en la escena
    /// </summary>
    void InicializarCache()
    {
        Debug.Log("🔧 ActionExecutor: Inicializando cache de referencias...");

        // Verificar referencias críticas
        if (constructorTablero == null)
        {
            Debug.LogError("❌ ConstructorTablero es NULL en ActionExecutor - ASIGNAR EN INSPECTOR");
        }
        else
        {
            Debug.Log($"✅ ConstructorTablero asignado correctamente (tamanioCelda={constructorTablero.tamanioCelda})");
        }

        if (gameManager == null)
        {
            Debug.LogWarning("⚠️ GameManager es NULL en ActionExecutor");
        }

        // Limpiar cache anterior
        astronautasCache.Clear();
        paredesCache.Clear();

        // Cachear astronautas por ID
        GameObject[] astronautas = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"🔍 Buscando astronautas con tag 'Player': {astronautas.Length} encontrados");
        
        foreach (var astronauta in astronautas)
        {
            var controller = astronauta.GetComponent<AstronautController>();
            if (controller != null)
            {
                astronautasCache[controller.astronautaID] = astronauta;
                Debug.Log($"  ✓ Astronauta ID {controller.astronautaID} encontrado: {astronauta.name}");
            }
            else
            {
                Debug.LogWarning($"  ⚠️ GameObject '{astronauta.name}' tiene tag Player pero NO tiene AstronautController");
            }
        }

        // Cachear paredes por posición+dirección
        GameObject[] paredes = GameObject.FindGameObjectsWithTag("Wall");
        Debug.Log($"🔍 Buscando paredes con tag 'Wall': {paredes.Length} encontrados");
        
        foreach (var pared in paredes)
        {
            var wallController = pared.GetComponent<Wall>();
            if (wallController != null)
            {
                string key = GenerarKeyPared(wallController.fila, wallController.columna, wallController.direccion);
                paredesCache[key] = pared;
            }
        }

        Debug.Log($"✅ Cache inicializado: {astronautasCache.Count} astronautas, {paredesCache.Count} paredes");
    }

    /// <summary>
    /// Reinicializa el cache (llamado por SimulacionPlayer después de construir tablero)
    /// </summary>
    public void ReinicializarCache()
    {
        InicializarCache();
    }

    /// <summary>
    /// Ejecuta una lista de acciones del JSON
    /// </summary>
    public IEnumerator EjecutarAcciones(List<AccionData> acciones)
    {
        if (acciones == null || acciones.Count == 0)
        {
            if (mostrarDebugLogs)
                Debug.Log("⏭️ No hay acciones para ejecutar este turno");
            yield break;
        }

        if (mostrarDebugLogs)
            Debug.Log($"🎬 Ejecutando {acciones.Count} acciones...");

        foreach (var accion in acciones)
        {
            yield return EjecutarAccion(accion);
        }
    }

    /// <summary>
    /// Ejecuta una acción individual
    /// </summary>
    IEnumerator EjecutarAccion(AccionData accion)
    {
        if (mostrarDebugLogs)
        {
            Debug.Log($"🔍 Ejecutando acción tipo '{accion.tipo}' para astronauta ID {accion.astronautaID}");
            // DEBUGGING CRÍTICO: Verificar valores de deserialización
            accion.LogValues("EjecutarAccion");
        }

        if (!astronautasCache.ContainsKey(accion.astronautaID))
        {
            Debug.LogWarning($"⚠️ No se encontró astronauta con ID {accion.astronautaID} en cache");
            Debug.LogWarning($"⚠️ Cache contiene {astronautasCache.Count} astronautas: {string.Join(", ", astronautasCache.Keys)}");
            yield break;
        }

        GameObject astronauta = astronautasCache[accion.astronautaID];
        
        if (astronauta == null)
        {
            Debug.LogError($"❌ Astronauta ID {accion.astronautaID} existe en cache pero es NULL");
            yield break;
        }

        switch (accion.tipo.ToLower())
        {
            case "mover":
            case "move":
                yield return EjecutarMovimiento(astronauta, accion);
                break;

            case "romper_pared":
            case "break_wall":
                yield return RomperPared(astronauta, accion);
                break;

            case "danar_pared":
            case "atacar_pared":
            case "attack_wall":
                yield return AtacarPared(astronauta, accion);
                break;

            case "abrir_puerta":
            case "open_door":
                yield return AbrirPuerta(astronauta, accion);
                break;

            case "apagar_fuego":
            case "extinguish_fire":
                yield return ApagarFuego(astronauta, accion);
                break;

            case "rescatar_victima":
            case "rescue_victim":
                yield return RescatarVictima(astronauta, accion);
                break;

            case "atacar":
            case "attack":
            case "atacar_arana":
                yield return AtacarArana(astronauta, accion);
                break;

            default:
                Debug.LogWarning($"⚠️ Tipo de acción desconocida: {accion.tipo}");
                break;
        }
    }

    /// <summary>
    /// Mueve un astronauta de una posición a otra con animación
    /// </summary>
    IEnumerator EjecutarMovimiento(GameObject astronauta, AccionData accion)
    {
        if (mostrarDebugLogs)
            Debug.Log($"🔍 [EjecutarMovimiento] Iniciando para astronauta ID {accion.astronautaID}");

        if (accion.hacia == null)
        {
            Debug.LogWarning("⚠️ Acción de movimiento sin destino");
            yield break;
        }

        if (astronauta == null)
        {
            Debug.LogError($"❌ GameObject astronauta es NULL para ID {accion.astronautaID}");
            yield break;
        }

        if (mostrarDebugLogs)
            Debug.Log($"🚶 Astronauta {accion.astronautaID}: ({accion.desde.fila},{accion.desde.columna}) → ({accion.hacia.fila},{accion.hacia.columna})");

        // Usar el método MoverA del AstronautController
        AstronautController controller = astronauta.GetComponent<AstronautController>();
        
        if (controller == null)
        {
            Debug.LogError($"❌ No se encontró AstronautController en astronauta {accion.astronautaID}");
            yield break;
        }

        if (constructorTablero == null)
        {
            Debug.LogError($"❌ ConstructorTablero es NULL en ActionExecutor");
            yield break;
        }

        if (mostrarDebugLogs)
            Debug.Log($"✅ Moviendo astronauta con tamanioCelda={constructorTablero.tamanioCelda}");

        yield return controller.MoverA(accion.hacia.fila, accion.hacia.columna, constructorTablero.tamanioCelda);
    }
 
    /// <summary>
    /// Rompe una pared (2 de daño)
    /// </summary>
    IEnumerator RomperPared(GameObject astronauta, AccionData accion)
    {
        // Obtener fila, columna, direccion desde pared o directamente
        int fila = accion.pared != null ? accion.pared.fila : accion.fila;
        int columna = accion.pared != null ? accion.pared.columna : accion.columna;
        string direccion = accion.pared != null ? accion.pared.direccion : accion.direccion;
        
        if (string.IsNullOrEmpty(direccion))
        {
            Debug.LogWarning("⚠️ Acción romper_pared sin dirección especificada");
            yield break;
        }

        string keyPared = GenerarKeyPared(fila, columna, direccion);
        
        if (!paredesCache.ContainsKey(keyPared))
        {
            Debug.LogWarning($"⚠️ No se encontró pared en {keyPared}");
            yield break;
        }

        GameObject pared = paredesCache[keyPared];
        
        // Cache el componente Wall ANTES de destrucción
        Wall wallController = pared != null ? pared.GetComponent<Wall>() : null;

        if (wallController != null)
        {
            Debug.Log($"💥🔨 Astronauta {accion.astronautaID} ROMPE pared en ({fila},{columna}) {direccion}");
            
            // Animar astronauta atacando
            AstronautController astronautaController = astronauta.GetComponent<AstronautController>();
            if (astronautaController != null)
            {
                yield return astronautaController.AnimarAtaque(pared.transform.position, 0.4f);
            }

            wallController.Romper(); // 2 de daño - destruye instantáneamente
            
            // Efecto visual de impacto doble
            CrearEfectoImpacto(pared.transform.position);
            yield return new WaitForSeconds(0.1f);
            CrearEfectoImpacto(pared.transform.position + Vector3.up * 0.3f);
            
            yield return new WaitForSeconds(0.4f); // Pausa para ver destrucción
            
            // Actualizar cache si el objeto sigue existiendo después del cambio de prefab
            if (wallController != null && wallController.gameObject != null)
            {
                paredesCache[keyPared] = wallController.gameObject;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Ataca una pared (1 de daño)
    /// </summary>
    IEnumerator AtacarPared(GameObject astronauta, AccionData accion)
    {
        // Obtener fila, columna, direccion desde pared o directamente
        // Si pared existe Y tiene valores válidos, usarla; sino usar propiedades directas
        int fila = (accion.pared != null && accion.pared.fila >= 0) ? accion.pared.fila : accion.fila;
        int columna = (accion.pared != null && accion.pared.columna >= 0) ? accion.pared.columna : accion.columna;
        string direccion = (accion.pared != null && !string.IsNullOrEmpty(accion.pared.direccion)) ? accion.pared.direccion : accion.direccion;
        
        if (string.IsNullOrEmpty(direccion))
        {
            Debug.LogWarning($"⚠️ Acción atacar_pared/danar_pared sin dirección especificada (fila={accion.fila}, columna={accion.columna}, direccion='{accion.direccion}')");
            yield break;
        }

        string keyPared = GenerarKeyPared(fila, columna, direccion);
        
        if (!paredesCache.ContainsKey(keyPared))
        {
            Debug.LogWarning($"⚠️ No se encontró pared en {keyPared}");
            yield break;
        }

        GameObject pared = paredesCache[keyPared];
        
        // CRÍTICO: Cache el componente Wall ANTES de cualquier operación que pueda destruir el GameObject
        Wall wallController = pared != null ? pared.GetComponent<Wall>() : null;

        if (wallController != null)
        {
            if (mostrarDebugLogs)
                Debug.Log($"⚔️ Astronauta {accion.astronautaID} ATACA pared en ({fila},{columna}) {direccion}");

            // Animar astronauta atacando hacia la pared
            AstronautController astronautaController = astronauta.GetComponent<AstronautController>();
            if (astronautaController != null)
            {
                yield return astronautaController.AnimarAtaque(pared.transform.position, 0.4f);
            }

            // Aplicar daño a la pared
            // Puertas reciben 1 de daño (atacar), paredes normales reciben 2 (romper)
            if (wallController.tipo == TipoPared.Puerta)
            {
                Debug.Log($"🚪 Atacando PUERTA (1 de daño)");
                wallController.Atacar(); // 1 de daño
            }
            else
            {
                Debug.Log($"🧱 Atacando PARED NORMAL (2 de daño - romper)");
                wallController.Romper(); // 2 de daño - destruye pared en 1 golpe
            }
            
            // Efecto visual de impacto (partículas, shake)
            CrearEfectoImpacto(pared.transform.position);
            yield return new WaitForSeconds(0.3f); // Pausa para ver efecto
            
            // Actualizar cache si la pared cambió de prefab
            if (wallController != null && wallController.gameObject != null)
            {
                paredesCache[keyPared] = wallController.gameObject;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// Crea efecto visual de impacto en una posición
    /// </summary>
    void CrearEfectoImpacto(Vector3 posicion)
    {
        // TODO: Instanciar prefab de partículas cuando esté disponible
        // Por ahora, solo debug visual
        Debug.Log($"💥 Efecto de impacto en {posicion}");
        
        // Crear un flash temporal (esfera roja que desaparece)
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.transform.position = posicion;
        flash.transform.localScale = Vector3.one * 0.5f;
        
        Renderer renderer = flash.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f, 0.3f, 0f, 0.8f); // Naranja brillante
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.red * 2f);
            renderer.material = mat;
        }
        
        // Destruir después de 0.2 segundos
        Destroy(flash, 0.2f);
    }

    /// <summary>
    /// Abre una puerta
    /// </summary>
    IEnumerator AbrirPuerta(GameObject astronauta, AccionData accion)
    {
        // Obtener fila, columna, direccion desde pared o directamente
        // Priorizar campos directos (fila, columna, direccion) sobre pared anidado
        int fila = accion.fila;
        int columna = accion.columna;
        string direccion = accion.direccion;
        
        Debug.Log($"🔍 [AbrirPuerta] AccionData recibida: fila={fila}, columna={columna}, direccion='{direccion}'");
        
        // Si no están en campos directos, buscar en pared anidado
        if (fila == 0 && columna == 0 && string.IsNullOrEmpty(direccion) && accion.pared != null)
        {
            Debug.Log($"⚠️ [AbrirPuerta] Coordenadas principales vacías, usando accion.pared");
            fila = accion.pared.fila;
            columna = accion.pared.columna;
            direccion = accion.pared.direccion;
        }
        
        Debug.Log($"🔍 [AbrirPuerta] Buscando puerta en ({fila},{columna}) dirección: {direccion}");
        Debug.Log($"🔍 [AbrirPuerta] Total paredes en cache: {paredesCache.Count}");
        
        if (string.IsNullOrEmpty(direccion))
        {
            Debug.LogWarning("⚠️ Acción abrir_puerta sin dirección especificada");
            
            // Intentar buscar puerta en cualquier dirección desde esta celda
            string[] direcciones = { "norte", "sur", "este", "oeste" };
            foreach (string dir in direcciones)
            {
                string key = GenerarKeyPared(fila, columna, dir);
                if (paredesCache.ContainsKey(key))
                {
                    GameObject obj = paredesCache[key];
                    Wall wall = obj.GetComponent<Wall>();
                    if (wall != null && wall.tipo == TipoPared.Puerta)
                    {
                        Debug.Log($"✅ Puerta encontrada en dirección {dir}");
                        direccion = dir;
                        break;
                    }
                }
            }
            
            if (string.IsNullOrEmpty(direccion))
            {
                Debug.LogError($"❌ No se encontró ninguna puerta en ({fila},{columna})");
                yield break;
            }
        }

        string keyPared = GenerarKeyPared(fila, columna, direccion);
        Debug.Log($"🔑 [AbrirPuerta] Key buscada: '{keyPared}'");
        
        if (!paredesCache.ContainsKey(keyPared))
        {
            Debug.LogWarning($"⚠️ [AbrirPuerta] No se encontró puerta con key: '{keyPared}' - Intentando corrección automática...");
            
            // Buscar puerta en celdas adyacentes (puede ser error de coordenadas en JSON)
            string puertaCorrecta = null;
            GameObject puertaObj = null;
            
            // Intentar direcciones opuestas y celdas adyacentes
            string[] direccionesAlternas = ObtenerDireccionesAlternas(direccion);
            int[] filasAdyacentes = { fila - 1, fila, fila + 1 };
            int[] columnasAdyacentes = { columna - 1, columna, columna + 1 };
            
            foreach (int f in filasAdyacentes)
            {
                foreach (int c in columnasAdyacentes)
                {
                    foreach (string dir in direccionesAlternas)
                    {
                        string keyAlterna = GenerarKeyPared(f, c, dir);
                        if (paredesCache.ContainsKey(keyAlterna))
                        {
                            Wall w = paredesCache[keyAlterna].GetComponent<Wall>();
                            if (w != null && w.tipo == TipoPared.Puerta)
                            {
                                Debug.Log($"✅ [AbrirPuerta] CORREGIDO: Encontrada puerta en '{keyAlterna}' (original: '{keyPared}')");
                                puertaCorrecta = keyAlterna;
                                puertaObj = paredesCache[keyAlterna];
                                fila = f;
                                columna = c;
                                direccion = dir;
                                goto FoundDoor;
                            }
                        }
                    }
                }
            }
            
            FoundDoor:
            if (puertaObj == null)
            {
                Debug.LogError($"❌ [AbrirPuerta] No se encontró ninguna puerta cerca de ({fila},{columna}) {direccion}");
                Debug.Log($"📜 [AbrirPuerta] Puertas disponibles en cache:");
                foreach (var kvp in paredesCache)
                {
                    Wall w = kvp.Value.GetComponent<Wall>();
                    if (w != null && w.tipo == TipoPared.Puerta)
                    {
                        Debug.Log($"  📌 '{kvp.Key}' → Puerta en ({w.fila},{w.columna}) {w.direccion}");
                    }
                }
                yield break;
            }
            
            keyPared = puertaCorrecta;
        }

        GameObject puerta = paredesCache[keyPared];
        Wall wallController = puerta.GetComponent<Wall>();

        if (wallController != null && wallController.tipo == TipoPared.Puerta)
        {
            Debug.Log($"🚪✨ Astronauta {accion.astronautaID} abre puerta en ({fila},{columna}) {direccion}");
            
            // AbrirPuerta() inicia automáticamente la animación con StartCoroutine
            wallController.AbrirPuerta();
            
            // Esperar un poco para que se vea la animación (0.8s de animación + 0.2s buffer)
            yield return new WaitForSeconds(1.0f);
        }
        else
        {
            Debug.LogWarning($"⚠️ Objeto en {keyPared} no es una puerta o no tiene Wall component");
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Apaga fuego en una celda
    /// </summary>
    IEnumerator ApagarFuego(GameObject astronauta, AccionData accion)
    {
        if (mostrarDebugLogs)
            Debug.Log($"🔥 Astronauta {accion.astronautaID} apaga fuego (pendiente de implementar)");

        // TODO: Implementar cuando tengamos sistema de fuego
        yield return new WaitForSeconds(0.3f);
    }

    /// <summary>
    /// Rescata una víctima
    /// </summary>
    IEnumerator RescatarVictima(GameObject astronauta, AccionData accion)
    {
        if (mostrarDebugLogs)
            Debug.Log($"👤 Astronauta {accion.astronautaID} rescata víctima (pendiente de implementar)");

        // TODO: Implementar cuando tengamos sistema de víctimas
        if (gameManager != null)
        {
            gameManager.victimasRescatadas++;
        }

        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Ataca a una araña (o apaga fuego - mismo sistema)
    /// </summary>
    IEnumerator AtacarArana(GameObject astronauta, AccionData accion)
    {
        if (accion.celda == null)
        {
            Debug.LogWarning("⚠️ Acción atacar sin datos de celda");
            yield break;
        }

        string keyCelda = $"{accion.celda.fila}_{accion.celda.columna}";
        
        if (!celdasCache.ContainsKey(keyCelda))
        {
            Debug.LogWarning($"⚠️ No se encontró celda en ({accion.celda.fila},{accion.celda.columna})");
            yield break;
        }

        GameObject celdaObj = celdasCache[keyCelda];
        CeldaTablero celda = celdaObj.GetComponent<CeldaTablero>();

        if (celda != null)
        {
            if (mostrarDebugLogs)
                Debug.Log($"⚔️ Astronauta {accion.astronautaID} ataca araña en ({accion.celda.fila},{accion.celda.columna})");

            celda.AtacarArana();
        }

        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Genera una key única para cachear paredes
    /// </summary>
    string GenerarKeyPared(int fila, int columna, string direccion)
    {
        return $"{fila}_{columna}_{direccion}";
    }
    
    /// <summary>
    /// Obtiene direcciones alternas para buscar puertas (incluye opuesta y adyacentes)
    /// </summary>
    string[] ObtenerDireccionesAlternas(string direccionOriginal)
    {
        // Priorizar la dirección original, luego opuesta, luego otras
        switch (direccionOriginal.ToLower())
        {
            case "norte":
                return new[] { "norte", "sur", "este", "oeste" };
            case "sur":
                return new[] { "sur", "norte", "este", "oeste" };
            case "este":
                return new[] { "este", "oeste", "norte", "sur" };
            case "oeste":
                return new[] { "oeste", "este", "norte", "sur" };
            default:
                return new[] { "norte", "sur", "este", "oeste" };
        }
    }

    /// <summary>
    /// Actualiza el estado completo de la escena desde el JSON
    /// NOTA: Método simplificado - SimulacionPlayer maneja el estado a través de acciones
    /// </summary>
    public void ActualizarEstadoCompleto(EscenarioData escenario)
    {
        if (escenario == null) return;

        // TODO: Implementar si se necesita actualizar estado directo desde EscenarioData
        // Por ahora comentado porque EscenarioData no tiene estructura jugadores/mapa
        /*
        // Actualizar posiciones de astronautas
        if (escenario.jugadores != null)
        {
            foreach (var jugador in escenario.jugadores)
            {
                if (astronautasCache.ContainsKey(jugador.id))
                {
                    GameObject astronauta = astronautasCache[jugador.id];
                    
                    Vector3 nuevaPosicion = new Vector3(
                        jugador.columna * constructorTablero.tamanioCelda,
                        astronauta.transform.position.y,
                        jugador.fila * constructorTablero.tamanioCelda
                    );

                    astronauta.transform.position = nuevaPosicion;

                    var controller = astronauta.GetComponent<AstronautController>();
                    if (controller != null)
                    {
                        controller.filaActual = jugador.fila;
                        controller.columnaActual = jugador.columna;
                    }
                }
            }
        }

        // Actualizar estado de paredes
        if (escenario.mapa?.paredes != null)
        {
            foreach (var paredData in escenario.mapa.paredes)
            {
                string key = GenerarKeyPared(paredData.fila, paredData.columna, paredData.direccion);
                
                if (paredesCache.ContainsKey(key))
                {
                    Wall wallController = paredesCache[key].GetComponent<Wall>();
                    if (wallController != null)
                    {
                        wallController.vidaActual = paredData.vida;
                        
                        if (paredData.destruida)
                        {
                            paredesCache[key].SetActive(false);
                        }
                        else if (paredData.tipo == "puerta" && paredData.abierta)
                        {
                            wallController.AbrirPuerta();
                        }
                    }
                }
            }
        }
        */

        if (mostrarDebugLogs)
            Debug.Log($"🔄 ActualizarEstadoCompleto llamado (método comentado temporalmente)");
    }
}

/// <summary>
/// Estructura de datos para una acción del JSON
/// </summary>
[System.Serializable]
public class AccionData
{
    public int astronautaID;
    public string tipo;
    public PosicionData desde;
    public PosicionData hacia;
    public ParedAccionData pared;
    public PosicionData celda; // Para acciones como apagar_fuego, atacar_arana
    public int costo;
    
    // Propiedades directas para acciones de pared (compatibilidad JSON plano)
    public int fila;
    public int columna;
    public string direccion;
    
    // Constructor para debugging
    public void LogValues(string context)
    {
        Debug.Log($"[{context}] AccionData: tipo={tipo}, astronautaID={astronautaID}, fila={fila}, columna={columna}, direccion='{direccion}', desde={(desde != null ? $"({desde.fila},{desde.columna})" : "null")}, hacia={(hacia != null ? $"({hacia.fila},{hacia.columna})" : "null")}");
    }
}

/// <summary>
/// Datos de posición
/// </summary>
[System.Serializable]
public class PosicionData
{
    public int fila;
    public int columna;
}

/// <summary>
/// Datos de pared para acciones
/// </summary>
[System.Serializable]
public class ParedAccionData
{
    public int fila;
    public int columna;
    public string direccion;
    public string tipo;
    public int vida;
    public bool destruida;
    public bool abierta;
}
