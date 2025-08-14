using System.Collections.ObjectModel;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
//using Intents;

namespace PrestamoApp;

public partial class CreditosPage : ContentPage
{

    private string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";

    public ObservableCollection<CreditoInfo> Creditos { get; set; } = new();

    public CreditosPage(long idPersona)
	{
		InitializeComponent();
        BindingContext = this;



        CargarCreditosActivos(idPersona);

     


    }


    


    private void CargarCreditosActivos(long idPersona)
    {
        try
        {
            Creditos.Clear(); // Limpiar antes de cargar

            using SqlConnection connection = new(connectionString);
            string query = @"
        SELECT 
            C.TipoCredito,
            C.Observacion,
            SUM(Cu.ImporteCuota) AS ImporteCuotaTotal
        FROM Creditos C
        INNER JOIN Cuotas Cu ON Cu.Id_Credito = C.Id_Credito
        WHERE C.Id_Personas = @IdPersona AND C.EstadoCredito = 'ACTIVO'
        GROUP BY C.TipoCredito, C.Observacion"; // tu query actual
            SqlCommand cmd = new(query, connection);
            cmd.Parameters.AddWithValue("@IdPersona", idPersona);

            connection.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Creditos.Add(new CreditoInfo
                {
                    TipoCredito = reader["TipoCredito"].ToString(),
                    Observacion = reader["Observacion"].ToString(),
                    ImporteCuota = reader.GetDecimal(reader.GetOrdinal("ImporteCuotaTotal"))
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar créditos: {ex.Message}");
           
        }
    }






    private async void OnCreditoSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CreditoInfo credito)
        {
            // Aquí podrías navegar a otra página con más detalles
            await DisplayAlert("Crédito seleccionado", $"Tipo: {credito.TipoCredito}\nObs: {credito.Observacion}", "OK");

            // Des-seleccionamos el ítem para evitar que quede marcado
            ((CollectionView)sender).SelectedItem = null;
        }
    }


}











