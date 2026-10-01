using Biblioteca.Entidades;

namespace Biblioteca.Datos.Interfaces;

public interface ISocioRepositorio
{
    Task<List<Socio>> ListarAsync(string? filtro = null, CancellationToken cancellationToken = default);
    Task<Socio?> ObtenerPorIdAsync(int socioId, CancellationToken cancellationToken = default);
    Task<bool> ExisteDNIAsync(string dni, int excluirSocioId = 0, CancellationToken cancellationToken = default);
    Task<bool> TienePrestamosPendientesAsync(int socioId, CancellationToken cancellationToken = default);
    Task InsertarAsync(Socio socio, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Socio socio, CancellationToken cancellationToken = default);
    Task EliminarLogicamenteAsync(int socioId, CancellationToken cancellationToken = default);
}
