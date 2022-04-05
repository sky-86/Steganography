using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SteganographyV3
{
    public partial class App : Application
    {
        public App ()
        {
            InitializeComponent();

            var homePage = new MainPage();
            var navPage = new NavigationPage(homePage);
            MainPage = navPage;
        }

        protected override void OnStart ()
        {
        }

        protected override void OnSleep ()
        {
        }

        protected override void OnResume ()
        {
        }
    }
}

