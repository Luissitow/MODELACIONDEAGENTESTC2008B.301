# 🚪 Sistema de Daño para Paredes y Puertas

## ✅ Características Implementadas

### 🔨 Sistema de Daño Unificado
- ✅ **Paredes Y puertas** ahora pueden recibir daño
- ✅ **2 golpes** destruyen tanto paredes como puertas
- ✅ **Estados visuales**: Intacta → Dañada → Destruida
- ✅ **Cambio de prefabs** automático según el daño

### 📊 Estados de Objetos

#### **Vida por Defecto:**
- `vidaMaxima = 2` (configurable en Inspector)
- `vidaActual` disminuye con cada golpe

#### **Estados Visuales:**
1. **Intacta** (vida = 2/2)
   - Usa `prefabNormal` o `materialIntacto`
   
2. **Dañada** (vida = 1/2)
   - Usa `prefabDanado` o `materialDanado`
   - Se muestra con grietas, manchas, etc.

3. **Destruida** (vida = 0/2)
   - Usa `prefabDestruido`
   - Si no hay prefab, se desactiva el objeto

---

## 🎮 Cómo Usar el Sistema

### **1. Configurar Prefabs en Unity**

Tienes **3 opciones** según los prefabs que tengas disponibles:

#### **Opción A: Solo Prefab Normal** (Funcional sin cambios visuales)
```
Wall (Script):
├── Prefab Normal      → [tu_ParedNormal]
├── Prefab Danado      → [None] ⚠️ Déjalo vacío
└── Prefab Destruido   → [None] ⚠️ Déjalo vacío
```
**Resultado:**
- ✅ Sistema de daño **SÍ funciona** (vida baja)
- ✅ Se puede destruir
- ⚠️ **NO hay cambio visual** (siempre se ve igual)
- 📊 Puedes ver el estado en la consola de Unity

#### **Opción B: Normal + Dañado** (Cambio visual a mitad de vida)
```
Wall (Script):
├── Prefab Normal      → [tu_ParedNormal]
├── Prefab Danado      → [tu_ParedConGrietas] ✅
└── Prefab Destruido   → [None] ⚠️ Déjalo vacío
```
**Resultado:**
- ✅ 1er golpe → cambia a prefab con grietas
- ✅ 2do golpe → el objeto **desaparece** (se desactiva)

#### **Opción C: Configuración Completa** (Recomendado - Máxima calidad visual)
```
Wall (Script):
├── Prefab Normal      → [tu_ParedNormal] ✅
├── Prefab Danado      → [tu_ParedConGrietas] ✅
└── Prefab Destruido   → [tu_ParedRota] ✅
```
**Resultado:**
- ✅ 1er golpe → cambia a grietas
- ✅ 2do golpe → cambia a escombros/rota
- 🎨 Máxima calidad visual

**IMPORTANTE:** 
- Si asignas un prefab, debe tener el componente `Wall.cs`
- Si lo dejas vacío (None), el sistema usará comportamiento por defecto

### **2. Aplicar Daño desde Código**

```csharp
// Obtener referencia a la pared/puerta
Wall pared = GameObject.Find("Pared_2_3_norte").GetComponent<Wall>();

// Método 1: Golpe normal (1 de daño)
pared.Atacar();

// Método 2: Golpe fuerte (2 de daño - destruye instantáneamente)
pared.Romper();

// Método 3: Daño personalizado
pared.RecibirDano(1); // o pared.RecibirDano(2);
```

### **3. Abrir Puertas (Sin Daño)**

```csharp
Wall puerta = GameObject.Find("Puerta_1_3_este").GetComponent<Wall>();

// Abrir puerta (se mueve hacia arriba)
puerta.AbrirPuerta();
```

### **4. Verificar Estado**

```csharp
Wall pared = GetComponent<Wall>();

// Ver vida actual
Debug.Log($"Vida: {pared.vidaActual}/{pared.vidaMaxima}");

// Verificar si está destruida
if (pared.estaDestruida)
    Debug.Log("¡Pared destruida!");

// Ver si es puerta y está abierta
if (pared.tipo == TipoPared.Puerta && pared.estaAbierta)
    Debug.Log("¡Puerta abierta!");

// Obtener info completa
Debug.Log(pared.ObtenerInfo());
```

---

## 🛠️ Configuración en el Inspector

### **Componente Wall:**

```
┌─ Wall (Script) ─────────────────────┐
│                                      │
│ Posición en Tablero:                 │
│ ├── Fila: 2                          │
│ ├── Columna: 3                       │
│ └── Direccion: "norte"               │
│                                      │
│ Tipo de Pared:                       │
│ └── Tipo: Madera / Puerta            │
│                                      │
│ Configuración:                       │
│ ├── Vida Maxima: 2                   │
│ ├── Altura Abrir Puerta: 3           │
│ └── Velocidad Apertura: 2            │
│                                      │
│ Prefabs para estados:                │
│ ├── Prefab Normal: [ParedIntacta]    │
│ ├── Prefab Danado: [ParedDanada]     │
│ └── Prefab Destruido: [ParedRota]    │
│                                      │
│ Materiales (Opcional):               │
│ ├── Material Intacto: [Mat_Normal]   │
│ └── Material Danado: [Mat_Damaged]   │
└──────────────────────────────────────┘
```

---

## 📝 Logs del Sistema

### **Daño a Paredes:**
```
⚔️ GOLPE en pared (2,3) norte | Vida: 1/2
🔧 Pared DAÑADA en (2,3) norte
🔄 Prefab cambiado para pared en (2,3) norte

🔨 GOLPE FUERTE en pared (2,3) norte | Vida: 0/2
💥 Pared DESTRUIDA en (2,3) norte - Cambió a prefab destruido
```

### **Daño a Puertas:**
```
⚔️ GOLPE en puerta (1,4) este | Vida: 1/2
🔧 Puerta DAÑADA en (1,4) este
🔄 Prefab cambiado para puerta en (1,4) este

🔨 GOLPE FUERTE en puerta (1,4) este | Vida: 0/2
💥 Puerta DESTRUIDA en (1,4) este - Cambió a prefab destruido
```

### **Abrir Puertas:**
```
🚪 Puerta abierta en (1,4) este
```

---

## 🎯 Flujo de Daño

### **Con Todos los Prefabs Asignados:**
```
Estado Inicial: INTACTA (2/2 vida)
         │
         │ Atacar() o RecibirDano(1)
         ▼
  Estado: DAÑADA (1/2 vida)
         │ ✅ Cambia a prefabDanado
         │ ✅ Muestra grietas/daño
         │ 📊 Log: "🔧 Pared DAÑADA en (X,Y) - Vida: 1/2"
         │
         │ Atacar() o RecibirDano(1)
         ▼
  Estado: DESTRUIDA (0/2 vida)
         │ ✅ Cambia a prefabDestruido
         │ ✅ Muestra escombros/rota
         │ 📊 Log: "💥 Pared DESTRUIDA en (X,Y) - Cambió a prefab destruido"
         │
         ▼
    [FIN - Ya no se puede dañar más]
```

### **Solo con Prefab Normal (Sin prefabs dañado/destruido):**
```
Estado Inicial: INTACTA (2/2 vida)
         │ 👀 Se ve normal
         │
         │ Atacar() o RecibirDano(1)
         ▼
  Estado: DAÑADA (1/2 vida)
         │ ⚠️ Sigue viéndose igual (sin cambio visual)
         │ 📊 Log: "⚠️ Pared DAÑADA en (X,Y) - Vida: 1/2 [Sin cambio visual - no hay prefab dañado]"
         │
         │ Atacar() o RecibirDano(1)
         ▼
  Estado: DESTRUIDA (0/2 vida)
         │ ❌ GameObject desaparece (se desactiva)
         │ 📊 Log: "💥 Pared DESTRUIDA en (X,Y) - Vida: 0/2 [GameObject desactivado]"
         │
         ▼
    [FIN - El objeto ya no es visible]
```

---

## 🚀 Ejemplos de Uso en el Juego

### **Ejemplo 1: Astronauta Golpea Pared**

```csharp
// En AstronautController o ActionExecutor
public void GolpearPared(int fila, int col, string direccion)
{
    // Buscar la pared en esa posición
    Wall pared = BuscarPared(fila, col, direccion);
    
    if (pared != null)
    {
        // Aplicar daño (1 golpe normal)
        pared.Atacar();
        
        // Verificar si se destruyó
        if (pared.estaDestruida)
        {
            Debug.Log("🎉 ¡Pared destruida! Ahora puedes pasar");
        }
    }
}
```

### **Ejemplo 2: Abrir Puerta**

```csharp
public void AbrirPuertaCerca()
{
    // Buscar puertas cercanas
    Wall[] puertas = FindObjectsOfType<Wall>();
    
    foreach (Wall obj in puertas)
    {
        if (obj.tipo == TipoPared.Puerta && !obj.estaAbierta)
        {
            // Verificar distancia
            if (Vector3.Distance(transform.position, obj.transform.position) < 2f)
            {
                obj.AbrirPuerta();
                break;
            }
        }
    }
}
```

### **Ejemplo 3: Explosión Daña Múltiples Paredes**

```csharp
public void Explosion(Vector3 centro, float radio)
{
    // Buscar todas las paredes en el radio
    Collider[] objetosCercanos = Physics.OverlapSphere(centro, radio);
    
    foreach (Collider col in objetosCercanos)
    {
        Wall pared = col.GetComponent<Wall>();
        if (pared != null && !pared.estaDestruida)
        {
            // Daño de explosión (2 puntos - destrucción inmediata)
            pared.Romper();
        }
    }
}
```

---

## 🎨 Creando los Prefabs

### **Paso 1: Crear Prefab Normal**
1. Crea el modelo 3D de pared/puerta intacta
2. Agrega componente `Wall.cs`
3. Configura parámetros en Inspector
4. Arrastra a carpeta Prefabs → Guarda

### **Paso 2: Crear Prefab Dañado**
1. Duplica el prefab normal
2. Modifica el modelo (agregar grietas, deformaciones)
3. Cambia el material a uno más oscuro/dañado
4. Mantén el componente `Wall.cs`

### **Paso 3: Crear Prefab Destruido**
1. Duplica el prefab dañado
2. Modelo más roto (agujeros, escombros)
3. Opcionalmente más pequeño o fragmentado
4. Mantén el componente `Wall.cs`

### **Paso 4: Conectar los Prefabs**
En el **Prefab Normal**, asigna:
- `Prefab Normal` → El mismo prefab normal
- `Prefab Danado` → El prefab dañado
- `Prefab Destruido` → El prefab destruido

**IMPORTANTE:** Cada prefab debe referenciar a los otros tres.

---

## 🔧 Solución de Problemas

### ❌ "No se puede romper una puerta"
**Antes:** Las puertas no se podían dañar
**Ahora:** ✅ Las puertas SÍ se pueden dañar y destruir

### ❌ Al cambiar prefab, se pierde el estado
**Solución:** El método `CambiarPrefab()` ahora copia:
- Posición en tablero (fila, columna, dirección)
- Tipo de pared
- Vida actual
- Estados (destruida, abierta)
- Referencias a otros prefabs

---

## ❓ Preguntas Frecuentes (FAQ)

### **Q1: ¿Puedo usar el mismo prefab en Normal, Dañado y Destruido?**
❌ **No recomendado.** Aunque técnicamente funciona, no habrá cambio visual y pierde el propósito del sistema.

### **Q2: ¿Qué pasa si solo tengo prefab Normal (sin dañado ni destruido)?**
✅ **Sí funciona.** El sistema de daño opera normalmente:
- La vida disminuye correctamente
- No hay cambio visual (siempre se ve igual)
- Al llegar a 0 vida → el objeto desaparece
- Puedes ver el estado en los logs de la consola

**Configuración:**
```
Prefab Normal: [tu_ParedNormal] ✅
Prefab Danado: [None] ⬅️ Déjalo vacío
Prefab Destruido: [None] ⬅️ Déjalo vacío
```

### **Q3: ¿Puedo dejar el campo Prefab Destruido vacío?**
✅ **Sí.** Si está vacío, al llegar a vida = 0:
- El GameObject se desactiva (desaparece)
- Funciona igual que si estuviera destruido
- Útil si solo quieres que el obstáculo desaparezca

### **Q4: ¿Qué prefabs necesito crear mínimo?**
📦 **Mínimo: 1 prefab (Normal)**
- Funcional pero sin feedback visual

🎨 **Recomendado: 3 prefabs**
- Normal (intacta)
- Dañado (grietas)
- Destruido (rota/escombros)

### **Q5: ¿Los prefabs Dañado y Destruido necesitan el script Wall.cs?**
✅ **Sí, pero se agrega automáticamente.** 
- Si el prefab no tiene Wall.cs, el sistema lo agrega
- Copia todo el estado del prefab anterior
- Mejor práctica: agrégalo manualmente para evitar warnings

### **Q6: ¿Puedo tener diferentes prefabs para diferentes tipos de paredes?**
✅ **Sí, totalmente.** Cada prefab de pared puede tener sus propios prefabs de daño:

```
ParedMadera:
├── Normal: ParedMadera_Normal
├── Dañado: ParedMadera_Grietas
└── Destruido: ParedMadera_Rota

ParedMetal:
├── Normal: ParedMetal_Normal
├── Dañado: ParedMetal_Abollada
└── Destruido: ParedMetal_Fundida

Puerta:
├── Normal: Puerta_Cerrada
├── Dañado: Puerta_Agrietada
└── Destruido: Puerta_Rota
```

### **Q7: ¿Las puertas se pueden abrir después de estar dañadas?**
✅ **Sí.** El sistema de apertura es independiente del daño:
- Puedes abrir una puerta dañada
- Puedes dañar una puerta abierta
- La animación de apertura se preserva

### **Q8: ¿Cómo sé si mi pared está destruida en código?**
```csharp
Wall pared = GetComponent<Wall>();

if (pared.estaDestruida)
    Debug.Log("¡Está destruida!");

if (pared.vidaActual <= 0)
    Debug.Log("¡Vida en 0!");
```

### **Q9: ¿Puedo hacer que algunas paredes sean indestructibles?**
✅ **Sí, solo aumenta la vida:**
```csharp
Wall pared = GetComponent<Wall>();
pared.vidaMaxima = 999;
pared.vidaActual = 999;
```
O crea un script especializado que ignore `RecibirDano()`.

### **Q10: ¿El sistema afecta el rendimiento?**
✅ **Mínimo impacto:**
- Instantiate solo se ejecuta al cambiar de estado (máx 2 veces por pared)
- No hay updates constantes
- Los prefabs reutilizan meshes y materiales

### ❌ El prefab dañado no aparece
**Verificar:**
1. ¿Está asignado `prefabDanado` en el Inspector?
2. ¿El prefab dañado tiene el componente `Wall.cs`?
3. ¿La vida bajó a 1/2 o menos?

---

## 📊 Resumen de Cambios

| Característica | Antes | Ahora |
|---------------|-------|-------|
| Puertas reciben daño | ❌ | ✅ |
| Cambio visual al dañar | ⚠️ Solo material | ✅ Prefab completo |
| Estado preservado | ⚠️ Parcial | ✅ Completo |
| Logs descriptivos | ⚠️ Básicos | ✅ Detallados |
| Soporte para puertas | ⚠️ Solo abrir | ✅ Abrir y dañar |

---

**¡Listo para probar!** 🎮

Ahora puedes dañar tanto paredes como puertas, y verás cambios visuales progresivos hasta su destrucción completa.
