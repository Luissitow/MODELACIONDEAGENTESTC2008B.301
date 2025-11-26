using UnityEngine;

/// <summary>
/// Script para manejar el estado y daño de paredes/puertas
/// </summary>
public class Wall : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] public int vidaMaxima = 2;  // Vida inicial (2 hits para destruir)
    [SerializeField] public bool esPuerta = false; // Si es puerta, no recibe daño normal
    [SerializeField] public float alturaAbrirPuerta = 3f; // Altura para abrir puerta (mover en Y)

    [Header("Prefabs para estados")]
    [SerializeField] public GameObject prefabNormal;
    [SerializeField] public GameObject prefabDanado;
    [SerializeField] public GameObject prefabDestruido;

    public int vidaActual;

    void Start()
    {
        vidaActual = vidaMaxima;
        // Si no hay prefabs asignados, usar el actual como normal
        if (prefabNormal == null) prefabNormal = gameObject;
    }

    /// <summary>
    /// Aplica daño a la pared
    /// </summary>
    /// <param name="cantidad">Cantidad de daño (1 o 2)</param>
    public void RecibirDanio(int cantidad)
    {
        if (esPuerta)
        {
            // Para puertas, abrir moviendo hacia arriba
            AbrirPuerta();
            return;
        }

        vidaActual -= cantidad;

        if (vidaActual == 1 && prefabDanado != null)
        {
            // Cambiar a prefab dañado
            CambiarPrefab(prefabDanado);
        }
        else if (vidaActual <= 0)
        {
            // Destruir o cambiar a destruido
            if (prefabDestruido != null)
            {
                CambiarPrefab(prefabDestruido);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Lógica para abrir puerta (mover hacia arriba)
    /// </summary>
    private void AbrirPuerta()
    {
        // Mover la puerta hacia arriba
        transform.Translate(0, alturaAbrirPuerta, 0);
        Debug.Log("Puerta abierta (movida hacia arriba)");
    }

    /// <summary>
    /// Cambia el prefab de la pared
    /// </summary>
    private void CambiarPrefab(GameObject nuevoPrefab)
    {
        // Instanciar nuevo prefab en la misma posición
        GameObject nuevo = Instantiate(nuevoPrefab, transform.position, transform.rotation, transform.parent);
        nuevo.name = gameObject.name; // Mantener nombre

        // Copiar componentes si es necesario (ej. Wall script)
        Wall nuevoWall = nuevo.GetComponent<Wall>();
        if (nuevoWall != null)
        {
            nuevoWall.vidaActual = vidaActual;
            nuevoWall.esPuerta = esPuerta;
        }

        // Destruir el actual
        Destroy(gameObject);
    }

    /// <summary>
    /// Obtiene la vida actual
    /// </summary>
    public int GetVidaActual() => vidaActual;
}