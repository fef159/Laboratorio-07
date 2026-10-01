using Biblioteca.Entidades;

namespace Biblioteca.Negocio.Interfaces;

public interface IAutorNegocio
{
    Task<List<Autor>> ListarActivosAsync(CancellationToken cancellationToken = default);
}
