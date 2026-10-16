using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitalHelse.Models;

namespace VitalHelse.Areas.Identity.Pages.Account.Manage;

public class BuisnessProfileModel : PageModel
{
    private readonly UserManager<AspNetUsers> _userManager;
    private readonly SignInManager<AspNetUsers> _signInManager;

    public BuisnessProfileModel(
        UserManager<AspNetUsers> userManager,
        SignInManager<AspNetUsers> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty] 
    public InputModel Input { get; set; } = default!;

    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>

        // All entities that shall be added from the businessRegister page
        
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; } = null!;

       
        [StringLength(100)]
        [Display(Name = "Organization Name")]
        public string OrgName { get; set; } = null!;
        
        [MinLength(9), MaxLength(9)]
        [Display(Name = "Organization Number")]
        public string? OrgNr { get; set; }
        

    }

    private async Task LoadAsync(AspNetUsers user)
    {
        var email = await _userManager.GetEmailAsync(user);
        Input = new InputModel
        {
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            OrgName = user.OrgName,
            OrgNr = user.OrgNr,
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
        
        user.OrgName = Input.OrgName;
        
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            StatusMessage = "En uventet feil oppsto ved lagring av brukerdata";
                return RedirectToPage();
        }
        
        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Profilen har blitt oppdatert";
        return RedirectToPage();
    }
    
    
}