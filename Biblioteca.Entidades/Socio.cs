namespace Biblioteca.Entidades;

public sealed class Socio
{
    public int SocioId { get; set; }
    public string DNI { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public override string ToString() => $"{Nombre} - {DNI}";
}
