namespace PrestamoApp;
using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.IO;


public partial class IngresarPago : ContentPage
{
    private readonly HojaRutaModel credito;
    private int idCuota;
    private decimal importeCuota;
    private DateTime fechaVto;
    public event EventHandler<bool> PagoFinalizado;


    public IngresarPago(HojaRutaModel credito)
	{
		InitializeComponent();
        this.credito = credito;
        CargarDatosCredito();
        BuscarCuotaPendiente();

    }


    private void CargarDatosCredito()
    {
        lblNombre.Text = credito.Nombre;
        lblTipoCredito.Text = credito.TipoCredito;
    }

    private void BuscarCuotaPendiente()
    {
        string connectionString = "workstation id = Credimax.mssql.somee.com; packet size = 4096; user id = mgonzy2_SQLLogin_1; pwd = s48wgh6m39; data source = Credimax.mssql.somee.com; persist security info = False; initial catalog = Credimax; TrustServerCertificate = True";
        string query = @"
            SELECT TOP 1 Id_Cuota, ImporteCuota, FechaVto
            FROM Cuotas
            WHERE Id_Credito = @IdCredito AND Estado != 'PAGADA'
            ORDER BY FechaVto ASC";

        using SqlConnection conn = new(connectionString);
        conn.Open();
        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@IdCredito", credito.Id_Credito);

        using SqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            idCuota = Convert.ToInt32(reader["Id_Cuota"]);
            importeCuota = Convert.ToDecimal(reader["ImporteCuota"]);
            fechaVto = Convert.ToDateTime(reader["FechaVto"]);

            lblImporteCuota.Text = importeCuota.ToString("C");
            lblFechaVto.Text = fechaVto.ToString("dd/MM/yyyy");
        }
        else
        {
            DisplayAlert("Info", "No hay cuotas pendientes para este crédito.", "OK");
            this.Navigation.PopAsync();
        }
    }

    private async void OnAceptarPagoClicked(object sender, EventArgs e)
    {
        if (!decimal.TryParse(txtMonto.Text, out decimal montoPagado) || montoPagado <= 0)
        {
            await DisplayAlert("Error", "Ingrese un monto válido.", "OK");
            return;
        }

        bool confirmar = await DisplayAlert("Confirmar Pago", $"¿Desea registrar el pago de {montoPagado:C}?", "Sí", "No");
        if (!confirmar)
            return;

        string nuevoEstado = montoPagado >= importeCuota ? "PAGADA" : "PARCIAL";

        try
        {
            string connectionString = "workstation id = Credimax.mssql.somee.com; packet size = 4096; user id = mgonzy2_SQLLogin_1; pwd = s48wgh6m39; data source = Credimax.mssql.somee.com; persist security info = False; initial catalog = Credimax; TrustServerCertificate = True";
            using SqlConnection conn = new(connectionString);
            conn.Open();

            // Insertar en Pagos
            string insertPago = "INSERT INTO Pagos (FechaPago, MontoPagado, Id_Cuota) VALUES (@fecha, @monto, @idCuota)";
            using SqlCommand cmdInsert = new(insertPago, conn);
            cmdInsert.Parameters.AddWithValue("@fecha", DateTime.Now);
            cmdInsert.Parameters.AddWithValue("@monto", montoPagado);
            cmdInsert.Parameters.AddWithValue("@idCuota", idCuota);
            cmdInsert.ExecuteNonQuery();

            // Actualizar estado de la cuota
            string updateCuota = "UPDATE Cuotas SET Estado = @estado WHERE Id_Cuota = @idCuota";
            using SqlCommand cmdUpdate = new(updateCuota, conn);
            cmdUpdate.Parameters.AddWithValue("@estado", nuevoEstado);
            cmdUpdate.Parameters.AddWithValue("@idCuota", idCuota);
            cmdUpdate.ExecuteNonQuery();



            /*await DisplayAlert("Éxito", "Pago registrado correctamente.", "OK");
            await Navigation.PopAsync();*/

            await DisplayAlert("Éxito", "Pago registrado correctamente.", "OK");
            PagoFinalizado?.Invoke(this, true); // Notificamos al llamador
            await Navigation.PopAsync();



        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al registrar el pago: {ex.Message}", "OK");
        }
    }
}






