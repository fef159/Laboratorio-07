namespace Biblioteca.Entidades;

public sealed class PrestamoReporte
{
    public int PrestamoId { get; set; }
    public string Socio { get; set; } = string.Empty;
    public string Libro { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaLimite { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public string Estado { get; set; } = string.Empty;
}
