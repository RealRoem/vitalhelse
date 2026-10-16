// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitalHelse.Models;

namespace VitalHelse.Areas.Identity.Pages.Account.Manage;

public class IndexModel : PageModel
{
    private readonly UserManager<AspNetUsers> _userManager;
    private readonly SignInManager<AspNetUsers> _signInManager;

    public IndexModel(
        UserManager<AspNetUsers> userManager,
        SignInManager<AspNetUsers> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = default!;

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Phone]
        [Display(Name = "Mobilnummer")]
        public string? PhoneNumber { get; set; }
        
        [Display(Name = "E-post")]
        public string? Email { get; set; }
        [Display(Name = "Brukernavn")]
        public string? Username { get; set; }

        [Display(Name = "Fornavn")]
        public string? FirstName { get; set; }

        [Display(Name = "Etternavn")]
        public string? LastName { get; set; }
      
    
        
    }

    private async Task LoadAsync(AspNetUsers user)
    {
        var email = await _userManager.GetEmailAsync(user);
        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

        Input = new InputModel
        {
            Email = user.Email,
            Username = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = phoneNumber,
        };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }
        
        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (Input.PhoneNumber != phoneNumber)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "En uventet feil oppstod ved lagring av telefonnummeret.";
                return RedirectToPage();
            }
        }
        
        user.FirstName = Input.FirstName;
        user.LastName = Input.LastName;
        
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            StatusMessage = "En uventet feil oppstod ved lagring av brukerdata";
            return RedirectToPage();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Profilen har blitt oppdatert";
        return RedirectToPage();
    }
}
