using MapQuest.Client.Gps;
using MapQuest.Client.Quests;
using MapQuest.Client.State;
using MapQuest.Client.Users;
using MapQuest.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Refit;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Popups;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjGyl/VkV+XU9AclRDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS3hTcEVnWXdacnFSRmJeWU91XA==");

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddTransient<CookieHandler>();

builder.Services.AddRefitClient<MapQuest.Client.Interfaces.IUserService>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<CookieHandler>();

builder.Services.AddRefitClient<MapQuest.Client.Interfaces.IQuestService>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<CookieHandler>();

builder.Services.AddScoped<SfDialogService>();
builder.Services.AddSyncfusionBlazor();
builder.Services.AddScoped<UserDataAdaptor>();
builder.Services.AddScoped<QuestDataAdaptor>();
builder.Services.AddSingleton<IGpsService, GpsService>();
builder.Services.AddSingleton<ApplicationStateService>();

var host = builder.Build();

var gpsService = host.Services.GetService<IGpsService>()!;
await gpsService.SetupAsync(host.Services);

var applicationStateService = host.Services.GetService<ApplicationStateService>()!;
await applicationStateService.SetupAsync(host.Services);

var jsRuntime = host.Services.GetRequiredService<IJSRuntime>();
await jsRuntime.InvokeVoidAsync("hideLoadingOverlay");

await host.RunAsync();
