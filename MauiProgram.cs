using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PlayWithMaps.Db;
using PlayWithMaps.Repositories;

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

        builder.Services.AddSingleton<IPath, DbPath>();
        builder.Services.AddDbContext<MyDbContext>();
        builder.Services.AddSingleton<IPositionRepository, PositionRepository>();
        builder.Services.AddSingleton<IPositionRecRepository, PositionRecRepository>();
        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

