namespace PrestamoApp;

public partial class IntroPage : ContentPage
{
	public IntroPage()
	{
		InitializeComponent();
        LanzarSiguientePantalla();
    }


    private async void LanzarSiguientePantalla()
    {
        await Task.Delay(3000); // Espera 10 segundos
        await Navigation.PushAsync(new MainPage());
    }


}