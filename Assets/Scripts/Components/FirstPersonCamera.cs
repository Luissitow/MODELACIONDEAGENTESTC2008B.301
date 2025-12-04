using UnityEngine;

/// <summary>
/// Cámara en primera persona que se puede adjuntar a un tripulante
/// </summary>
public class FirstPersonCamera : MonoBehaviour
{
    [Header("Configuración de Cámara")]
    [Tooltip("Altura de la cámara sobre el agente")]
    public float alturaCamera = 1.5f;
    
    [Tooltip("Offset adelante/atrás desde el centro")]
    public float offsetAdelante = 0f;
    
    [Header("Control de Rotación")]
    [Tooltip("Permitir rotación libre con el mouse")]
    public bool permitirRotacion = true;
    
    [Tooltip("Sensibilidad del mouse horizontal")]
    public float sensibilidadX = 2f;
    
    [Tooltip("Sensibilidad del mouse vertical")]
    public float sensibilidadY = 2f;
    
    [Tooltip("Límite de rotación vertical (arriba)")]
    public float limiteVerticalArriba = 80f;
    
    [Tooltip("Límite de rotación vertical (abajo)")]
    public float limiteVerticalAbajo = -80f;
    
    [Header("Opciones Avanzadas")]
    [Tooltip("Bloquear cursor cuando está activa")]
    public bool bloquearCursor = false;
    
    private float rotacionX = 0f;
    private float rotacionY = 0f;
    private Transform agente;
    private bool estaActiva = false;
    private bool cursorBloqueado = false;
    
    void Start()
    {
        agente = transform.parent;
        if (agente == null)
        {
            Debug.LogError("FirstPersonCamera debe ser hijo de un agente");
            enabled = false;
            return;
        }
        
        // Posicionar cámara
        transform.localPosition = new Vector3(offsetAdelante, alturaCamera, 0);
        transform.localRotation = Quaternion.identity;
    }
    
    void LateUpdate()
    {
        if (!estaActiva) return;
        
        if (permitirRotacion)
        {
            // Control de rotación con mouse
            rotacionX += Input.GetAxis("Mouse X") * sensibilidadX;
            rotacionY -= Input.GetAxis("Mouse Y") * sensibilidadY;
            
            // Limitar rotación vertical
            rotacionY = Mathf.Clamp(rotacionY, limiteVerticalAbajo, limiteVerticalArriba);
            
            // Aplicar rotación
            transform.localRotation = Quaternion.Euler(rotacionY, rotacionX, 0);
        }
    }
    
    /// <summary>
    /// Activa o desactiva esta cámara
    /// </summary>
    public void SetActiva(bool activa)
    {
        estaActiva = activa;
        
        if (activa)
        {
            // Resetear rotación al activar
            rotacionX = 0f;
            rotacionY = 0f;
            transform.localRotation = Quaternion.identity;
            
            // Bloquear cursor si está habilitado
            if (bloquearCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                cursorBloqueado = true;
            }
        }
        else
        {
            // Desbloquear cursor al desactivar
            if (cursorBloqueado)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                cursorBloqueado = false;
            }
        }
    }
    
    /// <summary>
    /// Verifica si esta cámara está activa
    /// </summary>
    public bool EstaActiva()
    {
        return estaActiva;
    }
    
    /// <summary>
    /// Establece si se permite la rotación
    /// </summary>
    public void SetPermitirRotacion(bool permitir)
    {
        permitirRotacion = permitir;
    }
    
    /// <summary>
    /// Reinicia la rotación a valores por defecto
    /// </summary>
    public void ReiniciarRotacion()
    {
        rotacionX = 0f;
        rotacionY = 0f;
        transform.localRotation = Quaternion.identity;
    }
}
