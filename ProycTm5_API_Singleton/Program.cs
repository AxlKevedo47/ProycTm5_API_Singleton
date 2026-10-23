using ProycTm5_API_Singleton.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IEmpleadoService, EmpleadoSingletonService>(); //CC22070: Registro del servicio Singleton para IEmpleadoService y su implementación EmpleadoSingletonService


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// JQ22003: Inicia la ejecución de la aplicación ASP.NET Core
app.Run();