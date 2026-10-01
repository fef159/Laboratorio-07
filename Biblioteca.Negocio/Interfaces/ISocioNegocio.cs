using Biblioteca.Entidades;

namespace Biblioteca.Negocio.Interfaces;

public interface ISocioNegocio
{
    Task<List<Socio>> ListarAsync(string? filtro = null, CancellationToken cancellationToken = default);
    Task RegistrarAsync(Socio socio, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Socio socio, CancellationToken cancellationToken = default);
    Task EliminarAsync(int socioId, CancellationToken cancellationToken = default);
}
