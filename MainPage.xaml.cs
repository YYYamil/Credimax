namespace PrestamoApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
          //  StartTimer();
        }



        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await Task.Delay(3000); // Esperar 3 segundos

            // Cambiar a MenuPage
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }





    }

}
