using Biblioteca.Entidades;

namespace Biblioteca.Negocio.Interfaces;

public interface ILibroNegocio
{
    Task<List<Libro>> ListarAsync(string? filtro = null, CancellationToken cancellationToken = default);
    Task<List<Libro>> ListarDisponiblesAsync(CancellationToken cancellationToken = default);
    Task RegistrarAsync(Libro libro, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Libro libro, CancellationToken cancellationToken = default);
    Task EliminarAsync(int libroId, CancellationToken cancellationToken = default);
}
