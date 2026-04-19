var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

bool isOn = true;
object stateLock = new();

app.MapGet("/API/RunFunction", (string name) =>
{
    if (name != "CurrentWorkParameters")
        return Results.BadRequest(new { Message = "Unknown function" });

    bool currentState;
    lock (stateLock) { currentState = isOn; }

    var values = new object[] {
        currentState ? 1 : 0, 300, 22.2, 90, 90, 100, 100, 43, 500, 0, 8, 0, 0, 0, 0, 4,
        80, 80, 25, 100, 80, 100, 20, 70, 30, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 22.5, 22.3, 22.2, 22.5, 0.0, 0.0, 0.0,
        0.0, 30, 40, 24, 25, 22, 0, 3, 2, 0, 0, 0, 0, 20, 25, 0, 0, 0, 0, 12, 13, 89, 540,
        530, 0, 87, 95, 41, 7, 13, 30, 29, 0, 0, 0, 0, 0, 60000, 12936
    };

    return Results.Ok(new { CurrentWorkParametersResult = true, Values = values });
});

app.MapPut("/toggle", () =>
{
    bool newState;
    lock (stateLock) { isOn = !isOn; newState = isOn; }
    return Results.Ok(new { isOn = newState });
});

app.Run();
