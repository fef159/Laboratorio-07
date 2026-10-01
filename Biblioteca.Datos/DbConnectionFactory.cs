using System.Configuration;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

internal static class DbConnectionFactory
{
    public static SqlConnection Create()
    {
        string? connectionString = ConfigurationManager.ConnectionStrings["BibliotecaDB"]?.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("No se encontró la cadena de conexión 'BibliotecaDB'.");

        return new SqlConnection(connectionString);
    }
}
