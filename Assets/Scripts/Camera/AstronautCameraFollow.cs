using UnityEngine;

/// <summary>
/// This camera follow script makes the camera follow the astronaut in a smooth way.
/// Based on TC2008B laboratory standards adapted for first-person perspective.
/// Standard coding documentation can be found in 
/// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments
/// </summary>
public class AstronautCameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    /// <summary>
    /// El objeto astronauta que la cámara debe seguir
    /// </summary>
    public GameObject astronaut;

    [Header("Camera Settings")]
    /// <summary>
    /// Offset de la cámara respecto al astronauta (para tercera persona)
    /// </summary>
    public Vector3 offset = new Vector3(0, 6, -7);
    
    /// <summary>
    /// Si true, usa primera persona. Si false, usa tercera persona.
    /// </summary>
    public bool firstPersonMode = true;
    
    /// <summary>
    /// Altura de la cámara en primera persona
    /// </summary>
    public float firstPersonHeight = 1.6f;
    
    /// <summary>
    /// Suavidad del seguimiento de la cámara
    /// </summary>
    public float followSmoothing = 5f;

    [Header("Mouse Look (Solo Primera Persona)")]
    /// <summary>
    /// Sensibilidad del mouse para primera persona
    /// </summary>
    public float mouseSensitivity = 100f;
    
    /// <summary>
    /// Rotación vertical de la cámara
    /// </summary>
    private float xRotation = 0f;

    /// <summary>
    /// This method is called before the first frame update
    /// </summary>
    void Start()
    {
        // Si no se asignó el astronauta, buscar uno automáticamente
        if (astronaut == null)
        {
            // Buscar por nombre común
            astronaut = GameObject.Find("Stylized Astronaut");
            if (astronaut == null)
                astronaut = GameObject.Find("Astronaut");
            if (astronaut == null)
                astronaut = GameObject.FindWithTag("Player");
        }

        if (astronaut == null)
        {
            Debug.LogError("❌ No se encontró el astronauta para seguir!");
            return;
        }

        // Si está en primera persona, configurar como cámara hija
        if (firstPersonMode)
        {
            transform.SetParent(astronaut.transform);
            transform.localPosition = new Vector3(0, firstPersonHeight, 0);
            transform.localRotation = Quaternion.identity;
            
            // Bloquear cursor en primera persona
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Debug.Log($"📷 Cámara configurada en modo: {(firstPersonMode ? "Primera Persona" : "Tercera Persona")}");
    }

    /// <summary>
    /// LateUpdate se ejecuta después de Update para un seguimiento más suave
    /// </summary>
    void LateUpdate()
    {
        if (astronaut == null) return;

        if (firstPersonMode)
        {
            HandleFirstPersonCamera();
        }
        else
        {
            HandleThirdPersonCamera();
        }

        // Cambiar modo de cámara con C
        if (Input.GetKeyDown("c"))
        {
            ToggleCameraMode();
        }
    }

    /// <summary>
    /// Maneja la cámara en primera persona
    /// </summary>
    void HandleFirstPersonCamera()
    {
        // La cámara ya está como hija del astronauta, solo manejar rotación del mouse
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Rotar el astronauta horizontalmente
            astronaut.transform.Rotate(Vector3.up * mouseX);

            // Rotar la cámara verticalmente
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    /// <summary>
    /// Maneja la cámara en tercera persona
    /// </summary>
    void HandleThirdPersonCamera()
    {
        // Posición objetivo de la cámara
        Vector3 targetPosition = astronaut.transform.position + offset;
        
        // Movimiento suave hacia la posición objetivo
        if (followSmoothing > 0)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSmoothing * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }

        // Mirar hacia el astronauta
        Vector3 lookDirection = astronaut.transform.position - transform.position;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, followSmoothing * Time.deltaTime);
        }
    }

    /// <summary>
    /// Cambia entre primera y tercera persona
    /// </summary>
    public void ToggleCameraMode()
    {
        firstPersonMode = !firstPersonMode;

        if (firstPersonMode)
        {
            // Cambiar a primera persona
            transform.SetParent(astronaut.transform);
            transform.localPosition = new Vector3(0, firstPersonHeight, 0);
            transform.localRotation = Quaternion.identity;
            xRotation = 0f;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Cambiar a tercera persona
            transform.SetParent(null);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Debug.Log($"📷 Cámara cambiada a: {(firstPersonMode ? "Primera Persona" : "Tercera Persona")}");
    }

    /// <summary>
    /// Configuración automática de la cámara
    /// </summary>
    [ContextMenu("Auto Setup Camera")]
    public void AutoSetupCamera()
    {
        Debug.Log("🔧 Configurando cámara automáticamente...");

        // Buscar astronauta automáticamente
        if (astronaut == null)
        {
            astronaut = GameObject.Find("Stylized Astronaut");
            if (astronaut == null)
                astronaut = GameObject.Find("Astronaut");
            if (astronaut == null)
                astronaut = GameObject.FindWithTag("Player");
        }

        if (astronaut != null)
        {
            Debug.Log($"✅ Astronauta encontrado: {astronaut.name}");
            
            // Configurar para primera persona por defecto
            firstPersonMode = true;
            transform.SetParent(astronaut.transform);
            transform.localPosition = new Vector3(0, firstPersonHeight, 0);
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogError("❌ No se pudo encontrar el astronauta");
        }
    }

    /// <summary>
    /// Muestra información de debug
    /// </summary>
    void OnGUI()
    {
        if (Application.isEditor)
        {
            string mode = firstPersonMode ? "Primera Persona" : "Tercera Persona";
            GUI.Label(new Rect(10, 100, 300, 20), $"Modo Cámara: {mode} (C para cambiar)");
            
            if (firstPersonMode)
            {
                GUI.Label(new Rect(10, 120, 300, 20), "Mouse para mirar - ESC para liberar cursor");
            }
        }
    }

    /// <summary>
    /// Dibuja gizmos para debug
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (astronaut != null && !firstPersonMode)
        {
            // Mostrar línea desde el astronauta hasta la cámara
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(astronaut.transform.position, transform.position);
            
            // Mostrar posición objetivo de la cámara
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(astronaut.transform.position + offset, 0.5f);
        }
    }
}