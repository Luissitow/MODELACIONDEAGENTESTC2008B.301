using System;

[System.Serializable]
public class MapConfiguration
{
    public ElementType[] elementTypes;
    
    public MapConfiguration()
    {
        // Configuración por defecto basada en tu ejemplo
        elementTypes = new ElementType[]
        {
            new ElementType { id = 0, name = "Floor", prefabName = "Floor", isWalkable = true },
            new ElementType { id = 1, name = "Wall", prefabName = "Wall", isWalkable = false },
            new ElementType { id = 2, name = "Victim", prefabName = "Victim", isWalkable = true },
            new ElementType { id = 3, name = "Fire", prefabName = "Fire", isWalkable = false },
            new ElementType { id = 4, name = "Agent", prefabName = "Agent", isWalkable = true },
            new ElementType { id = 5, name = "Door", prefabName = "Door", isWalkable = true },
            new ElementType { id = 6, name = "Explosion", prefabName = "Explosion", isWalkable = false }
        };
    }
    
    public ElementType GetElementType(int id)
    {
        foreach (var element in elementTypes)
        {
            if (element.id == id)
                return element;
        }
        return elementTypes[0]; // Retornar Floor por defecto
    }
}

[System.Serializable]
public class ElementType
{
    public int id;
    public string name;
    public string prefabName;
    public bool isWalkable;
    public UnityEngine.Color color = UnityEngine.Color.white;
}