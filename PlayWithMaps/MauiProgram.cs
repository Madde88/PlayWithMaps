
namespace PlayWithMaps;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.UseMauiCommunityToolkit();
        builder.ConfigureMauiHandlers(handlers =>
        {
#if IOS
            handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, Platforms.iOS.Handlers.CustomMapHandler>();
# endif
        });

        builder.Services.AddSingleton<IPath, DbPath>();
        builder.Services.AddDbContext<MyDbContext>();
        builder.Services.AddSingleton<IPositionRepository, PositionRepository>();
        builder.Services.AddSingleton<IPositionRecRepository, PositionRecRepository>();
        builder.Services.AddSingleton<IFilterLocation, FilterLocation>();
        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

