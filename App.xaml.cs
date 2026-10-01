using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using _01S.Data;
using _01S.Services;
using _01S.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _01S
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(
                    (context, services) =>
                    {
                        // 1. База данных SQLite
                        services.AddDbContext<ApplicationDbContext>(options =>
                            options.UseSqlite("Data Source=app.db")
                        );

                        // 2. Сервисы
                        services.AddSingleton<INavigationService, NavigationService>();

                        // 3. ViewModels
                        services.AddSingleton<MainViewModel>();
                        services.AddTransient<HomeViewModel>();
                        services.AddTransient<ProductsViewModel>();
                        services.AddTransient<AddProductViewModel>();
                        services.AddTransient<EditProductViewModel>();
                        services.AddTransient<DocumentsViewModel>();
                        services.AddTransient<DocumentDetailsViewModel>();

                        // 4. Окна
                        services.AddSingleton<MainWindow>();
                    }
                )
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            // Автоматическое создание локальной базы SQLite при первом запуске
            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
            }

            // Устанавливаем русскую культуру по умолчанию для всех WPF-привязок
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.GetCultureInfo("ru-RU").IetfLanguageTag)
                )
            );

            // Показ главного окна
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }
            base.OnExit(e);
        }
    }
}
