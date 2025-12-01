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
    public FaseDadosData fase_dados;
    public FaseAccionData fase_accion;
    public CambiosMapaData cambios_mapa;
    public EstadoJuegoData estado_juego;
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
    public PosicionData[] huevos_nuevos;
    public PosicionData[] huevos_removidos;
    public PosicionData[] arañas_nuevas;
    public PosicionData[] arañas_removidas;
    public ExplosionData[] explosiones;
    public ParedDanadaData[] paredes_dañadas;
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
    public int poi_id;
    public string tipo_revelado;
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
    
    public static Vector3 JSONaPosicionUnity(int fila, int columna)
    {
        float x = (columna - 1) * TAMANO_CELDA;
        float z = (fila - 1) * TAMANO_CELDA;
        return new Vector3(x, 0, z);
    }
    
    public static Quaternion ObtenerRotacionDireccion(string direccion)
    {
        switch(direccion.ToLower())
        {
            case "norte": return Quaternion.Euler(0, 0, 0);
            case "sur": return Quaternion.Euler(0, 180, 0);
            case "este": return Quaternion.Euler(0, 90, 0);
            case "oeste": return Quaternion.Euler(0, 270, 0);
            default: return Quaternion.identity;
        }
    }
    
    public static Vector3 ObtenerOffsetPared(string direccion)
    {
        float mitad = TAMANO_CELDA / 2f;
        switch(direccion.ToLower())
        {
            case "norte": return new Vector3(0, 0, -mitad);  // Hacia arriba en grid (menor Z en vista desde arriba)
            case "sur": return new Vector3(0, 0, mitad);     // Hacia abajo en grid (mayor Z en vista desde arriba)
            case "este": return new Vector3(mitad, 0, 0);    // Hacia derecha (mayor X)
            case "oeste": return new Vector3(-mitad, 0, 0);  // Hacia izquierda (menor X)
            default: return Vector3.zero;
        }
    }
    
    public static string GenerarKey(int fila, int columna, string direccion)
    {
        return $"{fila},{columna},{direccion.ToLower()}";
    }
}
