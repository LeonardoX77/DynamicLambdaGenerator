namespace Common.WebApi.Application.Models.User
{
    using System.ComponentModel.DataAnnotations;

    public class UserDto
    {
        [EmailAddress(ErrorMessage = "The email format is not valid.")]
        public string Email { get; set; }

        [RegularExpression(
            "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{6,}$",
            ErrorMessage = "The password must be at least 6 characters long, contain uppercase letters, lowercase letters, a number, and a symbol.")]
        public string Password { get; set; }
    }
}
