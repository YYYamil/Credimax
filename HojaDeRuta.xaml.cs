using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Data.SqlClient;

namespace PrestamoApp
{
    public partial class HojaDeRuta : ContentPage
    {
        private readonly HojaDeRutaViewModel _viewModel;

        public HojaDeRuta(long idPersona, string formaPago)
        {
            InitializeComponent();
            _viewModel = new HojaDeRutaViewModel(idPersona, formaPago);
            BindingContext = _viewModel;
        }
    }

    public class Cliente : INotifyPropertyChanged
    {
        private long _idPersonas;
        private string _apellido;
        private string _nombre;
        private string _direccion;
        private string _cel;

        public event PropertyChangedEventHandler PropertyChanged;

        public long Id_Personas
        {
            get => _idPersonas;
            set { _idPersonas = value; OnPropertyChanged(); }
        }

        public string Apellido
        {
            get => _apellido;
            set { _apellido = value; OnPropertyChanged(); OnPropertyChanged(nameof(NombreCompleto)); }
        }

        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); OnPropertyChanged(nameof(NombreCompleto)); }
        }

        public string Direccion
        {
            get => _direccion;
            set { _direccion = value; OnPropertyChanged(); }
        }

        public string Cel
        {
            get => _cel;
            set { _cel = value; OnPropertyChanged(); }
        }

        public string NombreCompleto => $"{Apellido}, {Nombre}";

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CreditoActivo : INotifyPropertyChanged
    {
        private string _tipoCredito;
        private string _observacion;
        private decimal _importePorCuota;
        private decimal _ValorCredito;
        private decimal _saldoALaFecha;
        private DateTime? _fechaUltimoPago;
        private decimal? _montoUltimoPago;
        private long _idCredito;

        public long Id_Credito
        {
            get => _idCredito;
            set { _idCredito = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string TipoCredito
        {
            get => _tipoCredito;
            set { _tipoCredito = value; OnPropertyChanged(); }
        }

        public string Observacion
        {
            get => _observacion;
            set { _observacion = value; OnPropertyChanged(); }
        }

        public decimal importePorCuota
        {
            get => _importePorCuota;
            set { _importePorCuota = value; OnPropertyChanged(); }
        }

        public decimal ValorCredito
        {
            get => _ValorCredito;
            set { _ValorCredito = value; OnPropertyChanged(); }
        }

        public decimal SaldoALaFecha
        {
            get => _saldoALaFecha;
            set { _saldoALaFecha = value; OnPropertyChanged(); }
        }

        public DateTime? FechaUltimoPago
        {
            get => _fechaUltimoPago;
            set { _fechaUltimoPago = value; OnPropertyChanged(); OnPropertyChanged(nameof(FechaUltimoPagoDisplay)); }
        }

        public decimal? MontoUltimoPago
        {
            get => _montoUltimoPago;
            set { _montoUltimoPago = value; OnPropertyChanged(); OnPropertyChanged(nameof(MontoUltimoPagoDisplay)); }
        }

        public string FechaUltimoPagoDisplay => FechaUltimoPago.HasValue ? $"Último Pago: {FechaUltimoPago.Value:dd/MM/yyyy}" : "Último Pago: Sin pagos";
        public string MontoUltimoPagoDisplay => MontoUltimoPago.HasValue ? $"Monto Pagado: {MontoUltimoPago.Value:C}" : "Monto Pagado: Sin pagos";

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HojaDeRutaViewModel : INotifyPropertyChanged
    {
        private long _idPersona;
        private string _formaPago;
        private bool _isBusy;
        private string _title;
        private Cliente _cliente;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public Cliente Cliente
        {
            get => _cliente;
            set { _cliente = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CreditoActivo> CreditosActivos { get; } = new();

        public bool IsNotBusy => !IsBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand SaveCreditCommand { get; }

        public HojaDeRutaViewModel(long idPersona, string formaPago)
        {
            _idPersona = idPersona;
            _formaPago = formaPago;
            _title = $"Detalles de Créditos - ID Persona: {_idPersona}";
            LoadDataCommand = new Command(async () => await LoadData());
            SaveCreditCommand = new Command<CreditoActivo>(async (credito) => await SaveCredit(credito));
            LoadDataCommand.Execute(null);
        }

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;
                CreditosActivos.Clear();

                string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string clienteQuery = @"SELECT Id_Personas, Apellido, Nombre, Direccion, Cel FROM Personas WHERE Id_Personas = @IdPersona";

                    using (SqlCommand clienteCommand = new SqlCommand(clienteQuery, connection))
                    {
                        clienteCommand.Parameters.AddWithValue("@IdPersona", _idPersona);

                        using (SqlDataReader reader = await clienteCommand.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                Cliente = new Cliente
                                {
                                    Id_Personas = (long)reader["Id_Personas"],
                                    Apellido = reader["Apellido"].ToString(),
                                    Nombre = reader["Nombre"].ToString(),
                                    Direccion = reader["Direccion"].ToString(),
                                    Cel = reader["Cel"].ToString()
                                };
                            }
                        }
                    }

                    string creditosQuery = @"SELECT DISTINCT
    C.Id_Credito,
    C.TipoCredito,
    C.Observacion,
    MAX(Cu.ImporteCuota) AS ImportePorCuota,
    SUM(Cu.ImporteCuota) AS ValorCredito,
    (
        SELECT ISNULL(SUM(Cu2.ImporteCuota), 0)
        FROM Cuotas Cu2
        WHERE Cu2.Id_Credito = C.Id_Credito
          AND Cu2.Estado = 'IMPAGA'
          AND Cu2.FechaVto <= CAST(GETDATE() AS DATE)
    ) AS SaldoALaFecha,
    (
        SELECT TOP 1 CAST(P.FechaPago AS DATE)
        FROM Cuotas Cu3
        INNER JOIN Pagos P ON P.Id_Cuota = Cu3.Id_Cuota
        WHERE Cu3.Id_Credito = C.Id_Credito
        ORDER BY P.FechaPago DESC
    ) AS FechaUltimoPago,
    (
        SELECT TOP 1 P.MontoPagado
        FROM Cuotas Cu3
        INNER JOIN Pagos P ON P.Id_Cuota = Cu3.Id_Cuota
        WHERE Cu3.Id_Credito = C.Id_Credito
        ORDER BY P.FechaPago DESC
    ) AS MontoUltimoPago
FROM Creditos C
INNER JOIN Cuotas Cu ON Cu.Id_Credito = C.Id_Credito
WHERE C.Id_Personas = @IdPersona
  AND C.EstadoCredito = 'ACTIVO'";

                    if (!string.IsNullOrEmpty(_formaPago))
                    {
                        creditosQuery += " AND C.FormaPago = @FormaPago";
                    }

                    creditosQuery += " GROUP BY C.TipoCredito, C.Observacion, C.Id_Credito";

                    using (SqlCommand creditosCommand = new SqlCommand(creditosQuery, connection))
                    {
                        creditosCommand.Parameters.AddWithValue("@IdPersona", _idPersona);

                        if (!string.IsNullOrEmpty(_formaPago))
                        {
                            creditosCommand.Parameters.AddWithValue("@FormaPago", _formaPago);
                        }

                        using (SqlDataReader reader = await creditosCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var credito = new CreditoActivo
                                {
                                    Id_Credito = reader.IsDBNull(0) ? 0 : reader.GetInt64(0),
                                    TipoCredito = reader.IsDBNull(1) ? "No especificado" : reader.GetString(1),
                                    Observacion = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                    importePorCuota = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                                    ValorCredito = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                                    SaldoALaFecha = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                                    FechaUltimoPago = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                    MontoUltimoPago = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7)
                                };
                                CreditosActivos.Add(credito);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveCredit(CreditoActivo credito)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
INSERT INTO HojaRuta (
    Id_Personas, TipoCredito, Observacion, ValorCredito, 
    SaldoALaFecha, FechaUltimoPago, MontoUltimoPago, FechaRegistro, 
    Apellido, Nombre, Id_Credito, ImporteValorCuota
)
VALUES (
    @IdPersona, @TipoCredito, @Observacion, @ValorCredito, 
    @SaldoALaFecha, @FechaUltimoPago, @MontoUltimoPago, @FechaRegistro, 
    @Apellido, @Nombre, @IdCredito, @ImporteValorCuota)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdPersona", _idPersona);
                        command.Parameters.AddWithValue("@TipoCredito", credito.TipoCredito);
                        command.Parameters.AddWithValue("@Observacion", credito.Observacion);
                        command.Parameters.AddWithValue("@ValorCredito", credito.ValorCredito);
                        command.Parameters.AddWithValue("@SaldoALaFecha", credito.SaldoALaFecha);
                        command.Parameters.AddWithValue("@FechaUltimoPago", (object)credito.FechaUltimoPago ?? DBNull.Value);
                        command.Parameters.AddWithValue("@MontoUltimoPago", (object)credito.MontoUltimoPago ?? DBNull.Value);
                        command.Parameters.AddWithValue("@FechaRegistro", DateTime.Now.Date);
                        command.Parameters.AddWithValue("@Apellido", Cliente.Apellido);
                        command.Parameters.AddWithValue("@Nombre", Cliente.Nombre);
                        command.Parameters.AddWithValue("@IdCredito", credito.Id_Credito);
                        command.Parameters.AddWithValue("@ImporteValorCuota", credito.importePorCuota);

                        await command.ExecuteNonQueryAsync();

                        CreditosActivos.Remove(credito);

                        await Application.Current.MainPage.DisplayAlert("Éxito", "Crédito guardado en hoja de ruta", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}