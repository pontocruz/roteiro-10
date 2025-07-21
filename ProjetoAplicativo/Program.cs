using Microsoft.EntityFrameworkCore;
using ProjetoAplicativo.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var conexaoRoteiro = builder.Configuration.GetConnectionString("DefaultConnection");
var versaoRoteiro = ServerVersion.AutoDetect(conexaoRoteiro);
builder.Services.AddDbContext<ModeloEntidades>(options => options.UseMySql(conexaoRoteiro, versaoRoteiro));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowReactApp");
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    DatabaseSeeder.Initialize(services);
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();