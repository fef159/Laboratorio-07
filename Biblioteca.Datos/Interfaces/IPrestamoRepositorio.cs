using Biblioteca.Entidades;

namespace Biblioteca.Datos.Interfaces;

public interface IPrestamoRepositorio
{
    Task<int> ContarLibrosPendientesAsync(int socioId, CancellationToken cancellationToken = default);
    Task<int> RegistrarAsync(Prestamo prestamo, CancellationToken cancellationToken = default);
    Task<List<DetallePrestamo>> ListarDetallesPendientesAsync(CancellationToken cancellationToken = default);
    Task<DetallePrestamo?> ObtenerDetallePendienteAsync(int prestamoId, int libroId, CancellationToken cancellationToken = default);
    Task RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fechaDevolucion, CancellationToken cancellationToken = default);
    Task<List<PrestamoReporte>> ReportarAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);
}
