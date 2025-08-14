using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PrestamoApp;

public class MainViewModel : BindableObject
{
    //private readonly string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";
    private readonly string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True;Connect Timeout=10";

    private int _creditosCancelados;
    public int CreditosCancelados
    {
        get => _creditosCancelados;
        set
        {
            _creditosCancelados = value;
            OnPropertyChanged();
        }
    }

    private DateTime _fechaConsulta = DateTime.Today;
    public DateTime FechaConsulta
    {
        get => _fechaConsulta;
        set
        {
            if (_fechaConsulta != value)
            {
                _fechaConsulta = value;
                OnPropertyChanged();
                CargarEstadisticas();
            }
        }
    }

    private string _apellidoBusqueda;
    public string ApellidoBusqueda
    {
        get => _apellidoBusqueda;
        set
        {
            _apellidoBusqueda = value;
            OnPropertyChanged();
            ((Command)BuscarCommand).ChangeCanExecute();
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

    private decimal _totalCreditosSolicitados;
    public decimal TotalCreditosSolicitados
    {
        get => _totalCreditosSolicitados;
        set
        {
            _totalCreditosSolicitados = value;
            OnPropertyChanged();
        }
    }

    private decimal _totalFinanciado;
    public decimal TotalFinanciado
    {
        get => _totalFinanciado;
        set
        {
            _totalFinanciado = value;
            OnPropertyChanged();
        }
    }

    private decimal _interesGanado;
    public decimal InteresGanado
    {
        get => _interesGanado;
        set
        {
            _interesGanado = value;
            OnPropertyChanged();
        }
    }

    private decimal _saldoGlobal;
    public decimal SaldoGlobal
    {
        get => _saldoGlobal;
        set
        {
            _saldoGlobal = value;
            OnPropertyChanged();
        }
    }

    private decimal _saldoActual;
    public decimal SaldoActual
    {
        get => _saldoActual;
        set
        {
            _saldoActual = value;
            OnPropertyChanged();
        }
    }

    private decimal _carteraActiva;
    public decimal CarteraActiva
    {
        get => _carteraActiva;
        set
        {
            _carteraActiva = value;
            OnPropertyChanged();
        }
    }

    private decimal _cobranzaDelDia;
    public decimal CobranzaDelDia
    {
        get => _cobranzaDelDia;
        set
        {
            _cobranzaDelDia = value;
            OnPropertyChanged();
        }
    }

    private int _cantidadCreditosEntregados;
    public int CantidadCreditosEntregados
    {
        get => _cantidadCreditosEntregados;
        set
        {
            _cantidadCreditosEntregados = value;
            OnPropertyChanged();
        }
    }

    private int _cantidadCreditosActivos;
    public int CantidadCreditosActivos
    {
        get => _cantidadCreditosActivos;
        set
        {
            _cantidadCreditosActivos = value;
            OnPropertyChanged();
        }
    }

    private int _creditosMensual;
    public int CreditosMensual
    {
        get => _creditosMensual;
        set
        {
            _creditosMensual = value;
            OnPropertyChanged();
        }
    }

    private int _creditosSemanal;
    public int CreditosSemanal
    {
        get => _creditosSemanal;
        set
        {
            _creditosSemanal = value;
            OnPropertyChanged();
        }
    }

    private int _creditosQuincenal;
    public int CreditosQuincenal
    {
        get => _creditosQuincenal;
        set
        {
            _creditosQuincenal = value;
            OnPropertyChanged();
        }
    }

    private int _creditosEfectivo;
    public int CreditosEfectivo
    {
        get => _creditosEfectivo;
        set
        {
            _creditosEfectivo = value;
            OnPropertyChanged();
        }
    }

    private int _creditosZapatillas;
    public int CreditosZapatillas
    {
        get => _creditosZapatillas;
        set
        {
            _creditosZapatillas = value;
            OnPropertyChanged();
        }
    }

    private int _creditosArticulo;
    public int CreditosArticulo
    {
        get => _creditosArticulo;
        set
        {
            _creditosArticulo = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<ClienteEstadistica> _clientes = new ObservableCollection<ClienteEstadistica>();
    public ObservableCollection<ClienteEstadistica> Clientes
    {
        get => _clientes;
        set
        {
            _clientes = value;
            OnPropertyChanged();
        }
    }

    private ClienteEstadistica _clienteSeleccionado;
    public ClienteEstadistica ClienteSeleccionado
    {
        get => _clienteSeleccionado;
        set
        {
            _clienteSeleccionado = value;
            OnPropertyChanged();
        }
    }

    public ICommand BuscarCommand { get; }
    public ICommand ClienteSeleccionadoCommand { get; }

    public MainViewModel()
    {
        BuscarCommand = new Command(async () => await BuscarClientes(), () => !string.IsNullOrWhiteSpace(ApellidoBusqueda));
        ClienteSeleccionadoCommand = new Command(ClienteSeleccionadoAction);
        CargarEstadisticas();
    }



    private async void CargarEstadisticas()
    {
        try
        {
            await using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                WITH PagosPorCuota AS (
                    SELECT Id_Cuota, SUM(MontoPagado) AS TotalPagado 
                    FROM Pagos 
                    GROUP BY Id_Cuota
                ),
                PagosTotales AS (
                    SELECT SUM(MontoPagado) AS SumaPagosTotales 
                    FROM Pagos
                ),
                PagosPorFecha AS (
                    SELECT SUM(MontoPagado) AS SumaPagosDia
                    FROM Pagos
                    WHERE CAST(FechaPago AS DATE) = @FechaConsulta
                ),
                Saldos AS (
                    SELECT
                        SUM(CASE WHEN FechaVto > GETDATE() THEN ImporteCuota - ISNULL(TotalPagado, 0) ELSE 0 END) AS SaldoGlobal,
                        SUM(CASE WHEN FechaVto <= GETDATE() THEN ImporteCuota - ISNULL(TotalPagado, 0) ELSE 0 END) AS SaldoActual
                    FROM Cuotas c
                    LEFT JOIN PagosPorCuota p ON c.Id_Cuota = p.Id_Cuota
                    WHERE Estado = 'Impaga'
                )
                SELECT
                    (SELECT SUM(MontoCredito) FROM Creditos) AS TotalCreditoSolicitados,
                    (SELECT SUM(ImporteCuota) FROM Cuotas) AS TotalFinanciado,
                    (SELECT SUM(ImporteCuota) FROM Cuotas) - (SELECT SUM(MontoCredito) FROM Creditos) AS InteresGanado,
                    s.SaldoGlobal,
                    s.SaldoActual,
                    (SELECT SUM(MontoCredito) FROM Creditos) - (SELECT SumaPagosTotales FROM PagosTotales) AS CarteraActiva,
                    (SELECT SumaPagosDia FROM PagosPorFecha) AS CobranzaDelDia,
                    (SELECT COUNT(*) FROM Creditos WHERE EstadoCredito = 'Cancelado') AS CreditosCancelados,
                    (SELECT COUNT(*) FROM Creditos) AS CantidadCreditosEntregados,
                    (SELECT COUNT(*) FROM Creditos WHERE EstadoCredito = 'Activo') AS CantidadCreditosActivos,
                    (SELECT COUNT(*) FROM Creditos WHERE FormaPago = 'MENSUAL') AS CreditosMensual,
                    (SELECT COUNT(*) FROM Creditos WHERE FormaPago = 'SEMANAL') AS CreditosSemanal,
                    (SELECT COUNT(*) FROM Creditos WHERE FormaPago = 'QUINCENAL') AS CreditosQuincenal,
                    (SELECT COUNT(*) FROM Creditos WHERE TipoCredito = 'EFECTIVO') AS CreditosEfectivo,
                    (SELECT COUNT(*) FROM Creditos WHERE TipoCredito = 'ZAPATILLA') AS CreditosZapatillas,
                    (SELECT COUNT(*) FROM Creditos WHERE TipoCredito = 'Artículo') AS CreditosArticulo
                FROM Saldos s;";

                await using (var command = new SqlCommand(query, connection))
                {
                    command.CommandTimeout = 15; // Este sigue siendo ajustable por comando
                    command.Parameters.AddWithValue("@FechaConsulta", FechaConsulta);
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            TotalCreditosSolicitados = reader.GetDecimal(0);
                            TotalFinanciado = reader.GetDecimal(1);
                            InteresGanado = reader.GetDecimal(2);
                            SaldoGlobal = reader.GetDecimal(3);
                            SaldoActual = reader.GetDecimal(4);
                            CarteraActiva = reader.GetDecimal(5);
                            CobranzaDelDia = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                            CreditosCancelados = reader.GetInt32(7);
                            CantidadCreditosEntregados = reader.GetInt32(8);
                            CantidadCreditosActivos = reader.GetInt32(9);
                            CreditosMensual = reader.GetInt32(10);
                            CreditosSemanal = reader.GetInt32(11);
                            CreditosQuincenal = reader.GetInt32(12);
                            CreditosEfectivo = reader.GetInt32(13);
                            CreditosZapatillas = reader.GetInt32(14);
                            CreditosArticulo = reader.GetInt32(15);
                        }
                    }
                }
            }
        }
        catch (SqlException ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", $"No se pudo conectar a la base de datos: {ex.Message}", "OK"));
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al cargar estadísticas: {ex.Message}", "OK"));
        }
    }



    /* private async void CargarEstadisticas()
     {
         try
         {
             await using (var connection = new SqlConnection(connectionString))
             {
                 connection.ConnectionTimeout = 10;
                 await connection.OpenAsync();
                 var query = @"
                     WITH PagosPorCuota AS (
                         SELECT Id_Cuota, SUM(MontoPagado) AS TotalPagado 
                         FROM Pagos 
                         GROUP BY Id_Cuota
                     ),
                     PagosTotales AS (
                         SELECT SUM(MontoPagado) AS SumaPagosTotales 
                         FROM Pagos
                     ),
                     PagosPorFecha AS (
                         SELECT SUM(MontoPagado) AS SumaPagosDia
                         FROM Pagos
                         WHERE CAST(FechaPago AS DATE) = @FechaConsulta
                     ),
                     Saldos AS (
                         SELECT
                             SUM(CASE WHEN FechaVto > GETDATE() THEN ImporteCuota - ISNULL(TotalPagado, 0) ELSE 0 END) AS SaldoGlobal,
                             SUM(CASE WHEN FechaVto <= GETDATE() THEN ImporteCuota - ISNULL(TotalPagado, 0) ELSE 0 END) AS SaldoActual
                         FROM Cuotas c
                         LEFT JOIN PagosPorCuota p ON c.Id_Cuota = p.Id_Cuota
                         WHERE Estado = 'Impaga'
                     )
                     SELECT
                         (SELECT SUM(MontoCredito) FROM Creditos) AS TotalCreditoSolicitados,
                         (SELECT SUM(ImporteCuota) FROM Cuotas) AS TotalFinanciado,
                         (SELECT SUM(ImporteCuota) FROM Cuotas) - (SELECT SUM(MontoCredito) FROM Creditos) AS InteresGanado,
                         s.SaldoGlobal,
                         s.SaldoActual,
                         (SELECT SUM(MontoCredito) FROM Creditos) - (SELECT SumaPagosTotales FROM PagosTotales) AS CarteraActiva,
                         (SELECT SumaPagosDia FROM PagosPorFecha) AS CobranzaDelDia,
                         (SELECT COUNT(*) FROM Creditos WHERE EstadoCredito = 'Cancelado') AS CreditosCancelados,
                         (SELECT COUNT(*) FROM Creditos) AS CantidadCreditosEntregados,
                         (SELECT COUNT(*) FROM Creditos WHERE EstadoCredito = 'Activo') AS CantidadCreditosActivos,
                         (SELECT COUNT(*) FROM Creditos WHERE FormaPago = 'MENSUAL') AS CreditosMensual,
                         (SELECT COUNT(*) FROM Creditos WHERE FormaPago = 'SEMANAL') AS CreditosSemanal,
                         (SELECT COUNT(*) FROM Creditos WHERE FormaPago = 'QUINCENAL') AS CreditosQuincenal,
                         (SELECT COUNT(*) FROM Creditos WHERE TipoCredito = 'EFECTIVO') AS CreditosEfectivo,
                         (SELECT COUNT(*) FROM Creditos WHERE TipoCredito = 'ZAPATILLA') AS CreditosZapatillas,
                         (SELECT COUNT(*) FROM Creditos WHERE TipoCredito = 'Artículo') AS CreditosArticulo
                     FROM Saldos s;";

                 await using (var command = new SqlCommand(query, connection))
                 {
                     command.CommandTimeout = 15;
                     command.Parameters.AddWithValue("@FechaConsulta", FechaConsulta);
                     await using (var reader = await command.ExecuteReaderAsync())
                     {
                         if (await reader.ReadAsync())
                         {
                             TotalCreditosSolicitados = reader.GetDecimal(0);
                             TotalFinanciado = reader.GetDecimal(1);
                             InteresGanado = reader.GetDecimal(2);
                             SaldoGlobal = reader.GetDecimal(3);
                             SaldoActual = reader.GetDecimal(4);
                             CarteraActiva = reader.GetDecimal(5);
                             CobranzaDelDia = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                             CreditosCancelados = reader.GetInt32(7);
                             CantidadCreditosEntregados = reader.GetInt32(8);
                             CantidadCreditosActivos = reader.GetInt32(9);
                             CreditosMensual = reader.GetInt32(10);
                             CreditosSemanal = reader.GetInt32(11);
                             CreditosQuincenal = reader.GetInt32(12);
                             CreditosEfectivo = reader.GetInt32(13);
                             CreditosZapatillas = reader.GetInt32(14);
                             CreditosArticulo = reader.GetInt32(15);
                         }
                     }
                 }
             }
         }
         catch (SqlException ex)
         {
             await MainThread.InvokeOnMainThreadAsync(async () =>
                 await Application.Current.MainPage.DisplayAlert("Error de Conexión", $"No se pudo conectar a la base de datos: {ex.Message}", "OK"));
         }
         catch (Exception ex)
         {
             await MainThread.InvokeOnMainThreadAsync(async () =>
                 await Application.Current.MainPage.DisplayAlert("Error", $"Error al cargar estadísticas: {ex.Message}", "OK"));
         }
     }
     */


    /*
    private async Task BuscarClientes()
    {
        await MainThread.InvokeOnMainThreadAsync(() => IsSearching = true);

        try
        {
            if (string.IsNullOrWhiteSpace(ApellidoBusqueda))
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Clientes.Clear();
                    Application.Current.MainPage.DisplayAlert("Información", "Ingrese un apellido para buscar.", "OK");
                });
                return;
            }

            await using (var connection = new SqlConnection(connectionString))
            {
                connection.ConnectionTimeout = 10;
                await connection.OpenAsync();

                var query = @"
                    SELECT
                        p.Id_Personas,
                        p.Apellido,
                        p.Nombre,
                        c.MontoCredito,
                        ISNULL(SUM(cu.ImporteCuota), 0) AS TotalFinanciado,
                        c.Observacion,
                        c.EstadoCredito
                    FROM Personas p
                    INNER JOIN Creditos c ON p.Id_Personas = c.Id_Personas
                    LEFT JOIN Cuotas cu ON c.Id_Credito = cu.Id_Credito
                    WHERE UPPER(p.Apellido) LIKE UPPER(@Apellido) + '%'
                    GROUP BY p.Id_Personas, p.Apellido, p.Nombre, c.MontoCredito, c.Observacion, c.EstadoCredito";

                await using (var command = new SqlCommand(query, connection))
                {
                    command.CommandTimeout = 15;
                    command.Parameters.AddWithValue("@Apellido", ApellidoBusqueda.Trim());

                    var clientes = new List<ClienteEstadistica>();
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            clientes.Add(new ClienteEstadistica
                            {
                                Id = reader.GetInt64(0),
                                Apellido = reader.GetString(1),
                                Nombre = reader.GetString(2),
                                MontoCredito = reader.GetDecimal(3),
                                TotalFinanciado = reader.GetDecimal(4),
                                Observacion = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                EstadoCredito = reader.GetString(6)
                            });
                        }
                    }

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        Clientes.Clear();
                        foreach (var cliente in clientes)
                        {
                            Clientes.Add(cliente);
                        }
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
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", $"No se pudo conectar a la base de datos: {ex.Message}", "OK"));
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al buscar clientes: {ex.Message}", "OK"));
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsSearching = false);
        }
    }

    */
    private async Task BuscarClientes()
    {
        await MainThread.InvokeOnMainThreadAsync(() => IsSearching = true);

        try
        {
            if (string.IsNullOrWhiteSpace(ApellidoBusqueda))
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Clientes.Clear();
                    Application.Current.MainPage.DisplayAlert("Información", "Ingrese un apellido para buscar.", "OK");
                });
                return;
            }

            await using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                SELECT
                    p.Id_Personas,
                    p.Apellido,
                    p.Nombre,
                    c.MontoCredito,
                    ISNULL(SUM(cu.ImporteCuota), 0) AS TotalFinanciado,
                    c.Observacion,
                    c.EstadoCredito
                FROM Personas p
                INNER JOIN Creditos c ON p.Id_Personas = c.Id_Personas
                LEFT JOIN Cuotas cu ON c.Id_Credito = cu.Id_Credito
                WHERE UPPER(p.Apellido) LIKE UPPER(@Apellido) + '%'
                GROUP BY p.Id_Personas, p.Apellido, p.Nombre, c.MontoCredito, c.Observacion, c.EstadoCredito";

                await using (var command = new SqlCommand(query, connection))
                {
                    command.CommandTimeout = 15; // Este sigue siendo ajustable por comando
                    command.Parameters.AddWithValue("@Apellido", ApellidoBusqueda.Trim());

                    var clientes = new List<ClienteEstadistica>();
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            clientes.Add(new ClienteEstadistica
                            {
                                Id = reader.GetInt64(0),
                                Apellido = reader.GetString(1),
                                Nombre = reader.GetString(2),
                                MontoCredito = reader.GetDecimal(3),
                                TotalFinanciado = reader.GetDecimal(4),
                                Observacion = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                EstadoCredito = reader.GetString(6)
                            });
                        }
                    }

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        Clientes.Clear();
                        foreach (var cliente in clientes)
                        {
                            Clientes.Add(cliente);
                        }
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
                await Application.Current.MainPage.DisplayAlert("Error de Conexión", $"No se pudo conectar a la base de datos: {ex.Message}", "OK"));
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al buscar clientes: {ex.Message}", "OK"));
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsSearching = false);
        }
    }














    private async void ClienteSeleccionadoAction()
    {
        if (ClienteSeleccionado != null)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Application.Current.MainPage.DisplayAlert(
                    "Cliente Seleccionado",
                    $"Nombre: {ClienteSeleccionado.NombreCompleto}\n" +
                    $"Monto Crédito: {ClienteSeleccionado.MontoCredito:C}\n" +
                    $"Total Financiado: {ClienteSeleccionado.TotalFinanciado:C}\n" +
                    $"Estado: {ClienteSeleccionado.EstadoCredito}",
                    "OK"));
        }
    }
}