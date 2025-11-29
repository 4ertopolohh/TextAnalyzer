using System.Configuration;
using System.Data;
using System.Windows;
using TextAnalyzer.Services;

namespace TextAnalyzer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DatabaseInitializer.Initialize();
            base.OnStartup(e);
        }
    }
}
