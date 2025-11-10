using Microsoft.Extensions.Logging;
using AppDashboard.Views;
using AppDashboard.ViewModels;
using AppDashboard.Services;

namespace AppDashboard
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Registrar Serviço como Singleton (instância única)
            builder.Services.AddSingleton<UsuarioService>();

            // Registrar Páginas
            builder.Services.AddTransient<AreaAdministrativaPage>();
            builder.Services.AddTransient<AdicionarUsuarioPage>();

            // Registrar ViewModels
            builder.Services.AddTransient<AreaAdministrativaViewModel>();
            builder.Services.AddTransient<AdicionarUsuarioViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}