using AuthorizationService.DAL.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationService.DAL.Database;

public class AuthorizationServiceDbContext : IdentityDbContext<User, Role, Guid>
{
    public AuthorizationServiceDbContext(DbContextOptions<AuthorizationServiceDbContext> options) : base(options)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}