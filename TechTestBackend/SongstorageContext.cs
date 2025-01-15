using Microsoft.EntityFrameworkCore;
using TechTestBackend.Models;

namespace TechTestBackend;

public class SongstorageContext : DbContext
{
    public SongstorageContext(DbContextOptions<SongstorageContext> options)
        : base(options)
    {
    }

    public DbSet<Spotifysong> Songs { get; set; }
}