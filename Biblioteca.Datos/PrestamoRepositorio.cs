using System.Data;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public sealed class PrestamoRepositorio : IPrestamoRepositorio
{
    public async Task<int> ContarLibrosPendientesAsync(int socioId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM DetallePrestamo d
            INNER JOIN Prestamos p ON p.PrestamoId = d.PrestamoId
            WHERE p.SocioId = @SocioId AND d.FechaDevolucion IS NULL
            """;
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@SocioId", SqlDbType.Int).Value = socioId;
        await connection.OpenAsync(cancellationToken);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int> RegistrarAsync(Prestamo prestamo, CancellationToken cancellationToken = default)
    {
        await using var connection = DbConnectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            const string insertPrestamo = """
                INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
                OUTPUT INSERTED.PrestamoId
                VALUES (@SocioId, @FechaPrestamo, @FechaLimite, 'Pendiente')
                """;
            await using var headerCommand = new SqlCommand(insertPrestamo, connection, transaction);
            headerCommand.Parameters.Add("@SocioId", SqlDbType.Int).Value = prestamo.SocioId;
            headerCommand.Parameters.Add("@FechaPrestamo", SqlDbType.Date).Value = prestamo.FechaPrestamo.Date;
            headerCommand.Parameters.Add("@FechaLimite", SqlDbType.Date).Value = prestamo.FechaLimite.Date;
            int prestamoId = Convert.ToInt32(await headerCommand.ExecuteScalarAsync(cancellationToken));

            foreach (DetallePrestamo detalle in prestamo.Detalles)
            {
                const string updateStock = """
                    UPDATE Libros WITH (UPDLOCK, ROWLOCK)
                    SET Ejemplares = Ejemplares - 1
                    WHERE LibroId = @LibroId AND Activo = 1 AND Ejemplares > 0
                    """;
                await using var stockCommand = new SqlCommand(updateStock, connection, transaction);
                stockCommand.Parameters.Add("@LibroId", SqlDbType.Int).Value = detalle.LibroId;
                if (await stockCommand.ExecuteNonQueryAsync(cancellationToken) != 1)
                    throw new InvalidOperationException("Un libro seleccionado ya no tiene ejemplares disponibles.");

                const string insertDetalle = "INSERT INTO DetallePrestamo (PrestamoId, LibroId) VALUES (@PrestamoId, @LibroId)";
                await using var detailCommand = new SqlCommand(insertDetalle, connection, transaction);
                detailCommand.Parameters.Add("@PrestamoId", SqlDbType.Int).Value = prestamoId;
                detailCommand.Parameters.Add("@LibroId", SqlDbType.Int).Value = detalle.LibroId;
                await detailCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return prestamoId;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<List<DetallePrestamo>> ListarDetallesPendientesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT d.PrestamoId, d.LibroId, l.Titulo, s.Nombre, p.FechaLimite, d.FechaDevolucion
            FROM DetallePrestamo d
            INNER JOIN Prestamos p ON p.PrestamoId = d.PrestamoId
            INNER JOIN Libros l ON l.LibroId = d.LibroId
            INNER JOIN Socios s ON s.SocioId = p.SocioId
            WHERE d.FechaDevolucion IS NULL
            ORDER BY p.FechaLimite, s.Nombre, l.Titulo
            """;
        return await ConsultarDetallesAsync(sql, null, null, cancellationToken);
    }

    public async Task<DetallePrestamo?> ObtenerDetallePendienteAsync(int prestamoId, int libroId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT d.PrestamoId, d.LibroId, l.Titulo, s.Nombre, p.FechaLimite, d.FechaDevolucion
            FROM DetallePrestamo d
            INNER JOIN Prestamos p ON p.PrestamoId = d.PrestamoId
            INNER JOIN Libros l ON l.LibroId = d.LibroId
            INNER JOIN Socios s ON s.SocioId = p.SocioId
            WHERE d.PrestamoId = @PrestamoId AND d.LibroId = @LibroId AND d.FechaDevolucion IS NULL
            """;
        return (await ConsultarDetallesAsync(sql, prestamoId, libroId, cancellationToken)).FirstOrDefault();
    }

    public async Task RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fechaDevolucion, CancellationToken cancellationToken = default)
    {
        await using var connection = DbConnectionFactory.Create();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            const string updateDetalle = """
                UPDATE DetallePrestamo SET FechaDevolucion = @FechaDevolucion
                WHERE PrestamoId = @PrestamoId AND LibroId = @LibroId AND FechaDevolucion IS NULL
                """;
            await using var detailCommand = new SqlCommand(updateDetalle, connection, transaction);
            detailCommand.Parameters.Add("@FechaDevolucion", SqlDbType.Date).Value = fechaDevolucion.Date;
            detailCommand.Parameters.Add("@PrestamoId", SqlDbType.Int).Value = prestamoId;
            detailCommand.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId;
            if (await detailCommand.ExecuteNonQueryAsync(cancellationToken) != 1)
                throw new InvalidOperationException("El libro ya fue devuelto o el detalle no existe.");

            await using var stockCommand = new SqlCommand("UPDATE Libros SET Ejemplares = Ejemplares + 1 WHERE LibroId = @LibroId", connection, transaction);
            stockCommand.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId;
            await stockCommand.ExecuteNonQueryAsync(cancellationToken);

            const string updateEstado = """
                UPDATE Prestamos SET Estado = 'Devuelto'
                WHERE PrestamoId = @PrestamoId
                  AND NOT EXISTS (SELECT 1 FROM DetallePrestamo WHERE PrestamoId = @PrestamoId AND FechaDevolucion IS NULL)
                """;
            await using var stateCommand = new SqlCommand(updateEstado, connection, transaction);
            stateCommand.Parameters.Add("@PrestamoId", SqlDbType.Int).Value = prestamoId;
            await stateCommand.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<List<PrestamoReporte>> ReportarAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT p.PrestamoId, s.Nombre, l.Titulo, p.FechaPrestamo, p.FechaLimite, d.FechaDevolucion, p.Estado
            FROM Prestamos p
            INNER JOIN DetallePrestamo d ON d.PrestamoId = p.PrestamoId
            INNER JOIN Libros l ON l.LibroId = d.LibroId
            INNER JOIN Socios s ON s.SocioId = p.SocioId
            WHERE p.FechaPrestamo BETWEEN @Desde AND @Hasta
            ORDER BY p.FechaPrestamo DESC, p.PrestamoId, l.Titulo
            """;
        var rows = new List<PrestamoReporte>();
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Desde", SqlDbType.Date).Value = desde.Date;
        command.Parameters.Add("@Hasta", SqlDbType.Date).Value = hasta.Date;
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new PrestamoReporte
            {
                PrestamoId = reader.GetInt32(0), Socio = reader.GetString(1), Libro = reader.GetString(2),
                FechaPrestamo = reader.GetDateTime(3), FechaLimite = reader.GetDateTime(4),
                FechaDevolucion = reader.IsDBNull(5) ? null : reader.GetDateTime(5), Estado = reader.GetString(6)
            });
        }
        return rows;
    }

    private static async Task<List<DetallePrestamo>> ConsultarDetallesAsync(string sql, int? prestamoId, int? libroId, CancellationToken cancellationToken)
    {
        var rows = new List<DetallePrestamo>();
        await using var connection = DbConnectionFactory.Create();
        await using var command = new SqlCommand(sql, connection);
        if (prestamoId.HasValue) command.Parameters.Add("@PrestamoId", SqlDbType.Int).Value = prestamoId.Value;
        if (libroId.HasValue) command.Parameters.Add("@LibroId", SqlDbType.Int).Value = libroId.Value;
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new DetallePrestamo
            {
                PrestamoId = reader.GetInt32(0), LibroId = reader.GetInt32(1), LibroTitulo = reader.GetString(2),
                SocioNombre = reader.GetString(3), FechaLimite = reader.GetDateTime(4),
                FechaDevolucion = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
            });
        }
        return rows;
    }
}
