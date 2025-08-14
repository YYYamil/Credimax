
using System.Windows.Input;
using Microsoft.Data.SqlClient;
namespace PrestamoApp;

public partial class AltaClientePage : ContentPage
{
    public ICommand NavigateToModificarCommand { get; }
    public AltaClientePage()
	{
		InitializeComponent();

        // Comando para navegar a ModificarCliente
        NavigateToModificarCommand = new Command(async () =>
        {
            await Navigation.PushAsync(new ModificarCliente());
        });

        // Asegurar que el BindingContext esté establecido
        BindingContext = this;
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

                string query = "INSERT INTO Personas (Apellido, Nombre, Direccion, Cel, Zona) VALUES (@Apellido, @Nombre, @Direccion, @Cel, @Zona)";
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

                        // Limpia los campos
                        entryApellido.Text = "";
                        entryNombre.Text = "";
                        entryDireccion.Text = "";
                        entryCelular.Text = "";
                        pickerZona.SelectedIndex = -1;
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo guardar el cliente.", "OK");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
    }






}