using UnityEngine;
using System.Collections;
using FireRescue.Components;

public class SimulacionRunner : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Pausa entre turnos (segundos)")]
    public float tiempoEntreTurnos = 0.1f;
    [Tooltip("Pausa entre acciones individuales (segundos)")]
    public float tiempoEntreAcciones = 0.05f;
    
    [Header("Prefabs de Efectos")]
    [Tooltip("Prefab opcional para explosión (si no se asigna, usa esfera simple)")]
    public GameObject explosionPrefab;
    
    private EscenarioData escenario;
    
    /// <summary>
    /// Inicia la simulación ejecutando todos los turnos secuencialmente
    /// </summary>
    public void IniciarSimulacion(EscenarioData escenarioData)
    {
        if (escenarioData == null || escenarioData.turnos == null)
        {
            Debug.LogError("Escenario inválido para simulación");
            return;
        }
        
        escenario = escenarioData;
        
        Debug.Log($"🎮 Iniciando simulación con {escenario.turnos.Length} turnos");
        
        StartCoroutine(EjecutarSimulacion());
    }
    
    private IEnumerator EjecutarSimulacion()
    {
        foreach (TurnoData turno in escenario.turnos)
        {
            yield return StartCoroutine(EjecutarTurno(turno));
            yield return new WaitForSeconds(tiempoEntreTurnos);
        }
        
        Debug.Log("✅ Simulación completada");
        MostrarResultadoFinal();
    }
    
    private IEnumerator EjecutarTurno(TurnoData turno)
    {
        Debug.Log($"================================================================================");
        Debug.Log($"TURNO {turno.numero_turno} - INICIO");
        Debug.Log($"================================================================================");
        
        // Contar estado actual
        ContarEstadoActual($"ANTES del turno {turno.numero_turno}");
        
        // Verificar si usa la nueva estructura intercalada
        if (turno.secuencia != null && turno.secuencia.Length > 0)
        {
            Debug.Log($"📋 Ejecutando turno con secuencia intercalada ({turno.secuencia.Length} elementos)");
            yield return StartCoroutine(EjecutarSecuenciaIntercalada(turno.secuencia));
        }
        else
        {
            // Compatibilidad con JSONs antiguos (estructura separada)
            Debug.LogWarning("⚠️ Usando estructura antigua (fase_dados + fase_accion separadas)");
            
            // Fase 1: Dados (propagación)
            if (turno.fase_dados != null)
            {
                Debug.Log($"🎲 FASE DE DADOS - {turno.fase_dados.tiradas.Length} tiradas");
                yield return StartCoroutine(EjecutarFaseDados(turno.fase_dados));
                ContarEstadoActual($"DESPUÉS de fase de dados");
            }
            
            // Fase 2: Acciones de jugadores
            if (turno.fase_accion != null)
            {
                Debug.Log($"⚡ FASE DE ACCIÓN - {turno.fase_accion.acciones.Length} acciones");
                yield return StartCoroutine(EjecutarFaseAccion(turno.fase_accion));
                ContarEstadoActual($"DESPUÉS de fase de acción");
            }
            
            // Fase 3: Aplicar cambios del mapa
            if (turno.cambios_mapa != null)
            {
                yield return StartCoroutine(AplicarCambiosMapa(turno.cambios_mapa));
            }
        }
        
        // Mostrar estado del juego
        if (turno.estado_juego != null)
        {
            MostrarEstadoJuego(turno.estado_juego);
        }
        
        Debug.Log($"================================================================================");
        Debug.Log($"TURNO {turno.numero_turno} - FIN");
        Debug.Log($"================================================================================\n");
    }
    
    private IEnumerator EjecutarSecuenciaIntercalada(SecuenciaData[] secuencia)
    {
        foreach (var elemento in secuencia)
        {
            if (elemento.tipo == "acciones_jugador")
            {
                Debug.Log($"👤 Jugador {elemento.jugador_id} - {elemento.acciones.Length} acciones");
                foreach (var accion in elemento.acciones)
                {
                    yield return StartCoroutine(EjecutarAccion(accion));
                    yield return new WaitForSeconds(tiempoEntreAcciones);
                }
            }
            else if (elemento.tipo == "tirada_dado")
            {
                Debug.Log($"🎲 Tirada de dado en ({elemento.tirada.fila},{elemento.tirada.columna})");
                yield return StartCoroutine(EjecutarTiradaDado(elemento.tirada));
            }
            else
            {
                Debug.LogWarning($"⚠️ Tipo de secuencia desconocido: {elemento.tipo}");
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private IEnumerator EjecutarTiradaDado(TiradaDadoData tirada)
    {
        Debug.Log($"Dado en ({tirada.fila},{tirada.columna}): {tirada.estado_anterior} → {tirada.estado_nuevo}");
        
        // Procesar según el tipo de evento
        switch (tirada.estado_nuevo.ToLower())
        {
            case "huevo":
                yield return StartCoroutine(AparecerHuevo(tirada.fila, tirada.columna));
                break;
                
            case "araña":
            case "arana":
                yield return StartCoroutine(EvolucionarHuevo(tirada.fila, tirada.columna));
                break;
                
            case "explosion":
            case "explosión":
                yield return StartCoroutine(ExplotarSpider(tirada.fila, tirada.columna));
                break;
                
            default:
                Debug.LogWarning($"⚠️ Estado de dado desconocido: {tirada.estado_nuevo}");
                break;
        }
        
        // Aplicar cambios de esta tirada
        if (tirada.cambios != null)
        {
            yield return StartCoroutine(AplicarCambiosMapa(tirada.cambios));
        }
        
        yield return new WaitForSeconds(0.1f);
    }
    
    private void ContarEstadoActual(string momento)
    {
        int numAranas = 0;
        int numHuevos = 0;
        
        // Contar todas las arañas en la escena
        Spider[] spiders = FindObjectsByType<Spider>(FindObjectsSortMode.None);
        numAranas = spiders.Length;
        
        // Contar todos los huevos en la escena
        Egg[] eggs = FindObjectsByType<Egg>(FindObjectsSortMode.None);
        numHuevos = eggs.Length;
        
        Debug.Log($"📊 {momento}: {numAranas} arañas, {numHuevos} huevos");
    }
    
    private IEnumerator EjecutarFaseDados(FaseDadosData fase)
    {
        Debug.Log("🎲 Fase de dados - Propagación");
        
        foreach (var tirada in fase.tiradas)
        {
            Debug.Log($"Dado en ({tirada.fila},{tirada.columna}): {tirada.estado_anterior} → {tirada.estado_nuevo}");
            
            // Procesar según el tipo de evento
            switch (tirada.estado_nuevo.ToLower())
            {
                case "huevo":
                    // Aparecer nuevo huevo
                    yield return StartCoroutine(AparecerHuevo(tirada.fila, tirada.columna));
                    break;
                    
                case "araña":
                case "arana":
                    // Evolucionar huevo a araña
                    yield return StartCoroutine(EvolucionarHuevo(tirada.fila, tirada.columna));
                    break;
                    
                case "explosion":
                case "explosión":
                    // Araña explota
                    yield return StartCoroutine(ExplotarSpider(tirada.fila, tirada.columna));
                    break;
                    
                default:
                    Debug.LogWarning($"⚠️ Estado de dado desconocido: {tirada.estado_nuevo}");
                    break;
            }
            
            // Aplicar cambios de esta tirada (ej: explosiones dañan paredes)
            if (tirada.cambios != null)
            {
                yield return StartCoroutine(AplicarCambiosMapa(tirada.cambios));
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private IEnumerator EjecutarFaseAccion(FaseAccionData fase)
    {
        Debug.Log("⚡ Fase de acción");
        
        foreach (var accion in fase.acciones)
        {
            yield return StartCoroutine(EjecutarAccion(accion));
            yield return new WaitForSeconds(tiempoEntreAcciones);
        }
    }
    
    private IEnumerator EjecutarAccion(AccionData accion)
    {
        GameObject crew = TableroBuilder.ObtenerTripulacion(accion.tripulacion_id);
        
        if (crew == null)
        {
            Debug.LogWarning($"Tripulación {accion.tripulacion_id} no encontrada");
            yield break;
        }
        
        Debug.Log($"Crew {accion.tripulacion_id} ejecuta: {accion.tipo}");
        
        switch (accion.tipo)
        {
            case "mover":
                yield return StartCoroutine(AnimarMovimiento(crew, accion.hacia));
                break;
                
            case "apagar_fuego":
                yield return StartCoroutine(AnimarApagarFuego(accion.hacia));
                break;
                
            case "revelar_poi":
                yield return StartCoroutine(AnimarRevelarPOI(accion.poi_id));
                break;
                
            case "recoger_victima":
            case "cargar_victima":
                yield return StartCoroutine(AnimarCargarVictima(accion.tripulacion_id, accion.desde));
                break;
                
            case "depositar_victima":
            case "dejar_victima_en_entrada":
                yield return StartCoroutine(AnimarDepositarVictima(accion.tripulacion_id, accion.hacia));
                break;
                
            case "abrir_puerta":
                // No hacer nada aquí, los cambios se aplican después
                break;
                
            case "eliminar_araña":
                yield return StartCoroutine(AnimarEliminarSpider(accion.desde));
                break;
                
            case "eliminar_huevo":
                yield return StartCoroutine(AnimarEliminarHuevo(accion.desde));
                break;
                
            case "danar_pared":
            case "dañar_pared":
                // Los cambios se aplican en AplicarCambiosMapa
                Debug.Log($"💥 Pared dañada en ({accion.desde.fila},{accion.desde.columna}) dirección {accion.direccion}");
                break;
                
            case "destruir_pared":
                // Los cambios se aplican en AplicarCambiosMapa
                Debug.Log($"💀 Pared destruida en ({accion.desde.fila},{accion.desde.columna}) dirección {accion.direccion}");
                break;
        }
        
        // Aplicar cambios de esta acción específica
        if (accion.cambios != null)
        {
            yield return StartCoroutine(AplicarCambiosMapa(accion.cambios));
        }
    }
    
    private IEnumerator AplicarCambiosMapa(CambiosMapaData cambios)
    {
        Debug.Log("🗺️ Aplicando cambios al mapa");
        
        // Remover huevos apagados
        if (cambios.huevos_removidos != null)
        {
            foreach (var pos in cambios.huevos_removidos)
            {
                GameObject huevo = TableroBuilder.ObtenerHuevo(pos.fila, pos.columna);
                if (huevo != null)
                {
                    Destroy(huevo);
                    Debug.Log($"Huevo apagado en ({pos.fila},{pos.columna})");
                }
            }
        }
        
        // Crear huevos nuevos (de tiradas de dados)
        if (cambios.huevos_nuevos != null)
        {
            TableroBuilder builder = FindFirstObjectByType<TableroBuilder>();
            if (builder != null)
            {
                foreach (var pos in cambios.huevos_nuevos)
                {
                    // Verificar que no haya ya un huevo en esa posición
                    GameObject huevoExistente = TableroBuilder.ObtenerHuevo(pos.fila, pos.columna);
                    if (huevoExistente == null)
                    {
                        builder.CrearHuevoDinamico(pos.fila, pos.columna);
                        Debug.Log($"🥚 Huevo nuevo creado en ({pos.fila},{pos.columna})");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Ya existe huevo en ({pos.fila},{pos.columna}), no se crea duplicado");
                    }
                }
            }
        }
        
        // Remover huevos (evolucionan a arañas o se apagan)
        if (cambios.huevos_removidos != null)
        {
            foreach (var pos in cambios.huevos_removidos)
            {
                GameObject huevo = TableroBuilder.ObtenerHuevo(pos.fila, pos.columna);
                if (huevo != null)
                {
                    Destroy(huevo);
                    TableroBuilder.RemoverHuevoDelDiccionario(pos.fila, pos.columna);
                    Debug.Log($"🥚💀 Huevo removido en ({pos.fila},{pos.columna})");
                }
            }
        }
        
        // Crear arañas nuevas (de explosiones u otras propagaciones)
        if (cambios.arañas_nuevas != null && cambios.arañas_nuevas.Length > 0)
        {
            Debug.Log($"🕷️ Creando {cambios.arañas_nuevas.Length} arañas nuevas por explosión...");
            TableroBuilder builder = FindFirstObjectByType<TableroBuilder>();
            if (builder != null)
            {
                foreach (var pos in cambios.arañas_nuevas)
                {
                    // Verificar que no haya ya una araña en esa posición
                    GameObject spiderExistente = TableroBuilder.ObtenerSpider(pos.fila, pos.columna);
                    if (spiderExistente == null)
                    {
                        builder.CrearSpiderDinamica(pos.fila, pos.columna);
                        Debug.Log($"  ✓ Araña nueva creada en ({pos.fila},{pos.columna})");
                    }
                    else
                    {
                        Debug.LogWarning($"  ⚠️ Ya existe araña en ({pos.fila},{pos.columna}), no se crea duplicada");
                    }
                }
            }
            Debug.Log($"✓ {cambios.arañas_nuevas.Length} arañas nuevas procesadas");
        }
        
        // Remover spiders apagadas
        if (cambios.arañas_removidas != null)
        {
            foreach (var pos in cambios.arañas_removidas)
            {
                GameObject spider = TableroBuilder.ObtenerSpider(pos.fila, pos.columna);
                if (spider != null)
                {
                    Destroy(spider);
                    TableroBuilder.RemoverSpiderDelDiccionario(pos.fila, pos.columna);
                    Debug.Log($"🕷️💀 Spider apagada en ({pos.fila},{pos.columna})");
                }
            }
        }
        
        // Dañar paredes
        if (cambios.paredes_dañadas != null)
        {
            foreach (var pared in cambios.paredes_dañadas)
            {
                GameObject paredObj = TableroBuilder.ObtenerPared(pared.fila, pared.columna, pared.direccion);
                if (paredObj != null)
                {
                    var wallComponent = paredObj.GetComponent<Wall>();
                    if (wallComponent != null)
                    {
                        // Priorizar nivel_dano si existe, sino usar nuevo_estado
                        int danoAplicar = 1;
                        if (pared.nivel_dano > 0)
                        {
                            danoAplicar = pared.nivel_dano;
                        }
                        else
                        {
                            string estado = pared.nuevo_estado?.ToLower() ?? "dañada";
                            if (estado == "destruida" || estado == "destruído")
                            {
                                danoAplicar = 2;
                            }
                        }
                        
                        // Aplicar daño
                        bool destruida = wallComponent.AplicarDano(danoAplicar);
                        
                        if (destruida)
                        {
                            Debug.Log($"💥💀 Pared ({pared.fila},{pared.columna},{pared.direccion}) → DESTRUIDA (nivel {danoAplicar})");
                        }
                        else
                        {
                            Debug.Log($"🧱 Pared ({pared.fila},{pared.columna},{pared.direccion}) → Dañada (grietas, nivel {danoAplicar})");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Pared en ({pared.fila},{pared.columna},{pared.direccion}) no tiene componente Wall");
                    }
                }
                else
                {
                    // La pared no existe en el tablero (probablemente la celda no tiene pared en esa dirección)
                    Debug.Log($"ℹ️ No hay pared en ({pared.fila},{pared.columna},{pared.direccion}) - celda sin pared en esa dirección");
                }
            }
        }
        
        // Destruir paredes (aplicar daño fatal directamente)
        if (cambios.paredes_destruidas != null)
        {
            foreach (var pared in cambios.paredes_destruidas)
            {
                GameObject paredObj = TableroBuilder.ObtenerPared(pared.fila, pared.columna, pared.direccion);
                if (paredObj != null)
                {
                    var wallComponent = paredObj.GetComponent<Wall>();
                    if (wallComponent != null)
                    {
                        // Aplicar daño suficiente para destruir completamente (3 puntos por seguridad)
                        wallComponent.AplicarDano(3);
                        Debug.Log($"💀 Pared ({pared.fila},{pared.columna},{pared.direccion}) → DESTRUIDA completamente");
                    }
                }
                else
                {
                    Debug.Log($"ℹ️ No hay pared en ({pared.fila},{pared.columna},{pared.direccion}) - ya destruida o inexistente");
                }
            }
        }
        
        // Abrir puertas
        if (cambios.puertas_abiertas != null)
        {
            foreach (var puerta in cambios.puertas_abiertas)
            {
                GameObject puertaObj = TableroBuilder.ObtenerPuerta(puerta.fila, puerta.columna, puerta.direccion);
                if (puertaObj != null)
                {
                    var doorComponent = puertaObj.GetComponent<Door>();
                    if (doorComponent != null)
                    {
                        doorComponent.Abrir();
                        Debug.Log($"Puerta abierta en ({puerta.fila},{puerta.columna},{puerta.direccion})");
                    }
                }
            }
        }
        
        // Revelar POIs
        if (cambios.pois_revelados != null)
        {
            foreach (var poi in cambios.pois_revelados)
            {
                GameObject poiObj = TableroBuilder.ObtenerPOI(poi.poi_id);
                if (poiObj != null)
                {
                    var poiComponent = poiObj.GetComponent<POI>();
                    if (poiComponent != null)
                    {
                        poiComponent.Revelar(poi.tipo_revelado);
                        Debug.Log($"POI {poi.poi_id} revelado como {poi.tipo_revelado}");
                    }
                }
            }
        }
        
        yield return new WaitForSeconds(0.1f);
    }
    
    // Animaciones simples
    private IEnumerator AnimarMovimiento(GameObject crew, PosicionData destino)
    {
        Vector3 posInicial = crew.transform.position;
        Vector3 posFinal = CoordenadasHelper.JSONaPosicionUnity(destino.fila, destino.columna);
        posFinal.y = 1f;
        
        float tiempo = 0;
        float duracion = 1f;
        
        while (tiempo < duracion)
        {
            crew.transform.position = Vector3.Lerp(posInicial, posFinal, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        crew.transform.position = posFinal;
    }
    
    private IEnumerator AnimarApagarFuego(PosicionData pos)
    {
        Debug.Log($"💧 Apagando fuego en ({pos.fila},{pos.columna})");
        // TODO: Efecto de partículas de agua
        yield return new WaitForSeconds(0.1f);
    }
    
    private IEnumerator AnimarRevelarPOI(int poiId)
    {
        GameObject poiObj = TableroBuilder.ObtenerPOI(poiId);
        
        if (poiObj == null)
        {
            Debug.LogWarning($"⚠️ No se encontró POI con id {poiId}");
            yield break;
        }
        
        POI poiComponent = poiObj.GetComponent<POI>();
        
        if (poiComponent != null)
        {
            Debug.Log($"🔍 Revelando POI {poiId}");
            
            // El tipo se obtiene de los cambios del mapa, pero por ahora
            // usamos una animación genérica. Se revelará correctamente en AplicarCambiosMapa
            
            // Animación visual simple: escalar y rotar
            float duracion = 0.8f;
            float tiempoTranscurrido = 0f;
            Vector3 escalaOriginal = poiObj.transform.localScale;
            
            while (tiempoTranscurrido < duracion)
            {
                tiempoTranscurrido += Time.deltaTime;
                float progreso = tiempoTranscurrido / duracion;
                
                // Pulsar
                float pulso = 1f + Mathf.Sin(progreso * Mathf.PI * 4) * 0.2f;
                poiObj.transform.localScale = escalaOriginal * pulso;
                
                // Rotar
                poiObj.transform.Rotate(Vector3.up, Time.deltaTime * 180f);
                
                yield return null;
            }
            
            poiObj.transform.localScale = escalaOriginal;
        }
        else
        {
            Debug.LogWarning($"⚠️ POI {poiId} no tiene componente POI");
        }
    }
    
    private IEnumerator AnimarAbrirPuerta(PosicionData pos)
    {
        Debug.Log($"🚪 Intentando abrir puerta en ({pos.fila},{pos.columna})");
        
        // Las puertas están en los cambios_mapa, necesitamos buscarlas ahí
        yield return new WaitForSeconds(0.1f);
    }
    
    // ========== Métodos de Fase de Dados ==========
    
    private IEnumerator AparecerHuevo(int fila, int columna)
    {
        Debug.Log($"🥚 Apareciendo nuevo huevo en ({fila},{columna})");
        
        // Verificar que no haya ya un huevo en esa posición
        GameObject huevoExistente = TableroBuilder.ObtenerHuevo(fila, columna);
        if (huevoExistente != null)
        {
            Debug.LogWarning($"⚠️ Ya existe huevo en ({fila},{columna}), no se crea duplicado");
            yield break;
        }
        
        TableroBuilder builder = FindFirstObjectByType<TableroBuilder>();
        if (builder != null)
        {
            GameObject huevo = builder.CrearHuevoDinamico(fila, columna);
            if (huevo != null)
            {
                // La animación de aparición se maneja automáticamente en Egg.Start()
                yield return new WaitForSeconds(0.2f);
            }
        }
        else
        {
            Debug.LogError("❌ No se encontró TableroBuilder en la escena");
        }
    }
    
    private IEnumerator EvolucionarHuevo(int fila, int columna)
    {
        Debug.Log($"🥚➡️🕷️ Evolucionando huevo en ({fila},{columna})");
        
        GameObject huevo = TableroBuilder.ObtenerHuevo(fila, columna);
        
        if (huevo != null)
        {
            Egg eggComponent = huevo.GetComponent<Egg>();
            
            if (eggComponent != null)
            {
                // Animar evolución
                yield return StartCoroutine(eggComponent.Evolucionar());
                
                // Remover del diccionario
                TableroBuilder.RemoverHuevoDelDiccionario(fila, columna);
            }
            else
            {
                Destroy(huevo);
            }
        }
        
        // Crear araña en su lugar
        TableroBuilder builder = FindFirstObjectByType<TableroBuilder>();
        if (builder != null)
        {
            builder.CrearSpiderDinamica(fila, columna);
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private IEnumerator ExplotarSpider(int fila, int columna)
    {
        Debug.Log($"💥💥💥 EXPLOSIÓN en ({fila},{columna}) 💥💥💥");
        
        GameObject spider = TableroBuilder.ObtenerSpider(fila, columna);
        
        if (spider == null)
        {
            Debug.LogError($"❌ ERROR: No hay araña en ({fila},{columna}) para explotar!");
            yield break;
        }
        
        Debug.Log($"✓ Araña encontrada en ({fila},{columna}), eliminándola...");
        
        Vector3 posicionExplosion = CoordenadasHelper.JSONaPosicionUnity(fila, columna);
        
        // Crear efecto visual de explosión
        GameObject explosionVisual = Explosion.Crear(posicionExplosion + Vector3.up * 1.5f, explosionPrefab);
        
        Spider spiderComponent = spider.GetComponent<Spider>();
        
        if (spiderComponent != null)
        {
            spiderComponent.Eliminar();
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            Destroy(spider);
        }
        
        TableroBuilder.RemoverSpiderDelDiccionario(fila, columna);
        Debug.Log($"✓ Araña en ({fila},{columna}) eliminada del diccionario");
        
        // Procesar efectos de explosión en celdas adyacentes
        ProcesarEfectosExplosion(fila, columna);
        
        Debug.Log($"💥 Explosión completada en ({fila},{columna})");
        
        // Esperar un momento para que se vea la explosión
        yield return new WaitForSeconds(0.5f);
    }
    
    private void ProcesarEfectosExplosion(int fila, int columna)
    {
        // Obtener referencia a TableroBuilder
        TableroBuilder builder = FindFirstObjectByType<TableroBuilder>();
        if (builder == null)
        {
            Debug.LogError("❌ No se encontró TableroBuilder para procesar efectos de explosión");
            return;
        }
        
        // Direcciones cardinales: Norte, Sur, Este, Oeste
        (int, int, string)[] direcciones = new[]
        {
            (fila - 1, columna, "sur"),    // Celda norte (la pared que nos conecta está al sur de esa celda)
            (fila + 1, columna, "norte"),  // Celda sur (la pared que nos conecta está al norte de esa celda)
            (fila, columna + 1, "oeste"),  // Celda este (la pared que nos conecta está al oeste de esa celda)
            (fila, columna - 1, "este")    // Celda oeste (la pared que nos conecta está al este de esa celda)
        };
        
        foreach (var (filaAdyacente, colAdyacente, direccionPared) in direcciones)
        {
            // Verificar límites del tablero
            if (filaAdyacente < 1 || filaAdyacente > 6 || colAdyacente < 1 || colAdyacente > 8)
            {
                continue; // Fuera de límites
            }
            
            // Buscar pared entre celda actual y adyacente
            GameObject paredObj = TableroBuilder.ObtenerPared(filaAdyacente, colAdyacente, direccionPared);
            
            if (paredObj != null)
            {
                // HAY PARED: Dañar la pared (1 punto de daño por explosión)
                var wallComponent = paredObj.GetComponent<Wall>();
                if (wallComponent != null)
                {
                    wallComponent.AplicarDano(1);
                    Debug.Log($"💥🧱 Explosión daña pared en ({filaAdyacente},{colAdyacente},{direccionPared})");
                }
            }
            else
            {
                // NO HAY PARED: Spawn araña en celda adyacente si está vacía
                GameObject spiderExistente = TableroBuilder.ObtenerSpider(filaAdyacente, colAdyacente);
                
                if (spiderExistente == null)
                {
                    // Celda vacía, spawn nueva araña
                    GameObject nuevaSpider = builder.CrearSpiderDinamica(filaAdyacente, colAdyacente);
                    
                    if (nuevaSpider != null)
                    {
                        Debug.Log($"💥🕷️ Explosión genera nueva araña en ({filaAdyacente},{colAdyacente})");
                    }
                }
                else
                {
                    Debug.Log($"💥⚠️ Explosión intenta spawn araña en ({filaAdyacente},{colAdyacente}) pero ya hay una");
                }
            }
        }
    }
    
    // ========== Métodos de Animación de Acciones ==========
    
    private IEnumerator AnimarDanarPared(PosicionData pos)
    {
        Debug.Log($"🔨 Dañando pared en ({pos.fila},{pos.columna})");
        yield return new WaitForSeconds(0.1f);
    }
    
    private IEnumerator AnimarEliminarSpider(PosicionData pos)
    {
        GameObject spider = TableroBuilder.ObtenerSpider(pos.fila, pos.columna);
        
        if (spider == null)
        {
            Debug.LogWarning($"⚠️ No se encontró araña en ({pos.fila},{pos.columna})");
            yield break;
        }
        
        Spider spiderComponent = spider.GetComponent<Spider>();
        
        if (spiderComponent != null)
        {
            Debug.Log($"🕷️💀 Eliminando araña en ({pos.fila},{pos.columna})");
            spiderComponent.Eliminar();
            
            // Esperar a que termine la animación de eliminación
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            Debug.LogWarning($"⚠️ Araña en ({pos.fila},{pos.columna}) no tiene componente Spider");
            Destroy(spider);
        }
    }
    
    private IEnumerator AnimarEliminarHuevo(PosicionData pos)
    {
        GameObject huevo = TableroBuilder.ObtenerHuevo(pos.fila, pos.columna);
        
        if (huevo == null)
        {
            Debug.LogWarning($"⚠️ No se encontró huevo en ({pos.fila},{pos.columna})");
            yield break;
        }
        
        Egg eggComponent = huevo.GetComponent<Egg>();
        
        if (eggComponent != null)
        {
            Debug.Log($"🥚💥 Eliminando huevo en ({pos.fila},{pos.columna})");
            eggComponent.Eliminar();
            
            // Esperar a que termine la animación de eliminación
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            Debug.LogWarning($"⚠️ Huevo en ({pos.fila},{pos.columna}) no tiene componente Egg");
            Destroy(huevo);
        }
    }
    
    private void MostrarEstadoJuego(EstadoJuegoData estado)
    {
        Debug.Log($"📊 Estado: Víctimas rescatadas: {estado.victimas_rescatadas}/{estado.victimas_perdidas} | Daño edificio: {estado.daño_edificio}/24");
        
        if (estado.juego_terminado)
        {
            Debug.Log($"🎯 JUEGO TERMINADO: {estado.resultado.ToUpper()}");
        }
    }
    
    private void MostrarResultadoFinal()
    {
        if (escenario.turnos.Length > 0)
        {
            var ultimoTurno = escenario.turnos[escenario.turnos.Length - 1];
            if (ultimoTurno.estado_juego != null)
            {
                string resultado = ultimoTurno.estado_juego.resultado ?? "desconocido";
                Debug.Log($"=== RESULTADO FINAL: {resultado.ToUpper()} ===");
                Debug.Log($"Víctimas rescatadas: {ultimoTurno.estado_juego.victimas_rescatadas}");
                Debug.Log($"Víctimas perdidas: {ultimoTurno.estado_juego.victimas_perdidas}");
                Debug.Log($"Falsas alarmas: {ultimoTurno.estado_juego.falsas_alarmas}");
                Debug.Log($"Daño al edificio: {ultimoTurno.estado_juego.daño_edificio}/24");
            }
        }
    }
    
    /// <summary>
    /// Anima cuando un crew recoge una víctima
    /// </summary>
    private IEnumerator AnimarCargarVictima(int crewId, PosicionData posicion)
    {
        // Obtener GameObject del crew
        GameObject crew = TableroBuilder.ObtenerTripulacion(crewId);
        if (crew == null)
        {
            Debug.LogWarning($"⚠️ No se encontró crew {crewId}");
            yield break;
        }
        
        // Obtener componente Crew
        Crew crewComponent = crew.GetComponent<Crew>();
        if (crewComponent == null)
        {
            Debug.LogWarning($"⚠️ Crew {crewId} no tiene componente Crew.cs");
            yield break;
        }
        
        // Activar indicador visual y cambiar color a VERDE
        crewComponent.CargarVictima();
        
        // ELIMINAR GameObject de la víctima POI
        GameObject poi = TableroBuilder.ObtenerPOIPorPosicion(posicion.fila, posicion.columna);
        if (poi != null)
        {
            Debug.Log($"🗑️ Eliminando POI víctima en ({posicion.fila},{posicion.columna})");
            Destroy(poi);
        }
        
        yield return new WaitForSeconds(0.1f);
    }
    
    /// <summary>
    /// Anima cuando un crew deposita una víctima
    /// </summary>
    private IEnumerator AnimarDepositarVictima(int crewId, PosicionData posicion)
    {
        // Obtener GameObject del crew
        GameObject crew = TableroBuilder.ObtenerTripulacion(crewId);
        if (crew == null)
        {
            Debug.LogWarning($"⚠️ No se encontró crew {crewId}");
            yield break;
        }
        
        // Obtener componente Crew
        Crew crewComponent = crew.GetComponent<Crew>();
        if (crewComponent == null)
        {
            Debug.LogWarning($"⚠️ Crew {crewId} no tiene componente Crew.cs");
            yield break;
        }
        
        // Desactivar indicador visual y restaurar color original
        crewComponent.DepositarVictima();
        
        Debug.Log($"✅ Crew {crewId} depositó víctima en ({posicion.fila},{posicion.columna}) - ¡RESCATADA!");
        
        yield return new WaitForSeconds(0.1f);
    }
}
