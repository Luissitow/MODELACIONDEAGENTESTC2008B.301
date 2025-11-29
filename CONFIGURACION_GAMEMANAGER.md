# 🎮 CONFIGURACIÓN COMPLETA DEL GAMEMANAGER

## ⚠️ IMPORTANTE: ESTRUCTURA DE OBJETOS EN LA ESCENA

### ✅ CONFIGURACIÓN CORRECTA:

```
Hierarchy:
└── GameManager (GameObject vacío)
    ├── Controlador Juego (Script)
    ├── Constructor Tablero (Script)
    ├── Game Manager (Script)
    ├── Simulacion Player (Script)
    └── Action Executor (Script)
```

### ❌ OBJETOS QUE DEBES BORRAR DE LA ESCENA:

Si tienes estos objetos SUELTOS en la Hierarchy (fuera de GameManager), **BÓRRALOS**:
- ❌ ConstructorTablero (objeto suelto)
- ❌ ActionExecutor (objeto suelto)
- ❌ ControladorJuego (objeto suelto)
- ❌ Cualquier otro script de gestión suelto

**SOLO debe existir 1 GameObject llamado "GameManager" con todos los scripts dentro.**

---

## 📋 CONFIGURACIÓN DEL GAMEMANAGER

### 1️⃣ **Controlador Juego (Script)**
```
Referencias:
├── Constructor Tablero: GameManager ✅ (arrastra el propio GameManager aquí)
```

### 2️⃣ **Constructor Tablero (Script)**
```
Prefabs (12 en total):
├── Piso: tu prefab de piso ✅
├── Pared: tu prefab de pared ✅
├── Araña: tu prefab de araña ✅
├── Huevos: tu prefab de huevos ✅
├── Tripulacion: tu prefab de tripulante ✅
├── Falsaalarma: tu prefab de falsa alarma ✅
├── Puntodeinteres: tu prefab de punto de interés ✅
├── Puerta: tu prefab de puerta ✅
├── Paredañada: tu prefab de pared dañada ✅
├── Pared Destruida: tu prefab de pared destruida ✅
├── Player1: tu prefab de astronauta 1 ✅
└── Player2: tu prefab de astronauta 2 ✅

Configuración:
└── Tamaño Celda: 3
```

### 3️⃣ **Game Manager (Script)**
```
Referencias:
├── Constructor Tablero: GameManager ✅
└── Controlador Juego: (puede quedar vacío)
```

### 4️⃣ **Simulacion Player (Script)**
```
Referencias:
├── Constructor Tablero: GameManager ✅
├── Action Executor: GameManager ✅
└── Game Manager: GameManager ✅

Configuración de Reproducción:
├── Archivo Simulacion: simulacion_completa.json
├── Velocidad Reproduccion: 1
├── Reproducir Automaticamente: ✅ (activado)
├── Pausar Entre Turnos: ✅ (activado)
└── Tiempo Entre Turnos: 10

Debug:
└── Mostrar Debug Logs: ✅ (activado)
```

### 5️⃣ **Action Executor (Script)**
```
Referencias:
├── Constructor Tablero: GameManager ✅
└── Game Manager: GameManager ✅

Configuración:
└── Tiempo Animacion: 0.5

Debug:
└── Mostrar Debug Logs: ✅ (activado)
```

---

## 🔍 CÓMO ASIGNAR CORRECTAMENTE

### Para las REFERENCIAS (Constructor Tablero, Action Executor, etc.):
1. En el Inspector del GameManager
2. Encuentra el campo que dice "Constructor Tablero" o similar
3. **Arrastra el MISMO GameObject "GameManager"** desde la Hierarchy
4. NO busques prefabs, NO busques en Assets - solo el GameManager de la escena

### Para los PREFABS (Piso, Pared, Player1, etc.):
1. Ve a la carpeta `Assets/Prefabs/`
2. Arrastra cada prefab al campo correspondiente
3. Ejemplo: Arrastra `Player1.prefab` al campo "Player1"

---

## 🎯 VERIFICACIÓN FINAL

### Antes de dar Play, verifica:

✅ Solo existe 1 GameObject "GameManager" en la Hierarchy  
✅ Todos los scripts están DENTRO del GameManager  
✅ No hay objetos sueltos de ConstructorTablero/ActionExecutor/etc.  
✅ Todas las referencias apuntan al GameManager (no a prefabs)  
✅ Todos los 12 prefabs están asignados en Constructor Tablero  
✅ Tiempo Entre Turnos = 10  
✅ Velocidad Reproduccion = 1  
✅ Mostrar Debug Logs = activado en ambos scripts  

---

## 🐛 LOGS QUE DEBERÍAS VER AL DAR PLAY

```
✅ ConstructorTablero asignado correctamente (tamanioCelda=3)
🔍 Buscando astronautas con tag 'Player': X encontrados
  ✓ Astronauta ID 1 encontrado: Tripulante_1_astronauta
  ✓ Astronauta ID 2 encontrado: Tripulante_2_astronauta
✅ Cache inicializado: 2 astronautas, X paredes
🎬 === TURNO 1/5 ===
🔍 Ejecutando acción tipo 'mover' para astronauta ID 1
🔍 [EjecutarMovimiento] Iniciando para astronauta ID 1
🚶 Astronauta 1: (0,0) → (1,0)
✅ Moviendo astronauta con tamanioCelda=3
⏸️ Pausa entre turnos (10s)
```

---

## ❌ SI VES ESTOS ERRORES:

### "ConstructorTablero es NULL"
→ NO asignaste el GameManager en el campo "Constructor Tablero"

### "No se encontró astronauta con ID X"
→ Los prefabs Player1/Player2 no tienen el tag "Player"  
→ O los Astronaut ID están duplicados (ambos tienen ID=1)

### "GameObject astronauta es NULL"
→ El cache encontró el astronauta pero luego se destruyó  
→ O hay objetos sueltos duplicados en la escena

---

## 🔧 PROBLEMAS COMUNES

### Problema: "No veo el tablero"
**Solución**: El tablero SÍ se construye, está en la Hierarchy bajo "tablero" o "0 (10)". Mueve la cámara con Tab + WASD para verlo.

### Problema: "La simulación va muy rápido"
**Solución**: Aumenta "Tiempo Entre Turnos" a 10 o más segundos.

### Problema: "Solo encuentra 1 astronauta"
**Solución**: Abre el prefab Player2, cambia "Astronauta ID" de 1 a 2, guarda.

### Problema: "Objetos duplicados"
**Solución**: Borra TODOS los objetos sueltos de scripts (ConstructorTablero, ActionExecutor, etc.) y deja solo el GameManager.
