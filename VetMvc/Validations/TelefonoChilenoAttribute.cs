// Validations/TelefonoChilenoAttribute.cs
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace VetMvc.Validations
{
    /// <summary>
    /// Valida formato de teléfono chileno.
    /// Acepta: +56912345678, 912345678, +56 9 1234 5678, 56-9-12345678, etc.
    /// </summary>
    public class TelefonoChilenoAttribute : ValidationAttribute
    {
        public TelefonoChilenoAttribute()
        {
            ErrorMessage = "Formato de teléfono inválido. Use: 912345678 o +56 9 1234 5678";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success; // [Required] se encarga del vacío

            // Normaliza SOLO para validar (no cambia lo que guardas)
            var telefono = Regex.Replace(value.ToString()!, "[\\s-]", "");

            // Patrón: opcional +56 o 56, luego 9 y 8 dígitos; o directamente 9 dígitos empezando con 9
            var patron = @"^(\+?56)?9\d{8}$";

            if (!Regex.IsMatch(telefono, patron))
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
