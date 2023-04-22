using Microsoft.AspNetCore.Identity;

namespace AuthorizationService.DAL.Models;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Patronymic { get; set; }
    public DateTime BirthDate { get; set; }
}