using System;
using System.Data.SqlClient;
using Microsoft.Maui.Controls;

namespace PrestamoApp;

public partial class PagarHojaRutaPage : ContentPage
{
    private readonly string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";
    private long idCredito;
    private decimal importeCuotaPendiente = 0;
    private long idCuotaPendiente = 0;

    public PagarHojaRutaPage(long idCredito)
    {
        InitializeComponent();
        this.idCredito = idCredito;
        CargarInformacionCredito();
    }

    private async void CargarInformacionCredito()
    {
        try
        {
            using SqlConnection conn = new(connectionString);
            await conn.OpenAsync();

            // Info del crédito
            string infoCreditoQuery = @"
                SELECT TipoCredito, MontoCredito
                FROM Creditos
                WHERE Id_Credito = @idCredito";

            using SqlCommand cmdInfo = new(infoCreditoQuery, conn);
            cmdInfo.Parameters.AddWithValue("@idCredito", idCredito);

            using SqlDataReader reader = await cmdInfo.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                string tipo = reader.GetString(0);
                decimal monto = reader.GetDecimal(1);
                lblCredito.Text = $"{tipo} - Monto: {monto:C}";
            }

            reader.Close();

            // Obtener la próxima cuota impaga o parcial
            string cuotaQuery = @"
                SELECT TOP 1 Id_Cuota, ImporteCuota
                FROM Cuotas
                WHERE Id_Credito = @idCredito AND (Estado = 'Impaga' OR Estado = 'Parcial')
                ORDER BY FechaVto ASC";

            using SqlCommand cmdCuota = new(cuotaQuery, conn);
            cmdCuota.Parameters.AddWithValue("@idCredito", idCredito);

            using SqlDataReader cuotaReader = await cmdCuota.ExecuteReaderAsync();
            if (await cuotaReader.ReadAsync())
            {
                idCuotaPendiente = cuotaReader.GetInt64(0);
                importeCuotaPendiente = cuotaReader.GetDecimal(1);
                entryMontoCuota.Text = $"{importeCuotaPendiente:C}";
            }
            else
            {
                entryMontoCuota.Text = "No hay cuotas pendientes";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnRegistrarPagoClicked(object sender, EventArgs e)
    {
        if (idCuotaPendiente == 0)
        {
            await DisplayAlert("Info", "No hay cuotas pendientes para este crédito.", "OK");
            return;
        }

        if (!decimal.TryParse(entryMontoPagado.Text, out decimal montoIngresado) || montoIngresado <= 0)
        {
            await DisplayAlert("Error", "Ingrese un monto válido.", "OK");
            return;
        }

        DateTime fechaPago = datePickerPago.Date;

        using SqlConnection conn = new(connectionString);
        await conn.OpenAsync();
        SqlTransaction tx = conn.BeginTransaction();

        try
        {
            decimal montoRestante = montoIngresado;

            while (montoRestante > 0)
            {
                // Obtener la próxima cuota impaga o parcial
                string cuotaQuery = @"
                    SELECT TOP 1 Id_Cuota, ImporteCuota
                    FROM Cuotas
                    WHERE Id_Credito = @idCredito AND (Estado = 'Impaga' OR Estado = 'Parcial')
                    ORDER BY FechaVto ASC";

                using SqlCommand cmd = new(cuotaQuery, conn, tx);
                cmd.Parameters.AddWithValue("@idCredito", idCredito);

                long idCuota = 0;
                decimal importeCuota = 0;

                using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    idCuota = reader.GetInt64(0);
                    importeCuota = reader.GetDecimal(1);
                }
                reader.Close();

                if (idCuota == 0) break;

                // Sumar pagos anteriores
                string sumaPagosQuery = "SELECT ISNULL(SUM(MontoPagado), 0) FROM Pagos WHERE Id_Cuota = @idCuota";
                using SqlCommand cmdSuma = new(sumaPagosQuery, conn, tx);
                cmdSuma.Parameters.AddWithValue("@idCuota", idCuota);
                decimal yaPagado = (decimal)await cmdSuma.ExecuteScalarAsync();

                decimal saldoCuota = importeCuota - yaPagado;
                decimal aPagar = Math.Min(montoRestante, saldoCuota);

                // Insertar pago
                string insertPago = "INSERT INTO Pagos (FechaPago, MontoPagado, Id_Cuota) VALUES (@fecha, @monto, @idCuota)";
                using SqlCommand cmdPago = new(insertPago, conn, tx);
                cmdPago.Parameters.AddWithValue("@fecha", fechaPago);
                cmdPago.Parameters.AddWithValue("@monto", aPagar);
                cmdPago.Parameters.AddWithValue("@idCuota", idCuota);
                await cmdPago.ExecuteNonQueryAsync();

                // Actualizar estado de cuota
                string nuevoEstado = (aPagar + yaPagado) >= importeCuota ? "PAGADA" : "PARCIAL";
                string updateEstado = "UPDATE Cuotas SET Estado = @estado WHERE Id_Cuota = @idCuota";
                using SqlCommand cmdEstado = new(updateEstado, conn, tx);
                cmdEstado.Parameters.AddWithValue("@estado", nuevoEstado);
                cmdEstado.Parameters.AddWithValue("@idCuota", idCuota);
                await cmdEstado.ExecuteNonQueryAsync();

                montoRestante -= aPagar;
            }

            await tx.CommitAsync();
            await DisplayAlert("Éxito", "Pago registrado correctamente.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
