using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using SQLite;
using ArcsomAssetManagement.Client.Models;

namespace ArcsomAssetManagement.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            builder.Services.AddSingleton<MainPageModel>();
            //builder.Services.AddSingleton<ProductRepository>();
            builder.Services.AddSingleton<ProductRepository2>();
            builder.Services.AddSingleton<ProductListPageModel>();
            //builder.Services.AddSingleton<ManufacturerRepository>();
            builder.Services.AddSingleton<ManufacturerListPageModel>();
            builder.Services.AddTransient<SyncService<Manufacturer>>();
            
            builder.Services.AddSingleton<ProductOfflineRepository>();

            builder.Services.AddSingleton<SQLiteAsyncConnection>(provider =>
            {
                return new SQLiteAsyncConnection(Constants.DatabasePath);
            });

            builder.Services.AddHttpClient();

            builder.Services.AddSingleton<IOfflineRepository<Manufacturer>>(provider =>
            {
                var sqliteConnection = provider.GetRequiredService<SQLiteAsyncConnection>();
                return new OfflineRepository<Manufacturer>(sqliteConnection);
            });

            builder.Services.AddSingleton<IOnlineRepository<Manufacturer>>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var apiUrl = Environment.GetEnvironmentVariable("API_URL") + "/api/manufacturer" ?? string.Empty;

                return new OnlineRepository<Manufacturer>(httpClientFactory.CreateClient(), apiUrl);
            });

            builder.Services.AddSingleton<IRepository<Manufacturer>>(provider =>
            {
                var onlineRepository = provider.GetRequiredService<IOnlineRepository<Manufacturer>>();
                var offlineRepository = provider.GetRequiredService<IOfflineRepository<Manufacturer>>();

                return new SyncRepository<Manufacturer>(onlineRepository, offlineRepository);
            });


            builder.Services.AddSingleton<IOfflineRepository<Product>>(provider =>
            {
                var sqliteConnection = provider.GetRequiredService<SQLiteAsyncConnection>();
                return new OfflineRepository<Product>(sqliteConnection);
            });

            builder.Services.AddSingleton<IOnlineRepository<Product>>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var apiUrl = Environment.GetEnvironmentVariable("API_URL") + "/api/product" ?? string.Empty;

                return new OnlineRepository<Product>(httpClientFactory.CreateClient(), apiUrl);
            });

            builder.Services.AddSingleton<IRepository<Product>>(provider =>
            {
                var onlineRepository = provider.GetRequiredService<IOnlineRepository<Product>>();
                var offlineRepository = provider.GetRequiredService<IOfflineRepository<Product>>();

                return new SyncRepository<Product>(onlineRepository, offlineRepository);
            });

            //builder.Services.AddTransient<IRepository<Manufacturer>, OfflineRepository<Manufacturer>>();

            builder.Services.AddTransientWithShellRoute<ManufacturerDetailPage, ManufacturerDetailPageModel>("manufacturer");
            builder.Services.AddTransientWithShellRoute<ProductDetailPage, ProductDetailPageModel>("product");

            //builder.Services.AddSingleton<ProjectRepository>();
            //builder.Services.AddSingleton<TaskRepository>();
            //builder.Services.AddSingleton<CategoryRepository>();
            //builder.Services.AddSingleton<TagRepository>();
            builder.Services.AddSingleton<SeedDataService>();
            builder.Services.AddSingleton<ModalErrorHandler>();
            //builder.Services.AddSingleton<ProjectListPageModel>();
            builder.Services.AddSingleton<ManageMetaPageModel>();

            //builder.Services.AddTransientWithShellRoute<ProjectDetailPage, ProjectDetailPageModel>("project");
            //builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");

            InitializeDatabaseAsync(builder.Services.BuildServiceProvider());

            return builder.Build();

        }
        private static void InitializeDatabaseAsync(IServiceProvider services)
        {
            var database = services.GetRequiredService<SQLiteAsyncConnection>();
            database.CreateTableAsync<Manufacturer>();
            database.CreateTableAsync<SyncQueueItem>();

            // TODO: Add other tables here
        }
    }
}
