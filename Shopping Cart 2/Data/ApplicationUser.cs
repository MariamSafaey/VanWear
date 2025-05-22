using Microsoft.AspNetCore.Identity;
using System;
namespace Shopping_Cart_2.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime DateOfJoin { get; set; } = DateTime.Now;
    public string Name { get; set; }

    public string ProfilePic { get; set; }

    public string Street { get; set; }

    public string City { get; set; }
}
