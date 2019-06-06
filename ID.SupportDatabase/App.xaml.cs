using System;
using System.Windows;
using ID.SupportDatabase.Services;
using ID.SupportDatabase.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ID.SupportDatabase
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var provider = BuildProvider();

            var connectionDialog = new ConnectionDialog
            {
                DataContext = provider.GetRequiredService<ConnectionDialogViewModel>()
            };
            if (connectionDialog.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            StartupUri = new Uri("/ID.SupportDatabase;component/Views/RootWindow.xaml", UriKind.Relative);

            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private IServiceProvider BuildProvider()
        {
            var serivces = new ServiceCollection();
            serivces.AddTransient<IStorageService, IsolatedStorageService>();
            serivces.AddSingleton<ConnectionDialogViewModel>();

            return serivces.BuildServiceProvider();
        }
    }
}
