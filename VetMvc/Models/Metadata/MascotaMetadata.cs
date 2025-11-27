using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using VetMvc.Validations; // 👈 IMPORTANTE

namespace VetMvc.Models;

[ModelMetadataType(typeof(MascotaMetadata))]
public partial class Mascota { }

public class MascotaMetadata
{
    [Required, StringLength(60)]
    public string Nombre { get; set; } = null!;

    [Required, RegularExpression("^[MH]$", ErrorMessage = "Use M (macho) o H (hembra).")]
    public string Sexo { get; set; } = "M";

    [Required, Display(Name = "Fecha de nacimiento")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [FechaNoFutura(ErrorMessage = "La fecha de nacimiento no puede ser futura")]
    public DateOnly FechaNacimiento { get; set; }

    [Range(0.1, 120)]
    [Display(Name = "Peso (kg)")]
    public decimal PesoKg { get; set; }

    [StringLength(30)]
    public string? Chip { get; set; }
}
