# -*- coding: utf-8 -*-
"""
Flash Point: Fire Rescue - Simulador Multi-Agentes para Unity
Genera JSON compatible con el cliente Unity (arañas/huevos en lugar de fuego/humo)
Incluye servidor HTTP simple para servir simulacion_completa.json
"""

import numpy as np
from mesa import Agent, Model
from mesa.space import MultiGrid
from typing import Optional, Tuple, List, Dict, Any
import random
import heapq
import json
from http.server import HTTPServer, BaseHTTPRequestHandler
import threading

# =============================================================================
# OBJETO PARED
# =============================================================================

class Wall:
    def __init__(self, wall_id=None, damage=0, door=0, enPie=True):
        self.id = wall_id
        self.damage = damage
        self.door = door  # 0=no hay puerta, 1=cerrada, 2=abierta
        self.enPie = enPie
        self.paredVecina = None

    def setParedVecina(self, paredV):
        self.paredVecina = paredV

    def abrir_door(self):
        if self.enPie and self.door == 1:
            self.door = 2
            if self.paredVecina:
                self.paredVecina.door = 2

    def cerrar_door(self):
        if self.enPie and self.door == 2:
            self.door = 1
            if self.paredVecina:
                self.paredVecina.door = 1

    def afecta_wall(self):
        if self.enPie:
            self.damage += 1
            if self.damage >= 2:
                self.destruirWall()
            elif self.paredVecina:
                self.paredVecina.damage = self.damage

    def destruirWall(self):
        if self.enPie:
            self.enPie = False
            self.door = 0
            self.damage = 2
            if self.paredVecina:
                self.paredVecina.enPie = False
                self.paredVecina.door = 0
                self.paredVecina.damage = 2

    def is_destroyed(self):
        return self.damage >= 2 or not self.enPie

    def puePasar(self):
        return self.is_destroyed() or self.door == 2


# =============================================================================
# OBJETO CELDA
# =============================================================================

class Cell:
    def __init__(self, cell_id, wall_info="0000", door_info="0000", pos=None):
        self.id = cell_id
        self.pos = pos
        self.poi = 0  # 0=nada, 1=victima, 2=falsa alarma
        self.poi_revelado = False
        # En Unity: araña=fuego, huevo=humo
        self.araña = False  # fire
        self.huevo = False  # smoke
        self.agents = []
        self.walls = self._crear_walls(wall_info, door_info)
        self.es_exterior = False
        self.wall_info_original = wall_info

    def _crear_walls(self, wall_info, door_info):
        walls = {}
        dir_mapping = {0: "N", 1: "O", 2: "S", 3: "E"}
        for i in range(min(4, len(wall_info))):
            if wall_info[i] == '1':
                dir_name = dir_mapping[i]
                door_state = int(door_info[i]) if i < len(door_info) and door_info[i] in ['1', '2'] else 0
                walls[dir_name] = Wall(wall_id=f"{self.id}-{dir_name}", door=door_state)
        return walls


# =============================================================================
# AGENTE ASTRONAUTA
# =============================================================================

class AstronautAgent(Agent):
    def __init__(self, unique_id, model):
        super().__init__(model)
        self.unique_id = unique_id
        self.x = 0
        self.y = 0
        self.ap = 0
        self.max_ap = 4
        self.alive = True
        self.knockdowns = 0
        self.max_knockdowns = 2
        self.cargando_victima = False
        self.target = None
        self.tarea_asignada = None

    def log(self, msg):
        print(f"    Agent-{self.unique_id}: {msg}")
        self.model._registrar_log(f"Agent-{self.unique_id}: {msg}")

    def reset_ap(self):
        self.ap = self.max_ap + min(self.ap, 4)

    def recibir_dano(self):
        self.knockdowns += 1
        self.log(f"KNOCKDOWN! ({self.knockdowns}/{self.max_knockdowns})")

        if self.cargando_victima:
            self.cargando_victima = False
            celda = self.model.get_cell(self.x, self.y)
            if celda:
                celda.poi = 1
                celda.poi_revelado = True
            self.log("Victima soltada")

        if self.knockdowns >= self.max_knockdowns:
            self.alive = False
            self.log("AGENTE FUERA DE JUEGO")
            return False

        self._mover_a_salida_cercana()
        return True

    def _mover_a_salida_cercana(self):
        if not self.model.salidas:
            return
        mejor_salida = min(self.model.salidas, key=lambda s: abs(self.x - s[0]) + abs(self.y - s[1]))
        celda_actual = self.model.get_cell(self.x, self.y)
        if celda_actual and self in celda_actual.agents:
            celda_actual.agents.remove(self)
        self.x, self.y = mejor_salida
        celda_nueva = self.model.get_cell(self.x, self.y)
        if celda_nueva:
            celda_nueva.agents.append(self)
        self.log(f"Reubicado en salida ({self.x},{self.y})")

    def step(self):
        if not self.alive or self.model.juego_terminado:
            return
        self.log(f"Turno | Pos: ({self.x},{self.y}) | AP: {self.ap} | Tarea: {self.tarea_asignada}")

        acciones = 0
        while self.ap > 0 and acciones < 20 and not self.model.juego_terminado and self.alive:
            if self.model.estrategia == "random":
                if not self._ejecutar_accion_random():
                    break
            else:
                if not self._ejecutar_mejor_accion():
                    break
            acciones += 1

    def _ejecutar_accion_random(self):
        celda = self.model.get_cell(self.x, self.y)

        if self.cargando_victima and self.model.es_salida(self.x, self.y):
            return self._soltar_victima()

        if celda.poi == 1 and celda.poi_revelado and not self.cargando_victima:
            return self._cargar_victima()

        if celda.poi > 0 and not celda.poi_revelado:
            return self._revelar_poi(celda)

        dirs = ['N', 'S', 'E', 'O']
        random.shuffle(dirs)

        for d in dirs:
            vecino_pos = self.model.get_neighbor_dir(self.x, self.y, d)
            if not vecino_pos:
                continue
            celda_destino = self.model.get_cell(*vecino_pos)
            if not celda_destino:
                continue

            # Eliminar araña (fuego) = 2 AP
            if celda_destino.araña and self.ap >= 2 and self._puede_ver(d):
                celda_destino.araña = False
                celda_destino.huevo = True  # araña -> huevo
                self.ap -= 2
                self.log(f"Elimina araña en {vecino_pos} [-2 AP]")
                self.model._registrar_accion(self.unique_id, "eliminar_araña", (self.x, self.y), vecino_pos, 2)
                return True

            if self._mover(d):
                return True
        return False

    def _puede_ver(self, direccion):
        celda = self.model.get_cell(self.x, self.y)
        if direccion in celda.walls:
            pared = celda.walls[direccion]
            if pared.enPie and not pared.is_destroyed() and pared.door == 0:
                return False
        return True

    def _ejecutar_mejor_accion(self):
        celda = self.model.get_cell(self.x, self.y)

        # P1: Depositar víctima en salida
        if self.cargando_victima and self.model.es_salida(self.x, self.y):
            return self._soltar_victima()

        # P2: Llevar víctima a salida
        if self.cargando_victima:
            return self._ir_a_salida()

        # P3: Cargar víctima revelada
        if celda.poi == 1 and celda.poi_revelado:
            return self._cargar_victima()

        # P4: Revelar POI
        if celda.poi > 0 and not celda.poi_revelado:
            return self._revelar_poi(celda)

        # P5: Eliminar araña/huevo en celda actual
        if celda.araña or celda.huevo:
            return self._extinguir(celda)

        # P6: Eliminar araña/huevo ADYACENTE
        for dx, dy in [(0,1),(0,-1),(1,0),(-1,0)]:
            nx, ny = self.x + dx, self.y + dy
            vecino = self.model.cells.get((nx, ny))
            if vecino and not self.model.paredEntremedio(self.x, self.y, nx, ny):
                if vecino.araña and self.ap >= 2:
                    vecino.araña = False
                    vecino.huevo = True
                    self.ap -= 2
                    self.log(f"Elimina araña adyacente ({nx},{ny}) [-2 AP]")
                    self.model._registrar_accion(self.unique_id, "eliminar_araña", (self.x, self.y), (nx, ny), 2)
                    return True
                elif vecino.huevo and self.ap >= 1:
                    vecino.huevo = False
                    self.ap -= 1
                    self.log(f"Elimina huevo adyacente ({nx},{ny}) [-1 AP]")
                    self.model._registrar_accion(self.unique_id, "eliminar_huevo", (self.x, self.y), (nx, ny), 1)
                    return True

        # P7: Seguir tarea asignada
        if self.target and self.tarea_asignada:
            return self._seguir_tarea()

        # P8: Buscar algo útil
        return self._buscar_tarea_autonoma()

    def _soltar_victima(self):
        self.cargando_victima = False
        self.model.victims_rescued += 1
        self.tarea_asignada = None
        self.target = None
        self.log(f"*** VICTIMA RESCATADA! Total: {self.model.victims_rescued}/7 ***")
        self.model._registrar_accion(self.unique_id, "depositar_victima", (self.x, self.y), (self.x, self.y), 0)
        if self.model.victims_rescued >= 7:
            self.model.juego_terminado = True
            self.model.resultado = "VICTORIA"
        return True

    def _cargar_victima(self):
        celda = self.model.get_cell(self.x, self.y)
        if celda.poi == 1 and celda.poi_revelado and not self.cargando_victima:
            self.cargando_victima = True
            celda.poi = 0
            celda.poi_revelado = False
            self.tarea_asignada = 'llevar_victima'
            self.target = self._salida_cercana()
            self.log(f"Cargo victima")
            self.model._registrar_accion(self.unique_id, "recoger_victima", (self.x, self.y), (self.x, self.y), 0)
            return True
        return False

    def _revelar_poi(self, celda):
        celda.poi_revelado = True
        tipo = "VICTIMA" if celda.poi == 1 else "Falsa alarma"
        self.log(f"Revelo POI: {tipo}!")
        self.model._registrar_accion(self.unique_id, "revelar_poi", (self.x, self.y), (self.x, self.y), 0,
            {"resultado": "victima" if celda.poi == 1 else "falsa_alarma"})
        if celda.poi == 1:
            return self._cargar_victima()
        else:
            celda.poi = 0
            celda.poi_revelado = False
            self.tarea_asignada = None
            self.target = None
            return True

    def _extinguir(self, celda):
        hizo_algo = False
        # Eliminar araña primero (2 AP)
        while celda.araña and self.ap >= 2:
            celda.araña = False
            celda.huevo = True
            self.ap -= 2
            self.log(f"Elimina araña -> huevo [-2 AP]")
            self.model._registrar_accion(self.unique_id, "eliminar_araña", (self.x, self.y), (self.x, self.y), 2)
            hizo_algo = True
        # Luego huevo si sobra AP (1 AP)
        while celda.huevo and self.ap >= 1:
            celda.huevo = False
            self.ap -= 1
            self.log(f"Elimina huevo [-1 AP]")
            self.model._registrar_accion(self.unique_id, "eliminar_huevo", (self.x, self.y), (self.x, self.y), 1)
            hizo_algo = True
        return hizo_algo

    def _seguir_tarea(self):
        if not self.target:
            self.tarea_asignada = None
            return False

        if (self.x, self.y) == self.target:
            celda = self.model.get_cell(self.x, self.y)

            if self.tarea_asignada in ['investigar_poi', 'rescatar_victima']:
                if celda.poi > 0 and not celda.poi_revelado:
                    return self._revelar_poi(celda)
                if celda.poi == 1 and celda.poi_revelado:
                    return self._cargar_victima()
                self.tarea_asignada = None
                self.target = None
                return False

            elif self.tarea_asignada == 'llevar_victima':
                if self.cargando_victima:
                    return self._soltar_victima()
                self.tarea_asignada = None
                self.target = None
                return False

            elif self.tarea_asignada in ['eliminar_araña', 'eliminar_huevo']:
                if celda.araña or celda.huevo:
                    return self._extinguir(celda)
                self.tarea_asignada = None
                self.target = None
                return False

        camino = self.a_estrella(self.target)
        if not camino:
            self.tarea_asignada = None
            self.target = None
            return False

        sig_pos = self.model.get_neighbor_dir(self.x, self.y, camino[0])
        if not sig_pos:
            return False

        celda_sig = self.model.get_cell(*sig_pos)
        if not celda_sig:
            return False

        # Si hay araña, eliminarla desde afuera
        if celda_sig.araña:
            if self.ap >= 2:
                celda_sig.araña = False
                celda_sig.huevo = True
                self.ap -= 2
                self.log(f"Elimina araña en camino {sig_pos} [-2 AP]")
                self.model._registrar_accion(self.unique_id, "eliminar_araña", (self.x, self.y), sig_pos, 2)
                return True
            else:
                return False

        return self._mover_hacia(self.target)

    def _buscar_tarea_autonoma(self):
        # Buscar POI más barato
        mejor_poi = None
        mejor_costo = 9999
        for pos, celda in self.model.cells.items():
            if celda.poi > 0 and not celda.es_exterior and not celda.araña:
                costo = self.calcular_costo_ap(pos)
                if costo < mejor_costo:
                    mejor_costo = costo
                    mejor_poi = pos

        if mejor_poi:
            self.target = mejor_poi
            self.tarea_asignada = 'investigar_poi'
            return self._seguir_tarea()

        # Si no hay POI, buscar araña más barata
        mejor_araña = None
        mejor_costo = 9999
        for pos, celda in self.model.cells.items():
            if celda.araña and not celda.es_exterior:
                costo = self.calcular_costo_ap(pos)
                if costo < mejor_costo:
                    mejor_costo = costo
                    mejor_araña = pos

        if mejor_araña:
            self.target = mejor_araña
            self.tarea_asignada = 'eliminar_araña'
            return self._seguir_tarea()

        return self._explorar()

    def _mover_hacia(self, objetivo):
        camino = self.a_estrella(objetivo)
        if not camino:
            self.tarea_asignada = None
            self.target = None
            return False
        return self._mover(camino[0]) if camino else False

    def _mover(self, direccion):
        if self.ap <= 0 or not self.alive:
            return False

        celda_actual = self.model.get_cell(self.x, self.y)
        opuestas = {"N": "S", "S": "N", "E": "O", "O": "E"}

        if direccion in celda_actual.walls:
            pared = celda_actual.walls[direccion]
            if pared.enPie and not pared.is_destroyed():
                if pared.door == 1:
                    if self.ap >= 1:
                        pared.abrir_door()
                        self.ap -= 1
                        self.log(f"Abrio puerta [-1 AP]")
                        vecino = self.model.get_neighbor_dir(self.x, self.y, direccion)
                        self.model._registrar_accion(self.unique_id, "abrir_puerta", (self.x, self.y), vecino, 1)
                    else:
                        return False
                elif pared.door == 0:
                    return False

        vecino = self.model.get_neighbor_dir(self.x, self.y, direccion)
        if not vecino:
            return False

        nx, ny = vecino
        celda_destino = self.model.get_cell(nx, ny)
        if not celda_destino:
            return False

        if opuestas[direccion] in celda_destino.walls:
            pared = celda_destino.walls[opuestas[direccion]]
            if pared.enPie and not pared.is_destroyed() and pared.door == 0:
                return False

        costo = 2 if self.cargando_victima else 1
        if celda_destino.araña:
            costo += 1

        if self.ap < costo:
            return False

        pos_ant = (self.x, self.y)
        if self in celda_actual.agents:
            celda_actual.agents.remove(self)
        self.x, self.y = nx, ny
        self.ap -= costo
        celda_destino.agents.append(self)

        self.log(f"Movio {pos_ant} -> ({nx},{ny}) [-{costo} AP]")
        self.model._registrar_accion(self.unique_id, "mover", pos_ant, (nx, ny), costo)

        if celda_destino.araña:
            self.recibir_dano()
        if celda_destino.poi > 0 and not celda_destino.poi_revelado:
            self._revelar_poi(celda_destino)
        return True

    def _ir_a_salida(self):
        salida = self._salida_cercana()
        if salida:
            self.target = salida
            return self._mover_hacia(salida)
        return False

    def _salida_cercana(self):
        if not self.model.salidas:
            return None
        return min(self.model.salidas, key=lambda s: self.calcular_costo_ap(s))

    def _explorar(self):
        dirs = ['N', 'S', 'E', 'O']
        random.shuffle(dirs)
        for d in dirs:
            if self._mover(d):
                return True
        return False

    def a_estrella(self, objetivo):
        inicio = (self.x, self.y)
        if inicio == objetivo:
            return []

        frontera = [(0, 0, inicio)]
        heapq.heapify(frontera)
        vino_de = {inicio: None}
        costo_g = {inicio: 0}
        dirs = {'N': (0, -1), 'S': (0, 1), 'E': (1, 0), 'O': (-1, 0)}

        while frontera:
            _, g, pos = heapq.heappop(frontera)
            if pos == objetivo:
                return self._reconstruir(vino_de, inicio, objetivo)

            celda = self.model.get_cell(*pos)
            if not celda:
                continue

            for d, (dx, dy) in dirs.items():
                vec = (pos[0] + dx, pos[1] + dy)
                if not (0 <= vec[0] < self.model.width and 0 <= vec[1] < self.model.height):
                    continue
                celda_v = self.model.get_cell(*vec)
                if not celda_v or celda_v.es_exterior:
                    continue

                costo = self._costo_paso(celda, celda_v, d)
                if costo is None:
                    continue

                nuevo_g = g + costo
                if vec not in costo_g or nuevo_g < costo_g[vec]:
                    costo_g[vec] = nuevo_g
                    f = nuevo_g + abs(vec[0] - objetivo[0]) + abs(vec[1] - objetivo[1])
                    heapq.heappush(frontera, (f, nuevo_g, vec))
                    vino_de[vec] = (pos, d)
        return None

    def _costo_paso(self, c1, c2, d):
        op = {"N": "S", "S": "N", "E": "O", "O": "E"}
        costo = 0
        if d in c1.walls:
            p = c1.walls[d]
            if p.enPie and not p.is_destroyed():
                if p.door == 2: pass
                elif p.door == 1: costo += 1
                else: return None
        if op[d] in c2.walls:
            p = c2.walls[op[d]]
            if p.enPie and not p.is_destroyed():
                if p.door == 2: pass
                elif p.door == 1: costo += 1
                else: return None
        return costo + (2 if self.cargando_victima else 1) + (10 if c2.araña else 0)

    def _reconstruir(self, vino_de, inicio, objetivo):
        camino = []
        actual = objetivo
        while actual != inicio:
            if actual not in vino_de or vino_de[actual] is None:
                return []
            ant, d = vino_de[actual]
            camino.append(d)
            actual = ant
        camino.reverse()
        return camino

    def calcular_costo_ap(self, objetivo):
        inicio = (self.x, self.y)
        if inicio == objetivo:
            return 0

        frontera = [(0, 0, inicio)]
        heapq.heapify(frontera)
        costo_g = {inicio: 0}
        dirs = {'N': (0, -1), 'S': (0, 1), 'E': (1, 0), 'O': (-1, 0)}

        while frontera:
            _, g, pos = heapq.heappop(frontera)
            if pos == objetivo:
                return costo_g[objetivo]

            celda = self.model.get_cell(*pos)
            if not celda:
                continue

            for d, (dx, dy) in dirs.items():
                vec = (pos[0] + dx, pos[1] + dy)
                if not (0 <= vec[0] < self.model.width and 0 <= vec[1] < self.model.height):
                    continue
                celda_v = self.model.get_cell(*vec)
                if not celda_v or celda_v.es_exterior:
                    continue

                costo = self._costo_paso(celda, celda_v, d)
                if costo is None:
                    continue

                nuevo_g = costo_g[pos] + costo
                if vec not in costo_g or nuevo_g < costo_g[vec]:
                    costo_g[vec] = nuevo_g
                    f = nuevo_g + abs(vec[0] - objetivo[0]) + abs(vec[1] - objetivo[1])
                    heapq.heappush(frontera, (f, nuevo_g, vec))

        return 9999


# =============================================================================
# SISTEMA DE SUBASTAS
# =============================================================================

class AgenteSubasta:
    def __init__(self, model):
        self.model = model

    def log(self, msg):
        print(f"  [SUBASTA] {msg}")
        self.model._registrar_log(f"[SUBASTA] {msg}")

    def subastar_tareas(self):
        self.log("=== SUBASTA DE TAREAS ===")

        for a in self.model.astronautas:
            if a.alive and not a.cargando_victima:
                a.tarea_asignada = None
                a.target = None

        disponibles = [a for a in self.model.astronautas if a.alive and not a.cargando_victima]

        # Agentes que cargan victima (FORZADO)
        for a in self.model.astronautas:
            if a.cargando_victima and a.alive:
                mejor_salida = min(self.model.salidas, key=lambda s: a.calcular_costo_ap(s)) if self.model.salidas else (0, 0)
                a.tarea_asignada = 'llevar_victima'
                a.target = mejor_salida
                self.log(f"Agente {a.unique_id} -> llevar_victima [FORZADO]")

        # Victimas reveladas (MAXIMA PRIORIDAD)
        victimas = [(pos, c) for pos, c in self.model.cells.items() if c.poi == 1 and c.poi_revelado and not c.es_exterior]
        for vic_pos, _ in victimas:
            if not disponibles:
                break
            mejor = min(disponibles, key=lambda a: a.calcular_costo_ap(vic_pos))
            mejor.tarea_asignada = 'rescatar_victima'
            mejor.target = vic_pos
            disponibles.remove(mejor)
            self.log(f"Agente {mejor.unique_id} -> rescatar_victima en {vic_pos}")

        # POIs no revelados
        pois = [pos for pos, c in self.model.cells.items() if c.poi > 0 and not c.poi_revelado and not c.es_exterior and not c.araña]
        max_poi = min(3, len(disponibles), len(pois))
        for _ in range(max_poi):
            if not pois or not disponibles:
                break
            mejor_a, mejor_p = min(((a, p) for a in disponibles for p in pois), key=lambda x: x[0].calcular_costo_ap(x[1]))
            mejor_a.tarea_asignada = 'investigar_poi'
            mejor_a.target = mejor_p
            disponibles.remove(mejor_a)
            pois.remove(mejor_p)
            self.log(f"Agente {mejor_a.unique_id} -> investigar_poi en {mejor_p}")

        # Arañas (resto de agentes)
        arañas = [pos for pos, c in self.model.cells.items() if c.araña and not c.es_exterior]
        for a in disponibles[:]:
            if not arañas:
                break
            mejor_a = min(arañas, key=lambda p: a.calcular_costo_ap(p))
            a.tarea_asignada = 'eliminar_araña'
            a.target = mejor_a
            arañas.remove(mejor_a)
            self.log(f"Agente {a.unique_id} -> eliminar_araña en {mejor_a}")


# =============================================================================
# MODELO NAVE
# =============================================================================

class Nave(Model):
    def __init__(self, width=8, height=6, num_agents=2, mapa_texto=None, estrategia="inteligente"):
        super().__init__()
        self.width = width
        self.height = height
        self.damage_counter = 0
        self.victims_rescued = 0
        self.victims_lost = 0
        self.turn = 0
        self.juego_terminado = False
        self.resultado = None
        self.total_explosions = 0
        self.estrategia = estrategia

        self.grid = MultiGrid(width, height, torus=False)
        self.cells = {}
        self.astronautas = []
        self.salidas = []
        self.entradas = []

        self.historial_turnos = []
        self._acciones_turno_actual = []
        self._logs_turno_actual = []
        self._dados_turno_actual = []

        if mapa_texto:
            self.cargar_desde_texto(mapa_texto)

        self._vincular_paredes()
        self.auctioneer = AgenteSubasta(self)
        self._crear_agentes(num_agents)

    def log(self, msg):
        print(f"  {msg}")
        self._registrar_log(msg)

    def _registrar_log(self, msg):
        self._logs_turno_actual.append(msg)

    def _registrar_accion(self, aid, tipo, desde, hacia, costo, extras=None):
        self._acciones_turno_actual.append({
            "tripulacion_id": aid + 1,
            "tipo": tipo,
            "desde": {"fila": desde[1] + 1, "columna": desde[0] + 1},
            "hacia": {"fila": hacia[1] + 1, "columna": hacia[0] + 1},
            "costo_ap": costo,
            "cambios": extras or {}
        })

    def cargar_desde_texto(self, texto):
        lineas = [l.strip() for l in texto.strip().split('\n') if l.strip()]
        idx = 0
        self.log("Cargando mapa...")

        for y in range(self.height):
            if idx >= len(lineas): break
            for x, g in enumerate(lineas[idx].split()):
                if len(g) >= 4:
                    self.cells[(x, y)] = Cell(cell_id=f"{x}-{y}", wall_info=g[:4], pos=(x, y))
            idx += 1

        # POIs
        while idx < len(lineas):
            p = lineas[idx].split()
            if len(p) >= 3 and p[2].lower() in ['v', 'f']:
                f, c = int(p[0]) - 1, int(p[1]) - 1
                if (c, f) in self.cells:
                    self.cells[(c, f)].poi = 1 if p[2].lower() == 'v' else 2
                idx += 1
            else: break

        # Arañas (fuegos)
        while idx < len(lineas):
            p = lineas[idx].split()
            if len(p) == 2:
                try:
                    f, c = int(p[0]) - 1, int(p[1]) - 1
                    if (c, f) in self.cells:
                        self.cells[(c, f)].araña = True
                    idx += 1
                except: break
            else: break

        # Puertas
        while idx < len(lineas):
            p = lineas[idx].split()
            if len(p) >= 4:
                try:
                    r1, c1, r2, c2 = [int(x) - 1 for x in p[:4]]
                    self._colocar_puerta(c1, r1, c2, r2)
                    idx += 1
                except: break
            else: break

        # Entradas
        while idx < len(lineas):
            p = lineas[idx].split()
            if len(p) >= 2:
                try:
                    f, c = int(p[0]) - 1, int(p[1]) - 1
                    self.entradas.append((c, f))
                    self.salidas.append((c, f))
                    idx += 1
                except: break
            else: break

        self.log(f"Mapa: {len(self.cells)} celdas, {len(self.entradas)} entradas")

    def _colocar_puerta(self, x1, y1, x2, y2):
        if (x1, y1) not in self.cells or (x2, y2) not in self.cells: return
        dx, dy = x2 - x1, y2 - y1
        dm = {(0, -1): 'N', (0, 1): 'S', (1, 0): 'E', (-1, 0): 'O'}
        op = {'N': 'S', 'S': 'N', 'E': 'O', 'O': 'E'}
        d = dm.get((dx, dy))
        if d:
            if d in self.cells[(x1, y1)].walls: self.cells[(x1, y1)].walls[d].door = 1
            if op[d] in self.cells[(x2, y2)].walls: self.cells[(x2, y2)].walls[op[d]].door = 1

    def _vincular_paredes(self):
        op = {"N": "S", "S": "N", "E": "O", "O": "E"}
        dl = {"N": (0, -1), "S": (0, 1), "E": (1, 0), "O": (-1, 0)}
        for (x, y), c in self.cells.items():
            for d, w in c.walls.items():
                nx, ny = x + dl[d][0], y + dl[d][1]
                if (nx, ny) in self.cells and op[d] in self.cells[(nx, ny)].walls:
                    w.setParedVecina(self.cells[(nx, ny)].walls[op[d]])

    def _crear_agentes(self, n):
        for i in range(n):
            ent = self.entradas[i % len(self.entradas)] if self.entradas else (0, 0)
            a = AstronautAgent(i, self)
            a.x, a.y = ent
            if ent in self.cells:
                self.cells[ent].agents.append(a)
            self.grid.place_agent(a, ent)
            self.astronautas.append(a)
        self.log(f"Creados {n} agentes [estrategia: {self.estrategia}]")

    def get_cell(self, x, y): return self.cells.get((x, y))

    def get_neighbor_dir(self, x, y, d):
        dl = {'N': (0, -1), 'S': (0, 1), 'E': (1, 0), 'O': (-1, 0)}
        if d not in dl: return None
        nx, ny = x + dl[d][0], y + dl[d][1]
        return (nx, ny) if (nx, ny) in self.cells else None

    def es_salida(self, x, y): return (x, y) in self.salidas

    def paredEntremedio(self, x1, y1, x2, y2):
        c = self.get_cell(x1, y1)
        if not c: return False
        dm = {(0, -1): 'N', (0, 1): 'S', (1, 0): 'E', (-1, 0): 'O'}
        d = dm.get((x2 - x1, y2 - y1))
        return d in c.walls and not c.walls[d].puePasar() if d else False

    def avanzar_peligro(self):
        """Avanza el peligro (dados): puede crear huevo o evolucionar huevo->araña->explosión"""
        dr, dn = random.randint(1, 6), random.randint(1, 8)
        x, y = dn - 1, dr - 1
        self.log(f"Dados: {dr},{dn} -> ({x},{y})")

        tirada = {"fila": y + 1, "columna": x + 1}

        if (x, y) not in self.cells:
            tirada.update({"estado_anterior": "fuera", "estado_nuevo": "fuera", "cambios": {}})
            self._dados_turno_actual.append(tirada)
            return

        c = self.cells[(x, y)]
        if c.araña:
            # Araña + dado = EXPLOSION
            tirada.update({"estado_anterior": "araña", "estado_nuevo": "explosion", "cambios": self._explosion(x, y)})
        elif c.huevo:
            # Huevo -> Araña
            c.huevo = False
            c.araña = True
            tirada.update({
                "estado_anterior": "huevo",
                "estado_nuevo": "araña",
                "cambios": {
                    "huevos_removidos": [{"fila": y + 1, "columna": x + 1}],
                    "arañas_nuevas": [{"fila": y + 1, "columna": x + 1}]
                }
            })
        else:
            # Vacío -> Huevo
            c.huevo = True
            tirada.update({
                "estado_anterior": "vacio",
                "estado_nuevo": "huevo",
                "cambios": {"huevos_nuevos": [{"fila": y + 1, "columna": x + 1}]}
            })

        self._dados_turno_actual.append(tirada)
        self._flashover()
        self._verificar_victimas()

    def _explosion(self, x, y):
        self.total_explosions += 1
        self.log(f"*** EXPLOSION ({x},{y}) ***")
        cambios = {"explosiones": [{"fila": y + 1, "columna": x + 1}], "paredes_dañadas": [], "arañas_nuevas": []}
        c = self.cells[(x, y)]
        dirs = {'N': (0, -1), 'S': (0, 1), 'E': (1, 0), 'O': (-1, 0)}
        dir_nombres = {'N': 'norte', 'S': 'sur', 'E': 'este', 'O': 'oeste'}

        for a in c.agents[:]:
            if a.alive:
                a.recibir_dano()

        for d, (dx, dy) in dirs.items():
            nx, ny = x + dx, y + dy
            if d in c.walls:
                p = c.walls[d]
                if p.enPie and not p.is_destroyed():
                    if p.door == 1:
                        p.destruirWall()
                        cambios["paredes_dañadas"].append({
                            "fila": y + 1, "columna": x + 1, "direccion": dir_nombres[d], "nuevo_estado": "destruida"
                        })
                    else:
                        p.afecta_wall()
                        self.damage_counter += 1
                        estado = "destruida" if p.is_destroyed() else "dañada"
                        cambios["paredes_dañadas"].append({
                            "fila": y + 1, "columna": x + 1, "direccion": dir_nombres[d], "nuevo_estado": estado
                        })
                    continue

            if (nx, ny) in self.cells:
                v = self.cells[(nx, ny)]
                for a in v.agents[:]:
                    if a.alive:
                        a.recibir_dano()
                if not v.araña:
                    v.huevo = False
                    v.araña = True
                    cambios["arañas_nuevas"].append({"fila": ny + 1, "columna": nx + 1})

        self.log(f"Dano: {self.damage_counter}/24")
        if self.damage_counter >= 24:
            self.juego_terminado = True
            self.resultado = "DERROTA - Colapso"
        return cambios

    def _flashover(self):
        """Huevos adyacentes a arañas se convierten en arañas"""
        changed = True
        while changed:
            changed = False
            for (x, y), c in self.cells.items():
                if c.huevo:
                    for dx, dy in [(0, -1), (0, 1), (1, 0), (-1, 0)]:
                        nx, ny = x + dx, y + dy
                        if (nx, ny) in self.cells:
                            v = self.cells[(nx, ny)]
                            if v.araña and not self.paredEntremedio(x, y, nx, ny):
                                c.huevo = False
                                c.araña = True
                                changed = True
                                break

    def _verificar_victimas(self):
        for (x, y), c in self.cells.items():
            if c.araña and c.poi == 1:
                self.victims_lost += 1
                c.poi = 0
                c.poi_revelado = False
                self.log(f"*** VICTIMA PERDIDA ({x},{y})! {self.victims_lost}/4 ***")
                if self.victims_lost >= 4:
                    self.juego_terminado = True
                    self.resultado = "DERROTA - 4 victimas"

    def reponer_poi(self):
        if self.juego_terminado: return
        total = sum(1 for c in self.cells.values() if c.poi > 0 and not c.poi_revelado)
        vict = sum(1 for c in self.cells.values() if c.poi == 1 and c.poi_revelado)
        while total + vict < 3:
            x, y = random.randint(0, self.width - 1), random.randint(0, self.height - 1)
            c = self.cells.get((x, y))
            if c and not c.araña and not c.huevo and c.poi == 0:
                c.poi = 1 if random.random() < 0.67 else 2
                self.log(f"Nuevo POI en ({x},{y})")
                total += 1

    def ejecutar_turno(self):
        if self.juego_terminado: return False

        self.turn += 1
        self._acciones_turno_actual = []
        self._logs_turno_actual = []
        self._dados_turno_actual = []

        print(f"\n{'='*50}\n RONDA {self.turn}\n{'='*50}")
        print(f"Rescatadas: {self.victims_rescued}/7 | Perdidas: {self.victims_lost}/4 | Dano: {self.damage_counter}/24")

        # Reset AP
        for a in self.astronautas:
            if a.alive:
                a.reset_ap()

        # Asignar tareas
        if self.estrategia == "inteligente":
            self.auctioneer.subastar_tareas()

        # Turnos de agentes
        for a in self.astronautas:
            if a.alive and not self.juego_terminado:
                a.step()
                if not self.juego_terminado:
                    self.avanzar_peligro()

        if not self.juego_terminado:
            self.reponer_poi()

        if self.victims_rescued >= 7:
            self.juego_terminado = True
            self.resultado = "VICTORIA"

        # Guardar turno en historial
        self.historial_turnos.append({
            "numero_turno": self.turn,
            "fase_dados": {"tiradas": self._dados_turno_actual},
            "fase_accion": {"acciones": self._acciones_turno_actual},
            "estado_juego": {
                "victimas_rescatadas": self.victims_rescued,
                "victimas_perdidas": self.victims_lost,
                "falsas_alarmas": 0,
                "daño_edificio": self.damage_counter,
                "juego_terminado": self.juego_terminado,
                "resultado": self.resultado or "en_progreso",
                "mensaje": f"Turno {self.turn} completado"
            }
        })

        if self.juego_terminado:
            print(f"\n{'='*60}\n{self.resultado}\nTurnos: {self.turn}")
        return not self.juego_terminado

    def get_estado_inicial(self):
        """Genera estado inicial compatible con Unity"""
        dir_nombres = {'N': 'norte', 'S': 'sur', 'E': 'este', 'O': 'oeste'}
        
        # Puertas
        puertas = []
        puertas_vistas = set()
        for (x, y), c in self.cells.items():
            for d, w in c.walls.items():
                if w.door >= 1:  # Es puerta
                    key = tuple(sorted([(x, y, d), self._get_celda_opuesta(x, y, d)]))
                    if key not in puertas_vistas:
                        puertas_vistas.add(key)
                        puertas.append({
                            "fila": y + 1,
                            "columna": x + 1,
                            "direccion": dir_nombres[d],
                            "abierta": w.door == 2
                        })

        return {
            "tablero": {
                "fila": self.height,
                "columna": self.width,
                "celdas": [self.cells.get((x, y)).wall_info_original if (x, y) in self.cells else "0000"
                          for y in range(self.height) for x in range(self.width)]
            },
            "estado_inicial": {
                "tripulacion": [
                    {"id": a.unique_id + 1, "tipo": "jugador", "fila": a.y + 1, "columna": a.x + 1, 
                     "puntos_accion": a.ap, "cargando_victima": a.cargando_victima}
                    for a in self.astronautas
                ],
                "puntosInteres": [
                    {"id": i + 1, "fila": y + 1, "columna": x + 1, "revelado": c.poi_revelado, 
                     "tipo": "victima" if c.poi == 1 else "falsa_alarma"}
                    for i, ((x, y), c) in enumerate([(pos, cel) for pos, cel in self.cells.items() if cel.poi > 0])
                ],
                "arañas": [{"fila": y + 1, "columna": x + 1} for (x, y), c in self.cells.items() if c.araña],
                "huevos": [{"fila": y + 1, "columna": x + 1} for (x, y), c in self.cells.items() if c.huevo],
                "puertas": puertas,
                "entradas": [{"fila": y + 1, "columna": x + 1, "direccion": self._get_direccion_entrada(x, y)} 
                            for x, y in self.entradas],
                "contador_colapso_edificio": self.damage_counter
            },
            "turnos": self.historial_turnos
        }

    def _get_celda_opuesta(self, x, y, d):
        dl = {'N': (0, -1), 'S': (0, 1), 'E': (1, 0), 'O': (-1, 0)}
        op = {'N': 'S', 'S': 'N', 'E': 'O', 'O': 'E'}
        dx, dy = dl[d]
        return (x + dx, y + dy, op[d])

    def _get_direccion_entrada(self, x, y):
        """Determina la dirección de la entrada basándose en su posición en el borde"""
        if y == 0: return "norte"
        if y == self.height - 1: return "sur"
        if x == 0: return "oeste"
        if x == self.width - 1: return "este"
        return "norte"

    def exportar_json(self):
        return json.dumps(self.get_estado_inicial(), indent=2, ensure_ascii=False)


# =============================================================================
# MAPA DE EJEMPLO
# =============================================================================

MAPA_EJEMPLO = """
1100 1000 1001 1100 1001 1100 1000 1001
0100 0000 0011 0110 0011 0110 0010 0011
0100 0001 1100 1000 1000 1001 1100 1001
0110 0011 0110 0010 0010 0011 0110 0011
1100 1000 1000 1000 1001 1100 1001 1101
0110 0010 0010 0010 0011 0110 0011 0111
2 4 v
5 2 f
5 7 v
1 2
2 3
3 4
3 5
4 5
5 6
1 3 1 4
2 5 2 6
2 8 3 8
3 2 3 3
4 4 5 4
4 6 4 7
6 5 6 6
6 7 6 8
1 6
3 1
4 8
6 3
"""


# =============================================================================
# SERVIDOR HTTP SIMPLE
# =============================================================================

class SimulacionHandler(BaseHTTPRequestHandler):
    json_data = None

    def do_GET(self):
        if self.path == '/simulation_data' or self.path == '/':
            self.send_response(200)
            self.send_header('Content-Type', 'application/json; charset=utf-8')
            self.send_header('Access-Control-Allow-Origin', '*')
            self.end_headers()
            self.wfile.write(SimulacionHandler.json_data.encode('utf-8'))
        else:
            self.send_response(404)
            self.end_headers()

    def do_POST(self):
        self.do_GET()

    def log_message(self, format, *args):
        print(f"[SERVER] {args[0]}")


def iniciar_servidor(json_data, puerto=8585):
    """Inicia el servidor HTTP en segundo plano"""
    SimulacionHandler.json_data = json_data
    server = HTTPServer(('localhost', puerto), SimulacionHandler)
    print(f"\n🌐 Servidor iniciado en http://localhost:{puerto}")
    print(f"   GET /simulation_data -> JSON de simulación")
    print(f"   Presiona Ctrl+C para detener\n")
    server.serve_forever()


# =============================================================================
# EJECUCION PRINCIPAL
# =============================================================================

def ejecutar_simulacion(num_turnos=10, num_agentes=2, seed=42):
    """Ejecuta la simulación y retorna el JSON"""
    random.seed(seed)
    print("="*50 + "\n FLASH POINT: FIRE RESCUE (Unity)\n" + "="*50)
    
    model = Nave(num_agents=num_agentes, mapa_texto=MAPA_EJEMPLO, estrategia="inteligente")
    
    for _ in range(num_turnos):
        if not model.ejecutar_turno():
            break
    
    json_data = model.exportar_json()
    
    # Guardar archivo
    with open("simulacion_completa.json", "w", encoding="utf-8") as f:
        f.write(json_data)
    print(f"\n✓ JSON guardado en simulacion_completa.json")
    print(f"  Resultado: {model.resultado or 'En progreso'}")
    print(f"  Turnos: {model.turn}")
    print(f"  Rescatadas: {model.victims_rescued}/7")
    
    return json_data


if __name__ == "__main__":
    # 1. Ejecutar simulación
    json_data = ejecutar_simulacion(num_turnos=10, num_agentes=2, seed=42)
    
    # 2. Iniciar servidor (bloquea hasta Ctrl+C)
    iniciar_servidor(json_data, puerto=8585)
