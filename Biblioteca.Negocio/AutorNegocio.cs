using Biblioteca.Datos;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;

namespace Biblioteca.Negocio;

public sealed class AutorNegocio : IAutorNegocio
{
    private readonly IAutorRepositorio _repositorio;

    public AutorNegocio() : this(new AutorRepositorio()) { }
    public AutorNegocio(IAutorRepositorio repositorio) => _repositorio = repositorio;

    public Task<List<Autor>> ListarActivosAsync(CancellationToken cancellationToken = default) =>
        _repositorio.ListarActivosAsync(cancellationToken);
}
