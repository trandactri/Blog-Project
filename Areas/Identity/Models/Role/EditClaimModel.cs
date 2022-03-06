using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace App.Areas.Identity.Models.RoleViewModels
{
  public class EditClaimModel
  {
    [Display(Name = "Claim Type")]
    [Required(ErrorMessage = "Please enter {0}")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} must last from {2} to {1} characters")]
    public string? ClaimType { get; set; }

    [Display(Name = "Claim Value")]
    [Required(ErrorMessage = "Please enter {0}")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} must last from {2} to {1} characters")]
    public string? ClaimValue { get; set; }

    public IdentityRole? role { get; set; }


  }
}
