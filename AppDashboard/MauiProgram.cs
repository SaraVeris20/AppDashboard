using AppDashboard.Data;
using AppDashboard.Services;
using AppDashboard.ViewModels;
using AppDashboard.Views;
using Microsoft.Extensions.Logging;
using System;

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

            // Registrar páginas
            builder.Services.AddTransient<AreaAdministrativaPage>();

            // Registrar ViewModels
            builder.Services.AddTransient<AreaAdministrativaViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}