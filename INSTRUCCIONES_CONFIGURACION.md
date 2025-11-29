# 🔧 INSTRUCCIONES DE CONFIGURACIÓN UNITY

## ✅ PROBLEMAS RESUELTOS EN CÓDIGO:
1. ✅ Tamaño de celda cambiado de 4 a **3 unidades**
2. ✅ Warning de puerta corregido (no aplica daño a puertas)
3. ✅ Archivo `simulacion_completa.json` creado (placeholder)

---

## ⚠️ CONFIGURACIÓN MANUAL EN UNITY:

### 1️⃣ **DEFINIR TAG "Wall"** (CRÍTICO)
El error `UnityException: Tag: Wall is not defined` se arregla así:

1. En Unity, ve a menú superior: **Edit → Project Settings**
2. En la ventana que se abre, selecciona **Tags and Layers**
3. En la sección **Tags**, haz clic en el botón **+**
4. Escribe: `Wall`
5. Presiona Enter para guardar
6. Cierra Project Settings

Luego, **asigna el tag a los prefabs de pared**:
- Selecciona `paredPrefab` en Project
- En Inspector, arriba, cambia Tag de "Untagged" a **"Wall"**
- Haz lo mismo con `puertaPrefab`, `paredDanadaPrefab`, `paredDestruidaPrefab`

---

### 2️⃣ **AUDIO LISTENERS** (3 encontrados, debe haber solo 1)
Problema: "There are 3 audio listeners in the scene"

**Solución**:
1. En la escena, busca todos los objetos con componente `Audio Listener`:
   - Main Camera (normalmente tiene uno)
   - Player1 prefab
   - Player2 prefab
   - CamaraLibre
2. **Elimina el componente Audio Listener** de Player1, Player2 y CamaraLibre
3. **Deja SOLO UNO** en Main Camera

**Cómo eliminar Audio Listener**:
- Selecciona el GameObject
- En Inspector, busca componente "Audio Listener"
- Haz clic en los 3 puntos (⋮) a la derecha del componente
- Selecciona "Remove Component"

---

### 3️⃣ **SCRIPT FALTANTE** (The referenced script is missing)
Problema: "The referenced script (Unknown) on this Behaviour is missing!"

**Solución**:
1. Abre la **Console** en Unity (Window → General → Console)
2. Haz clic en el error para ver qué GameObject tiene el problema
3. Selecciona ese GameObject en la escena
4. En Inspector, verás un componente con "(Script)" en gris
5. **Elimina ese componente** (clic en ⋮ → Remove Component)

---

### 4️⃣ **ESCALAS DE PREFABS** (Todo debe medir 3 unidades)
Con `tamanioCelda = 3f`, los prefabs deben tener estas escalas:

#### **Piso** (Cube):
- Scale: **(2.8, 0.1, 2.8)** - para caber en celda 3×3 con separación
- Material: Gris

#### **Pared** (Cube):
- Scale: **(0.15, 2, 3)** - grosor 0.15, altura 2, largo 3
- Material: Marrón/Madera

#### **Araña** (Sphere - representa fuego):
- Scale: **(0.4, 0.4, 0.4)** - esfera pequeña
- Material: Rojo brillante

#### **Huevo** (Cube - representa hazmat):
- Scale: **(0.25, 0.25, 0.25)** - cubo pequeño
- Material: Amarillo

#### **Tripulante** (Capsule - rescatable):
- Scale: **(0.4, 0.4, 0.4)** - persona pequeña
- Material: Verde

#### **Falsa Alarma** (Capsule - NO rescatable):
- Scale: **(0.4, 0.4, 0.4)** - igual que tripulante
- Material: Gris

#### **Punto de Interés** (Quad - marcador "?"):
- Scale: **(0.6, 0.6, 0.6)** - cartel flotante
- Rotation: **(90, 0, 0)** - para que se vea desde arriba
- Material: Amarillo con textura "?"

#### **Puerta** (Cube):
- Scale: **(0.15, 2, 3)** - igual que pared
- Material: Color diferente (azul/verde)

#### **Player1 y Player2** (Capsule):
- Scale: **(0.5, 0.5, 0.5)** - astronautas visibles
- Material: Azul (Player1) y Rojo (Player2)

---

### 5️⃣ **VERIFICAR REFERENCIAS EN INSPECTOR**

Asegúrate de que estos GameObjects tienen referencias asignadas:

#### **ConstructorTablero**:
- ✅ pisoPrefab
- ✅ paredPrefab  
- ✅ aranaPrefab
- ✅ huevoPrefab
- ✅ tripulantePrefab
- ✅ falsaAlarmaPrefab
- ✅ puntoInteresPrefab
- ✅ puertaPrefab
- ✅ paredDanadaPrefab
- ✅ paredDestruidaPrefab
- ✅ player1Prefab
- ✅ player2Prefab

#### **ActionExecutor**:
- ✅ constructorTablero (referencia al GameObject)
- ✅ gameManager (referencia al GameObject)

#### **SimulacionPlayer**:
- ✅ constructorTablero
- ✅ actionExecutor
- ✅ gameManager

---

## 🎮 RESULTADO ESPERADO:

Después de aplicar estos cambios, al presionar Play:

1. ✅ El mapa se construye con celdas de **3×3 unidades**
2. ✅ Los objetos se ven del tamaño correcto
3. ✅ No hay warning de puerta
4. ✅ Solo 1 Audio Listener activo
5. ✅ No hay error de script faltante
6. ✅ No hay error de Tag "Wall"
7. ✅ El JSON placeholder carga sin errores

---

## 📋 CHECKLIST FINAL:

- [ ] Tag "Wall" definido en Project Settings
- [ ] Tag "Wall" asignado a prefabs de pared
- [ ] Solo 1 Audio Listener en la escena (Main Camera)
- [ ] Script faltante eliminado
- [ ] Escalas de prefabs ajustadas a 3 unidades
- [ ] Referencias asignadas en Inspector
- [ ] Console sin errores al presionar Play

---

## 🐛 SI TODAVÍA HAY PROBLEMAS:

1. **Haz screenshot del Inspector** del GameObject que da error
2. **Copia SOLO los primeros 50 líneas** de la Console (no todo)
3. **Verifica que los prefabs tengan los materiales correctos**

---

¡Ahora todo debería medir 3 unidades y funcionar correctamente! 🎉
