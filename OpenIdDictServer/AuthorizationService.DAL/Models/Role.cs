using Microsoft.AspNetCore.Identity;

namespace AuthorizationService.DAL.Models;

public class Role : IdentityRole<Guid>
{
    public Role(){}
    
    public Role(string name) : base(name) { }
}