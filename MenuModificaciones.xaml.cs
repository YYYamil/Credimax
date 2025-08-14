namespace PrestamoApp;

public partial class MenuModificaciones : ContentPage
{
    public MenuModificaciones()
    {
        InitializeComponent();
    }

    private async void OnModificarClienteClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ModificarCliente());
    }

    private async void OnModificarCreditoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ModificarCredito());
    }
}