using UnityEngine;

/// <summary>
/// This astronaut controller class will handle the movement and camera controls for the astronaut player.
/// Based on TC2008B laboratory standards with first-person perspective adaptation.
/// Standard coding documentation can be found in 
/// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments
/// </summary>
public class AstronautController : MonoBehaviour
{
    [Header("Movement Settings")]
    /// <summary>
    /// Velocidad de caminar del astronauta
    /// </summary>
    public float walkSpeed = 6.0f;
    
    /// <summary>
    /// Velocidad de correr del astronauta
    /// </summary>
    public float runSpeed = 12.0f;
    
    /// <summary>
    /// Fuerza del salto
    /// </summary>
    public float jumpHeight = 2.0f;
    
    /// <summary>
    /// Gravedad aplicada al astronauta
    /// </summary>
    public float gravity = -15.0f;

    [Header("Mouse Look Settings")]
    /// <summary>
    /// Sensibilidad del mouse para mirar alrededor
    /// </summary>
    public float mouseSensitivity = 100f;

    [Header("Ground Check")]
    /// <summary>
    /// Distancia para detectar el suelo
    /// </summary>
    public float groundDistance = 0.4f;
    
    /// <summary>
    /// Máscara de capas para el suelo
    /// </summary>
    public LayerMask groundMask = 1;

    [Header("Referencias")]
    /// <summary>
    /// Transform del cuerpo del astronauta
    /// </summary>
    public Transform playerBody;
    
    /// <summary>
    /// Cámara en primera persona
    /// </summary>
    public Camera playerCamera;
    
    /// <summary>
    /// Punto para verificar si está en el suelo
    /// </summary>
    public Transform groundCheck;

    // Variables privadas para input
    private float horizontalInput;
    private float verticalInput;
    private bool jumpInput;
    private bool runInput;

    // Variables para el mouse look
    private float mouseX;
    private float mouseY;
    private float xRotation = 0f;

    // Variables para físicas
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    /// <summary>
    /// This method is called before the first frame update
    /// </summary>
    void Start()
    {
        // Obtener el CharacterController
        controller = GetComponent<CharacterController>();
        
        // Si no se asignó playerBody, usar este transform
        if (playerBody == null)
            playerBody = transform;
            
        // Si no se asignó la cámara, buscar una cámara hija
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
            
        // Si no se asignó groundCheck, crear uno
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = groundCheckObj.transform;
        }
        
        // Configurar cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("🚀 Astronaut Controller iniciado correctamente");
    }

    /// <summary>
    /// This method is called once per frame
    /// </summary>
    void Update()
    {
        // Obtener inputs
        GetInputs();
        
        // Verificar si está en el suelo
        CheckGrounded();
        
        // Manejar movimiento
        HandleMovement();
        
        // Manejar rotación con mouse
        HandleMouseLook();
        
        // Manejar salto
        HandleJump();
        
        // Aplicar gravedad
        ApplyGravity();
        
        // Mover el personaje
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Obtiene todos los inputs del teclado y mouse
    /// </summary>
    void GetInputs()
    {
        // Input de movimiento (WASD o flechas)
        horizontalInput = Input.GetAxis("Horizontal");  // A/D o Izquierda/Derecha
        verticalInput = Input.GetAxis("Vertical");      // W/S o Arriba/Abajo
        
        // Input de salto
        jumpInput = Input.GetKeyDown("space");
        
        // Input de correr
        runInput = Input.GetKey("left shift");
        
        // Input del mouse
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        // Liberar cursor si se presiona Escape
        if (Input.GetKeyDown("escape"))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    /// <summary>
    /// Verifica si el astronauta está tocando el suelo
    /// </summary>
    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Pequeño valor negativo para mantenerse en el suelo
        }
    }

    /// <summary>
    /// Maneja el movimiento horizontal del astronauta
    /// </summary>
    void HandleMovement()
    {
        // Calcular dirección de movimiento
        Vector3 move = transform.right * horizontalInput + transform.forward * verticalInput;
        
        // Determinar velocidad actual (caminar o correr)
        float currentSpeed = runInput ? runSpeed : walkSpeed;
        
        // Mover el astronauta
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Maneja la rotación de la cámara con el mouse
    /// </summary>
    void HandleMouseLook()
    {
        // Rotación vertical de la cámara (mirar arriba/abajo)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limitar rotación vertical
        
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // Rotación horizontal del cuerpo (girar izquierda/derecha)
        playerBody.Rotate(Vector3.up * mouseX);
    }

    /// <summary>
    /// Maneja el salto del astronauta
    /// </summary>
    void HandleJump()
    {
        if (jumpInput && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log("🚀 ¡Astronauta saltando!");
        }
    }

    /// <summary>
    /// Aplica la gravedad al astronauta
    /// </summary>
    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
    }

    /// <summary>
    /// Dibuja gizmos en la escena para debug
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

    /// <summary>
    /// Configuración automática del astronauta
    /// </summary>
    [ContextMenu("Auto Setup Astronaut")]
    public void AutoSetupAstronaut()
    {
        Debug.Log("🔧 Configurando astronauta automáticamente...");
        
        // Configurar CharacterController
        CharacterController cc = GetComponent<CharacterController>();
        if (cc == null)
            cc = gameObject.AddComponent<CharacterController>();
            
        cc.height = 2.0f;
        cc.radius = 0.5f;
        cc.center = new Vector3(0, 1f, 0);
        
        // Configurar referencias
        playerBody = transform;
        
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
            
        // Crear GroundCheck
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = groundCheckObj.transform;
        }
        
        Debug.Log("✅ Astronauta configurado correctamente");
    }

    /// <summary>
    /// Muestra información de debug en pantalla
    /// </summary>
    void OnGUI()
    {
        if (Application.isEditor)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"Velocidad: {(runInput ? "Corriendo" : "Caminando")}");
            GUI.Label(new Rect(10, 30, 300, 20), $"En suelo: {(isGrounded ? "Sí" : "No")}");
            GUI.Label(new Rect(10, 50, 300, 20), $"Input H: {horizontalInput:F2}, V: {verticalInput:F2}");
            GUI.Label(new Rect(10, 70, 300, 20), "Controles: WASD - Movimiento, Mouse - Mirar, Espacio - Saltar, Shift - Correr");
        }
    }
}