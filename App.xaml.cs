

namespace PlayWithMaps;

public partial class App : Application
{
    private readonly IServiceProvider serviceProvider;
    public App(IServiceProvider serviceProvider)
    {
		InitializeComponent();
        this.serviceProvider = serviceProvider;
        SetupDb().Wait();
        MainPage = new AppShell();
	}

    async Task SetupDb()
    {
        using (var scope = serviceProvider.CreateScope())
        {
            await using var appContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            await appContext.Database.EnsureCreatedAsync();
        }
    }
}

