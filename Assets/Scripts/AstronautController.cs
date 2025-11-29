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

    [Header("Identificación y Posición")]
    /// <summary>
    /// ID único del astronauta (asignable en Inspector)
    /// </summary>
    [SerializeField]
    public int astronautaID;
    /// <summary>
    /// Fila actual del astronauta en el tablero
    /// </summary>
    public int filaActual = -1; // -1 = zona exterior
    /// <summary>
    /// Columna actual del astronauta en el tablero
    /// </summary>
    public int columnaActual = 0;

    // Propiedades de compatibilidad con sistema antiguo
    public int id => astronautaID;
    public int row => filaActual;
    public int col => columnaActual;

    [Header("Modo de Control")]
    /// <summary>
    /// Si está en true, el astronauta se controla manualmente (WASD)
    /// Si está en false, se controla por JSON/simulación
    /// </summary>
    public bool modoManual = true;

    [Header("Movimiento Automático")]
    /// <summary>
    /// Velocidad de movimiento automático (para animación Lerp)
    /// </summary>
    public float velocidadMovimientoAuto = 2f;
    /// <summary>
    /// Indica si el astronauta está siendo movido automáticamente
    /// </summary>
    private bool estaMoviendoAutomaticamente = false;

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
        // Si está en modo manual, permitir control WASD
        if (modoManual && !estaMoviendoAutomaticamente)
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
        else if (!modoManual)
        {
            // En modo automático, solo aplicar gravedad para que se quede en el suelo
            CheckGrounded();
            ApplyGravity();
            controller.Move(velocity * Time.deltaTime);
        }
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
            string modo = modoManual ? "MANUAL (WASD)" : "AUTOMÁTICO (JSON)";
            GUI.Label(new Rect(10, 10, 300, 20), $"Modo: {modo}");
            GUI.Label(new Rect(10, 30, 300, 20), $"Posición Tablero: ({filaActual},{columnaActual})");
            GUI.Label(new Rect(10, 50, 300, 20), $"Velocidad: {(runInput ? "Corriendo" : "Caminando")}");
            GUI.Label(new Rect(10, 70, 300, 20), $"En suelo: {(isGrounded ? "Sí" : "No")}");
            
            if (modoManual)
                GUI.Label(new Rect(10, 90, 400, 20), "Controles: WASD - Movimiento, Mouse - Mirar, Espacio - Saltar, Shift - Correr");
        }
    }

    // ========== MÉTODOS PARA MOVIMIENTO AUTOMÁTICO DESDE JSON ==========

    /// <summary>
    /// Mueve el astronauta a una nueva posición en el tablero (usado por JSON/simulación)
    /// </summary>
    /// <param name="nuevaFila">Fila destino en el tablero</param>
    /// <param name="nuevaColumna">Columna destino en el tablero</param>
    /// <param name="tamanioCelda">Tamaño de cada celda del tablero</param>
    public System.Collections.IEnumerator MoverA(int nuevaFila, int nuevaColumna, float tamanioCelda)
    {
        if (estaMoviendoAutomaticamente)
        {
            Debug.LogWarning($"⚠️ Astronauta {astronautaID} ya está moviéndose");
            yield break;
        }

        estaMoviendoAutomaticamente = true;

        // Calcular posición 3D destino
        Vector3 posicionDestino = new Vector3(
            nuevaColumna * tamanioCelda,
            transform.position.y,
            nuevaFila * tamanioCelda
        );

        // Rotar hacia la dirección de movimiento
        Vector3 direccion = posicionDestino - transform.position;
        if (direccion.magnitude > 0.1f)
        {
            Quaternion rotacionDestino = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDestino, 0.5f);
        }

        // Mover suavemente con Lerp
        Vector3 posicionInicial = transform.position;
        float tiempoTranscurrido = 0f;
        float duracion = 1f / velocidadMovimientoAuto;

        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracion;
            transform.position = Vector3.Lerp(posicionInicial, posicionDestino, progreso);
            yield return null;
        }

        transform.position = posicionDestino;

        // Actualizar posición en tablero
        filaActual = nuevaFila;
        columnaActual = nuevaColumna;

        estaMoviendoAutomaticamente = false;

        Debug.Log($"✅ Astronauta {astronautaID} llegó a ({nuevaFila},{nuevaColumna})");
    }

    /// <summary>
    /// Teletransporta el astronauta a una posición sin animación
    /// </summary>
    public void TeletransportarA(int fila, int columna, float tamanioCelda)
    {
        Vector3 nuevaPosicion = new Vector3(
            columna * tamanioCelda,
            transform.position.y,
            fila * tamanioCelda
        );

        transform.position = nuevaPosicion;
        filaActual = fila;
        columnaActual = columna;

        Debug.Log($"📍 Astronauta {astronautaID} teletransportado a ({fila},{columna})");
    }

    /// <summary>
    /// Cambia entre modo manual y automático
    /// </summary>
    public void CambiarModoControl(bool manual)
    {
        modoManual = manual;
        
        if (manual)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log($"🎮 Astronauta {astronautaID}: Modo MANUAL activado");
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log($"🤖 Astronauta {astronautaID}: Modo AUTOMÁTICO activado");
        }
    }
} 