using AgileMindsUI.Client.Auth;
using AgileMindsUI.Client.Services;

using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using MudBlazor;
using MudBlazor.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register HttpClient for making requests
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:60000") });

// Add Blazored LocalStorage for managing the JWT token
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<GPTService>();
builder.Services.AddScoped<TaskStateContainer>();

// Add MudBlazor services with custom Snackbar configuration
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 2500;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});
//builder.Services.AddMudServices();

// Register ProjectService as scoped (or singleton if you don't need to inject HttpClient)
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<SprintService>();
builder.Services.AddScoped<NotificationService>();

// Register JwtAuthenticationStateProvider
builder.Services.AddScoped<JwtAuthenticationStateProvider>();

// Register the custom JwtAuthenticationStateProvider as AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider>(p => p.GetRequiredService<JwtAuthenticationStateProvider>());

// Add Authorization Core for handling [Authorize] directives
builder.Services.AddAuthorizationCore();

// Build and run the application
await builder.Build().RunAsync();
