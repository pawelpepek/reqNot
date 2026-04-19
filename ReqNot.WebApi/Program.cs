using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.HttpOverrides;
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
    var (isOn, lastChecked) = stateService.GetState();
    return Results.Ok(new { isOn, lastChecked });
});

app.MapGet("/check", async (DeviceChecker checker, DeviceStateService stateService) =>
{
    var isOn = await checker.CheckAsync();
    stateService.Update(isOn);
    return Results.Ok(new { isOn });
});

app.Run();
