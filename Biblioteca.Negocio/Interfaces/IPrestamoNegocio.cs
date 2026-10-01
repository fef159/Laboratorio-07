using Biblioteca.Entidades;

namespace Biblioteca.Negocio.Interfaces;

public interface IPrestamoNegocio
{
    Task<int> RegistrarAsync(int socioId, IEnumerable<int> libroIds, DateTime fechaPrestamo, DateTime fechaLimite, CancellationToken cancellationToken = default);
    Task<List<DetallePrestamo>> ListarPendientesAsync(CancellationToken cancellationToken = default);
    Task<ResultadoDevolucion> DevolverAsync(int prestamoId, int libroId, DateTime fechaDevolucion, CancellationToken cancellationToken = default);
    Task<List<PrestamoReporte>> ReportarAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);
}
