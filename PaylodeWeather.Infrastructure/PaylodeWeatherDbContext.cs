using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace PaylodeWeather.Infrastructure
{
    public class PaylodeWeatherDbContext : IdentityDbContext
    {
        public PaylodeWeatherDbContext([NotNull] DbContextOptions options) : base(options)
        {
        }
    }
}
