using UnityEngine;
using System;

// ============================================================================
// CLASES DE DATOS JSON (Entidades)
// ============================================================================

[System.Serializable]
public class EscenarioData
{
    public TableroConfig tablero;
    public EstadoInicial estado_inicial;
    public TurnoData[] turnos;
}

[System.Serializable]
public class TableroConfig
{
    public int fila;
    public int columna;
    public string[] celdas;
}

[System.Serializable]
public class EstadoInicial
{
    public TripulacionData[] tripulacion;
    public PuntoInteresData[] puntosInteres;
    public AranaData[] arañas;
    public HuevoData[] huevos;
    public PuertaData[] puertas;
    public EntradaData[] entradas;
    public int contador_colapso_edificio;
}

[System.Serializable]
public class TripulacionData
{
    public int id;
    public string tipo;
    public int fila;
    public int columna;
    public int puntos_accion;
    public bool cargando_victima;
}

[System.Serializable]
public class PuntoInteresData
{
    public int id;
    public int fila;
    public int columna;
    public bool revelado;
    public string tipo;
}

[System.Serializable]
public class AranaData
{
    public int fila;
    public int columna;
}

[System.Serializable]
public class HuevoData
{
    public int fila;
    public int columna;
}

[System.Serializable]
public class PuertaData
{
    public int fila;
    public int columna;
    public string direccion;
    public bool abierta;
}

[System.Serializable]
public class EntradaData
{
    public int fila;
    public int columna;
    public string direccion;
}

[System.Serializable]
public class TurnoData
{
    public int numero_turno;
    public SecuenciaData[] secuencia;  // Nueva estructura intercalada
    public EstadoJuegoData estado_juego;
    
    // DEPRECATED: Mantenidos por compatibilidad con JSONs antiguos
    public FaseDadosData fase_dados;
    public FaseAccionData fase_accion;
    public CambiosMapaData cambios_mapa;
}

[System.Serializable]
public class SecuenciaData
{
    public string tipo;  // "acciones_jugador" o "tirada_dado"
    public int jugador_id;  // Para tipo "acciones_jugador"
    public AccionData[] acciones;  // Para tipo "acciones_jugador"
    public TiradaDadoData tirada;  // Para tipo "tirada_dado"
}

[System.Serializable]
public class FaseDadosData
{
    public TiradaDadoData[] tiradas;
}

[System.Serializable]
public class TiradaDadoData
{
    public int fila;
    public int columna;
    public string estado_anterior;
    public string estado_nuevo;
    public CambiosMapaData cambios;  // Cambios causados por esta tirada (ej: explosiones dañan paredes)
}

[System.Serializable]
public class FaseAccionData
{
    public AccionData[] acciones;
}

[System.Serializable]
public class AccionData
{
    public int tripulacion_id;
    public string tipo;
    public PosicionData desde;
    public PosicionData hacia;
    public int poi_id;
    public string resultado;
    public int costo_ap;
    public string direccion;  // Para acciones de puertas y paredes
    public CambiosMapaData cambios;  // Cambios específicos de esta acción
}

[System.Serializable]
public class PosicionData
{
    public int fila;
    public int columna;
}

[System.Serializable]
public class CambiosMapaData
{
    public string tipo_evento;  // "explosion" si la araña explota (pero permanece)
    public PosicionData[] huevos_nuevos;
    public PosicionData[] huevos_removidos;
    public PosicionData[] arañas_nuevas;
    public PosicionData[] arañas_removidas;
    public ExplosionData[] explosiones;
    public ParedDanadaData[] paredes_dañadas;
    public ParedDestruidaData[] paredes_destruidas;
    public PuertaAbiertaData[] puertas_abiertas;
    public PoiReveladoData[] pois_revelados;
}

[System.Serializable]
public class ExplosionData
{
    public int fila;
    public int columna;
}

[System.Serializable]
public class ParedDanadaData
{
    public int fila;
    public int columna;
    public string direccion;
    public string nuevo_estado;
    public int nivel_dano;  // Nivel de daño (1, 2, etc.)
}

[System.Serializable]
public class ParedDestruidaData
{
    public int fila;
    public int columna;
    public string direccion;
}

[System.Serializable]
public class PuertaAbiertaData
{
    public int fila;
    public int columna;
    public string direccion;
}

[System.Serializable]
public class PoiReveladoData
{
    public int id;           // El JSON usa "id" no "poi_id"
    public string tipo;      // El JSON usa "tipo" no "tipo_revelado"
    
    // Propiedades compatibles con código anterior
    public int poi_id => id;
    public string tipo_revelado => tipo;
}

[System.Serializable]
public class EstadoJuegoData
{
    public int victimas_rescatadas;
    public int victimas_perdidas;
    public int falsas_alarmas;
    public int daño_edificio;
    public bool juego_terminado;
    public string resultado;
    public string mensaje;
}

// ============================================================================
// HELPER DE COORDENADAS
// ============================================================================

public static class CoordenadasHelper
{
    public const float TAMANO_CELDA = 3f;
    public const int TOTAL_COLUMNAS = 8;
    
    public static Vector3 JSONaPosicionUnity(int fila, int columna)
    {
        // Invertir X para que columna 1 esté a la derecha y columna 8 a la izquierda
        // Esto corrige la inversión horizontal del tablero
        float x = (TOTAL_COLUMNAS - columna) * TAMANO_CELDA;
        float z = (fila - 1) * TAMANO_CELDA;
        return new Vector3(x, 0, z);
    }
    
    public static Quaternion ObtenerRotacionDireccion(string direccion)
    {
        switch(direccion.ToLower())
        {
            case "norte": return Quaternion.Euler(0, 0, 0);
            case "sur": return Quaternion.Euler(0, 180, 0);
            case "este": return Quaternion.Euler(0, 270, 0);   // Invertido por espejo X
            case "oeste": return Quaternion.Euler(0, 90, 0);   // Invertido por espejo X
            default: return Quaternion.identity;
        }
    }
    
    public static Vector3 ObtenerOffsetPared(string direccion)
    {
        float mitad = TAMANO_CELDA / 2f;
        switch(direccion.ToLower())
        {
            case "norte": return new Vector3(0, 0, -mitad);  // Hacia arriba en grid (menor Z)
            case "sur": return new Vector3(0, 0, mitad);     // Hacia abajo en grid (mayor Z)
            case "este": return new Vector3(-mitad, 0, 0);   // Invertido: este ahora es menor X
            case "oeste": return new Vector3(mitad, 0, 0);   // Invertido: oeste ahora es mayor X
            default: return Vector3.zero;
        }
    }
    
    public static string GenerarKey(int fila, int columna, string direccion)
    {
        return $"{fila},{columna},{direccion.ToLower()}";
    }
}
