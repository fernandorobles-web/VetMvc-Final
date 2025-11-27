using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetMvc.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string NombreCompleto { get; set; } = null!;

        [Required, StringLength(50)]
        public string NombreUsuario { get; set; } = null!;

        [Required, StringLength(200), EmailAddress]
        public string Email { get; set; } = null!;

        [Required, StringLength(255)]
        public string PasswordHash { get; set; } = null!;

        [Required, StringLength(20)]
        public string Rol { get; set; } = "Recepcionista"; // Ej: Administrador/Veterinario/Recepcionista

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? UltimoAcceso { get; set; }
    }
}
