using System.ComponentModel.DataAnnotations;

namespace VitalHelse.Models
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public List<string> CurrentRoles { get; set; }
        public List<string> AllRoles { get; set; }
    }

    public class EditUserRolesViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public List<RoleSelection> Roles { get; set; }
    }
    public class AddUserViewModel
    {
        [Required(ErrorMessage = "E-post er påkrevd")]
        [EmailAddress(ErrorMessage = "Ugyldig e-postadresse")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Passord er påkrevd")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Passord må være minst 6 tegn")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Bekreft passord")]
        [Compare("Password", ErrorMessage = "Passordene stemmer ikke overens")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public List<RoleSelection> Roles { get; set; } = new List<RoleSelection>();
    }

    public class RoleSelection
    {
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}