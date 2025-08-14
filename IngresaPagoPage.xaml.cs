using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data; // <-- agregado para tipar parámetros (Date/DateTime)

namespace PrestamoApp;

public partial class IngresaPagoPage : ContentPage
{
    //private string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";
    private string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";
    private long idPersonaSeleccionada;
    private long idCreditoSeleccionado;
    private long idCuotaSeleccionada;

    public IngresaPagoPage()
    {
        InitializeComponent();
    }

    private async void OnBuscarClientesClicked(object sender, EventArgs e)
    {
        string apellido = txtApellido.Text?.Trim() ?? "";

        // Limpiar primero
        pickerClientes.Items.Clear();
        pickerClientes.SelectedIndex = -1;

        var clientes = await BuscarClientesPorApellido(apellido);

        // Actualizar en el hilo principal
        Device.BeginInvokeOnMainThread(() =>
        {
            pickerClientes.Items.Clear();
            foreach (var c in clientes)
            {
                pickerClientes.Items.Add($"{c.Id}| {c.NombreCompleto}");
            }

            if (clientes.Count > 0)
            {
                pickerClientes.Title = $"ENCONTRTADOS: {clientes.Count} CLIENTES ";
            }
            else
            {
                pickerClientes.Title = "No se encontraron clientes";
            }
        });
    }

    private async void OnClienteSeleccionado(object sender, EventArgs e)
    {
        if (pickerClientes.SelectedIndex == -1) return;

        string seleccionado = pickerClientes.SelectedItem.ToString();
        idPersonaSeleccionada = long.Parse(seleccionado.Split('|')[0]);

        pickerCreditos.Items.Clear();
        var creditos = await ObtenerCreditosVigentes(idPersonaSeleccionada);

        foreach (var c in creditos)
            pickerCreditos.Items.Add($"{c.IdCredito}| {c.TipoCredito}| {c.Observacion}| ${c.MontoCredito}");

        pickerCreditos.SelectedIndex = -1;
        entryMontoCuota.Text = "";

        // Mostrar la cantidad de créditos encontrados
        lblCantidadCreditos.IsVisible = true;
        lblCantidadCreditos.Text = creditos.Count == 0
            ? "Este cliente no tiene créditos activos"
            : $"Este cliente tiene {creditos.Count} crédito(s)";
    }

    private async void OnCreditoSeleccionado(object sender, EventArgs e)
    {
        if (pickerCreditos.SelectedIndex == -1) return;

        string seleccionado = pickerCreditos.SelectedItem.ToString();
        idCreditoSeleccionado = long.Parse(seleccionado.Split('|')[0]);

        var cuota = await ObtenerProximaCuota(idCreditoSeleccionado);

        if (cuota.IdCuota == 0)
        {
            await DisplayAlert("Aviso", "Este crédito no tiene cuotas pendientes.", "OK");
            entryMontoCuota.Text = "";
            return;
        }

        idCuotaSeleccionada = cuota.IdCuota;
        entryMontoCuota.Text = cuota.ImporteCuota.ToString("0.00");
        entryMontoPagado.Text = cuota.ImporteCuota.ToString("0.00");
    }

    private async void OnRegistrarPagoClicked(object sender, EventArgs e)
    {
        if (idCreditoSeleccionado == 0)
        {
            await DisplayAlert("Error", "Debe seleccionar un crédito válido.", "OK");
            return;
        }

        if (!decimal.TryParse(entryMontoPagado.Text, out decimal montoPagado) || montoPagado <= 0)
        {
            await DisplayAlert("Error", "Monto ingresado no válido.", "OK");
            return;
        }

        DateTime fechaPago = datePickerPago.Date;

        bool exito = await RegistrarPago(idCreditoSeleccionado, montoPagado, fechaPago);

        if (exito)
        {
            await DisplayAlert("Éxito", "Pago registrado correctamente.", "OK");
            entryMontoPagado.Text = "";
            entryMontoCuota.Text = "";
            pickerCreditos.SelectedIndex = -1;
            idCuotaSeleccionada = 0;

            // Actualizar la próxima cuota mostrada
            if (idCreditoSeleccionado != 0)
            {
                var cuota = await ObtenerProximaCuota(idCreditoSeleccionado);
                entryMontoCuota.Text = cuota.IdCuota == 0 ? "" : cuota.ImporteCuota.ToString("0.00");
            }
        }
        else
        {
            await DisplayAlert("Error", "Hubo un problema al registrar el pago.", "OK");
        }
    }

    // === FUNCIONES DE CONEXIÓN A DB ===

    private async Task<List<(long Id, string NombreCompleto)>> BuscarClientesPorApellido(string apellido)
    {
        var clientes = new List<(long, string)>();
        using SqlConnection conn = new(connectionString);
        await conn.OpenAsync();
        string query = "SELECT Id_Personas, Apellido + ', ' + Nombre AS NombreCompleto FROM Personas WHERE Apellido LIKE @apellido + '%'";
        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@apellido", apellido);
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            clientes.Add((reader.GetInt64(0), reader.GetString(1)));
        return clientes;
    }

    private async Task<List<(long IdCredito, string TipoCredito, decimal MontoCredito, string Observacion)>> ObtenerCreditosVigentes(long idPersona)
    {
        var lista = new List<(long, string, decimal, string)>();
        using SqlConnection conn = new(connectionString);
        await conn.OpenAsync();
        string query = "SELECT Id_Credito, TipoCredito,MontoCredito,Observacion FROM Creditos WHERE Id_Personas = 44 AND EstadoCredito = 'ACTIVO'";
        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@idPersona", idPersona);
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            lista.Add((reader.GetInt64(0), reader.GetString(1), reader.GetDecimal(2), reader.GetString(3)));
        return lista;
    }

    private async Task<(long IdCuota, decimal ImporteCuota, DateTime FechaVto)> ObtenerProximaCuota(long idCredito)
    {
        using SqlConnection conn = new(connectionString);
        await conn.OpenAsync();
        string query = """
                SELECT TOP 1 Id_Cuota, ImporteCuota, FechaVto
                FROM Cuotas
                WHERE Id_Credito = @idCredito AND Estado != 'PAGADA'
                ORDER BY FechaVto ASC
                """;
        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@idCredito", idCredito);
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return (reader.GetInt64(0), reader.GetDecimal(1), reader.GetDateTime(2));
        else
            return (0, 0, DateTime.MinValue);
    }

    private async Task<bool> RegistrarPago(long idCredito, decimal montoIngresado, DateTime fechaPago)
    {
        using SqlConnection conn = new(connectionString);
        await conn.OpenAsync();

        SqlTransaction tx = conn.BeginTransaction();
        try
        {
            // Verificar si el crédito ya está cancelado
            string estadoCredito = "";
            using (SqlCommand cmdEstado = new(
                "SELECT EstadoCredito FROM Creditos WHERE Id_Credito = @idCredito",
                conn, tx))
            {
                cmdEstado.Parameters.AddWithValue("@idCredito", idCredito);
                estadoCredito = (await cmdEstado.ExecuteScalarAsync())?.ToString();
            }

            if (estadoCredito == "CANCELADO")
            {
                await DisplayAlert("Error", "No se pueden registrar pagos en un crédito cancelado", "OK");
                await tx.RollbackAsync();
                return false;
            }

            decimal montoRestante = montoIngresado;

            // Obtener todas las cuotas no pagadas ordenadas por fecha de vencimiento
            var cuotasPendientes = new List<(long IdCuota, decimal Importe, decimal SaldoPendiente)>();

            using (SqlCommand cmdCuotas = new(
                @"SELECT c.Id_Cuota, c.ImporteCuota, 
                CASE 
                    WHEN c.Estado = 'PAGADA' THEN 0
                    ELSE c.ImporteCuota - ISNULL((SELECT SUM(p.MontoPagado) FROM Pagos p WHERE p.Id_Cuota = c.Id_Cuota), 0)
                END AS SaldoPendiente
              FROM Cuotas c
              WHERE c.Id_Credito = @idCredito 
              AND c.Estado != 'PAGADA'
              ORDER BY c.FechaVto ASC", conn, tx))
            {
                cmdCuotas.Parameters.AddWithValue("@idCredito", idCredito);

                using (var reader = await cmdCuotas.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        decimal saldo = reader.GetDecimal(2);
                        if (saldo > 0)
                        {
                            cuotasPendientes.Add((
                                reader.GetInt64(0),
                                reader.GetDecimal(1),
                                saldo
                            ));
                        }
                    }
                }
            }

            if (cuotasPendientes.Count == 0)
            {
                await DisplayAlert("Información", "No hay cuotas con saldo pendiente para este crédito", "OK");
                await tx.RollbackAsync();
                return false;
            }

            // Procesar cada cuota hasta agotar el monto ingresado
            foreach (var cuota in cuotasPendientes)
            {
                if (montoRestante <= 0) break;

                decimal montoAAplicar = Math.Min(montoRestante, cuota.SaldoPendiente);

                // Registrar el pago (GUARDANDO LA FECHA SELECCIONADA)
                using (SqlCommand cmdInsert = new(
                    "INSERT INTO Pagos (FechaPago, MontoPagado, Id_Cuota) VALUES (@fecha, @monto, @idCuota)",
                    conn, tx))
                {
                    // Tipamos la fecha para evitar inferencias raras
                    cmdInsert.Parameters.Add("@fecha", SqlDbType.Date).Value = fechaPago.Date;
                    cmdInsert.Parameters.AddWithValue("@monto", montoAAplicar);
                    cmdInsert.Parameters.AddWithValue("@idCuota", cuota.IdCuota);
                    await cmdInsert.ExecuteNonQueryAsync();
                }

                // Actualizar estado de la cuota
                string nuevoEstado = (montoAAplicar >= cuota.SaldoPendiente) ? "PAGADA" : "PARCIAL";
                using (SqlCommand cmdUpdate = new(
                    "UPDATE Cuotas SET Estado = @estado WHERE Id_Cuota = @idCuota",
                    conn, tx))
                {
                    cmdUpdate.Parameters.AddWithValue("@estado", nuevoEstado);
                    cmdUpdate.Parameters.AddWithValue("@idCuota", cuota.IdCuota);
                    await cmdUpdate.ExecuteNonQueryAsync();
                }

                montoRestante -= montoAAplicar;
            }

            // Actualizar estado del crédito usando la FECHA SELECCIONADA
            await ActualizarEstadoCredito(idCredito, conn, tx, montoIngresado, fechaPago);

            if (montoRestante > 0)
            {
                await DisplayAlert("Información",
                    $"Pago aplicado a cuotas pendientes. Sobrante: {montoRestante:C}", "OK");
            }

            await tx.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            await DisplayAlert("Error", $"Error al registrar pago: {ex.Message}", "OK");
            return false;
        }
    }

    // Firma modificada para recibir la fecha del DatePicker y evitar GETDATE()
    private async Task ActualizarEstadoCredito(long idCredito, SqlConnection conn, SqlTransaction tx, decimal montoPagado, DateTime fechaPago)
    {
        // Verificar si todas las cuotas están pagadas
        using (SqlCommand cmdCheck = new(
            "SELECT COUNT(*) FROM Cuotas WHERE Id_Credito = @idCredito AND Estado != 'PAGADA'",
            conn, tx))
        {
            cmdCheck.Parameters.AddWithValue("@idCredito", idCredito);
            int cuotasPendientes = (int)await cmdCheck.ExecuteScalarAsync();

            // Usamos la fecha seleccionada (solo fecha)
            DateTime fechaActualizacion = fechaPago.Date;

            if (cuotasPendientes == 0)
            {
                // Verificar si fue un pago completo o una cancelación
                string nuevoEstado = montoPagado > 0 ? "PAGADO" : "CANCELADO";

                using (SqlCommand cmdUpdate = new(
                    "UPDATE Creditos SET EstadoCredito = @estado, FechaUltimoPago = @fecha WHERE Id_Credito = @idCredito",
                    conn, tx))
                {
                    cmdUpdate.Parameters.AddWithValue("@estado", nuevoEstado);
                    cmdUpdate.Parameters.Add("@fecha", SqlDbType.Date).Value = fechaActualizacion;
                    cmdUpdate.Parameters.AddWithValue("@idCredito", idCredito);
                    await cmdUpdate.ExecuteNonQueryAsync();
                }
            }
            else
            {
                // Actualizar solo la fecha del último pago
                using (SqlCommand cmdUpdate = new(
                    "UPDATE Creditos SET FechaUltimoPago = @fecha WHERE Id_Credito = @idCredito",
                    conn, tx))
                {
                    cmdUpdate.Parameters.Add("@fecha", SqlDbType.Date).Value = fechaActualizacion;
                    cmdUpdate.Parameters.AddWithValue("@idCredito", idCredito);
                    await cmdUpdate.ExecuteNonQueryAsync();
                }
            }
        }
    }

}
