using UnityEngine;
using System.Collections;

/// <summary>
/// Script para manejar el estado y daño de paredes/puertas
/// Componente que va en el PREFAB de paredes y puertas
/// </summary>
public class Wall : MonoBehaviour
{
    [Header("Posición en Tablero")]
    public int fila;
    public int columna;
    public string direccion; // "norte", "sur", "este", "oeste"

    [Header("Tipo de Pared")]
    public TipoPared tipo = TipoPared.Madera;

    [Header("Configuración")]
    [SerializeField] public int vidaMaxima = 2;  // Vida inicial (2 hits para destruir)
    [SerializeField] public float alturaAbrirPuerta = 3f; // Altura para abrir puerta (mover en Y)
    #pragma warning disable 0414 // Campo asignado pero no usado (reservado para animación de apertura)
    [SerializeField] private float velocidadAperturaPuerta = 2f;
    #pragma warning restore 0414

    [Header("Prefabs para estados")]
    [SerializeField] public GameObject prefabNormal;
    [SerializeField] public GameObject prefabDanado;
    [SerializeField] public GameObject prefabDestruido;

    [Header("Materiales por Estado")]
    [SerializeField] private Material materialIntacto;
    [SerializeField] private Material materialDanado;
    [SerializeField] private MeshRenderer meshRenderer;

    public int vidaActual;
    public bool estaDestruida = false;
    public bool estaAbierta = false; // Solo para puertas
    
    // Propiedad de compatibilidad
    public bool esPuerta => tipo == TipoPared.Puerta;

    private WallEffects efectosVisuales; // Sistema de efectos
    private Vector3 posicionInicial; // Guardar posici\u00f3n inicial para cerrar puertas

    void Start()
    {
        vidaActual = vidaMaxima;
        posicionInicial = transform.position; // Guardar posici\u00f3n inicial
        
        // Si no hay prefabs asignados, usar el actual como normal
        if (prefabNormal == null) prefabNormal = gameObject;

        // Auto-encontrar MeshRenderer
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        // Auto-encontrar o agregar WallEffects
        efectosVisuales = GetComponent<WallEffects>();
        if (efectosVisuales == null)
        {
            efectosVisuales = gameObject.AddComponent<WallEffects>();
        }

        // Validar configuración de prefabs
        ValidarConfiguracion();

        ActualizarEstadoVisual();
    }

    /// <summary>
    /// Valida que los prefabs estén correctamente configurados
    /// </summary>
    void ValidarConfiguracion()
    {
        string tipoObjeto = tipo == TipoPared.Puerta ? "puerta" : "pared";
        
        if (prefabNormal != null)
            Debug.Log($"✅ {tipoObjeto} ({fila},{columna}) {direccion} - Prefab Normal: {prefabNormal.name}");
        
        if (prefabDanado != null)
            Debug.Log($"✅ {tipoObjeto} ({fila},{columna}) {direccion} - Prefab Dañado: {prefabDanado.name}");
        else
            Debug.LogWarning($"⚠️ {tipoObjeto} ({fila},{columna}) {direccion} - Prefab Dañado NO asignado. No habrá cambio visual al dañarse.");
        
        if (prefabDestruido != null)
            Debug.Log($"✅ {tipoObjeto} ({fila},{columna}) {direccion} - Prefab Destruido: {prefabDestruido.name}");
        else
            Debug.LogWarning($"⚠️ {tipoObjeto} ({fila},{columna}) {direccion} - Prefab Destruido NO asignado. Se desactivará al destruirse.");
    }

    /// <summary>
    /// Aplica daño a la pared o puerta
    /// </summary>
    /// <param name="cantidad">Cantidad de daño (1 o 2)</param>
    public void RecibirDano(int cantidad)
    {
        if (estaDestruida)
        {
            Debug.LogWarning($"⚠️ {(tipo == TipoPared.Puerta ? "La puerta" : "La pared")} en ({fila},{columna}) {direccion} ya está destruida");
            return;
        }

        int vidaAnterior = vidaActual;
        vidaActual -= cantidad;
        
        string emoji = cantidad == 2 ? "🔨" : "⚔️";
        string tipoObjeto = tipo == TipoPared.Puerta ? "puerta" : "pared";
        string accion = cantidad == 2 ? "GOLPE FUERTE" : "GOLPE";
        
        // Predicción de destrucción
        string estado = vidaActual <= 0 ? " → 💥 SE DESTRUIRÁ" : (vidaActual == 1 ? " → ⚠️ CRÍTICO" : "");
        Debug.Log($"{emoji} {accion} en {tipoObjeto} ({fila},{columna}) {direccion} | Vida: {vidaAnterior} → {vidaActual}/{vidaMaxima}{estado}");

        // Efectos visuales de daño
        if (efectosVisuales != null)
        {
            efectosVisuales.EfectoDano(cantidad);
        }

        if (vidaActual <= 0)
        {
            Destruir();
        }
        else
        {
            ActualizarEstadoVisual();
        }
    }

    /// <summary>
    /// Ataca la pared (1 de daño)
    /// </summary>
    public void Atacar()
    {
        RecibirDano(1);
    }

    /// <summary>
    /// Rompe la pared (2 de daño)
    /// </summary>
    public void Romper()
    {
        RecibirDano(2);
    }

    /// <summary>
    /// Anima la apertura de una puerta (movimiento hacia arriba)
    /// </summary>
    public IEnumerator AnimarAperturaPuerta(float duracion = 0.6f)
    {
        if (tipo != TipoPared.Puerta)
        {
            Debug.LogWarning($"⚠️ Se intentó animar apertura en objeto que no es puerta: ({fila},{columna}) {direccion}");
            yield break;
        }

        Vector3 posicionInicial = transform.position;
        Vector3 posicionArriba = posicionInicial + Vector3.up * 2.5f; // Sube 2.5 unidades

        float tiempoTranscurrido = 0f;

        Debug.Log($"🚪 Iniciando animación apertura puerta ({fila},{columna}) {direccion}");

        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracion;
            // Usar curva ease-in-out para movimiento suave
            float curva = Mathf.SmoothStep(0f, 1f, progreso);
            transform.position = Vector3.Lerp(posicionInicial, posicionArriba, curva);
            yield return null;
        }

        transform.position = posicionArriba;
        Debug.Log($"✅ Puerta ({fila},{columna}) {direccion} abierta completamente");
    }

    /// <summary>
    /// Anima el cierre de una puerta (movimiento hacia abajo)
    /// </summary>
    public IEnumerator AnimarCierrePuerta(float duracion = 0.5f)
    {
        if (tipo != TipoPared.Puerta)
        {
            Debug.LogWarning($"⚠️ Se intentó animar cierre en objeto que no es puerta: ({fila},{columna}) {direccion}");
            yield break;
        }

        Vector3 posicionActual = transform.position;
        Vector3 posicionAbajo = new Vector3(
            posicionActual.x,
            posicionInicial.y, // Volver a la Y original guardada en Start()
            posicionActual.z
        );

        float tiempoTranscurrido = 0f;

        Debug.Log($"🚪 Iniciando animación cierre puerta ({fila},{columna}) {direccion}");

        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracion;
            float curva = Mathf.SmoothStep(0f, 1f, progreso);
            transform.position = Vector3.Lerp(posicionActual, posicionAbajo, curva);
            yield return null;
        }

        transform.position = posicionAbajo;
        Debug.Log($"✅ Puerta ({fila},{columna}) {direccion} cerrada completamente");
    }

    /// <summary>
    /// Destruye la pared o puerta
    /// </summary>
    void Destruir()
    {
        estaDestruida = true;
        string tipoObjeto = tipo == TipoPared.Puerta ? "Puerta" : "Pared";
        string emoji = tipo == TipoPared.Puerta ? "🚪💥" : "🧱💥";

        // Efectos visuales de destrucción ANTES de cambiar prefab
        if (efectosVisuales != null)
        {
            efectosVisuales.EfectoDestruccion();
        }

        if (prefabDestruido != null)
        {
            Debug.Log($"{emoji} {tipoObjeto} DESTRUIDA en ({fila},{columna}) {direccion} → Cambiando a prefab: {prefabDestruido.name}");
            CambiarPrefab(prefabDestruido);
            Debug.Log($"✅ Cambio completado: Ahora muestra escombros/ruinas");
        }
        else
        {
            // Sin prefab destruido: desactivar el objeto (desaparece completamente)
            Debug.LogWarning($"{emoji} {tipoObjeto} DESTRUIDA en ({fila},{columna}) {direccion} → ⚠️ NO HAY prefabDestruido asignado!");
            Debug.Log($"   GameObject será DESACTIVADO (desaparece sin dejar escombros)");
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Abre una puerta (solo para tipo Puerta) - INICIA ANIMACIÓN AUTOMÁTICAMENTE
    /// </summary>
    public void AbrirPuerta()
    {
        if (tipo != TipoPared.Puerta)
        {
            Debug.LogWarning($"⚠️ No se puede abrir ({fila},{columna}) {direccion}, no es una puerta");
            return;
        }

        if (estaAbierta)
        {
            Debug.LogWarning($"⚠️ La puerta en ({fila},{columna}) {direccion} ya está abierta");
            return;
        }

        estaAbierta = true;
        Debug.Log($"🚪✨ Puerta ABIERTA en ({fila},{columna}) {direccion} - Iniciando animación automática...");
        
        // Iniciar animación usando la versión con parámetro (duracion = 0.6f por defecto)
        StartCoroutine(AnimarAperturaPuerta(0.8f)); // 0.8s para que sea más visible
    }

    /// <summary>
    /// Actualiza el estado visual según la vida actual
    /// Soporta tanto paredes como puertas
    /// </summary>
    void ActualizarEstadoVisual()
    {
        float porcentajeVida = (float)vidaActual / vidaMaxima;
        string tipoObjeto = tipo == TipoPared.Puerta ? "Puerta" : "Pared";

        // Opción 1: Cambiar prefab completo (más visual)
        if (porcentajeVida <= 0.5f && vidaActual > 0 && prefabDanado != null)
        {
            Debug.Log($"🔧 {tipoObjeto} DAÑADA en ({fila},{columna}) {direccion} - Vida: {vidaActual}/{vidaMaxima} - Cambiando a prefab dañado...");
            CambiarPrefab(prefabDanado);
            return;
        }
        else if (porcentajeVida <= 0.5f && vidaActual > 0 && prefabDanado == null)
        {
            // No hay prefab dañado, solo mostrar estado
            Debug.LogWarning($"⚠️ {tipoObjeto} DAÑADA en ({fila},{columna}) {direccion} - Vida: {vidaActual}/{vidaMaxima} [Sin cambio visual - no hay prefab dañado asignado]");
        }

        // Opción 2: Cambiar material (más sutil) - solo si no se cambió prefab
        if (meshRenderer != null && materialIntacto != null && materialDanado != null)
        {
            if (porcentajeVida > 0.5f)
            {
                meshRenderer.material = materialIntacto;
                Debug.Log($"🎨 Material cambiado a intacto para {tipoObjeto} ({fila},{columna}) {direccion}");
            }
            else if (vidaActual > 0)
            {
                meshRenderer.material = materialDanado;
                Debug.Log($"🎨 Material cambiado a dañado para {tipoObjeto} ({fila},{columna}) {direccion}");
            }
        }
    }

    /// <summary>
    /// Cambia el prefab de la pared/puerta manteniendo su estado
    /// </summary>
    private void CambiarPrefab(GameObject nuevoPrefab)
    {
        if (nuevoPrefab == null)
        {
            Debug.LogError($"❌ Intentando cambiar a un prefab NULL en ({fila},{columna}) {direccion}");
            return;
        }

        string tipoObjeto = tipo == TipoPared.Puerta ? "puerta" : "pared";
        Debug.Log($"🔄 Cambiando {tipoObjeto} ({fila},{columna}) {direccion} a prefab: {nuevoPrefab.name}");

        // Instanciar nuevo prefab en la misma posición y rotación
        GameObject nuevo = Instantiate(nuevoPrefab, transform.position, transform.rotation, transform.parent);
        nuevo.name = gameObject.name; // Mantener nombre para identificación

        // Asignar tag Wall (crítico para ActionExecutor)
        nuevo.tag = "Wall";

        // Copiar estado al nuevo Wall script
        Wall nuevoWall = nuevo.GetComponent<Wall>();
        if (nuevoWall == null)
        {
            // Si el nuevo prefab no tiene Wall, agregarlo
            Debug.LogWarning($"⚠️ Prefab {nuevoPrefab.name} no tiene componente Wall, agregándolo...");
            nuevoWall = nuevo.AddComponent<Wall>();
        }

        // Transferir todo el estado
        nuevoWall.fila = fila;
        nuevoWall.columna = columna;
        nuevoWall.direccion = direccion;
        nuevoWall.tipo = tipo;
        nuevoWall.vidaActual = vidaActual;
        nuevoWall.vidaMaxima = vidaMaxima;
        nuevoWall.estaDestruida = estaDestruida;
        nuevoWall.estaAbierta = estaAbierta;
        nuevoWall.alturaAbrirPuerta = alturaAbrirPuerta;
        
        // Transferir referencias de prefabs (CRÍTICO para futuros cambios)
        nuevoWall.prefabNormal = prefabNormal;
        nuevoWall.prefabDanado = prefabDanado;
        nuevoWall.prefabDestruido = prefabDestruido;
        
        // Transferir referencias de materiales
        nuevoWall.meshRenderer = nuevo.GetComponent<MeshRenderer>();

        Debug.Log($"✅ Cambio de prefab completado: {gameObject.name} → {nuevo.name} (vida: {vidaActual}/{vidaMaxima})");

        // Destruir el objeto actual
        Destroy(gameObject);
    }

    /// <summary>
    /// Obtiene la vida actual
    /// </summary>
    public int GetVidaActual() => vidaActual;

    /// <summary>
    /// Obtiene información de la pared
    /// </summary>
    public string ObtenerInfo()
    {
        string info = $"Pared {tipo} ({fila},{columna}) {direccion}\n";
        info += $"Vida: {vidaActual}/{vidaMaxima}\n";
        
        if (tipo == TipoPared.Puerta)
            info += estaAbierta ? "Estado: Abierta" : "Estado: Cerrada";
        else
            info += estaDestruida ? "Estado: Destruida" : "Estado: Intacta";

        return info;
    }
}

/// <summary>
/// Tipos de paredes en el juego
/// </summary>
public enum TipoPared
{
    Madera,    // 2 hits para destruir, se puede romper
    Concreto,  // 3+ hits para destruir, más resistente
    Puerta,    // No se puede romper, solo abrir
    Exterior   // Pared del borde del edificio, indestructible
}