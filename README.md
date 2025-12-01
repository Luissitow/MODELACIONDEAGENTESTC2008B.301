# 🚀 Fire Rescue 2 - Simulación de Rescate Espacial
## MODELACION DE AGENTES TC2008B.301

[![Unity Version](https://img.shields.io/badge/Unity-2022.3+-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen.svg)](https://github.com/Luissitow/MODELACIONDEAGENTESTC2008B.301)

> Juego de simulación basado en **Flash Point: Fire Rescue** adaptado a un escenario espacial donde astronautas deben rescatar tripulantes de una estación en peligro.

---

## 🆕 **ACTUALIZACIÓN: Sistema de Paredes Funcional** (29 Nov 2025)

### ⚡ Inicio Rápido
**¿Primera vez configurando?** → Lee [`INICIO_RAPIDO.md`](INICIO_RAPIDO.md) (10 minutos)

### 📚 Documentación del Sistema de Paredes
- 🚀 [`INICIO_RAPIDO.md`](INICIO_RAPIDO.md) - Configuración en 3 pasos
- 📖 [`GUIA_CONFIGURACION_PAREDES.md`](GUIA_CONFIGURACION_PAREDES.md) - Guía detallada
- 📊 [`RESUMEN_EJECUTIVO.md`](RESUMEN_EJECUTIVO.md) - Visión general técnica
- 🔧 [`RESUMEN_MEJORAS_PAREDES.md`](RESUMEN_MEJORAS_PAREDES.md) - Cambios implementados

### ✅ Características Nuevas
- ✅ **3 estados visuales** de paredes: Normal → Dañada → Destruida
- ✅ **Puertas que se abren** hacia arriba
- ✅ **Herramienta de validación** integrada en Unity
- ✅ **Creación automática** de prefabs placeholder
- ✅ **Logs descriptivos** con emojis para debugging fácil

---

## 📋 Índice

- [Configuración Rápida](#-actualización-sistema-de-paredes-funcional-29-nov-2025)
- [Características](#-características)
- [Instalación](#-instalación)
- [Uso](#-uso)
- [Arquitectura](#-arquitectura)
- [Sistema de Daño](#-sistema-de-daño)
- [Sistema de Simulación](#-sistema-de-simulación)
- [Contribuir](#-contribuir)
- [Changelog](#-changelog)

---

## ✨ Características

### 🎮 Jugabilidad
- ✅ **Control de astronautas** en primera persona
- ✅ **Sistema de simulación JSON** para reproducir partidas completas
- ✅ **Tablero dinámico** de 6×8 celdas construido desde JSON
- ✅ **Paredes y puertas destructibles** con estados visuales
- ✅ **Sistema de rescate** de víctimas/tripulantes
- ✅ **Detección de falsas alarmas**
- ✅ **Múltiples tipos de peligros**: fuego (arañas), hazmat (huevos)

### 🏗️ Sistemas Implementados

#### **Sistema de Daño** (v0.0.1.1)
- 🔨 Paredes y puertas con **3 estados**: intacta → dañada → destruida
- 🎨 **Cambio automático de prefabs** según el daño
- 💥 **2 golpes** destruyen cualquier obstáculo
- 📊 Estados visuales progresivos con prefabs intercambiables

#### **Sistema de Construcción**
- 🏭 **Construcción automática** del tablero desde `escenario.json`
- 📐 Tablero de **6 filas × 8 columnas** (celdas de 3×3 unidades)
- 🧱 Paredes con configuración de bits (4 direcciones)
- 🚪 Puertas entre habitaciones
- 🔥 Arañas (fuego) y huevos (hazmat)
- 👥 Víctimas rescatables y falsas alarmas

#### **Sistema de Simulación**
- 🎬 Reproducción automática desde `simulacion_completa.json`
- ⏯️ Control de velocidad de reproducción
- 📹 Registro de turnos y acciones por astronauta
- 🔄 Sistema de estado inicial y estados intermedios

### 🎨 Visualización
- 🌌 Escenario espacial con assets 3D
- 🎥 Cámara de seguimiento tercera persona
- 💡 Universal Render Pipeline (URP) optimizado
- 🚀 Modelos de astronautas, estación espacial y naves

---

## 🚀 Instalación

### Requisitos Previos
- **Unity 2022.3+**
- **Git** instalado
- **Universal Render Pipeline** (incluido en el proyecto)

### Pasos

1. **Clonar el repositorio:**
```bash
git clone https://github.com/Luissitow/MODELACIONDEAGENTESTC2008B.301.git
cd MODELACIONDEAGENTESTC2008B.301
```

2. **Abrir en Unity:**
   - Abre Unity Hub
   - Click en "Add" → Selecciona la carpeta del proyecto
   - Abre el proyecto (Unity descargará dependencias automáticamente)

3. **Configurar la escena:**
   - Abre `Assets/Scenes/spacerescue.unity`
   - Verifica que `GameManager` esté activo en la Hierarchy
   - Asegúrate de que **ControladorJuego está habilitado** (checkbox marcado)

4. **Ejecutar:**
   - Presiona **Play** ▶️
   - El tablero se construirá automáticamente
   - Los astronautas comenzarán la simulación

---

## 🎮 Uso

### Configuración del Tablero

El tablero se construye automáticamente desde `Assets/Resources/escenario.json`:

```json
{
  "fila": 6,
  "columna": 8,
  "celdas": ["1100", "1000", "1001", ...],
  "victimas": [{"row": 2, "col": 4, "type": "victima"}],
  "tripulacion": [{"row": 3, "col": 1, "tipo": "astronauta", "id": 1}],
  "arañas": [{"row": 1, "col": 2}],
  "puertas": [{"r1": 1, "c1": 3, "r2": 1, "c2": 4}]
}
```

### Sistema de Coordenadas
- **JSON**: 1-indexed (row: 1-6, col: 1-8)
- **Unity**: 0-indexed (fila: 0-5, columna: 0-7)
- **Conversión automática** en `ConstructorTablero.cs`

### Configuración de Bits para Paredes
Cada celda tiene 4 bits: `[Norte][Oeste][Sur][Este]`

```
Ejemplo: "1100"
├── Norte (1) → Pared al norte
├── Oeste (1) → Pared al oeste
├── Sur (0) → Sin pared al sur
└── Este (0) → Sin pared al este
```

### Dañar Paredes/Puertas

```csharp
// Obtener referencia
Wall pared = GameObject.Find("Pared_2_3_norte").GetComponent<Wall>();

// Aplicar daño
pared.Atacar();        // 1 punto de daño
pared.Romper();        // 2 puntos (destruye)
pared.RecibirDano(1);  // Daño personalizado

// Verificar estado
if (pared.estaDestruida)
    Debug.Log("¡Pared destruida!");
```

### Abrir Puertas

```csharp
Wall puerta = GameObject.Find("Puerta_1_3_este").GetComponent<Wall>();
puerta.AbrirPuerta(); // Se mueve hacia arriba
```

---

## 🏛️ Arquitectura

### Estructura del Proyecto

```
Assets/
├── Scripts/
│   ├── Data/              # Modelos de datos (JSON)
│   │   └── Model/
│   ├── Domain/            # Lógica del juego
│   ├── Framework/         # Managers y sistemas core
│   │   └── Managers/
│   │       ├── ConstructorTablero.cs
│   │       ├── ControladorJuego.cs
│   │       ├── GameManager.cs
│   │       ├── SimulacionPlayer.cs
│   │       ├── ActionExecutor.cs
│   │       └── Wall.cs
│   ├── Game/              # Scripts de gameplay
│   └── Utils/             # Utilidades
├── Resources/             # Archivos cargables en runtime
│   ├── escenario.json
│   └── simulacion_completa.json
├── Prefabs/               # Prefabs de objetos
│   ├── Piso/
│   ├── Paredes/
│   ├── Puertas/
│   └── Astronautas/
├── Scenes/
│   └── spacerescue.unity
└── ExternalAssets/        # Assets de terceros
```

### Clean Architecture

```
┌─────────────────────────────────────┐
│         Presentation Layer          │
│      (UI, Input, Visualization)     │
├─────────────────────────────────────┤
│          Framework Layer            │
│    (Managers, Orchestration)        │
├─────────────────────────────────────┤
│           Domain Layer              │
│        (Business Logic)             │
├─────────────────────────────────────┤
│            Data Layer               │
│      (Models, Persistence)          │
└─────────────────────────────────────┘
```

### Componentes Principales

#### **ControladorJuego**
- Carga `escenario.json` desde Resources
- Construye el tablero inicial
- Inicializa el GameManager

#### **ConstructorTablero**
- Instancia pisos, paredes, puertas
- Coloca arañas (fuego) y huevos (hazmat)
- Crea víctimas, falsas alarmas y astronautas
- Convierte coordenadas JSON a Unity

#### **SimulacionPlayer**
- Lee `simulacion_completa.json`
- Reproduce turnos secuencialmente
- Controla velocidad y pausas

#### **ActionExecutor**
- Ejecuta acciones: mover, dañar pared, abrir puerta
- Anima movimientos de astronautas
- Valida colisiones y restricciones

#### **Wall**
- Maneja estado de paredes y puertas
- Sistema de daño (vida actual/máxima)
- Cambio de prefabs según daño
- Animación de apertura de puertas

---

## 🔨 Sistema de Daño

### Estados de Objetos

| Estado | Vida | Visual | Funcionalidad |
|--------|------|--------|---------------|
| **Intacta** | 2/2 | Prefab normal | Bloquea paso |
| **Dañada** | 1/2 | Prefab con grietas | Bloquea paso |
| **Destruida** | 0/2 | Prefab roto o desactivado | Permite paso |

### Flujo de Daño

```
┌──────────────┐
│   INTACTA    │  Vida: 2/2
│  (Normal)    │
└──────┬───────┘
       │ Atacar() o RecibirDano(1)
       ▼
┌──────────────┐
│    DAÑADA    │  Vida: 1/2
│ (Con grietas)│  ✅ Cambia a prefabDanado
└──────┬───────┘
       │ Atacar() o RecibirDano(1)
       ▼
┌──────────────┐
│  DESTRUIDA   │  Vida: 0/2
│   (Rota)     │  ✅ Cambia a prefabDestruido
└──────────────┘  ✅ Ya no bloquea paso
```

### Configuración de Prefabs

Para cada pared/puerta necesitas **3 prefabs**:

1. **Normal** (intacta)
2. **Dañada** (grietas/manchas)
3. **Destruida** (rota/agujeros)

**Asignación en Inspector:**
```
Wall (Script):
├── Prefab Normal: [tu_prefab_intacto]
├── Prefab Danado: [tu_prefab_con_grietas]
└── Prefab Destruido: [tu_prefab_roto]
```

---

## 🎬 Sistema de Simulación

### Formato JSON

```json
{
  "duracion_total": 5,
  "turnos": [
    {
      "turno": 1,
      "timestamp": "00:00:00",
      "acciones": [
        {
          "astronautaID": 1,
          "tipo": "mover",
          "desde": {"fila": 0, "columna": 0},
          "hacia": {"fila": 1, "columna": 0},
          "costo": 1
        }
      ]
    }
  ]
}
```

### Tipos de Acciones

| Acción | Descripción | Costo |
|--------|-------------|-------|
| `mover` | Mover a celda adyacente | 1 |
| `abrir_puerta` | Abrir puerta en dirección | 1 |
| `danar_pared` | Golpear pared (1 daño) | 2 |
| `romper_pared` | Golpe fuerte (2 daño) | 3 |
| `rescatar` | Cargar víctima | 1 |
| `descargar` | Dejar víctima en salida | 1 |

---

## 👥 Contribuir

### Flujo de Trabajo

1. **Fork** el proyecto
2. Crea tu **feature branch**: `git checkout -b feature/nueva-caracteristica`
3. **Commit** tus cambios: `git commit -m 'feat: agregar nueva característica'`
4. **Push** al branch: `git push origin feature/nueva-caracteristica`
5. Abre un **Pull Request**

### Convenciones de Commits

Seguimos [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: nueva característica
fix: corrección de bug
docs: documentación
style: formato, espacios
refactor: reestructuración
test: agregar tests
chore: tareas de mantenimiento
```

---

## 📊 Changelog

Ver [CHANGELOG.md](CHANGELOG.md) para historial completo de cambios.

### Últimos Cambios (v0.0.1.1)
- ✅ Sistema de daño para paredes y puertas
- ✅ Puertas destructibles (2 golpes)
- ✅ Ajuste de escala de jugadores
- ✅ Cambio automático de prefabs según daño

---

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver [LICENSE](LICENSE) para detalles.

---

## 🙏 Agradecimientos

- **Flash Point: Fire Rescue** - Juego de mesa original
- **Unity Technologies** - Motor de juego
- **Comunidad de assets 3D** - Modelos espaciales

---

## 📞 Contacto

**Luis Oswaldo Jiménez Alvarado**
- GitHub: [@Luissitow](https://github.com/Luissitow)
- Proyecto: [MODELACIONDEAGENTESTC2008B.301](https://github.com/Luissitow/MODELACIONDEAGENTESTC2008B.301)

---

<div align="center">

**Hecho con ❤️ para TC2008B.301**

[![Unity](https://img.shields.io/badge/Made%20with-Unity-black.svg?style=flat&logo=unity)](https://unity.com/)
[![C#](https://img.shields.io/badge/Language-C%23-blue.svg?style=flat&logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)

</div>