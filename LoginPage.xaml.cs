using System.Text;
using System;
using Microsoft.Maui.Controls;
using System.Security.Cryptography;
using System.Text;
using System.Data.SqlClient;
namespace PrestamoApp;

public partial class LoginPage : ContentPage
{


    string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True"; // Ajusta esto
    public LoginPage()
	{
		InitializeComponent();
	}


    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MenuPage());
    }

    //private async void OnLoginClicked(object sender, EventArgs e)
    //{
    //    string usuario = entryUsuario.Text?.Trim();
    //    string contrasena = entryContrasena.Text;

    //    if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
    //    {
    //        lblMensaje.Text = "Ingrese usuario y contraseña";
    //        return;
    //    }

    //    string hash = CalcularHash(contrasena);

    //    using (SqlConnection conn = new SqlConnection(connectionString))
    //    {
    //        string query = "SELECT COUNT(*) FROM Usuarios WHERE Usuario = @usuario AND ContrasenaHash = @hash";
    //        SqlCommand cmd = new SqlCommand(query, conn);
    //        cmd.Parameters.AddWithValue("@usuario", usuario);
    //        cmd.Parameters.AddWithValue("@hash", hash);

    //        try
    //        {
    //            conn.Open();
    //            int count = (int)cmd.ExecuteScalar();

    //            if (count == 1)
    //            {
    //                // Usuario autenticado correctamente
    //                await Navigation.PushAsync(new MenuPage());
    //            }
    //            else
    //            {
    //                lblMensaje.Text = "Usuario o contraseña incorrectos";
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            await DisplayAlert("Error", ex.Message, "OK");
    //        }
    //    }
    //}

    private void OnOlvidasteContrasena(object sender, EventArgs e)
    {
        DisplayAlert("Recuperación", "Funcionalidad de recuperación en construcción", "OK");
    }

    private string CalcularHash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();

            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }


}












