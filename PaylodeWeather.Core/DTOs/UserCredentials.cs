using System.ComponentModel.DataAnnotations;

namespace PaylodeWeather.Core.DTOs
{
    public class UserCredentials
    {
        [Required]
        [EmailAddress]
        [RegularExpression("^[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\\.)+[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?", ErrorMessage = "Invalid email format!")]
        public string Email { get; set; }
        [Required]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[@$!%*?&])[A-Za-z0-9@$!%*?&]{6,}", ErrorMessage = "Invalid password format! Password must be alphanumeric and must contain at least one symbol and one uppercase letter!")]
        public string Password { get; set; }
    }
}
