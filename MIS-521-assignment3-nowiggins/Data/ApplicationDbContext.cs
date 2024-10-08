using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MIS_521_assignment3_nowiggins.Models;

namespace MIS_521_assignment3_nowiggins.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<MIS_521_assignment3_nowiggins.Models.Actor> Actor { get; set; } = default!;
        public DbSet<MIS_521_assignment3_nowiggins.Models.Movie> Movie { get; set; } = default!;
        public DbSet<MIS_521_assignment3_nowiggins.Models.ActorMovie> ActorMovie { get; set; } = default!;
    }
}
