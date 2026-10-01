using Biblioteca.Datos;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;

namespace Biblioteca.Negocio;

public sealed class LibroNegocio : ILibroNegocio
{
    private readonly ILibroRepositorio _repositorio;

    public LibroNegocio() : this(new LibroRepositorio()) { }
    public LibroNegocio(ILibroRepositorio repositorio) => _repositorio = repositorio;

    public Task<List<Libro>> ListarAsync(string? filtro = null, CancellationToken cancellationToken = default) =>
        _repositorio.ListarAsync(filtro, cancellationToken);

    public Task<List<Libro>> ListarDisponiblesAsync(CancellationToken cancellationToken = default) =>
        _repositorio.ListarDisponiblesAsync(cancellationToken);

    public async Task RegistrarAsync(Libro libro, CancellationToken cancellationToken = default)
    {
        await ValidarAsync(libro, cancellationToken);
        await _repositorio.InsertarAsync(libro, cancellationToken);
    }

    public async Task ActualizarAsync(Libro libro, CancellationToken cancellationToken = default)
    {
        if (libro.LibroId <= 0) throw new ReglaNegocioException("Debe seleccionar un libro para actualizar.");
        await ValidarAsync(libro, cancellationToken);
        await _repositorio.ActualizarAsync(libro, cancellationToken);
    }

    public async Task EliminarAsync(int libroId, CancellationToken cancellationToken = default)
    {
        if (libroId <= 0) throw new ReglaNegocioException("Debe seleccionar un libro.");
        if (await _repositorio.TienePrestamosPendientesAsync(libroId, cancellationToken))
            throw new ReglaNegocioException("No se puede dar de baja un libro con préstamos pendientes.");
        await _repositorio.EliminarLogicamenteAsync(libroId, cancellationToken);
    }

    private async Task ValidarAsync(Libro libro, CancellationToken cancellationToken)
    {
        libro.Titulo = libro.Titulo?.Trim() ?? string.Empty;
        libro.ISBN = libro.ISBN?.Trim().Replace("-", string.Empty) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(libro.Titulo)) throw new ReglaNegocioException("El título es obligatorio.");
        if (libro.Titulo.Length > 160) throw new ReglaNegocioException("El título no puede exceder 160 caracteres.");
        if (libro.ISBN.Length is not (10 or 13) || !libro.ISBN.All(char.IsDigit))
            throw new ReglaNegocioException("El ISBN debe contener 10 o 13 dígitos.");
        if (libro.AutorId <= 0) throw new ReglaNegocioException("Debe seleccionar un autor.");
        if (libro.Ejemplares < 0) throw new ReglaNegocioException("La cantidad de ejemplares no puede ser negativa.");
        if (await _repositorio.ExisteISBNAsync(libro.ISBN, libro.LibroId, cancellationToken))
            throw new ReglaNegocioException($"Ya existe un libro con el ISBN {libro.ISBN}.");
    }
}
