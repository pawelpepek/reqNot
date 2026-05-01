using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.HttpOverrides;
using ReqNot.WebApi.Models;
using ReqNot.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<DeviceStateService>();
builder.Services.AddSingleton<DeviceChecker>();
builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddSingleton<IFcmSender, FcmSender>();
builder.Services.AddHostedService<DevicePollingService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var credentialPath = builder.Configuration["Firebase:CredentialPath"];
if (!string.IsNullOrWhiteSpace(credentialPath))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(credentialPath)
    });
}

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapGet("/status", (DeviceStateService stateService) =>
{
    var (status, lastChecked) = stateService.GetState();
    return Results.Ok(new { status, lastChecked });
});

app.MapGet("/check", async (DeviceChecker checker, DeviceStateService stateService) =>
{
    try
    {
        var isOn = await checker.CheckAsync();
        var status = isOn ? DeviceStatus.On : DeviceStatus.Off;
        stateService.Update(status);
        return Results.Ok(new { status });
    }
    catch
    {
        stateService.Update(DeviceStatus.Unknown);
        return Results.Ok(new { status = DeviceStatus.Unknown });
    }
});

app.Run();
