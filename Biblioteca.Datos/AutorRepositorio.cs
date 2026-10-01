using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public sealed class AutorRepositorio : IAutorRepositorio
{
    public async Task<List<Autor>> ListarActivosAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT AutorId, Nombre, Nacionalidad, Activo FROM Autores WHERE Activo = 1 ORDER BY Nombre";
        var autores = new List<Autor>();

        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            autores.Add(new Autor
            {
                AutorId = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Nacionalidad = reader.GetString(2),
                Activo = reader.GetBoolean(3)
            });
        }

        return autores;
    }
}
