using Microsoft.EntityFrameworkCore;
using Jude.Server.Extensions;
using Jude.Config;
using Jude.Data.Repository;


var builder = WebApplication.CreateBuilder(args);

AppConfig.Initialize();

builder.Services.AddOpenApi();
builder.Services.ConfigureDatabase();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
