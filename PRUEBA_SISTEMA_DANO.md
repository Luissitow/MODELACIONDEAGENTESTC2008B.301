# 🧪 Prueba del Sistema de Daño - Instrucciones

## ✅ ¿Qué Se Modificó?

### 1. **simulacion_completa.json** - Acciones de prueba agregadas

Se agregaron las siguientes acciones de daño a la simulación:

#### **Turno 2** - Primer golpe a pared (Astronauta 1)
```json
{
  "tipo": "danar_pared",
  "fila": 2,
  "columna": 1,
  "direccion": "norte",
  "costo": 2
}
```
**Resultado esperado:** 
- 🔨 Pared en (2,1) norte recibe 1 daño
- 📊 Vida: 2 → 1
- 🎨 Si hay `prefabDanado` asignado → cambia visual
- 📝 Log: `"🔨 Pared atacada en (2,1) norte - Vida: 2 → 1"`

---

#### **Turno 3** - Segundo golpe a la MISMA pared (Astronauta 1)
```json
{
  "tipo": "danar_pared",
  "fila": 2,
  "columna": 1,
  "direccion": "norte",
  "costo": 2
}
```
**Resultado esperado:**
- 💥 Pared en (2,1) norte recibe 1 daño más
- 📊 Vida: 1 → 0
- ❌ Pared DESTRUIDA
- 🎨 Si hay `prefabDestruido` → cambia visual, si no → desaparece
- 📝 Log: `"💥 Pared DESTRUIDA en (2,1) norte"`

---

#### **Turno 3** - Abrir puerta (Astronauta 2)
```json
{
  "tipo": "abrir_puerta",
  "fila": 3,
  "columna": 6,
  "direccion": "oeste",
  "costo": 1
}
```
**Resultado esperado:**
- 🚪 Puerta se abre (animación hacia arriba)
- 📝 Log: `"🚪 Puerta abierta en (3,6) oeste"`

---

#### **Turno 4** - Romper pared instantáneamente (Astronauta 1)
```json
{
  "tipo": "romper_pared",
  "fila": 3,
  "columna": 2,
  "direccion": "este",
  "costo": 3
}
```
**Resultado esperado:**
- 💥💥 Pared recibe 2 daños de golpe
- 📊 Vida: 2 → 0 (destrucción inmediata)
- 🎨 Cambia directo a `prefabDestruido` o desaparece
- 📝 Log: `"💥 Pared DESTRUIDA en (3,2) este - Cambió a prefab destruido"` o `"[GameObject desactivado]"`

---

#### **Turno 4** - Primer golpe a otra pared (Astronauta 2)
```json
{
  "tipo": "danar_pared",
  "fila": 3,
  "columna": 5,
  "direccion": "sur",
  "costo": 2
}
```
**Resultado esperado:**
- 🔨 Pared en (3,5) sur recibe 1 daño
- 📊 Vida: 2 → 1
- 🎨 Cambia a dañada si hay prefab

---

## 🎮 Cómo Probar

### **Opción 1: Ejecutar en Unity (Recomendado)**

1. **Abre Unity:**
   - Abre el proyecto `FireRescue2`

2. **Verifica la Escena:**
   - Abre `Assets/Scenes/spacerescue.unity`
   - Asegúrate de que `GameManager` está activo
   - Verifica que `ControladorJuego` está **HABILITADO** ✅

3. **Configurar Consola:**
   - Abre la ventana **Console** (Window → General → Console)
   - Activa **Collapse** para ver logs agrupados
   - Puedes filtrar por tipo (Info, Warning, Error)

4. **Ejecutar:**
   - Presiona **Play ▶️**
   - Observa cómo se construye el tablero
   - La simulación comenzará automáticamente

5. **Observar Resultados:**
   
   **En la Escena (vista 3D):**
   - 👀 Turno 2: Astronauta 1 golpea pared norte en (2,1)
     - Si tienes prefab dañado: verás el cambio visual
     - Si no: se ve igual
   - 💥 Turno 3: La misma pared se destruye
     - Si tienes prefab destruido: cambia a roto
     - Si no: desaparece completamente
   - 🚪 Turno 3: Puerta se abre (sube hacia arriba)
   - 💥💥 Turno 4: Pared este en (3,2) se destruye de golpe

   **En la Consola:**
   ```
   🔨 Pared atacada en (2,1) norte - Vida: 2 → 1
   🔧 Pared DAÑADA en (2,1) norte - Vida: 1/2 [opciones según config]
   
   🔨 Pared atacada en (2,1) norte - Vida: 1 → 0
   💥 Pared DESTRUIDA en (2,1) norte [según config de prefabs]
   
   🚪 Puerta abierta en (3,6) oeste
   
   💥 Pared DESTRUIDA en (3,2) este [romper instantáneo]
   
   🔨 Pared atacada en (3,5) sur - Vida: 2 → 1
   ```

---

### **Opción 2: Script de Prueba Manual (Alternativa)**

Si prefieres probar manualmente sin la simulación, crea este script:

```csharp
using UnityEngine;

public class PruebaDano : MonoBehaviour
{
    void Start()
    {
        // Esperar 2 segundos y probar
        Invoke("ProbarSistemaDano", 2f);
    }

    void ProbarSistemaDano()
    {
        Debug.Log("====== INICIANDO PRUEBA DE SISTEMA DE DAÑO ======");

        // Buscar una pared cualquiera
        Wall pared = FindFirstObjectByType<Wall>();

        if (pared == null)
        {
            Debug.LogError("❌ No se encontró ninguna pared para probar");
            return;
        }

        Debug.Log($"✅ Pared encontrada: ({pared.fila},{pared.columna}) {pared.direccion}");
        Debug.Log($"📊 Vida inicial: {pared.vidaActual}/{pared.vidaMaxima}");

        // Primer golpe
        Debug.Log("\n--- PRIMER GOLPE ---");
        pared.Atacar();
        Debug.Log($"📊 Vida después del 1er golpe: {pared.vidaActual}/{pared.vidaMaxima}");

        // Segundo golpe (después de 2 segundos)
        Invoke("SegundoGolpe", 2f);
    }

    void SegundoGolpe()
    {
        Wall pared = FindFirstObjectByType<Wall>();
        
        if (pared != null && !pared.estaDestruida)
        {
            Debug.Log("\n--- SEGUNDO GOLPE ---");
            pared.Atacar();
            Debug.Log($"📊 Vida después del 2do golpe: {pared.vidaActual}/{pared.vidaMaxima}");
            Debug.Log($"❌ ¿Destruida?: {pared.estaDestruida}");
        }

        Debug.Log("\n====== PRUEBA COMPLETADA ======");
    }
}
```

**Para usarlo:**
1. Crea el archivo: `Assets/Scripts/Utils/PruebaDano.cs`
2. Agrégalo al `GameManager` en el Inspector
3. Ejecuta la escena

---

## 📊 Interpretando los Resultados

### **Configuración: Solo Prefab Normal**

**Logs esperados:**
```
🔨 Pared atacada en (2,1) norte - Vida: 2 → 1
⚠️ Pared DAÑADA en (2,1) norte - Vida: 1/2 [Sin cambio visual - no hay prefab dañado asignado]

🔨 Pared atacada en (2,1) norte - Vida: 1 → 0
💥 Pared DESTRUIDA en (2,1) norte - Vida: 0/2 [GameObject desactivado - no hay prefab destruido]
```

**Visual:**
- No cambia al dañarse
- Desaparece al destruirse

---

### **Configuración: Todos los Prefabs**

**Logs esperados:**
```
🔨 Pared atacada en (2,1) norte - Vida: 2 → 1
🔧 Pared DAÑADA en (2,1) norte - Vida: 1/2 - Cambió a prefab dañado
🔄 Prefab cambiado para pared en (2,1) norte

🔨 Pared atacada en (2,1) norte - Vida: 1 → 0
💥 Pared DESTRUIDA en (2,1) norte - Cambió a prefab destruido
🔄 Prefab cambiado para pared en (2,1) norte
```

**Visual:**
- ✅ Cambia a grietas al dañarse
- ✅ Cambia a roto/escombros al destruirse

---

## ✅ Checklist de Verificación

Después de ejecutar, verifica:

- [ ] **Turno 2:** ¿Se ve en la consola el primer golpe a (2,1) norte?
- [ ] **Turno 3:** ¿Se ve el segundo golpe y destrucción de (2,1) norte?
- [ ] **Turno 3:** ¿Se abre la puerta en (3,6) oeste?
- [ ] **Turno 4:** ¿Se destruye instantáneamente la pared en (3,2) este?
- [ ] **Turno 4:** ¿Se daña la pared en (3,5) sur?
- [ ] **Visual:** ¿Las paredes cambian de aspecto? (si tienes prefabs asignados)
- [ ] **Visual:** ¿Las paredes destruidas desaparecen o muestran escombros?

---

## 🐛 Problemas Comunes

### "No veo logs de daño en la consola"
✅ **Solución:**
- Verifica que la consola esté abierta
- Asegúrate de que los filtros no estén bloqueando Info/Warning
- Verifica que `ActionExecutor.cs` esté llamando a `Wall.Atacar()`

### "Las acciones no se ejecutan"
✅ **Solución:**
- Verifica que `ControladorJuego` esté **HABILITADO** ✅
- Revisa que `SimulacionPlayer` esté activo
- Chequea que `simulacion_completa.json` se cargó correctamente

### "Las paredes no cambian de aspecto"
✅ **Normal si:**
- Solo tienes prefab Normal asignado
- Los campos `prefabDanado` y `prefabDestruido` están en **None**

✅ **Para arreglarlo:**
- Crea los prefabs variantes
- Asígnalos en el Inspector

### "Error: NullReferenceException"
✅ **Posibles causas:**
- Coordenadas de pared incorrectas en JSON
- Pared no existe en esa posición
- Dirección mal escrita ("norte" vs "Norte")

---

## 🎯 Próximos Pasos

Una vez que veas que funciona:

1. **Ajustar prefabs:**
   - Crear variantes visuales (dañado, destruido)
   - Asignar en Inspector

2. **Agregar más acciones:**
   - Daño a puertas específicas
   - Diferentes intensidades de golpe
   - Efectos de sonido

3. **Optimizar:**
   - Pooling de prefabs destruidos
   - Efectos de partículas al romper
   - Animaciones de impacto

---

## 📞 ¿Necesitas Ayuda?

Si algo no funciona:

1. Copia los logs de la consola de Unity
2. Toma screenshot de la escena
3. Describe qué esperabas vs qué obtuviste
4. Comparte el estado de los prefabs en el Inspector

---

**¡A probar! 🚀** Ejecuta Unity y observa cómo los astronautas destruyen paredes en tiempo real.
