namespace PrestamoApp;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

public partial class ListadoClientesPages : ContentPage
{
    private List<ClienteListado> clientes;
    private string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";
    private string selectedZona;

    public ListadoClientesPages()
    {
        InitializeComponent();
        LoadClientes();
    }

    private async void LoadClientes()
    {
        try
        {
            clientes = await GetClientesAsync(null); // Cargar todos los clientes inicialmente
            clientesListView.ItemsSource = clientes;

            // Cargar las zonas en el Picker
            var zonas = await GetZonasAsync();
            zonas.Insert(0, "Todas");
            zonaPicker.ItemsSource = zonas;
            zonaPicker.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task<List<string>> GetZonasAsync()
    {
        List<string> zonas = new List<string>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            SqlCommand command = new SqlCommand("SELECT DISTINCT Zona FROM Personas ORDER BY Zona", connection);
            using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    zonas.Add(reader.GetString(0));
                }
            }
        }
        return zonas;
    }

    private async Task<List<ClienteListado>> GetClientesAsync(string zona)
    {
        List<ClienteListado> clientes = new List<ClienteListado>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            string query = string.IsNullOrEmpty(zona) || zona == "Todas"
                ? "SELECT DISTINCT Id_Personas, Apellido, Nombre, Direccion, Cel, Zona FROM Personas ORDER BY Apellido"
                : "SELECT DISTINCT Id_Personas, Apellido, Nombre, Direccion, Cel, Zona FROM Personas WHERE Zona = @Zona ORDER BY Apellido";
            SqlCommand command = new SqlCommand(query, connection);

            if (!string.IsNullOrEmpty(zona) && zona != "Todas")
            {
                command.Parameters.AddWithValue("@Zona", zona);
            }

            using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    clientes.Add(new ClienteListado
                    {
                        Id = reader.GetInt64(0),
                        Apellido = reader.GetString(1),
                        Nombre = reader.GetString(2),
                        Direccion = reader.GetString(3),
                        Celular = reader.GetString(4),
                        Zona = reader.GetString(5)
                    });
                }
            }
        }
        return clientes;
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchTerm = e.NewTextValue?.ToLower() ?? string.Empty;
        var filteredClientes = clientes.Where(c => c.Apellido.ToLower().Contains(searchTerm)).ToList();
        clientesListView.ItemsSource = filteredClientes;
    }

    private async void OnZonaSelected(object sender, EventArgs e)
    {
        try
        {
            selectedZona = zonaPicker.SelectedItem?.ToString();
            clientes = await GetClientesAsync(selectedZona);
            OnSearchTextChanged(searchBar, new TextChangedEventArgs(searchBar.Text, searchBar.Text));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnClienteSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null) return;

        var cliente = e.SelectedItem as ClienteListado;
        await Navigation.PushAsync(new ListadoCredito(cliente.Id));

        clientesListView.SelectedItem = null;
    }

    


}

public class ClienteListado
{
    public long Id { get; set; }
    public string Apellido { get; set; }
    public string Nombre { get; set; }
    public string Direccion { get; set; }
    public string Celular { get; set; }
    public string Zona { get; set; }

    public string NombreCompleto => $"{Apellido}, {Nombre}";
}
