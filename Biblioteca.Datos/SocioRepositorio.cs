using System.Data;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public sealed class SocioRepositorio : ISocioRepositorio
{
    public async Task<List<Socio>> ListarAsync(string? filtro = null, CancellationToken cancellationToken = default)
    {
        string sql = "SELECT SocioId, DNI, Nombre, Email, Activo FROM Socios WHERE Activo = 1";
        if (!string.IsNullOrWhiteSpace(filtro)) sql += " AND (Nombre LIKE @Filtro OR DNI LIKE @Filtro)";
        sql += " ORDER BY Nombre";

        var socios = new List<Socio>();
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        if (!string.IsNullOrWhiteSpace(filtro))
            command.Parameters.Add("@Filtro", SqlDbType.NVarChar, 140).Value = $"%{filtro.Trim()}%";
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) socios.Add(Map(reader));
        return socios;
    }

    public async Task<Socio?> ObtenerPorIdAsync(int socioId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT SocioId, DNI, Nombre, Email, Activo FROM Socios WHERE SocioId = @SocioId";
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@SocioId", SqlDbType.Int).Value = socioId;
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<bool> ExisteDNIAsync(string dni, int excluirSocioId = 0, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM Socios WHERE DNI = @DNI AND SocioId <> @SocioId";
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@DNI", SqlDbType.VarChar, 12).Value = dni;
        command.Parameters.Add("@SocioId", SqlDbType.Int).Value = excluirSocioId;
        await connection.OpenAsync(cancellationToken);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }

    public async Task<bool> TienePrestamosPendientesAsync(int socioId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM Prestamos WHERE SocioId = @SocioId AND Estado = 'Pendiente'";
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@SocioId", SqlDbType.Int).Value = socioId;
        await connection.OpenAsync(cancellationToken);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }

    public Task InsertarAsync(Socio socio, CancellationToken cancellationToken = default) =>
        EjecutarEscrituraAsync("INSERT INTO Socios (DNI, Nombre, Email) VALUES (@DNI, @Nombre, @Email)", socio, cancellationToken);

    public Task ActualizarAsync(Socio socio, CancellationToken cancellationToken = default) =>
        EjecutarEscrituraAsync("UPDATE Socios SET DNI=@DNI, Nombre=@Nombre, Email=@Email WHERE SocioId=@SocioId AND Activo=1", socio, cancellationToken);

    public async Task EliminarLogicamenteAsync(int socioId, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE Socios SET Activo = 0 WHERE SocioId = @SocioId";
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@SocioId", SqlDbType.Int).Value = socioId;
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EjecutarEscrituraAsync(string sql, Socio socio, CancellationToken cancellationToken)
    {
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@DNI", SqlDbType.VarChar, 12).Value = socio.DNI;
        command.Parameters.Add("@Nombre", SqlDbType.NVarChar, 120).Value = socio.Nombre;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 160).Value = socio.Email;
        command.Parameters.Add("@SocioId", SqlDbType.Int).Value = socio.SocioId;
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static Socio Map(SqlDataReader reader) => new()
    {
        SocioId = reader.GetInt32(0),
        DNI = reader.GetString(1),
        Nombre = reader.GetString(2),
        Email = reader.GetString(3),
        Activo = reader.GetBoolean(4)
    };
}
