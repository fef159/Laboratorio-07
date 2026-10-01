using System.Data;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public sealed class LibroRepositorio : ILibroRepositorio
{
    private const string SelectSql = """
        SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, a.Nombre, l.Ejemplares, l.Activo
        FROM Libros l
        INNER JOIN Autores a ON a.AutorId = l.AutorId
        """;

    public Task<List<Libro>> ListarAsync(string? filtro = null, CancellationToken cancellationToken = default) =>
        ListarInternoAsync(filtro, soloDisponibles: false, cancellationToken);

    public Task<List<Libro>> ListarDisponiblesAsync(CancellationToken cancellationToken = default) =>
        ListarInternoAsync(null, soloDisponibles: true, cancellationToken);

    private static async Task<List<Libro>> ListarInternoAsync(string? filtro, bool soloDisponibles, CancellationToken cancellationToken)
    {
        string sql = SelectSql + " WHERE l.Activo = 1 AND a.Activo = 1";
        if (soloDisponibles) sql += " AND l.Ejemplares > 0";
        if (!string.IsNullOrWhiteSpace(filtro)) sql += " AND (l.Titulo LIKE @Filtro OR a.Nombre LIKE @Filtro)";
        sql += " ORDER BY l.Titulo";

        var libros = new List<Libro>();
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        if (!string.IsNullOrWhiteSpace(filtro))
            command.Parameters.Add("@Filtro", SqlDbType.NVarChar, 160).Value = $"%{filtro.Trim()}%";

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) libros.Add(Map(reader));
        return libros;
    }

    public async Task<Libro?> ObtenerPorIdAsync(int libroId, CancellationToken cancellationToken = default)
    {
        const string sql = SelectSql + " WHERE l.LibroId = @LibroId";
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId;
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<bool> ExisteISBNAsync(string isbn, int excluirLibroId = 0, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM Libros WHERE ISBN = @ISBN AND LibroId <> @LibroId";
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@ISBN", SqlDbType.VarChar, 20).Value = isbn;
        command.Parameters.Add("@LibroId", SqlDbType.Int).Value = excluirLibroId;
        await connection.OpenAsync(cancellationToken);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }

    public async Task<bool> TienePrestamosPendientesAsync(int libroId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM DetallePrestamo WHERE LibroId = @LibroId AND FechaDevolucion IS NULL";
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId;
        await connection.OpenAsync(cancellationToken);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }

    public async Task InsertarAsync(Libro libro, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares) VALUES (@Titulo, @ISBN, @AutorId, @Ejemplares)";
        await EjecutarEscrituraAsync(sql, libro, cancellationToken);
    }

    public async Task ActualizarAsync(Libro libro, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE Libros SET Titulo=@Titulo, ISBN=@ISBN, AutorId=@AutorId, Ejemplares=@Ejemplares WHERE LibroId=@LibroId AND Activo=1";
        await EjecutarEscrituraAsync(sql, libro, cancellationToken);
    }

    public async Task EliminarLogicamenteAsync(int libroId, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE Libros SET Activo = 0 WHERE LibroId = @LibroId";
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId;
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EjecutarEscrituraAsync(string sql, Libro libro, CancellationToken cancellationToken)
    {
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Titulo", SqlDbType.NVarChar, 160).Value = libro.Titulo;
        command.Parameters.Add("@ISBN", SqlDbType.VarChar, 20).Value = libro.ISBN;
        command.Parameters.Add("@AutorId", SqlDbType.Int).Value = libro.AutorId;
        command.Parameters.Add("@Ejemplares", SqlDbType.Int).Value = libro.Ejemplares;
        command.Parameters.Add("@LibroId", SqlDbType.Int).Value = libro.LibroId;
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static Libro Map(SqlDataReader reader) => new()
    {
        LibroId = reader.GetInt32(0),
        Titulo = reader.GetString(1),
        ISBN = reader.GetString(2),
        AutorId = reader.GetInt32(3),
        AutorNombre = reader.GetString(4),
        Ejemplares = reader.GetInt32(5),
        Activo = reader.GetBoolean(6)
    };
}
