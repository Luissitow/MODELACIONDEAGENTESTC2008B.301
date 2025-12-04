using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestiona el cambio entre múltiples cámaras de diferentes agentes
/// Controles: 
/// - Teclas numéricas (1-9) para cambiar al agente correspondiente
/// - Tab para cambiar entre primera y tercera persona
/// - F para cámara libre
/// </summary>
public class CameraManager : MonoBehaviour
{
    [Header("Configuración de Cámaras")]
    [Tooltip("Lista de todos los agentes (Crew) en la escena")]
    public List<GameObject> agentes = new List<GameObject>();
    
    [Tooltip("Cámara libre para observar toda la escena")]
    public Camera camaraLibre;
    
    [Tooltip("Cámara aérea / vista de pájaro")]
    public Camera camaraAerea;
    
    [Header("Tipos de Vista")]
    [Tooltip("Usar primera persona por defecto")]
    public bool iniciarEnPrimeraPersona = false;
    
    [Header("Configuración UI")]
    [Tooltip("Mostrar información de cámara actual en consola")]
    public bool mostrarInfo = true;
    
    // Estado interno
    private int indiceAgenteActual = 0;
    private bool enPrimeraPersona = false;
    private bool enCamaraLibre = false;
    private bool enCamaraAerea = false;
    
    // Referencias a cámaras
    private List<Camera> camarasPrimeraPersona = new List<Camera>();
    private List<Camera> camarasTerceraPersona = new List<Camera>();
    
    void Start()
    {
        // Buscar cámara libre si no está asignada
        if (camaraLibre == null)
        {
            CamaraLibre camLibre = FindAnyObjectByType<CamaraLibre>();
            if (camLibre != null)
            {
                camaraLibre = camLibre.GetComponent<Camera>();
            }
        }
        
        // Buscar cámara aérea si no está asignada
        if (camaraAerea == null)
        {
            GameObject camaraAereaObj = GameObject.Find("CamaraAerea");
            if (camaraAereaObj != null)
            {
                camaraAerea = camaraAereaObj.GetComponent<Camera>();
                if (camaraAerea != null && mostrarInfo)
                {
                    Debug.Log("📹 CamaraAerea encontrada y asignada automáticamente");
                }
            }
            else if (mostrarInfo)
            {
                Debug.LogWarning("⚠️ No se encontró CamaraAerea en la escena. Asígnala manualmente o créala.");
            }
        }
        
        // Configurar vista inicial
        enPrimeraPersona = iniciarEnPrimeraPersona;
        
        // Esperar un frame para que TableroBuilder cree los agentes
        StartCoroutine(InicializarConRetraso());
    }
    
    private System.Collections.IEnumerator InicializarConRetraso()
    {
        // Esperar 2 frames para asegurar que TableroBuilder haya terminado
        yield return null;
        yield return null;
        
        // Buscar agentes automáticamente si no fueron agregados manualmente
        if (agentes.Count == 0)
        {
            BuscarAgentesEnEscena();
        }
        
        // Configurar cámaras para cada agente
        if (agentes.Count > 0)
        {
            ConfigurarCamarasDeAgentes();
            
            // Iniciar en cámara aérea por defecto
            if (camaraAerea != null)
            {
                enCamaraAerea = true;
                DesactivarTodasLasCamaras();
                camaraAerea.enabled = true;
                
                if (mostrarInfo)
                {
                    Debug.Log($"📹 CameraManager inicializado con {agentes.Count} agentes");
                    Debug.Log($"📹 Iniciando en VISTA AÉREA");
                    Debug.Log($"   Controles: ← → para cambiar agente | Tab para cambiar vista | F para vista aérea");
                }
            }
            else
            {
                // Si no hay cámara aérea, iniciar con primer agente
                ActivarCamara(0, enPrimeraPersona);
                
                if (mostrarInfo)
                {
                    Debug.Log($"📹 CameraManager inicializado con {agentes.Count} agentes");
                    Debug.Log($"   Controles: ← → para cambiar agente | Tab para cambiar vista");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ CameraManager: No se encontraron agentes en la escena");
        }
    }
    
    void Update()
    {
        // Alternar entre primera y tercera persona con Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!enCamaraLibre && !enCamaraAerea)
            {
                AlternarTipoDeVista();
            }
        }
        
        // Activar/desactivar cámara aérea con F
        if (Input.GetKeyDown(KeyCode.F))
        {
            AlternarCamaraAerea();
        }
        
        // Navegación con flechas (siguiente/anterior agente)
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SiguienteAgente();
        }
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            AnteriorAgente();
        }
    }
    
    /// <summary>
    /// Busca automáticamente todos los agentes Crew en la escena
    /// </summary>
    private void BuscarAgentesEnEscena()
    {
        Crew[] crews = FindObjectsByType<Crew>(FindObjectsSortMode.None);
        foreach (Crew crew in crews)
        {
            agentes.Add(crew.gameObject);
        }
        
        if (agentes.Count == 0)
        {
            Debug.LogWarning("⚠️ No se encontraron agentes Crew en la escena");
        }
    }
    
    /// <summary>
    /// Configura las cámaras de primera y tercera persona para cada agente
    /// </summary>
    private void ConfigurarCamarasDeAgentes()
    {
        for (int i = 0; i < agentes.Count; i++)
        {
            GameObject agente = agentes[i];
            
            // Buscar o crear cámara de primera persona
            FirstPersonCamera fpCam = agente.GetComponentInChildren<FirstPersonCamera>();
            if (fpCam == null)
            {
                fpCam = CrearCamaraPrimeraPersona(agente, i);
            }
            camarasPrimeraPersona.Add(fpCam.GetComponent<Camera>());
            
            // Buscar o crear cámara de tercera persona
            FollowCamera followCam = FindCameraFollowingAgent(agente);
            if (followCam == null)
            {
                followCam = CrearCamaraTerceraPersona(agente, i);
            }
            camarasTerceraPersona.Add(followCam.GetComponent<Camera>());
        }
    }
    
    /// <summary>
    /// Crea una cámara de primera persona para un agente
    /// </summary>
    private FirstPersonCamera CrearCamaraPrimeraPersona(GameObject agente, int indice)
    {
        GameObject camObj = new GameObject($"FPCamera_Agent{indice + 1}");
        camObj.transform.SetParent(agente.transform);
        camObj.transform.localPosition = new Vector3(0, 1.6f, 0);
        camObj.transform.localRotation = Quaternion.identity;
        
        Camera cam = camObj.AddComponent<Camera>();
        cam.enabled = false;
        
        FirstPersonCamera fpCam = camObj.AddComponent<FirstPersonCamera>();
        fpCam.alturaCamera = 1.6f;
        
        return fpCam;
    }
    
    /// <summary>
    /// Crea una cámara de tercera persona para un agente
    /// </summary>
    private FollowCamera CrearCamaraTerceraPersona(GameObject agente, int indice)
    {
        GameObject camObj = new GameObject($"FollowCamera_Agent{indice + 1}");
        
        Camera cam = camObj.AddComponent<Camera>();
        cam.enabled = false;
        
        FollowCamera followCam = camObj.AddComponent<FollowCamera>();
        followCam.objetivo = agente.transform;
        followCam.distancia = 8f;
        followCam.altura = 5f;
        followCam.suavizado = 5f;
        
        return followCam;
    }
    
    /// <summary>
    /// Busca una FollowCamera que esté siguiendo a un agente específico
    /// </summary>
    private FollowCamera FindCameraFollowingAgent(GameObject agente)
    {
        FollowCamera[] allFollowCams = FindObjectsByType<FollowCamera>(FindObjectsSortMode.None);
        foreach (FollowCamera cam in allFollowCams)
        {
            if (cam.objetivo == agente.transform)
            {
                return cam;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Cambia a un agente específico
    /// </summary>
    public void CambiarAAgente(int indice)
    {
        if (indice < 0 || indice >= agentes.Count)
        {
            Debug.LogWarning($"⚠️ Índice de agente fuera de rango: {indice}");
            return;
        }
        
        enCamaraLibre = false;
        enCamaraAerea = false;
        indiceAgenteActual = indice;
        ActivarCamara(indice, enPrimeraPersona);
    }
    
    /// <summary>
    /// Cambia al siguiente agente
    /// </summary>
    public void SiguienteAgente()
    {
        int nuevoIndice = (indiceAgenteActual + 1) % agentes.Count;
        CambiarAAgente(nuevoIndice);
    }
    
    /// <summary>
    /// Cambia al agente anterior
    /// </summary>
    public void AnteriorAgente()
    {
        int nuevoIndice = indiceAgenteActual - 1;
        if (nuevoIndice < 0) nuevoIndice = agentes.Count - 1;
        CambiarAAgente(nuevoIndice);
    }
    
    /// <summary>
    /// Alterna entre primera y tercera persona
    /// </summary>
    public void AlternarTipoDeVista()
    {
        enPrimeraPersona = !enPrimeraPersona;
        ActivarCamara(indiceAgenteActual, enPrimeraPersona);
    }
    
    /// <summary>
    /// Alterna la cámara libre
    /// </summary>
    public void AlternarCamaraLibre()
    {
        enCamaraLibre = !enCamaraLibre;
        enCamaraAerea = false;
        
        if (enCamaraLibre)
        {
            DesactivarTodasLasCamaras();
            if (camaraLibre != null)
            {
                camaraLibre.enabled = true;
                CamaraLibre camLibre = camaraLibre.GetComponent<CamaraLibre>();
                if (camLibre != null)
                {
                    camLibre.enabled = true;
                }
                
                if (mostrarInfo)
                {
                    Debug.Log($"📹 Cámara LIBRE activada");
                }
            }
        }
        else
        {
            if (camaraLibre != null)
            {
                camaraLibre.enabled = false;
                CamaraLibre camLibre = camaraLibre.GetComponent<CamaraLibre>();
                if (camLibre != null)
                {
                    camLibre.enabled = false;
                }
            }
            ActivarCamara(indiceAgenteActual, enPrimeraPersona);
        }
    }
    
    /// <summary>
    /// Alterna la cámara aérea (vista de pájaro)
    /// </summary>
    public void AlternarCamaraAerea()
    {
        enCamaraAerea = !enCamaraAerea;
        enCamaraLibre = false;
        
        if (enCamaraAerea)
        {
            DesactivarTodasLasCamaras();
            if (camaraAerea != null)
            {
                camaraAerea.enabled = true;
                
                if (mostrarInfo)
                {
                    Debug.Log($"📹 Cámara AÉREA activada (vista de pájaro)");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Cámara aérea no asignada. Créala en la escena y asígnala al CameraManager.");
                enCamaraAerea = false;
            }
        }
        else
        {
            if (camaraAerea != null)
            {
                camaraAerea.enabled = false;
            }
            ActivarCamara(indiceAgenteActual, enPrimeraPersona);
        }
    }
    
    /// <summary>
    /// Activa la cámara de un agente específico
    /// </summary>
    private void ActivarCamara(int indice, bool primeraPersona)
    {
        DesactivarTodasLasCamaras();
        
        if (primeraPersona && indice < camarasPrimeraPersona.Count)
        {
            Camera cam = camarasPrimeraPersona[indice];
            cam.enabled = true;
            
            FirstPersonCamera fpCam = cam.GetComponent<FirstPersonCamera>();
            if (fpCam != null)
            {
                fpCam.enabled = true;
                fpCam.SetActiva(true);
            }
            
            if (mostrarInfo)
            {
                Debug.Log($"📹 Agente {indice + 1} - PRIMERA PERSONA");
            }
        }
        else if (!primeraPersona && indice < camarasTerceraPersona.Count)
        {
            Camera cam = camarasTerceraPersona[indice];
            cam.enabled = true;
            
            FollowCamera followCam = cam.GetComponent<FollowCamera>();
            if (followCam != null)
            {
                followCam.enabled = true;
            }
            
            if (mostrarInfo)
            {
                Debug.Log($"📹 Agente {indice + 1} - TERCERA PERSONA");
            }
        }
    }
    
    /// <summary>
    /// Desactiva todas las cámaras de agentes
    /// </summary>
    private void DesactivarTodasLasCamaras()
    {
        // Desactivar cámaras de primera persona
        foreach (Camera cam in camarasPrimeraPersona)
        {
            if (cam != null)
            {
                cam.enabled = false;
                FirstPersonCamera fpCam = cam.GetComponent<FirstPersonCamera>();
                if (fpCam != null)
                {
                    fpCam.SetActiva(false);
                    fpCam.enabled = false;
                }
            }
        }
        
        // Desactivar cámaras de tercera persona
        foreach (Camera cam in camarasTerceraPersona)
        {
            if (cam != null)
            {
                cam.enabled = false;
                FollowCamera followCam = cam.GetComponent<FollowCamera>();
                if (followCam != null)
                {
                    followCam.enabled = false;
                }
            }
        }
        
        // Desactivar cámara libre
        if (camaraLibre != null)
        {
            camaraLibre.enabled = false;
        }
    }
    
    /// <summary>
    /// Obtiene el agente actualmente seleccionado
    /// </summary>
    public GameObject GetAgenteActual()
    {
        if (indiceAgenteActual >= 0 && indiceAgenteActual < agentes.Count)
        {
            return agentes[indiceAgenteActual];
        }
        return null;
    }
    
    /// <summary>
    /// Agrega un nuevo agente al sistema de cámaras
    /// </summary>
    public void AgregarAgente(GameObject nuevoAgente)
    {
        if (nuevoAgente == null || agentes.Contains(nuevoAgente))
            return;
            
        agentes.Add(nuevoAgente);
        
        if (mostrarInfo)
        {
            Debug.Log($"📹 Agente registrado: {nuevoAgente.name} (Total: {agentes.Count})");
        }
    }
}
