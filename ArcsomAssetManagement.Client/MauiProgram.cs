using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using SQLite;
using ArcsomAssetManagement.Client.Models;
using ArcsomAssetManagement.Client.DTOs.Business;
using ArcsomAssetManagement.Client.DTOs.Mapping;
using AutoMapper;

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
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Services.AddSingleton<MainPageModel>();
            //builder.Services.AddSingleton<ProductRepository>();
            builder.Services.AddSingleton<ProductListPageModel>();
            //builder.Services.AddSingleton<ManufacturerRepository>();
            builder.Services.AddSingleton<ManufacturerListPageModel>();
            builder.Services.AddSingleton<SyncService<Manufacturer, ManufacturerDto>>(provider =>
            {
                var sqliteConnection = provider.GetRequiredService<SQLiteAsyncConnection>();
                var onlineRepository = provider.GetRequiredService<ManufacturerOnlineRepository>();
                var mapper = provider.GetRequiredService<IMapper>();

                return new SyncService<Manufacturer, ManufacturerDto>(sqliteConnection, onlineRepository, mapper);
            });

            builder.Services.AddTransient<SQLiteAsyncConnection>(provider =>
            {
                return new SQLiteAsyncConnection(Constants.DatabasePath);
            });

            builder.Services.AddHttpClient();

            builder.Services.AddSingleton(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var apiUrl = Environment.GetEnvironmentVariable("API_URL") + "/api/manufacturer" ?? string.Empty;

                return new ManufacturerOnlineRepository(httpClientFactory.CreateClient(), apiUrl);
            });

            builder.Services.AddSingleton<ManufacturerOfflineRepository>(provider =>
            {
                var sqliteConnection = provider.GetRequiredService<SQLiteAsyncConnection>();
                var logger = provider.GetRequiredService<ILogger<ManufacturerOfflineRepository>>();
                return new ManufacturerOfflineRepository(logger, sqliteConnection);
            });

            builder.Services.AddSingleton<IRepository<Manufacturer>>(provider =>
            {
                var onlineRepository = provider.GetRequiredService<ManufacturerOnlineRepository>();
                var offlineRepository = provider.GetRequiredService<ManufacturerOfflineRepository>();
                var mapper = provider.GetRequiredService<IMapper>();

                return new SyncRepository<Manufacturer, ManufacturerDto>(onlineRepository, offlineRepository, mapper);
            });

            builder.Services.AddSingleton(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var apiUrl = Environment.GetEnvironmentVariable("API_URL") + "/api/product" ?? string.Empty;

                return new ProductOnlineRepository(httpClientFactory.CreateClient(), apiUrl);
            });

            builder.Services.AddSingleton<ProductOfflineRepository>(provider =>
            {
                var sqliteConnection = provider.GetRequiredService<SQLiteAsyncConnection>();
                var logger = provider.GetRequiredService<ILogger<ProductOfflineRepository>>();
                return new ProductOfflineRepository(logger, sqliteConnection);
            });

            builder.Services.AddSingleton<IRepository<Product>>(provider =>
            {
                var onlineRepository = provider.GetRequiredService<ProductOnlineRepository>();
                var offlineRepository = provider.GetRequiredService<ProductOfflineRepository>();
                var mapper = provider.GetRequiredService<IMapper>();

                return new SyncRepository<Product, ProductDto>(onlineRepository, offlineRepository, mapper);
            });


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
