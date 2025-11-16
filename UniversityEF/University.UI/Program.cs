using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Infrastructure.Data;
using University.UI.Configuration;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        ServiceConfiguration.ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UniversityDbContext>();
            await context.Database.EnsureCreatedAsync();
        }

        TGuiApp.Init();
        try
        {
            var app = new MainWindow(serviceProvider);
            TGuiApp.Run(app);
        }
        finally
        {
            TGuiApp.Shutdown();
        }
    }
}
