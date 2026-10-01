namespace Biblioteca.Entidades;

public sealed class ResultadoDevolucion
{
    public DateTime FechaDevolucion { get; set; }
    public int DiasRetraso { get; set; }
    public decimal Multa { get; set; }
}
