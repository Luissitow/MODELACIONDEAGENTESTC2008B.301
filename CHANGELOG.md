# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.1.1] - 2025-11-29

### Added
- **Sistema de daño para paredes y puertas** con 3 estados visuales (intacta, dañada, destruida)
- **Puertas destructibles**: ahora las puertas pueden recibir daño y romperse después de 2 golpes
- **Cambio automático de prefabs** según el estado de daño de paredes/puertas
- **Ajuste de escala de jugadores** a 0.75x para celdas de 3×3 unidades
- Sistema de logs descriptivos con emojis para debug de daño y estados
- Método `CambiarPrefab()` mejorado que preserva todo el estado (posición, vida, tipo, referencias)
- Componente `Door.cs` actualizado con soporte para animaciones y daño
- Documentación completa: `SISTEMA_DANO_PAREDES_PUERTAS.md`

### Fixed
- **Tamaño de jugadores**: escala ajustada automáticamente para tablero de 3×3 unidades
- **Construcción del tablero**: problema resuelto habilitando ControladorJuego en Inspector
- Preservación de estado al cambiar prefabs de paredes/puertas dañadas

### Changed
- `Wall.cs`: las puertas ahora pueden recibir daño (antes solo se podían abrir)
- `ActualizarEstadoVisual()`: soporte unificado para paredes y puertas
- Logs mejorados con emojis y mensajes descriptivos

## [0.0.1] - 2025-11-19 to 2025-11-26

### Added
- **Sistema de simulación JSON** con reproducción automática de movimientos de astronautas
- **Sistema de paredes especiales** con daño, estados de vida y apertura de puertas
- **Prefabs de puertas** con soporte para estados (intacta, dañada, destruida)
- **Sistema de construcción de tablero** basado en JSON con escenario.json
- **Separación de entidades**: víctimas, falsas alarmas y tripulación como objetos independientes
- Control de escala automático para objetos del tablero
- Ajuste de alturas Y corregidas para visualización correcta
- Scripts: `SimulacionPlayer.cs`, `ActionExecutor.cs`, `Wall.cs`
- Configuración de JSON: `escenario.json`, `simulacion_completa.json`

### Changed
- Tamaño de celdas actualizado de 4×4 a 3×3 unidades
- Sistema de coordenadas unificado (JSON 1-indexed, Unity 0-indexed)
- Mejoras en `ConstructorTablero.cs` para manejo de prefabs y validaciones

## [0.0.0.1] - 2025-11-13 to 2025-11-18

### Added
- Estructura Clean Architecture implementada (Data, Domain, Framework, Utils)
- Carpetas Data/ con modelos de entidades para Flash Point: Fire Rescue
- Carpetas Domain/ para requerimientos del juego
- Carpetas Framework/ para managers y agentes
- Script AstronautController.cs con controles primera persona
- Script AstronautCameraFollow.cs para seguimiento de cámara
- Assets externos y modelos 3D para escenarios espaciales
- Configuración URP (Universal Render Pipeline) para PC y Mobile
- Creación de mapa inicial en Unity con cuartos y assets 3D
- Nave de evacuación y nave de rescatados
- Correcciones en el mapa base

### Removed
- Asset pack sFuture Modules Pro (limpieza de assets no utilizados)

## [0.0.0] - 2025-11-13
### Added
- Proyecto Unity inicial estructurado (Managers, Environment, Systems, Canvases).
- Scripts base: GameMap, MapGenerator, FireRescueGameManager, WallElement, FireRescueDecoder, MapAPIHelper.
- Data JSON de ejemplo `map.json` y `game_config.json`.
- `.gitignore` optimizado para Unity.
- `README.md` con explicación de bitmask y flujo Git.

