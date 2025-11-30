# 🎨 Guía Rápida: Configuración de Prefabs de Daño

## 📦 ¿Qué Prefabs Necesito?

### ✅ Opción Básica (FUNCIONAL - Sin cambios visuales)
```
Solo necesitas: 1 prefab
├── ParedNormal.prefab (con Wall.cs)
```

**Configuración en Inspector:**
```
Wall (Script):
├── Prefab Normal: [ParedNormal] ✅
├── Prefab Danado: [None] ⬅️ Déjalo vacío
└── Prefab Destruido: [None] ⬅️ Déjalo vacío
```

**¿Qué pasa al dañar?**
- ✅ Vida disminuye (2 → 1 → 0)
- ❌ No hay cambio visual (siempre se ve igual)
- ✅ Al destruirse: GameObject desaparece
- 📊 Puedes ver el estado en los logs

---

### 🎨 Opción Media (Feedback visual básico)
```
Necesitas: 2 prefabs
├── ParedNormal.prefab (intacta)
└── ParedDanada.prefab (con grietas)
```

**Configuración en Inspector:**
```
Wall (Script):
├── Prefab Normal: [ParedNormal] ✅
├── Prefab Danado: [ParedDanada] ✅
└── Prefab Destruido: [None] ⬅️ Déjalo vacío
```

**¿Qué pasa al dañar?**
- ✅ 1er golpe → cambia a ParedDanada (grietas visibles)
- ✅ 2do golpe → GameObject desaparece
- 🎨 El jugador ve el daño progresivo

---

### 🌟 Opción Completa (RECOMENDADO - Máxima calidad)
```
Necesitas: 3 prefabs
├── ParedNormal.prefab (intacta)
├── ParedDanada.prefab (con grietas)
└── ParedDestruida.prefab (rota/escombros)
```

**Configuración en Inspector:**
```
Wall (Script):
├── Prefab Normal: [ParedNormal] ✅
├── Prefab Danado: [ParedDanada] ✅
└── Prefab Destruido: [ParedDestruida] ✅
```

**¿Qué pasa al dañar?**
- ✅ 1er golpe → cambia a ParedDanada (grietas visibles)
- ✅ 2do golpe → cambia a ParedDestruida (escombros/rota)
- 🌟 Experiencia visual completa y profesional

---

## 🔨 Ejemplos Visuales

### Caso 1: Solo Prefab Normal
```
┌─────────────┐     Atacar()      ┌─────────────┐     Atacar()      ┌─────────┐
│   Intacta   │  ────────────►    │   Dañada    │  ────────────►    │ [VACÍO] │
│   2/2 vida  │                   │   1/2 vida  │                   │ 0/0 vida│
│     🧱      │                   │     🧱      │                   │    ❌   │
│  (Normal)   │                   │  (Normal)   │                   │Desactivado│
└─────────────┘                   └─────────────┘                   └─────────┘
  Se ve igual                      ⚠️ Se ve igual                    Desaparece
```

### Caso 2: Normal + Dañado
```
┌─────────────┐     Atacar()      ┌─────────────┐     Atacar()      ┌─────────┐
│   Intacta   │  ────────────►    │   Dañada    │  ────────────►    │ [VACÍO] │
│   2/2 vida  │                   │   1/2 vida  │                   │ 0/0 vida│
│     🧱      │                   │    🧱💥     │                   │    ❌   │
│  (Normal)   │                   │  (Dañado)   │                   │Desactivado│
└─────────────┘                   └─────────────┘                   └─────────┘
  Perfecto                        ✅ Cambia visual!                  Desaparece
                                   Grietas visibles
```

### Caso 3: Configuración Completa
```
┌─────────────┐     Atacar()      ┌─────────────┐     Atacar()      ┌─────────────┐
│   Intacta   │  ────────────►    │   Dañada    │  ────────────►    │ Destruida   │
│   2/2 vida  │                   │   1/2 vida  │                   │  0/2 vida   │
│     🧱      │                   │    🧱💥     │                   │   🪨💥💥   │
│  (Normal)   │                   │  (Dañado)   │                   │ (Destruido) │
└─────────────┘                   └─────────────┘                   └─────────────┘
  Perfecto                        ✅ Grietas                         ✅ Escombros
                                                                      Sigue visible
```

---

## 📝 Checklist Rápido

### Para empezar rápido (sin crear nuevos assets):
- [ ] Abre tu prefab de pared/puerta en Unity
- [ ] Verifica que tenga el componente `Wall.cs`
- [ ] En el Inspector, asigna:
  - [ ] `Prefab Normal`: arrastra el mismo prefab
  - [ ] `Prefab Danado`: déjalo en **None**
  - [ ] `Prefab Destruido`: déjalo en **None**
- [ ] Guarda el prefab
- [ ] ¡Listo! Ya funciona el sistema de daño

### Para máxima calidad visual (cuando tengas tiempo):
- [ ] Duplica tu prefab de pared 2 veces
- [ ] Renombra: `ParedNormal`, `ParedDanada`, `ParedDestruida`
- [ ] Edita `ParedDanada`: agrega grietas, texturas dañadas
- [ ] Edita `ParedDestruida`: modelo roto, agujeros, escombros
- [ ] En `ParedNormal`, asigna los 3 prefabs en el Inspector
- [ ] Copia las referencias a los otros 2 prefabs también
- [ ] Guarda todos los prefabs
- [ ] 🎉 ¡Sistema completo!

---

## 💡 Tips Pro

### 🎨 Crear variantes rápidas sin modelar:
1. **Dañada:** Cambia el shader a uno más oscuro, agrega decals de daño
2. **Destruida:** Reduce la escala Y (altura) al 50%, rota levemente

### 🚀 Optimización:
- Reutiliza el mismo mesh para Normal y Dañada (solo cambia material)
- El prefab Destruido puede ser un modelo low-poly simple

### 🔍 Debug:
- Activa los logs de Unity para ver:
  ```
  🔨 Pared atacada en (2,3) norte - Vida: 2 → 1
  🔧 Pared DAÑADA en (2,3) norte - Vida: 1/2 [Sin cambio visual - no hay prefab dañado]
  💥 Pared DESTRUIDA en (2,3) norte - Vida: 0/2 [GameObject desactivado]
  ```

---

## 🆘 Ayuda Rápida

### "No veo cambios visuales al dañar"
✅ **Normal.** Si solo tienes prefab Normal, no hay cambio visual.
- Crea prefabs Dañado/Destruido para ver cambios

### "Al cambiar de prefab, pierde la posición"
❌ **Bug.** Asegúrate de usar la versión actualizada de `Wall.cs`
- El método `CambiarPrefab()` debe copiar `transform.position`

### "El prefab Dañado no tiene el script Wall"
⚠️ **Warning.** El sistema lo agrega automáticamente
- Mejor práctica: agrégalo manualmente al prefab

### "¿Puedo mezclar? Algunas con 3 prefabs, otras con 1"
✅ **Sí, totalmente.**
- Cada pared/puerta es independiente
- Configura según necesites

---

**¿Dudas?** Revisa `SISTEMA_DANO_PAREDES_PUERTAS.md` para documentación completa.
