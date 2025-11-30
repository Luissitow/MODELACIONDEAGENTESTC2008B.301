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
    [SerializeField] private float velocidadAperturaPuerta = 2f;

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

    void Start()
    {
        vidaActual = vidaMaxima;
        
        // Si no hay prefabs asignados, usar el actual como normal
        if (prefabNormal == null) prefabNormal = gameObject;

        // Auto-encontrar MeshRenderer
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        ActualizarEstadoVisual();
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

        vidaActual -= cantidad;
        
        string emoji = cantidad == 2 ? "🔨" : "⚔️";
        string tipoObjeto = tipo == TipoPared.Puerta ? "puerta" : "pared";
        string accion = cantidad == 2 ? "GOLPE FUERTE" : "GOLPE";
        Debug.Log($"{emoji} {accion} en {tipoObjeto} ({fila},{columna}) {direccion} | Vida: {vidaActual}/{vidaMaxima}");

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
    /// Destruye la pared o puerta
    /// </summary>
    void Destruir()
    {
        estaDestruida = true;
        string tipoObjeto = tipo == TipoPared.Puerta ? "Puerta" : "Pared";

        if (prefabDestruido != null)
        {
            CambiarPrefab(prefabDestruido);
            Debug.Log($"💥 {tipoObjeto} DESTRUIDA en ({fila},{columna}) {direccion} - Cambió a prefab destruido");
        }
        else
        {
            // Sin prefab destruido: desactivar el objeto (desaparece)
            gameObject.SetActive(false);
            Debug.Log($"💥 {tipoObjeto} DESTRUIDA en ({fila},{columna}) {direccion} - Vida: 0/{vidaMaxima} [GameObject desactivado - no hay prefab destruido]");
        }
    }

    /// <summary>
    /// Abre una puerta (solo para tipo Puerta)
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
        StartCoroutine(AnimarAperturaPuerta());
        Debug.Log($"🚪 Puerta abierta en ({fila},{columna}) {direccion}");
    }

    /// <summary>
    /// Anima la apertura de la puerta moviéndola hacia arriba
    /// </summary>
    IEnumerator AnimarAperturaPuerta()
    {
        Vector3 posicionInicial = transform.position;
        Vector3 posicionFinal = posicionInicial + Vector3.up * alturaAbrirPuerta;
        
        float tiempoTranscurrido = 0f;
        float duracion = 1f / velocidadAperturaPuerta;

        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracion;
            transform.position = Vector3.Lerp(posicionInicial, posicionFinal, progreso);
            yield return null;
        }

        transform.position = posicionFinal;
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
            CambiarPrefab(prefabDanado);
            Debug.Log($"🔧 {tipoObjeto} DAÑADA en ({fila},{columna}) {direccion} - Vida: {vidaActual}/{vidaMaxima} - Cambió a prefab dañado");
            return;
        }
        else if (porcentajeVida <= 0.5f && vidaActual > 0 && prefabDanado == null)
        {
            // No hay prefab dañado, solo mostrar estado
            Debug.Log($"⚠️ {tipoObjeto} DAÑADA en ({fila},{columna}) {direccion} - Vida: {vidaActual}/{vidaMaxima} [Sin cambio visual - no hay prefab dañado asignado]");
        }

        // Opción 2: Cambiar material (más sutil)
        if (meshRenderer != null && materialIntacto != null && materialDanado != null)
        {
            if (porcentajeVida > 0.5f)
            {
                meshRenderer.material = materialIntacto;
            }
            else
            {
                meshRenderer.material = materialDanado;
            }
        }
    }

    /// <summary>
    /// Cambia el prefab de la pared/puerta manteniendo su estado
    /// </summary>
    private void CambiarPrefab(GameObject nuevoPrefab)
    {
        // Instanciar nuevo prefab en la misma posición y rotación
        GameObject nuevo = Instantiate(nuevoPrefab, transform.position, transform.rotation, transform.parent);
        nuevo.name = gameObject.name; // Mantener nombre para identificación

        // Asignar tag si el actual lo tiene
        if (!string.IsNullOrEmpty(gameObject.tag))
            nuevo.tag = gameObject.tag;

        // Copiar estado al nuevo Wall script
        Wall nuevoWall = nuevo.GetComponent<Wall>();
        if (nuevoWall == null)
        {
            // Si el nuevo prefab no tiene Wall, agregarlo
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
        
        // Transferir referencias de prefabs
        nuevoWall.prefabNormal = prefabNormal;
        nuevoWall.prefabDanado = prefabDanado;
        nuevoWall.prefabDestruido = prefabDestruido;

        Debug.Log($"🔄 Prefab cambiado para {(tipo == TipoPared.Puerta ? "puerta" : "pared")} en ({fila},{columna}) {direccion}");

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