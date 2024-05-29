using CommunityToolkit.Maui;
using MauiBleRfIdBridgeExampleApp.Services;
using MauiBleRfIdBridgeExampleApp.Services.Interfaces;
using MauiBleRfIdBridgeExampleApp.ViewModels;
using MauiBleRfIdBridgeExampleApp.Views;
using Microsoft.Extensions.Logging;

namespace MauiBleRfIdBridgeExampleApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                    .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddTransient<RfidReaderView>();
            builder.Services.AddSingleton<RfidReaderViewModel>();
            builder.Services.AddSingleton<IRfidSensorBle, RfidSensorBle>();

            return builder.Build();
        }
    }
}
