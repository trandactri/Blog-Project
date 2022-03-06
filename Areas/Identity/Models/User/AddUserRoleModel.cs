using System.Collections.Generic;
using System.ComponentModel;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace App.Areas.Identity.Models.UserViewModels
{
  public class AddUserRoleModel
  {
    public AppUser? user { get; set; }

    [DisplayName("User Roles")]
    public string[]? RoleNames { get; set; }

    public List<IdentityRoleClaim<string>>? claimsInRole { get; set; }
    public List<IdentityUserClaim<string>>? claimsInUserClaim { get; set; }

  }
}