using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using SQLite;
using ArcsomAssetManagement.Client.DTOs.Mapping;
using ArcsomAssetManagement.Client.Models;
using ArcsomAssetManagement.Client.DTOs.Business;
using AutoMapper;
using ArcsomAssetManagement.Client.Settings;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace ArcsomAssetManagement.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit(options =>
                {
                    options.SetShouldEnableSnackbarOnWindows(true);
                })
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

            using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").Result;
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            var jsonDoc = JsonDocument.Parse(content);

            var apiSettings = JsonSerializer.Deserialize<ApiSettings>(
                jsonDoc.RootElement.GetProperty("ApiSettings").GetRawText()
            );

            builder.Services.AddSingleton(apiSettings);

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            // Pages
            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<ProductListPageModel>();
            builder.Services.AddSingleton<ManufacturerListPageModel>();
            builder.Services.AddSingleton<AssetListPageModel>();
            builder.Services.AddTransientWithShellRoute<ManufacturerDetailPage, ManufacturerDetailPageModel>("manufacturer");
            builder.Services.AddTransientWithShellRoute<ProductDetailPage, ProductDetailPageModel>("product");
            builder.Services.AddTransientWithShellRoute<AssetDetailPage, AssetDetailPageModel>("asset");
            builder.Services.AddSingleton<ManageMetaPageModel>();
            
            // Services
            builder.Services.AddSingleton<SeedDataService>();
            builder.Services.AddSingleton<ConnectivityService>();
            builder.Services.AddSingleton<ModalErrorHandler>(); 
            builder.Services.AddTransient<AuthHeaderHandler>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<AppFaker>();

            builder.Services.AddSingleton<SyncService<Manufacturer, ManufacturerDto>>(provider =>
            {
                var sqliteConnection = provider.GetRequiredService<SQLiteAsyncConnection>();
                var onlineRepository = provider.GetRequiredService<ManufacturerRepository>();
                var mapper = provider.GetRequiredService<IMapper>();
                return new SyncService<Manufacturer, ManufacturerDto>(sqliteConnection, onlineRepository, mapper);
            });

            // Repositories
            builder.Services.AddTransient<SQLiteAsyncConnection>(provider =>
            {
                return new SQLiteAsyncConnection(Constants.DatabasePath);
            });

            builder.Services.AddHttpClient("AuthorizedClient", client =>
            {
                client.BaseAddress = new Uri(apiSettings.ApiUrl); 
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddSingleton<ManufacturerRepository>();
            builder.Services.AddSingleton<ProductRepository>();
            builder.Services.AddSingleton<AssetRepository>();
            builder.Services.AddSingleton<AuthRepository>();

            return builder.Build();
        }
    }
}
