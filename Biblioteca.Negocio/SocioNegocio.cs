using System.Net.Mail;
using Biblioteca.Datos;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;

namespace Biblioteca.Negocio;

public sealed class SocioNegocio : ISocioNegocio
{
    private readonly ISocioRepositorio _repositorio;

    public SocioNegocio() : this(new SocioRepositorio()) { }
    public SocioNegocio(ISocioRepositorio repositorio) => _repositorio = repositorio;

    public Task<List<Socio>> ListarAsync(string? filtro = null, CancellationToken cancellationToken = default) =>
        _repositorio.ListarAsync(filtro, cancellationToken);

    public async Task RegistrarAsync(Socio socio, CancellationToken cancellationToken = default)
    {
        await ValidarAsync(socio, cancellationToken);
        await _repositorio.InsertarAsync(socio, cancellationToken);
    }

    public async Task ActualizarAsync(Socio socio, CancellationToken cancellationToken = default)
    {
        if (socio.SocioId <= 0) throw new ReglaNegocioException("Debe seleccionar un socio para actualizar.");
        await ValidarAsync(socio, cancellationToken);
        await _repositorio.ActualizarAsync(socio, cancellationToken);
    }

    public async Task EliminarAsync(int socioId, CancellationToken cancellationToken = default)
    {
        if (socioId <= 0) throw new ReglaNegocioException("Debe seleccionar un socio.");
        if (await _repositorio.TienePrestamosPendientesAsync(socioId, cancellationToken))
            throw new ReglaNegocioException("No se puede dar de baja un socio con préstamos pendientes.");
        await _repositorio.EliminarLogicamenteAsync(socioId, cancellationToken);
    }

    private async Task ValidarAsync(Socio socio, CancellationToken cancellationToken)
    {
        socio.DNI = socio.DNI?.Trim() ?? string.Empty;
        socio.Nombre = socio.Nombre?.Trim() ?? string.Empty;
        socio.Email = socio.Email?.Trim() ?? string.Empty;
        if (socio.DNI.Length != 8 || !socio.DNI.All(char.IsDigit))
            throw new ReglaNegocioException("El DNI debe contener exactamente 8 dígitos.");
        if (string.IsNullOrWhiteSpace(socio.Nombre)) throw new ReglaNegocioException("El nombre es obligatorio.");
        if (!MailAddress.TryCreate(socio.Email, out var address) || address.Address != socio.Email)
            throw new ReglaNegocioException("El correo electrónico no es válido.");
        if (await _repositorio.ExisteDNIAsync(socio.DNI, socio.SocioId, cancellationToken))
            throw new ReglaNegocioException($"Ya existe un socio con el DNI {socio.DNI}.");
    }
}
