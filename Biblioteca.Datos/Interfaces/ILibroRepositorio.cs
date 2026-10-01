using Biblioteca.Entidades;

namespace Biblioteca.Datos.Interfaces;

public interface ILibroRepositorio
{
    Task<List<Libro>> ListarAsync(string? filtro = null, CancellationToken cancellationToken = default);
    Task<List<Libro>> ListarDisponiblesAsync(CancellationToken cancellationToken = default);
    Task<Libro?> ObtenerPorIdAsync(int libroId, CancellationToken cancellationToken = default);
    Task<bool> ExisteISBNAsync(string isbn, int excluirLibroId = 0, CancellationToken cancellationToken = default);
    Task<bool> TienePrestamosPendientesAsync(int libroId, CancellationToken cancellationToken = default);
    Task InsertarAsync(Libro libro, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Libro libro, CancellationToken cancellationToken = default);
    Task EliminarLogicamenteAsync(int libroId, CancellationToken cancellationToken = default);
}
