namespace PrestamoApp;

public partial class MenuConsulta : ContentPage
{
	public MenuConsulta()
	{
		InitializeComponent();
	}


    /*private async void OnListadoClicked(object sender, EventArgs e)
    {
       await Navigation.PushAsync(new ListadoClientesPages());
    }*/
    private async void OnContabilidadClicked(object sender, EventArgs e)
    {
         await Navigation.PushAsync(new ContabilidadPage());
    }

}