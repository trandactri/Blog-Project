﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace App.Areas.Identity.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter {0}")]
        [EmailAddress(ErrorMessage = "Wrong {0} syntax")]
        [Display(Name = "Email")]
        public string? Email { get; set; }


        [Required(ErrorMessage = "Please enter {0}")]
        [StringLength(100, ErrorMessage = "{0} must last from {2} to {1} characters.", MinimumLength = 2)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "{0} doesn't match password.")]
        public string? ConfirmPassword { get; set; }


        [DataType(DataType.Text)]
        [Display(Name = "Username")]
        [Required(ErrorMessage = "Please enter {0}")]
        [StringLength(100, ErrorMessage = "{0} must last from {2} to {1} characters.", MinimumLength = 3)]
        public string? UserName { get; set; }

    }
}
