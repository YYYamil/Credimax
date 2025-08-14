namespace PrestamoApp;

public class ClienteEstadistica
{
    public long Id { get; set; }
    public string Apellido { get; set; }
    public string Nombre { get; set; }
    public string NombreCompleto => $"{Apellido}, {Nombre}";
    public decimal MontoCredito { get; set; }
    public decimal TotalFinanciado { get; set; }
    public string Observacion { get; set; }
    public string EstadoCredito { get; set; }
}