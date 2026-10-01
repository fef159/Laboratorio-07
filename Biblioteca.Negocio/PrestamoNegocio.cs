using Biblioteca.Datos;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;
using Biblioteca.Negocio.Interfaces;

namespace Biblioteca.Negocio;

public sealed class PrestamoNegocio : IPrestamoNegocio
{
    public const int MaximoLibrosPendientes = 3;
    public const decimal MultaDiaria = 1.50m;

    private readonly IPrestamoRepositorio _prestamos;
    private readonly ISocioRepositorio _socios;
    private readonly ILibroRepositorio _libros;

    public PrestamoNegocio() : this(new PrestamoRepositorio(), new SocioRepositorio(), new LibroRepositorio()) { }
    public PrestamoNegocio(IPrestamoRepositorio prestamos, ISocioRepositorio socios, ILibroRepositorio libros)
    {
        _prestamos = prestamos;
        _socios = socios;
        _libros = libros;
    }

    public async Task<int> RegistrarAsync(int socioId, IEnumerable<int> libroIds, DateTime fechaPrestamo, DateTime fechaLimite, CancellationToken cancellationToken = default)
    {
        Socio? socio = await _socios.ObtenerPorIdAsync(socioId, cancellationToken);
        if (socio is null || !socio.Activo) throw new ReglaNegocioException("Debe seleccionar un socio activo.");
        if (fechaLimite.Date < fechaPrestamo.Date) throw new ReglaNegocioException("La fecha límite no puede ser anterior a la fecha del préstamo.");

        List<int> ids = libroIds.Distinct().ToList();
        if (ids.Count == 0) throw new ReglaNegocioException("Debe agregar al menos un libro al préstamo.");
        int pendientes = await _prestamos.ContarLibrosPendientesAsync(socioId, cancellationToken);
        if (pendientes + ids.Count > MaximoLibrosPendientes)
            throw new ReglaNegocioException($"El socio no puede superar {MaximoLibrosPendientes} libros pendientes. Actualmente tiene {pendientes}.");

        var detalles = new List<DetallePrestamo>();
        foreach (int id in ids)
        {
            Libro? libro = await _libros.ObtenerPorIdAsync(id, cancellationToken);
            if (libro is null || !libro.Activo) throw new ReglaNegocioException("Uno de los libros seleccionados no está activo.");
            if (libro.Ejemplares <= 0) throw new ReglaNegocioException($"'{libro.Titulo}' no tiene ejemplares disponibles.");
            detalles.Add(new DetallePrestamo { LibroId = id, LibroTitulo = libro.Titulo });
        }

        return await _prestamos.RegistrarAsync(new Prestamo
        {
            SocioId = socioId,
            FechaPrestamo = fechaPrestamo.Date,
            FechaLimite = fechaLimite.Date,
            Detalles = detalles
        }, cancellationToken);
    }

    public Task<List<DetallePrestamo>> ListarPendientesAsync(CancellationToken cancellationToken = default) =>
        _prestamos.ListarDetallesPendientesAsync(cancellationToken);

    public async Task<ResultadoDevolucion> DevolverAsync(int prestamoId, int libroId, DateTime fechaDevolucion, CancellationToken cancellationToken = default)
    {
        DetallePrestamo? detalle = await _prestamos.ObtenerDetallePendienteAsync(prestamoId, libroId, cancellationToken);
        if (detalle is null) throw new ReglaNegocioException("El detalle seleccionado ya fue devuelto o no existe.");

        int diasRetraso = Math.Max(0, (fechaDevolucion.Date - detalle.FechaLimite.Date).Days);
        decimal multa = diasRetraso * MultaDiaria;
        await _prestamos.RegistrarDevolucionAsync(prestamoId, libroId, fechaDevolucion.Date, cancellationToken);
        return new ResultadoDevolucion { FechaDevolucion = fechaDevolucion.Date, DiasRetraso = diasRetraso, Multa = multa };
    }

    public async Task<List<PrestamoReporte>> ReportarAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken = default)
    {
        if (hasta.Date < desde.Date) throw new ReglaNegocioException("La fecha final no puede ser anterior a la fecha inicial.");
        return await _prestamos.ReportarAsync(desde, hasta, cancellationToken);
    }
}
