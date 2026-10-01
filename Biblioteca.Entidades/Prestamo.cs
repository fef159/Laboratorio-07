namespace Biblioteca.Entidades;

public sealed class Prestamo
{
    public int PrestamoId { get; set; }
    public int SocioId { get; set; }
    public string SocioNombre { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaLimite { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public List<DetallePrestamo> Detalles { get; set; } = [];
}
