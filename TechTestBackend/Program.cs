using Microsoft.EntityFrameworkCore;
using TechTestBackend;
using TechTestBackend.Configuration;
using TechTestBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextFactory<SongstorageContext>(options => options.UseInMemoryDatabase("Songstorage"));

builder.Services.Configure<SpotifyConfiguration>(
    builder.Configuration.GetSection("Spotify"));

builder.Services.AddScoped<ISpotifyHttpService, SpotifyHttpService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();