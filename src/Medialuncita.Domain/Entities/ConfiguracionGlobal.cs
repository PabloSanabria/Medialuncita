using Medialuncita.Domain.Enums;

namespace Medialuncita.Domain.Entities;

/// <summary>
/// Configuración global de la aplicación. Se espera una única fila en la tabla
/// (Id = 1). Provee valores por defecto que cada ProductoVariante puede
/// sobreescribir puntualmente (ver *Override en ProductoVariante).
/// </summary>
public class ConfiguracionGlobal
{
    public int Id { get; set; }

    public decimal TarifaManoDeObraPorHora { get; set; }

    public EstrategiaPrecio EstrategiaPrecioDefault { get; set; } = EstrategiaPrecio.Margen;
    public decimal MargenPorcentualDefault { get; set; }
    public decimal MultiplicadorDefault { get; set; }
    public EstrategiaRedondeo EstrategiaRedondeoDefault { get; set; } = EstrategiaRedondeo.SinRedondeo;
}
