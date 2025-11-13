using UnityEngine;
using TMPro;

public class ElementInfoDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI elementInfoText;
    public GameObject infoPanel;
    
    [Header("References")]
    public Camera playerCamera;
    public MapGenerator mapGenerator;
    public FireRescueUI gameUI;
    
    void Start()
    {
        // Encontrar componentes si no están asignados
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        if (mapGenerator == null)
            mapGenerator = FindObjectOfType<MapGenerator>();
            
        if (gameUI == null)
            gameUI = FindObjectOfType<FireRescueUI>();
        
        // Inicializar panel de información
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }
    
    void Update()
    {
        // Detectar clic del mouse para mostrar información de elementos
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
        
        // Presionar ESC para cerrar panel de información
        if (Input.GetKeyDown(KeyCode.Escape) && infoPanel != null && infoPanel.activeSelf)
        {
            HideElementInfo();
        }
    }
    
    // Manejar clic del mouse para obtener información
    private void HandleMouseClick()
    {
        if (playerCamera == null) return;
        
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Obtener posición en el grid
            Vector2Int gridPosition = mapGenerator.WorldToGrid(hit.point);
            
            // Obtener información del elemento
            ShowElementInfo(gridPosition, hit.transform);
        }
    }
    
    // Mostrar información del elemento seleccionado
    public void ShowElementInfo(Vector2Int gridPos, Transform elementTransform)
    {
        if (elementInfoText == null) return;
        
        string elementInfo = GetElementInfoText(gridPos, elementTransform);
        
        // Actualizar texto de información (siguiendo patrón Chuck Norris)
        elementInfoText.text = elementInfo;
        
        // Activar panel si existe
        if (infoPanel != null)
            infoPanel.SetActive(true);
        
        // También mostrar en la UI principal si está disponible
        if (gameUI != null)
        {
            string elementType = GetElementType(elementTransform);
            gameUI.ShowElementInfo(gridPos, elementType);
        }
    }
    
    // Obtener texto de información detallada del elemento
    private string GetElementInfoText(Vector2Int gridPos, Transform element)
    {
        string elementType = GetElementType(element);
        string info = $"<size=18><b>{elementType}</b></size>\n\n";
        info += $"<b>Posición:</b> ({gridPos.x}, {gridPos.y})\n";
        
        switch (elementType.ToLower())
        {
            case "victim":
                info += "<b>Estado:</b> Necesita rescate\n";
                info += "<b>Puntos:</b> 100 por rescatar\n";
                info += "<b>Acción:</b> Acercarse para rescatar";
                break;
                
            case "fire":
                info += "<b>Estado:</b> Fuego activo\n";
                info += "<b>Peligro:</b> Bloquea el paso\n";
                info += "<b>Acción:</b> Usar extintor";
                break;
                
            case "agent":
                info += "<b>Estado:</b> Agente de rescate\n";
                info += "<b>Función:</b> Rescatar y extinguir\n";
                info += "<b>Movimiento:</b> Controlado por IA";
                break;
                
            case "wall":
                info += "<b>Estado:</b> Estructura sólida\n";
                info += "<b>Función:</b> Bloquea movimiento\n";
                info += "<b>Tipo:</b> Indestructible";
                break;
                
            case "door":
                info += "<b>Estado:</b> Entrada/Salida\n";
                info += "<b>Función:</b> Punto de evacuación\n";
                info += "<b>Uso:</b> Llevar víctimas aquí";
                break;
                
            case "explosion":
                info += "<b>Estado:</b> Zona peligrosa\n";
                info += "<b>Peligro:</b> ¡Área letal!\n";
                info += "<b>Evitar:</b> No transitar";
                break;
                
            case "floor":
            default:
                info += "<b>Estado:</b> Área libre\n";
                info += "<b>Función:</b> Espacio transitable\n";
                info += "<b>Uso:</b> Ruta de movimiento";
                break;
        }
        
        return info;
    }
    
    // Obtener tipo de elemento basado en su nombre
    private string GetElementType(Transform element)
    {
        if (element == null) return "Unknown";
        
        string name = element.name.ToLower();
        
        if (name.Contains("victim")) return "Victim";
        if (name.Contains("fire")) return "Fire";
        if (name.Contains("agent")) return "Agent";
        if (name.Contains("wall")) return "Wall";
        if (name.Contains("door")) return "Door";
        if (name.Contains("explosion")) return "Explosion";
        if (name.Contains("floor")) return "Floor";
        
        return "Unknown";
    }
    
    // Ocultar información del elemento
    public void HideElementInfo()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
            
        if (elementInfoText != null)
            elementInfoText.text = "";
    }
    
    // Método público para mostrar información desde otros scripts
    public void DisplayInfo(string title, string description)
    {
        if (elementInfoText != null)
        {
            elementInfoText.text = $"<size=18><b>{title}</b></size>\n\n{description}";
        }
        
        if (infoPanel != null)
            infoPanel.SetActive(true);
    }
}