using WaterMeterAPI.Data;
using WaterMeterAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new FirebirdHelper(
    @"C:\Users\DUDU\Desktop\AQUASENSE\Firebird\WATER_METER.FDB"));

builder.Services.AddHostedService<SerialReaderService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDashboard", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseCors("AllowDashboard");
app.UseAuthorization();
app.MapControllers();

app.Run();