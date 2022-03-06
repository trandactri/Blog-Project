using System.ComponentModel.DataAnnotations;

namespace App.Areas.Identity.Models.UserViewModels
{
  public class AddUserClaimModel
  {
    [Display(Name = "Claim Name")]
    [Required(ErrorMessage = "Please enter {0}")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} must last from {2} to {1} ký tự")]
    public string? ClaimType { get; set; }

    [Display(Name = "Claim Value")]
    [Required(ErrorMessage = "Please enter {0}")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} must last from {2} to {1} ký tự")]
    public string? ClaimValue { get; set; }

  }
}