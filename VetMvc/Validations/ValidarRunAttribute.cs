using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace VetMvc.Validations
{
    /// Valida RUN chileno:
    /// - Puntos opcionales
    /// - Guion obligatorio
    /// - 1 a 8 dígitos antes del guion
    /// - DV: 0-9 o K/k
    /// - Verifica dígito verificador (módulo 11)
    public class ValidarRunAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value == null) return ValidationResult.Success;

            var raw = value.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(raw)) return ValidationResult.Success; // [Required] se encarga

            // Quitar puntos solamente; el guion DEBE permanecer para el formato
            var runConGuion = raw.Replace(".", "");

            // Debe tener exactamente un guion y no al inicio/fin
            var parts = runConGuion.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return new ValidationResult("RUN debe tener guion (ej: 12345678-9).");

            var numero = parts[0];
            var dvText = parts[1];

            // 1 a 8 dígitos en la parte numérica
            if (numero.Length is < 1 or > 8 || !numero.All(char.IsDigit))
                return new ValidationResult("RUN inválido: se permiten 1 a 8 dígitos antes del guion.");

            // DV debe ser exactamente un carácter: 0-9 o K/k
            if (dvText.Length != 1 || !(char.IsDigit(dvText[0]) || dvText[0] is 'k' or 'K'))
                return new ValidationResult("RUN inválido: el dígito verificador debe ser 0-9 o K.");

            // Calcular DV (módulo 11)
            int suma = 0, mult = 2;
            for (int i = numero.Length - 1; i >= 0; i--)
            {
                suma += (numero[i] - '0') * mult;
                mult = (mult == 7) ? 2 : mult + 1;
            }

            int resto = suma % 11;
            int dvCalc = 11 - resto;
            char dvEsperado = dvCalc switch
            {
                11 => '0',
                10 => 'K',
                _ => (char)('0' + dvCalc)
            };

            if (char.ToUpperInvariant(dvText[0]) != dvEsperado)
                return new ValidationResult($"RUN no es válido. DV esperado: {dvEsperado}");

            return ValidationResult.Success;
        }
    }
}
