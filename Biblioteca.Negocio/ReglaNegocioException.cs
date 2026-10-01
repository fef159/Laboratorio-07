namespace Biblioteca.Negocio;

public sealed class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string message) : base(message) { }
}
