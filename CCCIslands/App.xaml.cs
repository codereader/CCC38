using System.Windows;

namespace CCCIslands;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainViewModel = new MainViewModel();
        var window = new MainWindow()
        {
            DataContext = mainViewModel
        };
        window.Show();
    }
}
