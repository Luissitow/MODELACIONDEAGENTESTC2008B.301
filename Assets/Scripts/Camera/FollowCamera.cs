using UnityEngine;

/// <summary>
/// Cámara que sigue a un objetivo en tercera persona con control de órbita
/// Basado en los estándares del laboratorio TC2008B
/// </summary>
public class FollowCamera : MonoBehaviour
{
    [Header("Objetivo")]
    [Tooltip("Transform del agente a seguir")]
    public Transform objetivo;
    
    [Header("Distancia y Posición")]
    [Tooltip("Distancia desde el objetivo")]
    public float distancia = 8f;
    
    [Tooltip("Altura sobre el objetivo")]
    public float altura = 5f;
    
    [Tooltip("Offset lateral (útil para dividir pantalla)")]
    public float offsetLateral = 0f;
    
    [Header("Suavizado")]
    [Tooltip("Velocidad de seguimiento (mayor = más rápido)")]
    public float suavizado = 5f;
    
    [Tooltip("Suavizado de rotación")]
    public float suavizadoRotacion = 3f;
    
    [Header("Órbita con Mouse")]
    [Tooltip("Permitir rotar alrededor del objetivo con el mouse")]
    public bool permitirOrbita = true;
    
    [Tooltip("Sensibilidad del mouse para órbita")]
    public float sensibilidadMouse = 3f;
    
    [Tooltip("Tecla para activar órbita (ej: botón derecho del mouse)")]
    public KeyCode teclaOrbita = KeyCode.Mouse1;
    
    [Tooltip("Límite de rotación vertical (arriba)")]
    public float limiteVerticalArriba = 80f;
    
    [Tooltip("Límite de rotación vertical (abajo)")]
    public float limiteVerticalAbajo = 10f;
    
    [Header("Zoom")]
    [Tooltip("Permitir hacer zoom con la rueda del mouse")]
    public bool permitirZoom = true;
    
    [Tooltip("Distancia mínima de zoom")]
    public float distanciaMinima = 3f;
    
    [Tooltip("Distancia máxima de zoom")]
    public float distanciaMaxima = 15f;
    
    [Tooltip("Velocidad de zoom")]
    public float velocidadZoom = 2f;
    
    [Header("Colisiones")]
    [Tooltip("Detectar colisiones con paredes")]
    public bool detectarColisiones = true;
    
    [Tooltip("Capas con las que puede colisionar")]
    public LayerMask capasColision = -1;
    
    [Tooltip("Radio de detección de colisión")]
    public float radioColision = 0.3f;
    
    // Variables privadas para control de órbita
    private float anguloHorizontal = 0f;
    private float anguloVertical = 30f;
    private Vector3 posicionDeseada;
    private bool estaActiva = false;
    
    void Start()
    {
        if (objetivo == null)
        {
            Debug.LogWarning("⚠️ FollowCamera no tiene objetivo asignado");
            enabled = false;
            return;
        }
        
        // Calcular ángulos iniciales basados en la posición actual
        Vector3 direccion = transform.position - objetivo.position;
        if (direccion != Vector3.zero)
        {
            anguloHorizontal = Mathf.Atan2(direccion.x, direccion.z) * Mathf.Rad2Deg;
            anguloVertical = Mathf.Asin(direccion.normalized.y) * Mathf.Rad2Deg;
        }
        
        estaActiva = true;
    }
    
    void LateUpdate()
    {
        if (objetivo == null || !estaActiva) return;
        
        // Manejar zoom
        if (permitirZoom)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            distancia -= scroll * velocidadZoom;
            distancia = Mathf.Clamp(distancia, distanciaMinima, distanciaMaxima);
        }
        
        // Manejar órbita
        if (permitirOrbita && Input.GetKey(teclaOrbita))
        {
            anguloHorizontal += Input.GetAxis("Mouse X") * sensibilidadMouse;
            anguloVertical -= Input.GetAxis("Mouse Y") * sensibilidadMouse;
            anguloVertical = Mathf.Clamp(anguloVertical, limiteVerticalAbajo, limiteVerticalArriba);
        }
        
        // Calcular posición deseada
        CalcularPosicionDeseada();
        
        // Mover cámara suavemente
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, Time.deltaTime * suavizado);
        
        // Rotar cámara para mirar al objetivo
        Vector3 direccionMirada = objetivo.position - transform.position;
        if (direccionMirada != Vector3.zero)
        {
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccionMirada);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, Time.deltaTime * suavizadoRotacion);
        }
    }
    
    /// <summary>
    /// Calcula la posición deseada de la cámara
    /// </summary>
    private void CalcularPosicionDeseada()
    {
        // Convertir ángulos a radianes
        float angHRad = anguloHorizontal * Mathf.Deg2Rad;
        float angVRad = anguloVertical * Mathf.Deg2Rad;
        
        // Calcular offset basado en ángulos
        Vector3 offset = new Vector3(
            Mathf.Sin(angHRad) * Mathf.Cos(angVRad),
            Mathf.Sin(angVRad),
            Mathf.Cos(angHRad) * Mathf.Cos(angVRad)
        );
        
        // Posición deseada sin colisiones
        posicionDeseada = objetivo.position + offset * distancia + Vector3.right * offsetLateral;
        
        // Ajustar por altura adicional
        posicionDeseada.y += altura;
        
        // Detectar colisiones
        if (detectarColisiones)
        {
            Vector3 direccion = posicionDeseada - objetivo.position;
            float distanciaTotal = direccion.magnitude;
            
            RaycastHit hit;
            if (Physics.SphereCast(objetivo.position, radioColision, direccion.normalized, out hit, distanciaTotal, capasColision))
            {
                // Ajustar posición para evitar atravesar paredes
                posicionDeseada = objetivo.position + direccion.normalized * (hit.distance - radioColision);
            }
        }
    }
    
    /// <summary>
    /// Reinicia la cámara a su posición por defecto
    /// </summary>
    public void ReiniciarPosicion()
    {
        anguloHorizontal = 0f;
        anguloVertical = 30f;
    }
    
    /// <summary>
    /// Establece el objetivo a seguir
    /// </summary>
    public void SetObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
        if (objetivo != null)
        {
            estaActiva = true;
            ReiniciarPosicion();
        }
    }
    
    /// <summary>
    /// Activa o desactiva la cámara
    /// </summary>
    public void SetActiva(bool activa)
    {
        estaActiva = activa;
    }
    
    /// <summary>
    /// Establece la distancia de la cámara
    /// </summary>
    public void SetDistancia(float nuevaDistancia)
    {
        distancia = Mathf.Clamp(nuevaDistancia, distanciaMinima, distanciaMaxima);
    }
    
    /// <summary>
    /// Establece la altura de la cámara
    /// </summary>
    public void SetAltura(float nuevaAltura)
    {
        altura = nuevaAltura;
    }
    
    /// <summary>
    /// Obtiene si la cámara está activa
    /// </summary>
    public bool EstaActiva()
    {
        return estaActiva;
    }
    
    void OnDrawGizmosSelected()
    {
        if (objetivo == null) return;
        
        // Dibujar línea desde objetivo a cámara
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(objetivo.position, transform.position);
        
        // Dibujar esfera en posición del objetivo
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(objetivo.position, 0.5f);
        
        // Dibujar esfera de colisión
        if (detectarColisiones)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radioColision);
        }
    }
}
