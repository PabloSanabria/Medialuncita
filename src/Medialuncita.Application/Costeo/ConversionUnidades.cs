using Medialuncita.Domain.Entities;
using Medialuncita.Domain.Enums;

namespace Medialuncita.Application.Costeo;

public static class ConversionUnidades
{
    /// <summary>
    /// Indica si es posible convertir una cantidad expresada en <paramref name="desde"/>
    /// hacia el tipo <paramref name="tipoDestino"/>, SIN hacer la conversión. Pensado para
    /// validar en el momento de carga (ej. al armar una receta) si la unidad elegida es
    /// compatible con la unidad de compra del ingrediente, antes de intentar costear.
    /// Misma regla que usa ConvertirAUnidadBase: mismo tipo siempre es válido; cruce
    /// Peso&lt;-&gt;Volumen solo es válido si hay densidad definida; cualquier otra combinación
    /// (por ejemplo, contra Unidad) nunca es válida.
    /// </summary>
    public static bool EsConversionValida(UnidadMedida desde, TipoUnidad tipoDestino, decimal? densidadGramosPorMililitro = null)
    {
        if (desde.Tipo == tipoDestino) return true;

        var esCrucePesoVolumen =
            (desde.Tipo == TipoUnidad.Peso && tipoDestino == TipoUnidad.Volumen) ||
            (desde.Tipo == TipoUnidad.Volumen && tipoDestino == TipoUnidad.Peso);

        return esCrucePesoVolumen && densidadGramosPorMililitro is > 0;
    }

    /// <summary>
    /// Convierte una cantidad expresada en <paramref name="desde"/> a la unidad base
    /// del tipo de <paramref name="hacia"/> (gramos, mililitros o unidades).
    /// Si ambas unidades son del mismo Tipo, es una conversión directa por factor.
    /// Si son de tipos distintos (Peso &lt;-&gt; Volumen), requiere densidadGramosPorMililitro.
    /// Lanza InvalidOperationException si la conversión no es posible (tipos incompatibles
    /// sin densidad, o intentar cruzar con Unidad).
    /// </summary>
    public static decimal ConvertirAUnidadBase(
        decimal cantidad,
        UnidadMedida desde,
        TipoUnidad tipoDestino,
        decimal? densidadGramosPorMililitro = null)
    {
        if (desde.Tipo == tipoDestino)
        {
            return cantidad * desde.FactorAUnidadBase;
        }

        // Cruce Peso <-> Volumen mediado por densidad (g/ml).
        if (desde.Tipo == TipoUnidad.Peso && tipoDestino == TipoUnidad.Volumen)
        {
            if (densidadGramosPorMililitro is null or 0)
            {
                throw new InvalidOperationException(
                    $"No se puede convertir '{desde.Nombre}' (Peso) a Volumen sin densidad definida.");
            }
            var gramos = cantidad * desde.FactorAUnidadBase;
            return gramos / densidadGramosPorMililitro.Value; // -> mililitros
        }

        if (desde.Tipo == TipoUnidad.Volumen && tipoDestino == TipoUnidad.Peso)
        {
            if (densidadGramosPorMililitro is null or 0)
            {
                throw new InvalidOperationException(
                    $"No se puede convertir '{desde.Nombre}' (Volumen) a Peso sin densidad definida.");
            }
            var mililitros = cantidad * desde.FactorAUnidadBase;
            return mililitros * densidadGramosPorMililitro.Value; // -> gramos
        }

        throw new InvalidOperationException(
            $"Conversión no soportada entre '{desde.Tipo}' y '{tipoDestino}'.");
    }

    /// <summary>
    /// Calcula el precio por unidad-base (g, ml o u) a partir de un precio de compra
    /// expresado en una unidad determinada. Ej: precio 2000 en "kg" (factor 1000) -> 2 por gramo.
    /// </summary>
    public static decimal PrecioPorUnidadBase(decimal precioEnUnidadCompra, UnidadMedida unidadCompra)
    {
        if (unidadCompra.FactorAUnidadBase <= 0)
        {
            throw new InvalidOperationException(
                $"La unidad '{unidadCompra.Nombre}' tiene un FactorAUnidadBase inválido.");
        }
        return precioEnUnidadCompra / unidadCompra.FactorAUnidadBase;
    }
}
