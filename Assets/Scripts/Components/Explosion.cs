using System.Collections;
using UnityEngine;

namespace FireRescue.Components
{ 
    /// <summary>
    /// Componente para efectos visuales de explosión.
    /// Se instancia temporalmente en el centro de una celda cuando una araña explota.
    /// </summary>
    public class Explosion : MonoBehaviour
    {
        [Header("Configuración Visual")]
        [SerializeField] private float duracionExplosion = 1.0f;
        [SerializeField] private float escalaMaxima = 3.0f;
        [SerializeField] private Color colorInicial = new Color(1f, 0.5f, 0f, 1f); // Naranja brillante
        [SerializeField] private Color colorFinal = new Color(1f, 0f, 0f, 0f); // Rojo transparente
        
        [Header("Efectos de Partículas")]
        [SerializeField] private ParticleSystem particulasExplosion;
        
        private Renderer rendererExplosion;
        private MaterialPropertyBlock propBlock;
        
        private void Awake()
        {
            rendererExplosion = GetComponent<Renderer>();
            propBlock = new MaterialPropertyBlock();
        }
        
        private void Start()
        {
            StartCoroutine(AnimarExplosion());
        }
        
        /// <summary>
        /// Crea una explosión en la posición especificada.
        /// </summary>
        public static GameObject Crear(Vector3 posicion, GameObject prefabExplosion = null)
        {
            GameObject explosion;
            
            if (prefabExplosion != null)
            {
                explosion = Instantiate(prefabExplosion, posicion, Quaternion.identity);
            }
            else
            {
                // Crear explosión simple con esfera si no hay prefab
                explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                explosion.name = "Explosion_Temp";
                explosion.transform.position = posicion;
                explosion.transform.localScale = Vector3.zero;
                
                // Configurar material emisivo
                var renderer = explosion.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.red * 2f);
                    mat.color = new Color(1f, 0.5f, 0f);
                    renderer.material = mat;
                }
                
                // Agregar componente Explosion
                explosion.AddComponent<Explosion>();
            }
            
            return explosion;
        }
        
        private IEnumerator AnimarExplosion()
        {
            float tiempoTranscurrido = 0f;
            Vector3 escalaInicial = Vector3.zero;
            Vector3 escalaFinal = Vector3.one * escalaMaxima;
            
            // Activar partículas si existen
            if (particulasExplosion != null)
            {
                particulasExplosion.Play();
            }
            
            // Fase 1: Expansión rápida (30% del tiempo)
            float duracionExpansion = duracionExplosion * 0.3f;
            while (tiempoTranscurrido < duracionExpansion)
            {
                float t = tiempoTranscurrido / duracionExpansion;
                transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t);
                
                // Cambiar color gradualmente
                if (rendererExplosion != null)
                {
                    Color colorActual = Color.Lerp(colorInicial, colorFinal, t * 0.5f);
                    rendererExplosion.GetPropertyBlock(propBlock);
                    propBlock.SetColor("_Color", colorActual);
                    rendererExplosion.SetPropertyBlock(propBlock);
                }
                
                tiempoTranscurrido += Time.deltaTime;
                yield return null;
            }
            
            // Fase 2: Desvanecimiento (70% del tiempo)
            float duracionDesvanecimiento = duracionExplosion * 0.7f;
            tiempoTranscurrido = 0f;
            
            while (tiempoTranscurrido < duracionDesvanecimiento)
            {
                float t = tiempoTranscurrido / duracionDesvanecimiento;
                
                // Mantener escala máxima pero desvanecer
                if (rendererExplosion != null)
                {
                    Color colorActual = Color.Lerp(colorInicial, colorFinal, t);
                    rendererExplosion.GetPropertyBlock(propBlock);
                    propBlock.SetColor("_Color", colorActual);
                    rendererExplosion.SetPropertyBlock(propBlock);
                }
                
                tiempoTranscurrido += Time.deltaTime;
                yield return null;
            }
            
            // Destruir explosión
            Debug.Log($"💥 Explosión completada - Destruyendo GameObject");
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Reproduce sonido de explosión (placeholder para futuro sistema de audio).
        /// </summary>
        public void ReproducirSonido()
        {
            // TODO: Integrar con sistema de audio
            Debug.Log("🔊 BOOM! Sonido de explosión");
        }
    }
}
