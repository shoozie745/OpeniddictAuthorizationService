using System.Text.Json.Serialization;
using AuthorizationService.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AuthorizationService.DAL.Database;

public class DatabaseInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthorizationServiceDbContext _context;
    public DatabaseInitializer(IServiceProvider serviceProvider, AuthorizationServiceDbContext context)
        {
            _serviceProvider = serviceProvider;
            _context = context;
        }
        
        public async Task Seed(AdminUser admin)
        {
            await _context!.Database.EnsureCreatedAsync();
            var pending = await _context.Database.GetPendingMigrationsAsync(); //Асинхронно получает все миграции, определенные в сборке, но не примененные к целевой базе данных.
            if (pending.Any())
            {
                await _context!.Database.MigrateAsync();
            }
            var user = new User
            {
                Email = admin.Email,
                UserName = admin.UserName,
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                PhoneNumber = admin.PhoneNumber,
                EmailConfirmed = admin.EmailConfirmed,
                PhoneNumberConfirmed = admin.PhoneNumberConfirmed,
                BirthDate = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString("D"),
            };
            await SeedRoles();
            await SeedUsers(user);
        }
         private async Task SeedUsers(User user)
        {
            using var scope = _serviceProvider.CreateScope();
            
            if (_context.Users.Any())
            {
                return;
            }
            
            if (!_context!.Users.Any(u => u.UserName == user.UserName)) //Проверка на то, что админ уже существует
            {
                var password = new PasswordHasher<User>();
                user.PasswordHash = password.HashPassword(user, "qwe123!@#");
                
                var userManager = scope.ServiceProvider.GetService<UserManager<User>>();
                var userResult = await userManager.CreateAsync(user);
                if (!userResult.Succeeded)
                {
                    throw new InvalidOperationException($"Cannot create account {userResult.Errors.FirstOrDefault()?.Description}");
                }
                
                var roleResult = await userManager.AddToRoleAsync(user, UserRole.Admin);
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException("Cannot add roles to account");
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedRoles()
        {
            using var scope = _serviceProvider.CreateScope();

            var roles = new string[] { "Admin", "User" };
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            foreach (var role in roles)
            {
                bool check = await roleManager.RoleExistsAsync(role);
                if (!check)
                {
                    await roleManager.CreateAsync(new Role(role));
                }
            }

            await _context.SaveChangesAsync();
        }
}

public class AdminUser
{
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
}