using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Herramienta de validaci\u00f3n para el sistema de paredes
/// A\u00f1ade un bot\u00f3n en el Inspector de ConstructorTablero para verificar configuraci\u00f3n
/// </summary>
#if UNITY_EDITOR
[CustomEditor(typeof(ConstructorTablero))]
public class ConstructorTableroEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ConstructorTablero constructor = (ConstructorTablero)target;
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Herramientas de Validaci\u00f3n", EditorStyles.boldLabel);
        
        if (GUILayout.Button("\ud83d\udd0d Validar Configuraci\u00f3n de Paredes", GUILayout.Height(40)))
        {
            ValidarConfiguracion(constructor);
        }
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("\ud83d\udee0\ufe0f Crear Prefabs de Estados (Placeholder)", GUILayout.Height(30)))
        {
            CrearPrefabsPlaceholder(constructor);
        }
    }
    
    void ValidarConfiguracion(ConstructorTablero constructor)
    {
        Debug.Log("====================================");
        Debug.Log("\ud83d\udd0d VALIDACI\u00d3N DE CONFIGURACI\u00d3N DE PAREDES");
        Debug.Log("====================================");
        
        int errores = 0;
        int advertencias = 0;
        
        // 1. Verificar prefabs b\u00e1sicos
        if (constructor.aranaPrefab == null)
        {
            Debug.LogError("\u274c aranaPrefab NO asignado");
            errores++;
        }
        else
        {
            Debug.Log("\u2705 aranaPrefab: " + constructor.aranaPrefab.name);
        }
        
        if (constructor.huevoPrefab == null)
        {
            Debug.LogError("\u274c huevoPrefab NO asignado");
            errores++;
        }
        else
        {
            Debug.Log("\u2705 huevoPrefab: " + constructor.huevoPrefab.name);
        }
        
        // 2. Verificar prefabs de pared
        var paredPrefabField = typeof(ConstructorTablero).GetField("paredPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        GameObject paredPrefab = paredPrefabField?.GetValue(constructor) as GameObject;
        
        if (paredPrefab == null)
        {
            Debug.LogError("\u274c paredPrefab NO asignado en ConstructorTablero");
            errores++;
        }
        else
        {
            Debug.Log("\u2705 paredPrefab: " + paredPrefab.name);
            ValidarPrefabPared(paredPrefab, ref errores, ref advertencias);
        }
        
        // 3. Verificar paredDanadaPrefab
        var paredDanadaField = typeof(ConstructorTablero).GetField("paredDanadaPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        GameObject paredDanadaPrefab = paredDanadaField?.GetValue(constructor) as GameObject;
        
        if (paredDanadaPrefab == null)
        {
            Debug.LogWarning("\u26a0\ufe0f paredDanadaPrefab NO asignado - Las paredes NO cambiar\u00e1n de apariencia al da\u00f1arse");
            advertencias++;
        }
        else
        {
            Debug.Log("\u2705 paredDanadaPrefab: " + paredDanadaPrefab.name);
            ValidarPrefabPared(paredDanadaPrefab, ref errores, ref advertencias);
        }
        
        // 4. Verificar paredDestruidaPrefab
        var paredDestruidaField = typeof(ConstructorTablero).GetField("paredDestruidaPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        GameObject paredDestruidaPrefab = paredDestruidaField?.GetValue(constructor) as GameObject;
        
        if (paredDestruidaPrefab == null)
        {
            Debug.LogWarning("\u26a0\ufe0f paredDestruidaPrefab NO asignado - Las paredes desaparecer\u00e1n al destruirse");
            advertencias++;
        }
        else
        {
            Debug.Log("\u2705 paredDestruidaPrefab: " + paredDestruidaPrefab.name);
            ValidarPrefabPared(paredDestruidaPrefab, ref errores, ref advertencias);
        }
        
        // 5. Verificar puertaPrefab
        var puertaPrefabField = typeof(ConstructorTablero).GetField("puertaPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        GameObject puertaPrefab = puertaPrefabField?.GetValue(constructor) as GameObject;
        
        if (puertaPrefab == null)
        {
            Debug.LogError("\u274c puertaPrefab NO asignado");
            errores++;
        }
        else
        {
            Debug.Log("\u2705 puertaPrefab: " + puertaPrefab.name);
            ValidarPrefabPared(puertaPrefab, ref errores, ref advertencias);
        }
        
        // 6. Verificar tag Wall
        bool tagExists = false;
        try
        {
            GameObject.FindGameObjectWithTag("Wall");
            tagExists = true;
        }
        catch
        {
            tagExists = false;
        }
        
        if (!tagExists)
        {
            Debug.LogError("\u274c Tag 'Wall' NO existe - Crea el tag en Project Settings > Tags and Layers");
            errores++;
        }
        else
        {
            Debug.Log("\u2705 Tag 'Wall' existe");
        }
        
        // Resumen
        Debug.Log("====================================");
        if (errores == 0 && advertencias == 0)
        {
            Debug.Log("\ud83c\udf89 \u00a1CONFIGURACI\u00d3N PERFECTA! Todo listo para usar.");
        }
        else if (errores == 0)
        {
            Debug.LogWarning($"\u26a0\ufe0f Configuraci\u00f3n funcional con {advertencias} advertencia(s). El sistema funcionar\u00e1 pero sin cambios visuales completos.");
        }
        else
        {
            Debug.LogError($"\u274c Se encontraron {errores} error(es) y {advertencias} advertencia(s). Revisa la configuraci\u00f3n.");
        }
        Debug.Log("====================================");
    }
    
    void ValidarPrefabPared(GameObject prefab, ref int errores, ref int advertencias)
    {
        if (prefab == null) return;
        
        // Verificar componente Wall
        Wall wall = prefab.GetComponent<Wall>();
        if (wall == null)
        {
            Debug.LogWarning($"  \u26a0\ufe0f {prefab.name} NO tiene componente Wall - Se agregar\u00e1 autom\u00e1ticamente en runtime");
            advertencias++;
        }
        else
        {
            // Verificar referencias de prefabs
            if (wall.prefabNormal == null)
            {
                Debug.LogWarning($"  \u26a0\ufe0f {prefab.name} > Wall > prefabNormal NO asignado");
                advertencias++;
            }
            
            if (wall.prefabDanado == null)
            {
                Debug.LogWarning($"  \u26a0\ufe0f {prefab.name} > Wall > prefabDanado NO asignado - No habr\u00e1 cambio visual al da\u00f1arse");
                advertencias++;
            }
            
            if (wall.prefabDestruido == null)
            {
                Debug.LogWarning($"  \u26a0\ufe0f {prefab.name} > Wall > prefabDestruido NO asignado - Se desactivar\u00e1 al destruirse");
                advertencias++;
            }
        }
        
        // Verificar tag
        if (prefab.tag != "Wall")
        {
            Debug.LogWarning($"  \u26a0\ufe0f {prefab.name} NO tiene tag 'Wall' - Puede causar problemas en ActionExecutor");
            advertencias++;
        }
    }
    
    void CrearPrefabsPlaceholder(ConstructorTablero constructor)
    {
        Debug.Log("====================================");
        Debug.Log("\ud83d\udee0\ufe0f CREANDO PREFABS PLACEHOLDER");
        Debug.Log("====================================");
        
        // Obtener paredPrefab
        var paredPrefabField = typeof(ConstructorTablero).GetField("paredPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        GameObject paredPrefab = paredPrefabField?.GetValue(constructor) as GameObject;
        
        if (paredPrefab == null)
        {
            Debug.LogError("\u274c No se puede crear placeholders sin paredPrefab asignado");
            return;
        }
        
        // Crear carpeta si no existe
        string carpeta = "Assets/Prefabs/Paredes";
        if (!AssetDatabase.IsValidFolder(carpeta))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Paredes");
        }
        
        // Crear ParedDanada (color gris oscuro)
        GameObject paredDanada = PrefabUtility.InstantiatePrefab(paredPrefab) as GameObject;
        paredDanada.name = "ParedDanada_Placeholder";
        
        var renderer = paredDanada.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.4f, 0.3f, 0.3f); // Gris oscuro
            renderer.material = mat;
        }
        
        string pathDanada = carpeta + "/ParedDanada_Placeholder.prefab";
        PrefabUtility.SaveAsPrefabAsset(paredDanada, pathDanada);
        DestroyImmediate(paredDanada);
        Debug.Log($"\u2705 Creado: {pathDanada}");
        
        // Crear ParedDestruida (m\u00e1s peque\u00f1a, rotada)
        GameObject paredDestruida = PrefabUtility.InstantiatePrefab(paredPrefab) as GameObject;
        paredDestruida.name = "ParedDestruida_Placeholder";
        paredDestruida.transform.localScale = new Vector3(0.7f, 0.3f, 0.7f);
        paredDestruida.transform.rotation = Quaternion.Euler(0, 0, 15);
        
        var renderer2 = paredDestruida.GetComponent<MeshRenderer>();
        if (renderer2 != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.2f, 0.2f); // Muy oscuro
            renderer2.material = mat;
        }
        
        string pathDestruida = carpeta + "/ParedDestruida_Placeholder.prefab";
        PrefabUtility.SaveAsPrefabAsset(paredDestruida, pathDestruida);
        DestroyImmediate(paredDestruida);
        Debug.Log($"\u2705 Creado: {pathDestruida}");
        
        Debug.Log("====================================");
        Debug.Log("\u2705 Prefabs placeholder creados en: " + carpeta);
        Debug.Log("\ud83d\udc49 Ahora asigna estos prefabs en ConstructorTablero y en cada prefab individual");
        Debug.Log("====================================");
        
        AssetDatabase.Refresh();
    }
}
#endif
