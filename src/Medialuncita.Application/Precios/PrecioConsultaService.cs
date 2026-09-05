using Medialuncita.Application.Abstractions;
using Medialuncita.Domain.Entities;

namespace Medialuncita.Application.Precios;

public interface IPrecioConsultaService
{
    /// <summary>Precio vigente (último registro por fecha), expresado en la UnidadCompra
    /// del ingrediente. Lanza si el ingrediente no tiene ningún precio cargado todavía.</summary>
    Task<decimal> GetPrecioVigenteIngredienteAsync(int ingredienteId, CancellationToken ct = default);

    Task<decimal> GetPrecioVigenteMaterialAsync(int materialId, CancellationToken ct = default);

    Task<List<HistorialPrecioIngrediente>> GetHistorialIngredienteAsync(int ingredienteId, DateTime? desde = null, DateTime? hasta = null, CancellationToken ct = default);
    Task<List<HistorialPrecioMaterial>> GetHistorialMaterialAsync(int materialId, DateTime? desde = null, DateTime? hasta = null, CancellationToken ct = default);

    /// <summary>Carga un precio nuevo. Es el ÚNICO camino para modificar el precio de un
    /// ingrediente/material: nunca se actualiza un campo suelto, siempre se inserta un
    /// registro nuevo de historial. El precio se expresa en la UnidadCompra del ingrediente/material.</summary>
    Task RegistrarPrecioIngredienteAsync(int ingredienteId, decimal precio, DateTime fecha, CancellationToken ct = default);
    Task RegistrarPrecioMaterialAsync(int materialId, decimal precio, DateTime fecha, CancellationToken ct = default);
}

/// <summary>
/// Resuelve precio vigente = último registro de historial por fecha, y expone consulta
/// de precios históricos. No calcula costos: eso es responsabilidad exclusiva de CosteoService.
/// </summary>
public class PrecioConsultaService : IPrecioConsultaService
{
    private readonly IIngredienteRepository _ingredientes;
    private readonly IMaterialRepository _materiales;
    private readonly IUnitOfWork _uow;

    public PrecioConsultaService(IIngredienteRepository ingredientes, IMaterialRepository materiales, IUnitOfWork uow)
    {
        _ingredientes = ingredientes;
        _materiales = materiales;
        _uow = uow;
    }

    public async Task<decimal> GetPrecioVigenteIngredienteAsync(int ingredienteId, CancellationToken ct = default)
    {
        var historial = await _ingredientes.GetPrecioVigenteAsync(ingredienteId, ct: ct)
            ?? throw new InvalidOperationException($"El ingrediente {ingredienteId} no tiene ningún precio cargado.");
        return historial.Precio;
    }

    public async Task<decimal> GetPrecioVigenteMaterialAsync(int materialId, CancellationToken ct = default)
    {
        var historial = await _materiales.GetPrecioVigenteAsync(materialId, ct: ct)
            ?? throw new InvalidOperationException($"El material {materialId} no tiene ningún precio cargado.");
        return historial.Precio;
    }

    public Task<List<HistorialPrecioIngrediente>> GetHistorialIngredienteAsync(int ingredienteId, DateTime? desde = null, DateTime? hasta = null, CancellationToken ct = default)
        => _ingredientes.GetHistorialAsync(ingredienteId, desde, hasta, ct);

    public Task<List<HistorialPrecioMaterial>> GetHistorialMaterialAsync(int materialId, DateTime? desde = null, DateTime? hasta = null, CancellationToken ct = default)
        => _materiales.GetHistorialAsync(materialId, desde, hasta, ct);

    public async Task RegistrarPrecioIngredienteAsync(int ingredienteId, decimal precio, DateTime fecha, CancellationToken ct = default)
    {
        if (precio <= 0) throw new ArgumentOutOfRangeException(nameof(precio), "El precio debe ser mayor a cero.");

        await _ingredientes.AgregarPrecioAsync(new HistorialPrecioIngrediente
        {
            IngredienteId = ingredienteId,
            Precio = precio,
            Fecha = fecha
        }, ct);

        await _uow.SaveChangesAsync(ct);
    }

    public async Task RegistrarPrecioMaterialAsync(int materialId, decimal precio, DateTime fecha, CancellationToken ct = default)
    {
        if (precio <= 0) throw new ArgumentOutOfRangeException(nameof(precio), "El precio debe ser mayor a cero.");

        await _materiales.AgregarPrecioAsync(new HistorialPrecioMaterial
        {
            MaterialId = materialId,
            Precio = precio,
            Fecha = fecha
        }, ct);

        await _uow.SaveChangesAsync(ct);
    }
}
