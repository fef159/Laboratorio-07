namespace Biblioteca.Entidades;

public sealed class DetallePrestamo
{
    public int PrestamoId { get; set; }
    public int LibroId { get; set; }
    public string LibroTitulo { get; set; } = string.Empty;
    public string SocioNombre { get; set; } = string.Empty;
    public DateTime FechaLimite { get; set; }
    public DateTime? FechaDevolucion { get; set; }
}
