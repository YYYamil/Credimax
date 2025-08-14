using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using System.Linq;



namespace PrestamoApp
{
    public partial class ZonaPage : ContentPage
    {
        private readonly ZonaViewModel _viewModel;

        public ZonaPage(string zona)
        {
            InitializeComponent();
            _viewModel = new ZonaViewModel(zona);
            _viewModel.OnNavigateToHojaDeRuta += async (sender, args) =>
            {
                await Navigation.PushAsync(new HojaDeRuta(args.IdPersona, args.FormaPago));
            };
            BindingContext = _viewModel;
        }

        // Clase para representar un cliente
        public class Cliente
        {
            public long Id_Personas { get; set; }
            public string Apellido { get; set; }
            public string Nombre { get; set; }
            public string Direccion { get; set; }
            public string Cel { get; set; }
            public string Zona { get; set; }
            public string FormaPago { get; set; }

            public string NombreCompleto => $"{Apellido}, {Nombre}";
        }

        // ViewModel para manejar la lógica
        public class ZonaViewModel : BindableObject
        {
            private string _zona;
            private bool _isBusy;
            private string _formaPagoSeleccionada = "Todos";

            public class NavigateToHojaDeRutaEventArgs : EventArgs
            {
                public long IdPersona { get; set; }
                public string FormaPago { get; set; }
            }

            public event EventHandler<NavigateToHojaDeRutaEventArgs> OnNavigateToHojaDeRuta;

            public string ZonaTitle => $"Clientes - Zona {_zona}";
            public ObservableCollection<Cliente> Clientes { get; } = new();
            public List<string> FormasPago { get; } = new List<string> { "Todos", "Mensual", "Quincenal", "Semanal" };

            public ICommand LoadClientesCommand { get; }
            public ICommand VerClienteCommand { get; }

            public bool IsBusy
            {
                get => _isBusy;
                set
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }

            public string FormaPagoSeleccionada
            {
                get => _formaPagoSeleccionada;
                set
                {
                    if (_formaPagoSeleccionada != value)
                    {
                        _formaPagoSeleccionada = value;
                        OnPropertyChanged();
                        LoadClientesCommand.Execute(null);
                    }
                }
            }

            public ZonaViewModel(string zona)
            {
                _zona = zona;
                LoadClientesCommand = new Command(async () => await CargarClientes());
                VerClienteCommand = new Command<Cliente>(VerCliente);

                LoadClientesCommand.Execute(null);
            }

            //private async Task CargarClientes()
            //{
            //    try
            //    {
            //        IsBusy = true;
            //        Clientes.Clear();

            //        string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";
            //        var clientesList = new List<Cliente>();

            //        using (SqlConnection connection = new SqlConnection(connectionString))
            //        {
            //            await connection.OpenAsync();

            //            string query;
            //            if (FormaPagoSeleccionada == "Todos")
            //            {
            //                query = "SELECT DISTINCT Id_Personas, Apellido, Nombre, Direccion, Cel, Zona FROM Personas WHERE Zona = @Zona ORDER BY Apellido";
            //            }
            //            else
            //            {
            //                query = @"SELECT DISTINCT Personas.Id_Personas, Apellido, Nombre, Direccion, Cel, Zona, Creditos.FormaPago 
            //                          FROM personas, creditos
            //                          WHERE creditos.id_personas = Personas.Id_Personas
            //                          AND Creditos.EstadoCredito = 'activo'
            //                          AND Creditos.FormaPago = @formapago
            //                          AND Zona = @Zona
            //                          ORDER BY Apellido";
            //            }

            //            using (SqlCommand command = new SqlCommand(query, connection))
            //            {
            //                command.Parameters.AddWithValue("@Zona", _zona);

            //                if (FormaPagoSeleccionada != "Todos")
            //                {
            //                    command.Parameters.AddWithValue("@formapago", FormaPagoSeleccionada);
            //                }

            //                using (SqlDataReader reader = await command.ExecuteReaderAsync())
            //                {
            //                    while (await reader.ReadAsync())
            //                    {
            //                        clientesList.Add(new Cliente
            //                        {
            //                            Id_Personas = (long)reader["Id_Personas"],
            //                            Apellido = reader["Apellido"].ToString(),
            //                            Nombre = reader["Nombre"].ToString(),
            //                            Direccion = reader["Direccion"].ToString(),
            //                            Cel = reader["Cel"].ToString(),
            //                            Zona = reader["Zona"].ToString(),
            //                            FormaPago = FormaPagoSeleccionada == "Todos" ? "" : reader["FormaPago"].ToString()
            //                        });
            //                    }
            //                }
            //            }
            //        }

            //        // Group by Id_Personas and select the first occurrence to ensure uniqueness
            //        var clientesUnicos = clientesList
            //            .GroupBy(c => c.Id_Personas)
            //            .Select(g => g.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre).First())
            //            .OrderBy(c => c.Apellido)
            //            .ThenBy(c => c.Nombre)
            //            .ToList();

            //        foreach (var cliente in clientesUnicos)
            //        {
            //            Clientes.Add(cliente);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            //    }
            //    finally
            //    {
            //        IsBusy = false;
            //    }
            //}

            private async Task CargarClientes()
            {
                try
                {
                    IsBusy = true;
                    Clientes.Clear();

                    string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";
                    var clientesList = new List<Cliente>();

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        string query;
                        if (FormaPagoSeleccionada == "Todos")
                        {
                            query = "SELECT DISTINCT Id_Personas, Apellido, Nombre, Direccion, Cel, Zona FROM Personas WHERE Zona = @Zona ORDER BY Apellido";
                        }
                        else
                        {
                            query = @"SELECT DISTINCT 
                            P.Id_Personas, 
                            P.Apellido, 
                            P.Nombre, 
                            P.Direccion, 
                            P.Cel, 
                            P.Zona, 
                            C.FormaPago 
                          FROM Personas P
                          INNER JOIN Creditos C ON C.Id_Personas = P.Id_Personas
                          WHERE C.EstadoCredito = 'activo'
                            AND C.FormaPago = @formapago
                            AND P.Zona = @Zona
                          ORDER BY P.Apellido";
                        }

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Zona", _zona);

                            if (FormaPagoSeleccionada != "Todos")
                            {
                                command.Parameters.AddWithValue("@formapago", FormaPagoSeleccionada);
                            }

                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    clientesList.Add(new Cliente
                                    {
                                        Id_Personas = (long)reader["Id_Personas"],
                                        Apellido = reader["Apellido"].ToString(),
                                        Nombre = reader["Nombre"].ToString(),
                                        Direccion = reader["Direccion"].ToString(),
                                        Cel = reader["Cel"].ToString(),
                                        Zona = reader["Zona"].ToString(),
                                        FormaPago = FormaPagoSeleccionada == "Todos" ? "" : reader["FormaPago"].ToString()
                                    });
                                }
                            }
                        }
                    }

                    // Eliminar duplicados basados en Id_Personas
                    //var clientesUnicos = clientesList
                    //.GroupBy(c => c.Id_Personas)
                    //.Select(g => g.First())
                    //.OrderBy(c => c.Apellido)
                    //.ThenBy(c => c.Nombre)
                    //.ToList();

                    Clientes.Clear();

                    foreach (var cliente in clientesList)
                    {
                        Clientes.Add(cliente);
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

            private void VerCliente(Cliente cliente)
            {
                if (cliente != null)
                {
                    OnNavigateToHojaDeRuta?.Invoke(this, new NavigateToHojaDeRutaEventArgs
                    {
                        IdPersona = cliente.Id_Personas,
                        FormaPago = cliente.FormaPago
                    });
                }
            }
        }
    }
}