using PrestamoApp;

namespace PrestamoApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
//            MainPage = new MainPage();
            //MainPage = new NavigationPage(new MainPage());
            MainPage = new NavigationPage(new IntroPage());

           // MainPage = new NavigationPage(new LoginPage());


        }

    }
}




