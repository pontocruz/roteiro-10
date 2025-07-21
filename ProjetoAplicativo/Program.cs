using Microsoft.EntityFrameworkCore;
using ProjetoAplicativo.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var conexaoRoteiro = builder.Configuration.GetConnectionString("DefaultConnection"); 
var versaoRoteiro = ServerVersion.AutoDetect(conexaoRoteiro);
builder.Services.AddDbContext<ModeloEntidades>(options => options.UseMySql(conexaoRoteiro, versaoRoteiro));
var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
app.Run();
