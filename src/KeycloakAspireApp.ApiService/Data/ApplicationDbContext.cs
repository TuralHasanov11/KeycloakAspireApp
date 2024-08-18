using Microsoft.EntityFrameworkCore;

namespace KeycloakAspireApp.ApiService.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
}