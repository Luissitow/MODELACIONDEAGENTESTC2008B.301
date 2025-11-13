using UnityEngine;

[CreateAssetMenu(fileName = "MapConfig", menuName = "Fire Rescue/Map Configuration")]
public class MapConfigurationAsset : ScriptableObject
{
    [Header("Configuración de Elementos")]
    public ElementConfig[] elementConfigs;
    
    [Header("Colores de Debug")]
    public Color floorColor = Color.white;
    public Color wallColor = Color.gray;
    public Color victimColor = Color.blue;
    public Color fireColor = Color.red;
    public Color agentColor = Color.green;
    public Color doorColor = Color.yellow;
    public Color explosionColor = Color.orange;
    
    void OnEnable()
    {
        if (elementConfigs == null || elementConfigs.Length == 0)
        {
            InitializeDefaultConfig();
        }
    }
    
    private void InitializeDefaultConfig()
    {
        elementConfigs = new ElementConfig[]
        {
            new ElementConfig { 
                id = 0, 
                name = "Floor", 
                description = "Suelo libre para caminar",
                symbol = ".", 
                isWalkable = true, 
                color = floorColor 
            },
            new ElementConfig { 
                id = 1, 
                name = "Wall", 
                description = "Pared sólida",
                symbol = "#", 
                isWalkable = false, 
                color = wallColor 
            },
            new ElementConfig { 
                id = 2, 
                name = "Victim", 
                description = "Persona que necesita rescate",
                symbol = "v", 
                isWalkable = true, 
                color = victimColor 
            },
            new ElementConfig { 
                id = 3, 
                name = "Fire", 
                description = "Fuego que debe ser extinguido",
                symbol = "f", 
                isWalkable = false, 
                color = fireColor 
            },
            new ElementConfig { 
                id = 4, 
                name = "Agent", 
                description = "Agente de rescate",
                symbol = "a", 
                isWalkable = true, 
                color = agentColor 
            },
            new ElementConfig { 
                id = 5, 
                name = "Door", 
                description = "Puerta de entrada/salida",
                symbol = "d", 
                isWalkable = true, 
                color = doorColor 
            },
            new ElementConfig { 
                id = 6, 
                name = "Explosion", 
                description = "Zona de explosión peligrosa",
                symbol = "x", 
                isWalkable = false, 
                color = explosionColor 
            }
        };
    }
    
    public ElementConfig GetElementConfig(int id)
    {
        foreach (var config in elementConfigs)
        {
            if (config.id == id)
                return config;
        }
        return elementConfigs[0]; // Retornar Floor por defecto
    }
    
    public ElementConfig GetElementConfig(CellType cellType)
    {
        return GetElementConfig((int)cellType);
    }
    
    public Color GetColorForType(CellType type)
    {
        return GetElementConfig(type).color;
    }
}

[System.Serializable]
public class ElementConfig
{
    [Header("Identificación")]
    public int id;
    public string name;
    public string description;
    
    [Header("Representación")]
    public string symbol;           // Símbolo para representación en texto
    public Color color;             // Color para debug y UI
    public GameObject prefab;       // Prefab para instanciar
    
    [Header("Propiedades")]
    public bool isWalkable;
    public bool isDestructible;
    public bool blocksVision;
    public int scoreValue;          // Puntos otorgados por interactuar con este elemento
}