
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MovieProject.Models.Settings;
using Microsoft.Extensions.Options;


namespace MovieProject.Areas.Identity.Pages.Account
{
    public class DemoAdminModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _config;

        public DemoAdminModel(SignInManager<IdentityUser> signInManager, IConfiguration config)
        {
            _signInManager = signInManager;
            _config = config;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var email = _config["DefaultCredentials:Email"];
            var password = _config["DefaultCredentials:Password"];

            //Add role of admin to existing user in db.
            var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index");
        }
    }
}