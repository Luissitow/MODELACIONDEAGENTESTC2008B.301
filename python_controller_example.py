"""
Fire Rescue 2 - Python Controller Example
==========================================

Este script demuestra cómo controlar el juego desde Python
modificando el archivo escenario.json

Autor: FireRescue2 Team
Fecha: 29 de noviembre de 2025
"""

import json
import time
import os
from typing import List, Dict, Any

class FireRescueController:
    """Controlador principal para interactuar con el juego Unity"""
    
    def __init__(self, json_path: str = "Assets/Resources/escenario.json"):
        """
        Inicializa el controlador
        
        Args:
            json_path: Ruta al archivo escenario.json
        """
        self.json_path = json_path
        self.data = None
        self.cargar_escenario()
    
    def cargar_escenario(self):
        """Carga el archivo JSON del escenario"""
        try:
            with open(self.json_path, 'r', encoding='utf-8') as f:
                self.data = json.load(f)
            print(f"✅ Escenario cargado: {self.data['fila']}x{self.data['columna']}")
        except FileNotFoundError:
            print(f"❌ Error: No se encontró el archivo {self.json_path}")
        except json.JSONDecodeError:
            print("❌ Error: El archivo JSON está mal formado")
    
    def guardar_escenario(self):
        """Guarda los cambios al archivo JSON"""
        try:
            with open(self.json_path, 'w', encoding='utf-8') as f:
                json.dump(self.data, f, indent=2, ensure_ascii=False)
            print("💾 Escenario guardado correctamente")
        except Exception as e:
            print(f"❌ Error al guardar: {e}")
    
    def obtener_astronauta(self, astronauta_id: int) -> Dict[str, Any]:
        """
        Obtiene los datos de un astronauta por su ID
        
        Args:
            astronauta_id: ID del astronauta (1, 2, 3, etc.)
            
        Returns:
            Diccionario con datos del astronauta o None si no existe
        """
        for miembro in self.data['tripulacion']:
            if miembro['id'] == astronauta_id:
                return miembro
        return None
    
    def mover_astronauta(self, astronauta_id: int, fila_destino: int, col_destino: int):
        """
        Mueve un astronauta a una posición específica
        
        Args:
            astronauta_id: ID del astronauta
            fila_destino: Fila destino (1-6)
            col_destino: Columna destino (1-8)
        """
        astronauta = self.obtener_astronauta(astronauta_id)
        if astronauta is None:
            print(f"⚠️ No se encontró astronauta con ID {astronauta_id}")
            return
        
        # Añadir movimiento a la cola
        movimiento = {
            "tipo": "mover",
            "destino": {
                "row": fila_destino,
                "col": col_destino
            }
        }
        
        if 'movimientos' not in astronauta:
            astronauta['movimientos'] = []
        
        astronauta['movimientos'].append(movimiento)
        print(f"🚶 Astronauta {astronauta_id} se moverá a ({fila_destino}, {col_destino})")
    
    def abrir_puerta(self, astronauta_id: int, direccion: str):
        """
        Abre una puerta en la dirección especificada
        
        Args:
            astronauta_id: ID del astronauta
            direccion: "norte", "sur", "este", "oeste"
        """
        astronauta = self.obtener_astronauta(astronauta_id)
        if astronauta is None:
            print(f"⚠️ No se encontró astronauta con ID {astronauta_id}")
            return
        
        movimiento = {
            "tipo": "abrir_puerta",
            "direccion": direccion.lower()
        }
        
        if 'movimientos' not in astronauta:
            astronauta['movimientos'] = []
        
        astronauta['movimientos'].append(movimiento)
        print(f"🚪 Astronauta {astronauta_id} abrirá puerta al {direccion}")
    
    def danar_pared(self, astronauta_id: int, direccion: str):
        """
        Daña una pared en la dirección especificada
        
        Args:
            astronauta_id: ID del astronauta
            direccion: "norte", "sur", "este", "oeste"
        """
        astronauta = self.obtener_astronauta(astronauta_id)
        if astronauta is None:
            print(f"⚠️ No se encontró astronauta con ID {astronauta_id}")
            return
        
        movimiento = {
            "tipo": "danar_pared",
            "direccion": direccion.lower()
        }
        
        if 'movimientos' not in astronauta:
            astronauta['movimientos'] = []
        
        astronauta['movimientos'].append(movimiento)
        print(f"💥 Astronauta {astronauta_id} dañará pared al {direccion}")
    
    def recoger_victima(self, astronauta_id: int):
        """
        Hace que el astronauta recoja una víctima
        
        Args:
            astronauta_id: ID del astronauta
        """
        astronauta = self.obtener_astronauta(astronauta_id)
        if astronauta is None:
            print(f"⚠️ No se encontró astronauta con ID {astronauta_id}")
            return
        
        movimiento = {"tipo": "recoger_victima"}
        
        if 'movimientos' not in astronauta:
            astronauta['movimientos'] = []
        
        astronauta['movimientos'].append(movimiento)
        print(f"👤 Astronauta {astronauta_id} recogerá víctima")
    
    def dejar_victima(self, astronauta_id: int):
        """
        Hace que el astronauta deje una víctima en la entrada
        
        Args:
            astronauta_id: ID del astronauta
        """
        astronauta = self.obtener_astronauta(astronauta_id)
        if astronauta is None:
            print(f"⚠️ No se encontró astronauta con ID {astronauta_id}")
            return
        
        movimiento = {"tipo": "dejar_victima"}
        
        if 'movimientos' not in astronauta:
            astronauta['movimientos'] = []
        
        astronauta['movimientos'].append(movimiento)
        print(f"🏠 Astronauta {astronauta_id} dejará víctima en entrada")
    
    def limpiar_movimientos(self, astronauta_id: int = None):
        """
        Limpia los movimientos de un astronauta o de todos
        
        Args:
            astronauta_id: ID del astronauta o None para todos
        """
        if astronauta_id is None:
            for miembro in self.data['tripulacion']:
                miembro['movimientos'] = []
            print("🧹 Todos los movimientos limpiados")
        else:
            astronauta = self.obtener_astronauta(astronauta_id)
            if astronauta:
                astronauta['movimientos'] = []
                print(f"🧹 Movimientos del astronauta {astronauta_id} limpiados")
    
    def añadir_arana(self, fila: int, col: int):
        """
        Añade una araña al mapa
        
        Args:
            fila: Fila (1-6)
            col: Columna (1-8)
        """
        if 'arañas' not in self.data:
            self.data['arañas'] = []
        
        self.data['arañas'].append({"row": fila, "col": col})
        print(f"🕷️ Araña añadida en ({fila}, {col})")
    
    def eliminar_arana(self, fila: int, col: int):
        """
        Elimina una araña del mapa
        
        Args:
            fila: Fila (1-6)
            col: Columna (1-8)
        """
        if 'arañas' not in self.data:
            return
        
        self.data['arañas'] = [
            a for a in self.data['arañas'] 
            if not (a['row'] == fila and a['col'] == col)
        ]
        print(f"❌ Araña eliminada de ({fila}, {col})")
    
    def mostrar_estado(self):
        """Muestra el estado actual del juego"""
        print("\n" + "="*50)
        print("📊 ESTADO DEL JUEGO")
        print("="*50)
        print(f"Dimensiones: {self.data['fila']}x{self.data['columna']}")
        print(f"Víctimas: {len(self.data.get('victimas', []))}")
        print(f"Falsas alarmas: {len(self.data.get('falsasAlarmas', []))}")
        print(f"Arañas: {len(self.data.get('arañas', []))}")
        print(f"Tripulación: {len(self.data.get('tripulacion', []))}")
        print("\n👥 TRIPULACIÓN:")
        for miembro in self.data.get('tripulacion', []):
            tipo = "Jugador" if miembro.get('esJugador', False) else "NPC"
            movs = len(miembro.get('movimientos', []))
            print(f"  - ID {miembro['id']} ({tipo}): ({miembro['row']}, {miembro['col']}) - {movs} movimientos")
        print("="*50 + "\n")


# ============================================================================
# EJEMPLO DE USO
# ============================================================================

def ejemplo_rescate_simple():
    """
    Ejemplo: Un astronauta se mueve para rescatar una víctima
    """
    print("\n🎮 INICIANDO EJEMPLO: RESCATE SIMPLE\n")
    
    # Crear controlador
    controller = FireRescueController()
    
    # Limpiar movimientos anteriores
    controller.limpiar_movimientos()
    
    # Astronauta 1: Moverse a víctima, recogerla, llevarla a entrada
    print("\n📝 Planificando rescate del Astronauta 1:")
    controller.mover_astronauta(1, 2, 4)        # Moverse a la víctima
    controller.recoger_victima(1)               # Recoger víctima
    controller.mover_astronauta(1, 1, 6)        # Moverse a entrada
    controller.dejar_victima(1)                 # Dejar víctima (¡rescate!)
    
    # Guardar cambios
    controller.guardar_escenario()
    controller.mostrar_estado()
    
    print("✅ Movimientos enviados. Unity los ejecutará en ~1 segundo")


def ejemplo_abrir_puertas():
    """
    Ejemplo: Un astronauta abre puertas para explorar
    """
    print("\n🎮 INICIANDO EJEMPLO: ABRIR PUERTAS\n")
    
    controller = FireRescueController()
    controller.limpiar_movimientos()
    
    print("\n📝 Planificando exploración del Astronauta 2:")
    controller.mover_astronauta(2, 5, 8)        # Moverse a celda
    controller.abrir_puerta(2, "norte")         # Abrir puerta
    controller.mover_astronauta(2, 4, 8)        # Avanzar
    
    controller.guardar_escenario()
    controller.mostrar_estado()
    
    print("✅ Movimientos enviados")


def ejemplo_propagacion_aranas():
    """
    Ejemplo: Simular propagación de arañas
    """
    print("\n🎮 INICIANDO EJEMPLO: PROPAGACIÓN DE ARAÑAS\n")
    
    controller = FireRescueController()
    
    # Añadir nuevas arañas (simulando propagación)
    print("\n🕷️ Propagando arañas:")
    controller.añadir_arana(2, 4)
    controller.añadir_arana(3, 6)
    
    # Guardar cambios
    controller.guardar_escenario()
    controller.mostrar_estado()
    
    print("✅ Arañas añadidas. Unity las creará en ~1 segundo")


# ============================================================================
# MAIN
# ============================================================================

if __name__ == "__main__":
    print("""
    ╔════════════════════════════════════════════════╗
    ║   Fire Rescue 2 - Python Controller           ║
    ║   Controlador de ejemplo para Unity           ║
    ╚════════════════════════════════════════════════╝
    """)
    
    # Descomentar el ejemplo que quieras ejecutar:
    
    ejemplo_rescate_simple()
    # ejemplo_abrir_puertas()
    # ejemplo_propagacion_aranas()
    
    print("\n🎮 Para ejecutar otro ejemplo, modifica la llamada en el código\n")
