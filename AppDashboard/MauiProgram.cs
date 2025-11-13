using AppDashboard.Data;
using AppDashboard.Services;
using AppDashboard.ViewModels;
using AppDashboard.Views;
using Microsoft.Extensions.Logging;

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

            // Registrar DbContext
            builder.Services.AddSingleton<AppDbContext>();

            // Registrar serviços
            builder.Services.AddSingleton<UsuarioService>();

            // Registrar ViewModels
            builder.Services.AddTransient<AreaAdministrativaViewModel>();
            builder.Services.AddTransient<AdicionarUsuarioViewModel>();

            // Registrar páginas com injeção de dependência
            builder.Services.AddTransient<AreaAdministrativaPage>();
            builder.Services.AddTransient<AdicionarUsuarioPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}