using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using VetMvc.Validations;

namespace VetMvc.Models;

[ModelMetadataType(typeof(DuenoMetadata))]
public partial class Dueno { }

public class DuenoMetadata
{
    [Display(Name = "RUN")]
    [Required(ErrorMessage = "El RUN es obligatorio.")]
    [StringLength(14)] // permite 12.345.678-5 (12) y 12345678-5 (10). 14 da holgura.
    // Acepta sin puntos (7-8 dígitos) o con puntos (1-2 . 3 . 3), guion obligatorio, DV 0-9/K
    [RegularExpression(@"^(?:\d{7,8}|\d{1,2}(?:\.\d{3}){2})-[0-9Kk]$",
        ErrorMessage = "Formato RUN válido: 12345678-9 o 12.345.678-9")]
    [ValidarRun(ErrorMessage = "RUN no es válido (DV incorrecto).")]
    public string Rut { get; set; } = null!; // nombre del campo puede seguir siendo Rut por compatibilidad con BD

    [Required, StringLength(80)]
    public string Nombres { get; set; } = null!;

    [Required, StringLength(80)]
    public string Apellidos { get; set; } = null!;

    [EmailAddress, StringLength(120)]
    public string? Email { get; set; }

    [Required]
    [Display(Name = "Teléfono")]
    // Permite cliente flexible: 912345678, +56 9 1234 5678, 56-9-12345678, etc.
    [RegularExpression(@"^(\+?56)?\s*9([\s-]?\d){8}$", ErrorMessage = "Formato válido: 912345678 o +56 9 1234 5678")]
    [StringLength(16)] // holgura para +56 y separadores
    [TelefonoChileno]  // Validación servidor definitiva
    public string Telefono { get; set; } = null!;
}
