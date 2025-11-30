using UnityEngine;

/// <summary>
/// Cámara libre para observar la escena durante el desarrollo
/// Controles: WASD - Movimiento, Mouse - Mirar, Q/E - Subir/Bajar, Shift - Rápido
/// </summary>
public class CamaraLibre : MonoBehaviour
{
    [Header("Velocidades")]
    [SerializeField] private float velocidadNormal = 10f;
    [SerializeField] private float velocidadRapida = 50f;
    [SerializeField] private float sensibilidadMouse = 3f;
    
    [Header("Configuración")]
    [SerializeField] private bool bloquearCursor = true;
    
    private float rotacionX = 0f;
    private float rotacionY = 0f;
    private bool cursorBloqueado = false;
    
    void Start()
    {
        if (bloquearCursor)
        {
            BloquearCursor();
        }
    }
    
    void Update()
    {
        // Toggle cursor con Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (cursorBloqueado)
                DesbloquearCursor();
            else
                BloquearCursor();
        }
        
        // Solo mover si el cursor está bloqueado
        if (cursorBloqueado)
        {
            MoverCamara();
            RotarCamara();
        }
    }
    
    void MoverCamara()
    {
        // Velocidad actual
        float velocidad = Input.GetKey(KeyCode.LeftShift) ? velocidadRapida : velocidadNormal;
        
        // Movimiento WASD
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S
        
        // Subir/Bajar con Q/E
        float arriba = 0f;
        if (Input.GetKey(KeyCode.E)) arriba = 1f;
        if (Input.GetKey(KeyCode.Q)) arriba = -1f;
        
        // Calcular dirección
        Vector3 direccion = transform.right * horizontal + transform.forward * vertical + Vector3.up * arriba;
        
        // Mover
        transform.position += direccion * velocidad * Time.deltaTime;
    }
    
    void RotarCamara()
    {
        // Input del mouse
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadMouse;
        
        // Rotar horizontal (Y)
        rotacionY += mouseX;
        
        // Rotar vertical (X) con límite
        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -90f, 90f);
        
        // Aplicar rotación
        transform.rotation = Quaternion.Euler(rotacionX, rotacionY, 0f);
    }
    
    void BloquearCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorBloqueado = true;
        Debug.Log("🎮 Cursor bloqueado - Tab para desbloquear");
    }
    
    void DesbloquearCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorBloqueado = false;
        Debug.Log("🖱️ Cursor libre - Tab para bloquear");
    }
    
    void OnGUI()
    {
        // Instrucciones en pantalla
        GUI.Label(new Rect(10, 10, 400, 20), "WASD - Moverse | Mouse - Mirar | Q/E - Subir/Bajar");
        GUI.Label(new Rect(10, 30, 400, 20), "Shift - Rápido | Tab - Bloquear/Desbloquear cursor");
        GUI.Label(new Rect(10, 50, 400, 20), $"Estado: {(cursorBloqueado ? "Cámara Libre" : "Cursor Libre")}");
    }
}
