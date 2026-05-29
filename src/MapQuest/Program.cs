using MapQuest.Client.Pages;
using MapQuest.Components;
using MapQuest.Components.Account;
using MapQuest.Data;
using MapQuest.Data.Document;
using MapQuest.Interfaces;
using MapQuest.QuestService;
using MapQuest.UserService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

var settingsFolder = Path.Combine(builder.Environment.ContentRootPath, "Settings");
var settingsPath = Path.Combine(settingsFolder, "appsettings.json");

if (!Directory.Exists(settingsFolder))
{
    Directory.CreateDirectory(settingsFolder);
}
if (!File.Exists(settingsPath))
{
    File.Copy(Path.Combine(builder.Environment.ContentRootPath, "appsettings.json"), settingsPath);
}

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(settingsPath, optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDocumentRepository, Repository>();
builder.Services.AddScoped<IQuestService, QuestService>();

var app = builder.Build();

bool _pipelineReady = false;
app.Use(async (context, next) =>
{
    if (!_pipelineReady)
    {
        await Task.Run(() =>
        {
            while (!_pipelineReady)
            {
                Thread.Sleep(500);
            }
        });
    }
    await next.Invoke();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Redirect direct page requests (other than root) to "/"
app.Use(async (context, next) =>
{
    if (HttpMethods.IsGet(context.Request.Method))
    {
        var endpoint = context.GetEndpoint();
        var componentMetadata = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Components.Endpoints.ComponentTypeMetadata>();
        if (componentMetadata != null)
        {
            var componentType = componentMetadata.Type;
            var assemblyName = componentType.Assembly.GetName().Name;

            if (context.Request.Path != "/" && assemblyName?.StartsWith("MapQuest.Client") == true)
            {
                context.Response.Redirect("/");
                return;
            }
        }
    }
    await next();
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(MapQuest.Client._Imports).Assembly, 
        typeof(MapQuest.Client.Users._Imports).Assembly,
        typeof(MapQuest.Client.Map._Imports).Assembly,
        typeof(MapQuest.Client.Quests._Imports).Assembly);

app.MapUsersEndpoints();
app.MapQuestEndpoints();

#pragma warning disable CS4014
Task.Run(async () =>
{
    // Apply pending database migrations on startup
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        // Seed default roles if they do not exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Seed default administrator user if it does not exist
        var adminEmail = "admin@mapquest.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        await repository.SetupAsync();
    }

    _pipelineReady = true;
});
#pragma warning restore CS4014

app.Run();
