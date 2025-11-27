using System.ComponentModel.DataAnnotations;

namespace VetMvc.Models
{
    public class UsuarioFormVM
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = null!;

        [Required, StringLength(50)]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; } = null!;

        [Required, StringLength(200), EmailAddress]
        public string Email { get; set; } = null!;

        [Required, StringLength(20)]
        public string Rol { get; set; } = "Recepcionista";

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Solo para formulario (crear/cambiar password)
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [MinLength(8, ErrorMessage = "Mínimo 8 caracteres")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "No coincide con la contraseña")]
        public string? ConfirmPassword { get; set; }
    }
}
