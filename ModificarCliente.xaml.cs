using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PrestamoApp
{
    public partial class ModificarCliente : ContentPage
    {
        private ObservableCollection<ClienteModificado> clientes = new ObservableCollection<ClienteModificado>();
        private ClienteModificado selectedCliente;

        public ModificarCliente()
        {
            InitializeComponent();
            clientesList.ItemsSource = clientes;
        }

        // Clase ClienteModificado
        public class ClienteModificado : INotifyPropertyChanged
        {
            private long id;
            private string apellido;
            private string nombre;
            private string direccion;
            private string cel;
            private string zona;

            public long Id_Personas
            {
                get => id;
                set { id = value; OnPropertyChanged(); }
            }

            public string Apellido
            {
                get => apellido;
                set { apellido = value; OnPropertyChanged(); }
            }

            public string Nombre
            {
                get => nombre;
                set { nombre = value; OnPropertyChanged(); }
            }

            public string Direccion
            {
                get => direccion;
                set { direccion = value; OnPropertyChanged(); }
            }

            public string Cel
            {
                get => cel;
                set { cel = value; OnPropertyChanged(); }
            }

            public string Zona
            {
                get => zona;
                set { zona = value; OnPropertyChanged(); }
            }

            public string NombreCompleto => $"{Apellido}, {Nombre}";

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            string searchText = entrySearch.Text?.Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                await DisplayAlert("Error", "Ingrese un criterio de búsqueda.", "OK");
                return;
            }

            clientes.Clear();
            string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT Id_Personas, Apellido, Nombre, Direccion, Cel, Zona FROM Personas WHERE Apellido LIKE @Search OR Nombre LIKE @Search";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Search", $"%{searchText}%");
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                long id = reader.GetInt64(0);
                                string apellido = reader.IsDBNull(1) ? "" : reader.GetString(1);
                                string nombre = reader.IsDBNull(2) ? "" : reader.GetString(2);
                                string direccion = reader.IsDBNull(3) ? "" : reader.GetString(3);
                                string cel = reader.IsDBNull(4) ? "" : reader.GetString(4);
                                string zona = reader.IsDBNull(5) ? "" : reader.GetString(5);

                                clientes.Add(new ClienteModificado
                                {
                                    Id_Personas = id,
                                    Apellido = apellido,
                                    Nombre = nombre,
                                    Direccion = direccion,
                                    Cel = cel,
                                    Zona = zona
                                });
                            }
                        }
                    }
                }
                if (clientes.Count == 0)
                {
                    await DisplayAlert("Información", "No se encontraron clientes.", "OK");
                }
            }
            catch (SqlException ex)
            {
                await DisplayAlert("Error SQL", $"Error en la base de datos: {ex.Message}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error al buscar: {ex.Message}", "OK");
            }
        }

        private void OnClienteSelected(object sender, SelectionChangedEventArgs e)
        {
            selectedCliente = e.CurrentSelection.FirstOrDefault() as ClienteModificado;
            if (selectedCliente != null)
            {
                entryApellido.Text = selectedCliente.Apellido;
                entryNombre.Text = selectedCliente.Nombre;
                entryDireccion.Text = selectedCliente.Direccion;
                entryCelular.Text = selectedCliente.Cel;
                pickerZona.SelectedItem = selectedCliente.Zona;
                // Mantenemos selectedCliente para usarlo en el guardado
            }
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            string apellido = entryApellido.Text;
            string nombre = entryNombre.Text;
            string direccion = entryDireccion.Text;
            string cel = entryCelular.Text;
            string zona = pickerZona.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(nombre))
            {
                await DisplayAlert("Error", "Apellido y Nombre son obligatorios.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(zona))
            {
                await DisplayAlert("Error", "Debe seleccionar una Zona.", "OK");
                return;
            }

            string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query;
                    // Actualizamos selectedCliente con los valores editados antes de guardar
                    if (selectedCliente != null && selectedCliente.Id_Personas > 0)
                    {
                        selectedCliente.Apellido = apellido;
                        selectedCliente.Nombre = nombre;
                        selectedCliente.Direccion = direccion;
                        selectedCliente.Cel = cel;
                        selectedCliente.Zona = zona;

                        query = "UPDATE Personas SET Apellido = @Apellido, Nombre = @Nombre, Direccion = @Direccion, Cel = @Cel, Zona = @Zona WHERE Id_Personas = @Id";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Id", selectedCliente.Id_Personas);
                            command.Parameters.AddWithValue("@Apellido", selectedCliente.Apellido);
                            command.Parameters.AddWithValue("@Nombre", selectedCliente.Nombre);
                            command.Parameters.AddWithValue("@Direccion", selectedCliente.Direccion ?? "");
                            command.Parameters.AddWithValue("@Cel", selectedCliente.Cel ?? "");
                            command.Parameters.AddWithValue("@Zona", selectedCliente.Zona);

                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            if (rowsAffected > 0)
                            {
                                await DisplayAlert("Éxito", "Cliente actualizado correctamente.", "OK");
                                // Actualizamos la lista con los nuevos valores
                                var existingCliente = clientes.FirstOrDefault(c => c.Id_Personas == selectedCliente.Id_Personas);
                                if (existingCliente != null)
                                {
                                    existingCliente.Apellido = apellido;
                                    existingCliente.Nombre = nombre;
                                    existingCliente.Direccion = direccion;
                                    existingCliente.Cel = cel;
                                    existingCliente.Zona = zona;
                                }
                                selectedCliente = null; // Limpia la selección después de guardar
                            }
                            else
                            {
                                await DisplayAlert("Error", "No se pudo actualizar el cliente.", "OK");
                            }
                        }
                    }
                    else
                    {
                        // Solo insertamos si no hay un ID válido
                        query = "INSERT INTO Personas (Apellido, Nombre, Direccion, Cel, Zona) VALUES (@Apellido, @Nombre, @Direccion, @Cel, @Zona)";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Apellido", apellido);
                            command.Parameters.AddWithValue("@Nombre", nombre);
                            command.Parameters.AddWithValue("@Direccion", direccion ?? "");
                            command.Parameters.AddWithValue("@Cel", cel ?? "");
                            command.Parameters.AddWithValue("@Zona", zona);

                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            if (rowsAffected > 0)
                            {
                                await DisplayAlert("Éxito", "Cliente guardado correctamente.", "OK");
                            }
                            else
                            {
                                await DisplayAlert("Error", "No se pudo guardar el cliente.", "OK");
                            }
                        }
                    }

                    // Limpia los campos
                    entryApellido.Text = "";
                    entryNombre.Text = "";
                    entryDireccion.Text = "";
                    entryCelular.Text = "";
                    pickerZona.SelectedIndex = -1;
                    clientes.Clear(); // Opcional: recarga la lista si es necesario
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }
    }
}