using System.ComponentModel.DataAnnotations;

namespace TrivaService.ViewModels.Auth
{
    public class LoginViewModel
    {
        [Display(Name = "Kullanıcı Adı")]
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Parola")]
        [Required(ErrorMessage = "Parola zorunludur.")]
        public string Password { get; set; } = string.Empty;
    }
}
