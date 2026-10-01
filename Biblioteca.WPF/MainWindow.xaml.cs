using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades;
using Biblioteca.Negocio;
using Biblioteca.Negocio.Interfaces;

namespace Biblioteca.WPF;

public partial class MainWindow : Window
{
    private readonly IAutorNegocio _autores = new AutorNegocio();
    private readonly ILibroNegocio _libros = new LibroNegocio();
    private readonly ISocioNegocio _socios = new SocioNegocio();
    private readonly IPrestamoNegocio _prestamos = new PrestamoNegocio();

    private Libro? _libroEditando;
    private Socio? _socioEditando;
    public ObservableCollection<Libro> LibrosSeleccionados { get; } = [];

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        dpFechaPrestamo.SelectedDate = DateTime.Today;
        dpFechaLimite.SelectedDate = DateTime.Today.AddDays(7);
        dpReporteDesde.SelectedDate = DateTime.Today.AddMonths(-1);
        dpReporteHasta.SelectedDate = DateTime.Today;
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e) =>
        await EjecutarAsync(async () =>
        {
            await CargarAutoresAsync();
            await CargarLibrosAsync();
            await CargarSociosAsync();
            await CargarDatosPrestamoAsync();
            await CargarPendientesAsync();
        });

    private async Task CargarAutoresAsync() => cboAutores.ItemsSource = await _autores.ListarActivosAsync();

    private async Task CargarLibrosAsync() =>
        dgLibros.ItemsSource = await _libros.ListarAsync(txtBuscarLibro.Text);

    private async Task CargarSociosAsync() =>
        dgSocios.ItemsSource = await _socios.ListarAsync(txtBuscarSocio.Text);

    private async Task CargarDatosPrestamoAsync()
    {
        cboSociosPrestamo.ItemsSource = await _socios.ListarAsync();
        cboLibrosDisponibles.ItemsSource = await _libros.ListarDisponiblesAsync();
    }

    private async Task CargarPendientesAsync() =>
        dgPendientes.ItemsSource = await _prestamos.ListarPendientesAsync();

    private async Task EjecutarAsync(Func<Task> action, string mensajeExito = "")
    {
        busyBar.Visibility = Visibility.Visible;
        IsEnabled = false;
        try
        {
            await action();
            txtStatus.Text = string.IsNullOrWhiteSpace(mensajeExito) ? "Listo" : mensajeExito;
        }
        catch (ReglaNegocioException ex)
        {
            MessageBox.Show(ex.Message, "Regla de negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtStatus.Text = ex.Message;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No fue posible completar la operación.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            txtStatus.Text = "Ocurrió un error";
        }
        finally
        {
            IsEnabled = true;
            busyBar.Visibility = Visibility.Collapsed;
        }
    }

    private async void txtBuscarLibro_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsLoaded) await EjecutarAsync(CargarLibrosAsync);
    }

    private void dgLibros_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgLibros.SelectedItem is not Libro libro) return;
        _libroEditando = libro;
        txtLibroTitulo.Text = libro.Titulo;
        txtLibroISBN.Text = libro.ISBN;
        cboAutores.SelectedValue = libro.AutorId;
        txtEjemplares.Text = libro.Ejemplares.ToString();
    }

    private void btnNuevoLibro_Click(object sender, RoutedEventArgs e) => LimpiarLibro();

    private async void btnGuardarLibro_Click(object sender, RoutedEventArgs e)
    {
        var libro = new Libro
        {
            LibroId = _libroEditando?.LibroId ?? 0,
            Titulo = txtLibroTitulo.Text,
            ISBN = txtLibroISBN.Text,
            AutorId = cboAutores.SelectedValue is int autorId ? autorId : 0,
            Ejemplares = int.TryParse(txtEjemplares.Text, out int cantidad) ? cantidad : -1
        };
        bool nuevo = libro.LibroId == 0;
        await EjecutarAsync(async () =>
        {
            if (nuevo) await _libros.RegistrarAsync(libro); else await _libros.ActualizarAsync(libro);
            LimpiarLibro();
            await CargarLibrosAsync();
            await CargarDatosPrestamoAsync();
        }, nuevo ? "Libro registrado" : "Libro actualizado");
    }

    private async void btnEliminarLibro_Click(object sender, RoutedEventArgs e)
    {
        if (_libroEditando is null) return;
        if (MessageBox.Show($"¿Dar de baja '{_libroEditando.Titulo}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        int id = _libroEditando.LibroId;
        await EjecutarAsync(async () =>
        {
            await _libros.EliminarAsync(id);
            LimpiarLibro();
            await CargarLibrosAsync();
            await CargarDatosPrestamoAsync();
        }, "Libro dado de baja");
    }

    private void LimpiarLibro()
    {
        _libroEditando = null;
        dgLibros.SelectedItem = null;
        txtLibroTitulo.Clear();
        txtLibroISBN.Clear();
        cboAutores.SelectedIndex = -1;
        txtEjemplares.Text = "1";
    }

    private async void txtBuscarSocio_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsLoaded) await EjecutarAsync(CargarSociosAsync);
    }

    private void dgSocios_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgSocios.SelectedItem is not Socio socio) return;
        _socioEditando = socio;
        txtSocioDNI.Text = socio.DNI;
        txtSocioNombre.Text = socio.Nombre;
        txtSocioEmail.Text = socio.Email;
    }

    private void btnNuevoSocio_Click(object sender, RoutedEventArgs e) => LimpiarSocio();

    private async void btnGuardarSocio_Click(object sender, RoutedEventArgs e)
    {
        var socio = new Socio
        {
            SocioId = _socioEditando?.SocioId ?? 0,
            DNI = txtSocioDNI.Text,
            Nombre = txtSocioNombre.Text,
            Email = txtSocioEmail.Text
        };
        bool nuevo = socio.SocioId == 0;
        await EjecutarAsync(async () =>
        {
            if (nuevo) await _socios.RegistrarAsync(socio); else await _socios.ActualizarAsync(socio);
            LimpiarSocio();
            await CargarSociosAsync();
            await CargarDatosPrestamoAsync();
        }, nuevo ? "Socio registrado" : "Socio actualizado");
    }

    private async void btnEliminarSocio_Click(object sender, RoutedEventArgs e)
    {
        if (_socioEditando is null) return;
        if (MessageBox.Show($"¿Dar de baja a '{_socioEditando.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        int id = _socioEditando.SocioId;
        await EjecutarAsync(async () =>
        {
            await _socios.EliminarAsync(id);
            LimpiarSocio();
            await CargarSociosAsync();
            await CargarDatosPrestamoAsync();
        }, "Socio dado de baja");
    }

    private void LimpiarSocio()
    {
        _socioEditando = null;
        dgSocios.SelectedItem = null;
        txtSocioDNI.Clear();
        txtSocioNombre.Clear();
        txtSocioEmail.Clear();
    }

    private void btnAgregarLibro_Click(object sender, RoutedEventArgs e)
    {
        if (cboLibrosDisponibles.SelectedItem is not Libro libro) return;
        if (LibrosSeleccionados.All(item => item.LibroId != libro.LibroId)) LibrosSeleccionados.Add(libro);
    }

    private void btnQuitarLibro_Click(object sender, RoutedEventArgs e)
    {
        if (dgLibrosPrestamo.SelectedItem is Libro libro) LibrosSeleccionados.Remove(libro);
    }

    private async void btnRegistrarPrestamo_Click(object sender, RoutedEventArgs e)
    {
        int socioId = cboSociosPrestamo.SelectedValue is int id ? id : 0;
        DateTime fecha = dpFechaPrestamo.SelectedDate ?? DateTime.Today;
        DateTime limite = dpFechaLimite.SelectedDate ?? fecha;
        await EjecutarAsync(async () =>
        {
            int prestamoId = await _prestamos.RegistrarAsync(socioId, LibrosSeleccionados.Select(l => l.LibroId), fecha, limite);
            LibrosSeleccionados.Clear();
            await CargarDatosPrestamoAsync();
            await CargarLibrosAsync();
            await CargarPendientesAsync();
            MessageBox.Show($"Préstamo N.º {prestamoId} registrado correctamente.", "Préstamo", MessageBoxButton.OK, MessageBoxImage.Information);
        }, "Préstamo registrado");
    }

    private async void btnDevolver_Click(object sender, RoutedEventArgs e)
    {
        if (dgPendientes.SelectedItem is not DetallePrestamo detalle) return;
        await EjecutarAsync(async () =>
        {
            ResultadoDevolucion resultado = await _prestamos.DevolverAsync(detalle.PrestamoId, detalle.LibroId, DateTime.Today);
            await CargarPendientesAsync();
            await CargarDatosPrestamoAsync();
            await CargarLibrosAsync();
            MessageBox.Show($"Devolución registrada.\nDías de retraso: {resultado.DiasRetraso}\nMulta: S/ {resultado.Multa:N2}", "Devolución", MessageBoxButton.OK, MessageBoxImage.Information);
        }, "Devolución registrada");
    }

    private async void btnConsultarReporte_Click(object sender, RoutedEventArgs e)
    {
        DateTime desde = dpReporteDesde.SelectedDate ?? DateTime.Today;
        DateTime hasta = dpReporteHasta.SelectedDate ?? DateTime.Today;
        await EjecutarAsync(async () => dgReporte.ItemsSource = await _prestamos.ReportarAsync(desde, hasta));
    }
}
