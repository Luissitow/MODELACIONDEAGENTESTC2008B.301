using UnityEngine;

[System.Serializable]
public class WallElement : MonoBehaviour
{
    [Header("Identificación de Pared")]
    public int wallId;              // ID único de esta pared
    public WallType wallType;       // Tipo de pared
    public WallDirection direction; // Dirección de la pared (0=Norte, 1=Este, 2=Sur, 3=Oeste)
    public Vector2Int gridPosition; // Posición en el grid del mapa
    public int bitPosition;         // Posición del bit en el número binario (0-3)
    
    [Header("Estados de la Pared")]
    public GameObject normalState;   // Pared normal
    public GameObject damagedState;  // Pared dañada
    public GameObject destroyedState; // Pared destruida
    
    [Header("Configuración")]
    public bool canBeDamaged = true;
    public bool canBeDestroyed = true;
    public int maxHealth = 100;
    
    private int currentHealth;
    private WallState currentState = WallState.Normal;
    
    // Eventos para notificar cambios
    public System.Action<WallElement, WallState> OnWallStateChanged;
    
    void Start()
    {
        currentHealth = maxHealth;
        SetWallState(WallState.Normal);
        
        // Registrar esta pared en el sistema
        RegisterWall();
    }
    
    // Registrar esta pared para que pueda ser controlada por JSON
    private void RegisterWall()
    {
        MapGenerator mapGen = FindObjectOfType<MapGenerator>();
        if (mapGen != null)
        {
            mapGen.RegisterWallElement(this);
        }
    }
    
    // Cambiar estado de la pared
    public void SetWallState(WallState newState)
    {
        if (currentState == newState) return;
        
        currentState = newState;
        
        // Activar/desactivar gameobjects según el estado
        if (normalState != null) normalState.SetActive(newState == WallState.Normal);
        if (damagedState != null) damagedState.SetActive(newState == WallState.Damaged);
        if (destroyedState != null) destroyedState.SetActive(newState == WallState.Destroyed);
        
        // Notificar cambio
        OnWallStateChanged?.Invoke(this, newState);
    }
    
    // Dañar la pared
    public void DamageWall(int damage)
    {
        if (!canBeDamaged || currentState == WallState.Destroyed) return;
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        if (currentHealth <= 0 && canBeDestroyed)
        {
            SetWallState(WallState.Destroyed);
        }
        else if (currentHealth <= maxHealth / 2)
        {
            SetWallState(WallState.Damaged);
        }
    }
    
    // Reparar la pared
    public void RepairWall()
    {
        currentHealth = maxHealth;
        SetWallState(WallState.Normal);
    }
    
    // Obtener información de la pared
    public WallInfo GetWallInfo()
    {
        return new WallInfo
        {
            wallId = this.wallId,
            gridPosition = this.gridPosition,
            direction = this.direction,
            bitPosition = this.bitPosition,
            state = this.currentState,
            health = this.currentHealth,
            maxHealth = this.maxHealth
        };
    }
    
    // Verificar si esta pared debería existir según la matriz binaria
    public bool ShouldExistFromBinaryMatrix(int binaryValue)
    {
        // Verificar si el bit correspondiente está activado
        return (binaryValue & (1 << bitPosition)) != 0;
    }
    
    // Calcular ID único basado en posición y dirección (como Fire Rescue)
    public static int CalculateWallId(Vector2Int gridPos, int direction, int mapWidth)
    {
        // Fórmula: (fila * ancho * 4) + (columna * 4) + dirección
        return (gridPos.y * mapWidth * 4) + (gridPos.x * 4) + direction;
    }
    
    // Aplicar información desde JSON
    public void ApplyWallInfo(WallInfo info)
    {
        this.wallId = info.wallId;
        this.gridPosition = info.gridPosition;
        this.direction = info.direction;
        this.currentHealth = info.health;
        SetWallState(info.state);
    }
}

public enum WallType
{
    Exterior = 0,    // Pared exterior
    Interior = 1,    // Pared interior
    Support = 2,     // Pared de soporte
    Window = 3,      // Ventana
    Door = 4         // Puerta
}

public enum WallState
{
    Normal = 0,      // Pared intacta
    Damaged = 1,     // Pared dañada
    Destroyed = 2    // Pared destruida
}

// Clase para serializar información de paredes en JSON
[System.Serializable]
public class WallInfo
{
    public int wallId;
    public Vector2Int gridPosition;
    public WallDirection direction;
    public int bitPosition;     // Posición del bit (0-3)
    public WallState state;
    public int health;
    public int maxHealth;
}