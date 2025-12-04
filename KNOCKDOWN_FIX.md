# 🔴 KNOCKDOWN SYSTEM - ARREGLOS FINALES

## ✅ Problema Resuelto: ID Mismatch

### **Error Original**
Python enviaba `unique_id` (0-indexed: 0, 1, 2, 3, 4, 5)  
Unity esperaba `tripulacion_id` (1-indexed: 1, 2, 3, 4, 5, 6)

**Resultado**: Unity no encontraba crews → "⚠️ No se encontró crew con ID 0"

---

## 🔧 Cambios Aplicados

### 1. **multiagentes.py** (Líneas 1087 y 1110)
```python
# ANTES:
cambios["knockdowns"].append(a.unique_id)

# AHORA:
cambios["knockdowns"].append(a.unique_id + 1)  # +1 para tripulacion_id
```

**Por qué**: Unity busca `Crew_1_jugador`, `Crew_2_jugador`, etc. (IDs 1-6)

---

### 2. **Materiales de Knockdown**
Creados dos nuevos materiales en `Assets/Materials/`:

- **Crew_Knockdown1.mat** → Color ROJO (primer knockdown)
- **Crew_Knockdown2.mat** → Color NEGRO (segundo knockdown/muerte)

**Ventaja**: Cambios de material más visibles que cambio de color directo.

---

### 3. **Crew.cs - Sistema de Materiales**
```csharp
[Header("Knockdown Materials")]
public Material materialKnockdown1;  // Asignar en Inspector
public Material materialKnockdown2;  // Asignar en Inspector
```

**Comportamiento mejorado**:
- Intenta cargar materiales desde `Resources/Materials/` automáticamente
- Si no hay materiales asignados, usa fallback con `Color.red` y `Color.black`
- Logs claros: `🎨 Crew_X → Material ROJO aplicado`

---

## 🎮 Cómo Probar

### **Paso 1: Regenerar JSON**
```bash
cd Assets/python/simulation
python multiagentes.py
```
Esto genera `simulation_completa.json` con IDs corregidos.

### **Paso 2: Configurar Materiales en Unity**
1. Abrir Unity
2. Seleccionar cada `Crew_X_jugador` en Hierarchy
3. En Inspector → Component `Crew`:
   - Arrastrar `Crew_Knockdown1.mat` al campo **Material Knockdown1**
   - Arrastrar `Crew_Knockdown2.mat` al campo **Material Knockdown2**

**Alternativa**: El script intenta cargar automáticamente desde `Resources/Materials/` si no están asignados.

### **Paso 3: Play Mode**
1. Presionar ▶️ Play
2. Esperar a que ocurra una explosión
3. Observar:
   - **Primer knockdown**: Astronauta ROJO + 80% tamaño + shake
   - **Segundo knockdown**: Astronauta NEGRO + 50% tamaño + caída/fade

---

## 📊 Logs Esperados

### ✅ Correcto (después del fix):
```
💥 1 tripulante(s) afectado(s) por explosión
🎯 Buscando crew con ID: 3
✅ Crew encontrado: Crew_3_jugador
🎨 Crew_3_jugador → Material ROJO aplicado
⚠️ Crew_3_jugador recibe PRIMER KNOCKDOWN (1/2) - Color ROJO, escala 80%
```

### ❌ Incorrecto (antes del fix):
```
💥 1 tripulante(s) afectado(s) por explosión
⚠️ No se encontró crew con ID 0
```

---

## 🚨 Troubleshooting

### "No se encontró crew con ID X"
- Verifica que `simulation_completa.json` tenga IDs 1-6 en `knockdowns`
- Regenera JSON con `python multiagentes.py` actualizado

### "No se encontró material Crew_Knockdown1"
- Asegúrate de que `Crew_Knockdown1.mat` esté en `Assets/Materials/`
- **O** copia materiales a `Assets/Resources/Materials/` para carga automática
- El sistema tiene fallback con colores directos si fallan los materiales

### Cambio de color no visible
- Verifica que el crew tenga un `Renderer` component
- Comprueba en Console: debe aparecer `🎨 Crew_X → Material ROJO aplicado`
- Si usas shaders custom, pueden no respetar `material.color` → usa los materiales `.mat`

---

## 📝 Sistema Completo

### Estados Knockdown:
| Estado | Color | Escala | Animación | Condición |
|--------|-------|--------|-----------|-----------|
| **Normal** | Original | 100% | - | knockdownCount = 0 |
| **Primer KD** | 🔴 ROJO | 80% | Shake (0.4s) | knockdownCount = 1 |
| **Muerte** | ⚫ NEGRO | 50% | Fall+Fade (1.5s) | knockdownCount ≥ 2 |

### Flujo:
1. Python: Explosión afecta agentes → `knockdowns: [3, 5]` (IDs corregidos +1)
2. JSON: Unity recibe `cambios.knockdowns`
3. Unity: `SimulacionRunner.AplicarCambiosMapa()` procesa cada ID
4. Crew: `AplicarKnockdown()` cambia material/color + escala
5. Animación: Shake o Fall+Fade según contador

---

## 🎯 Resultado Final

**Antes**: Knockdowns no visibles (IDs incorrectos)  
**Ahora**: 
- ✅ IDs corregidos (1-6)
- ✅ Materiales ROJO/NEGRO
- ✅ Animaciones smooth
- ✅ Logs claros para debugging

**Prueba superada** cuando veas astronautas rojos tras explosiones 💥🔴
