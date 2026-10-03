using ProycTm5_API_Singleton.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IEmpleadoService, EmpleadoSingletonService>();//CC22070: Registro del servicio Singleton para IEmpleadoService y su implementación EmpleadoSingletonService


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();

// JQ203: Aqui se registran los Controllers y se configura Swagger para la documentación de la API