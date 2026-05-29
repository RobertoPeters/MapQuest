using System.ComponentModel.DataAnnotations;

namespace MapQuest.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "E-mailadres is verplicht.")]
    [EmailAddress(ErrorMessage = "Ongeldig e-mailadres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wachtwoord is verplicht.")]
    public string Password { get; set; } = string.Empty;
}
