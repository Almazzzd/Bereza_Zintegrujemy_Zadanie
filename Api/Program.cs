using Microsoft.Data.Sqlite;
using System.Data;
using Api.Controllers;
using Api.Service;

var builder = WebApplication.CreateBuilder(args);
SQLitePCL.Batteries.Init();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<DbService>();

builder.Services.AddSingleton(builder.Configuration);
builder.Services.AddTransient<IDbConnection>((sp) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Zintegrujemy");
    return new SqliteConnection(connectionString);
});
builder.Services.AddTransient<Requed_EndPoints>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
