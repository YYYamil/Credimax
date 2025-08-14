
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace PrestamoApp;

public partial class AltaCreditoPage : ContentPage
{
    private readonly AltaCreditoViewModel _viewModel;

    public AltaCreditoPage()
    {
        InitializeComponent();
        _viewModel = new AltaCreditoViewModel();
        BindingContext = _viewModel;


        // Inicializar visibilidad de controles
        labelClienteSeleccionado.IsVisible = false;
        entryBuscarApellido.IsVisible = true;


        // Eventos para recalcular el monto cuota
        entryMonto.TextChanged += OnDatosCreditoCambiados;
        entryInteres.TextChanged += OnDatosCreditoCambiados;
        entryNumCuotas.TextChanged += OnDatosCreditoCambiados;
    }

    private void OnDatosCreditoCambiados(object sender, TextChangedEventArgs e)
    {
        if (decimal.TryParse(entryMonto.Text, out decimal monto) &&
            decimal.TryParse(entryInteres.Text, out decimal interes) &&
            int.TryParse(entryNumCuotas.Text, out int numCuotas) &&
            numCuotas > 0)
        {
            decimal total = monto * (1 + (interes / 100));
            decimal montoCuota = Math.Round(total / numCuotas, 2);
            entryMontoCuota.Text = montoCuota.ToString("0.00");
        }
        else
        {
            entryMontoCuota.Text = "";
        }
    }

    private async void OnApellidoBusquedaTextChanged(object sender, TextChangedEventArgs e)
    {
        await _viewModel.BuscarClientesConDebounce(e.NewTextValue);
    }

    private async void OnBuscarClienteClicked(object sender, EventArgs e)
    {
        await _viewModel.BuscarClientes();
    }

    private void OnClienteSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Cliente selectedCliente)
        {
            _viewModel.ClienteSeleccionado = selectedCliente;
            entryBuscarApellido.IsVisible = false;
            labelClienteSeleccionado.IsVisible = true;

            // Forzar actualización de la UI
            labelClienteSeleccionado.Text = selectedCliente.NombreCompleto;
        }

        /* _viewModel.ClienteSeleccionado = e.CurrentSelection.FirstOrDefault() as Cliente;
         if (_viewModel.ClienteSeleccionado != null)
         {
             entryBuscarApellido.IsVisible = false;
         }*/




    }

    private async void OnGuardarCreditoClicked(object sender, EventArgs e)
    {
        try
        {
            // Validaciones básicas
            if (_viewModel.ClienteSeleccionado == null)
            {
                await DisplayAlert("Error", "Seleccione un cliente válido.", "OK");
                return;
            }

            if (!decimal.TryParse(entryMonto.Text, out decimal monto) || monto <= 0)
            {
                await DisplayAlert("Error", "Ingrese un monto válido.", "OK");
                return;
            }

            if (!decimal.TryParse(entryInteres.Text, out decimal interes) || interes < 0)
            {
                await DisplayAlert("Error", "Ingrese un interés válido.", "OK");
                return;
            }

            string formaPago = pickerFormaPago.SelectedItem?.ToString();
            if (formaPago == null)
            {
                await DisplayAlert("Error", "Seleccione una forma de pago.", "OK");
                return;
            }

            if (!int.TryParse(entryNumCuotas.Text, out int numCuotas) || numCuotas <= 0)
            {
                await DisplayAlert("Error", "Ingrese un número de cuotas válido.", "OK");
                return;
            }

            DateTime fechaInicio = datePickerInicio.Date;
            
            DateTime fechaOtorgamiento = datePickerOtorgamiento.Date;
            
            string observacion = entryObservacion.Text ?? "";
            string tipoCredito = pickerTipoCredito.SelectedItem?.ToString();

            if (tipoCredito == null)
            {
                await DisplayAlert("Error", "Seleccione un tipo de crédito.", "OK");
                return;
            }

            long idPersona = _viewModel.ClienteSeleccionado.Id;
            //decimal importeTotal = monto * (1 + (interes / 100));
            //decimal importeCuota = Math.Round(importeTotal / numCuotas, 2);

            decimal importeCuota;

            if (chkEditarMontoCuota.IsChecked)
            {
                if (!decimal.TryParse(entryMontoCuota.Text, out importeCuota) || importeCuota <= 0)
                {
                    await DisplayAlert("Error", "Ingrese un monto de cuota válido.", "OK");
                    return;
                }
            }
            else
            {
                decimal importeTotal = monto * (1 + (interes / 100));
                importeCuota = Math.Round(importeTotal / numCuotas, 2);
            }


            // Confirmación antes de guardar
            bool confirmar = await DisplayAlert(
                "Confirmar crédito",
                $"Se generarán {numCuotas} cuotas de ${importeCuota:0.00}\nForma de pago: {formaPago}\n\n¿Desea guardar el crédito?",
                "Sí", "No");

            if (!confirmar)
                return;

            await using (SqlConnection conn = new SqlConnection(_viewModel.ConnectionString))
            {
                await conn.OpenAsync();

                // INSERTAR CREDITO
                string insertCreditoQuery = @"
                    INSERT INTO Creditos (MontoCredito, Interes, FormaPago, NumCuotas, FechaInicio, Observacion, Saldo, FechaUltimoPago, EstadoCredito, TipoCredito, Id_Personas, FechaOtorgamiento)
                    OUTPUT INSERTED.Id_Credito
                    VALUES (@Monto, @Interes, @FormaPago, @NumCuotas, @FechaInicio, @Observacion, 0, NULL, 'ACTIVO', @TipoCredito, @IdPersona, @FechaOtorgamiento)";

                await using (SqlCommand cmdCredito = new SqlCommand(insertCreditoQuery, conn))
                {
                    cmdCredito.Parameters.AddWithValue("@Monto", monto);
                    cmdCredito.Parameters.AddWithValue("@Interes", interes);
                    cmdCredito.Parameters.AddWithValue("@FormaPago", formaPago);
                    cmdCredito.Parameters.AddWithValue("@NumCuotas", numCuotas);
                    cmdCredito.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                           

                    cmdCredito.Parameters.AddWithValue("@Observacion", observacion);
                    cmdCredito.Parameters.AddWithValue("@TipoCredito", tipoCredito);
                    cmdCredito.Parameters.AddWithValue("@IdPersona", idPersona);

                    cmdCredito.Parameters.AddWithValue("@FechaOtorgamiento", fechaOtorgamiento);

                    long idCredito = (long)await cmdCredito.ExecuteScalarAsync();

                    // INSERTAR CUOTAS
                    for (int i = 0; i < numCuotas; i++)
                    {
                        //DateTime fechaVto = formaPago switch
                        //{
                        //    "Semanal" => fechaInicio.AddDays(7 * (i + 1)),
                        //    "Quincenal" => fechaInicio.AddDays(15 * (i + 1)),
                        //    "Mensual" => fechaInicio.AddMonths(i + 1),
                        //    _ => throw new Exception("Forma de pago no válida")
                        //};

                        DateTime fechaVto = formaPago switch
                        {
                            "Semanal" => fechaInicio.AddDays(7 * i),
                            "Quincenal" => fechaInicio.AddDays(15 * i),
                            "Mensual" => fechaInicio.AddMonths(i),
                            _ => throw new Exception("Forma de pago no válida")
                        };


                        string insertCuotaQuery = @"
                            INSERT INTO Cuotas (ImporteCuota, FechaVto, Estado, Id_Credito)
                            VALUES (@ImporteCuota, @FechaVto, 'Impaga', @IdCredito)";

                        await using (SqlCommand cmdCuota = new SqlCommand(insertCuotaQuery, conn))
                        {
                            cmdCuota.Parameters.AddWithValue("@ImporteCuota", importeCuota);
                            cmdCuota.Parameters.AddWithValue("@FechaVto", fechaVto);
                            cmdCuota.Parameters.AddWithValue("@IdCredito", idCredito);

                            await cmdCuota.ExecuteNonQueryAsync();
                        }
                    }
                }

                await DisplayAlert("Éxito", "Crédito y cuotas guardadas correctamente.", "OK");
                _viewModel.LimpiarCampos();
                entryMonto.Text = string.Empty;
                entryInteres.Text = string.Empty;
                pickerFormaPago.SelectedItem = null;
                entryNumCuotas.Text = string.Empty;
                entryMontoCuota.Text = string.Empty;
                datePickerInicio.Date = DateTime.Today;
                entryObservacion.Text = string.Empty;
                pickerTipoCredito.SelectedItem = null;
                entryBuscarApellido.IsVisible = true;
                labelClienteSeleccionado.IsVisible = false;
                
                datePickerOtorgamiento.Date = DateTime.Today;

            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
    }

    public class Cliente
    {
        public long Id { get; set; }
        public string NombreCompleto { get; set; }
        public override string ToString() => NombreCompleto;
    }

    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int count) return count > 0;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => value != null;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class AltaCreditoViewModel : BindableObject
     {
        public string ConnectionString { get; } = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";
        private CancellationTokenSource _searchCts = new CancellationTokenSource();

        private string _apellidoBusqueda;
        public string ApellidoBusqueda
        {
            get => _apellidoBusqueda;
            set
            {
                _apellidoBusqueda = value;
                OnPropertyChanged();
            }
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                _isSearching = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Cliente> _clientes = new ObservableCollection<Cliente>();
        public ObservableCollection<Cliente> Clientes
        {
            get => _clientes;
            set
            {
                _clientes = value;
                OnPropertyChanged();
            }
        }

        private Cliente _clienteSeleccionado;
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                _clienteSeleccionado = value;
                OnPropertyChanged();
                if (value != null)
                {
                    Clientes.Clear();
                }
            }
        }

        public async Task BuscarClientesConDebounce(string searchText)
        {
            // Cancel any previous search
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();

            try
            {
                // Debounce: wait 300ms before executing the search
                await Task.Delay(300, _searchCts.Token);

                // Update ApellidoBusqueda on the main thread
                await MainThread.InvokeOnMainThreadAsync(() => ApellidoBusqueda = searchText);

                await BuscarClientes();
            }
            catch (TaskCanceledException)
            {
                // Ignore cancellation
            }
        }

        public async Task BuscarClientes()
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsSearching = true);

            try
            {
                if (string.IsNullOrWhiteSpace(ApellidoBusqueda))
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Clientes.Clear();
                        //Application.Current.MainPage.DisplayAlert("Información", "Ingrese un apellido para buscar.", "OK");
                    });
                    return;
                }

                await using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT Id_Personas, Apellido, Nombre FROM Personas WHERE UPPER(Apellido) LIKE UPPER(@Apellido) + '%'";

                    await using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Apellido", ApellidoBusqueda.Trim());
                        var clientes = new ObservableCollection<Cliente>();
                        await using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                clientes.Add(new Cliente
                                {
                                    Id = reader.GetInt64(0),
                                    NombreCompleto = $"{reader.GetString(1)}, {reader.GetString(2)}"
                                });
                            }
                        }

                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            Clientes = clientes;
                            if (clientes.Count == 0)
                            {
                                await Application.Current.MainPage.DisplayAlert(
                                    "Sin resultados",
                                    $"No se encontraron clientes con el apellido '{ApellidoBusqueda}'.",
                                    "OK");
                            }
                        });
                    }
                }
            }
            catch (SqlException ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Application.Current.MainPage.DisplayAlert(
                        "Error de base de datos",
                        $"No se pudo conectar a la base de datos: {ex.Message}",
                        "OK"));
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Error al buscar clientes: {ex.Message}",
                        "OK"));
            }
            finally
            {
                await MainThread.InvokeOnMainThreadAsync(() => IsSearching = false);
            }
        }

        public void LimpiarCampos()
        {
            ApellidoBusqueda = string.Empty;
            ClienteSeleccionado = null;
            Clientes.Clear();
        }
    }


    private void OnEditarMontoCuotaChanged(object sender, CheckedChangedEventArgs e)
    {
        entryMontoCuota.IsReadOnly = !e.Value;
    }

}