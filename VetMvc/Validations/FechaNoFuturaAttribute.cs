using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VetMvc.Validations
{
    /// Valida que la fecha (date o datetime-local) no sea posterior a "ahora".
    /// - En servidor compara con DateTime.Now.
    /// - Expone reglas para validación cliente (jQuery Unobtrusive).
    public class FechaNoFuturaAttribute : ValidationAttribute, IClientModelValidator
    {
        public FechaNoFuturaAttribute()
        {
            ErrorMessage = "La fecha no puede ser futura";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success;

            // Soporta DateTime y DateOnly (por si lo usas)
            DateTime toCheck;
            if (value is DateTime dt)
            {
                toCheck = dt;
            }
#if NET7_0_OR_GREATER
            else if (value is DateOnly dOnly)
            {
                // Consideramos el final del día para "no futura"
                toCheck = dOnly.ToDateTime(new TimeOnly(23, 59, 59));
            }
#endif
            else
            {
                return ValidationResult.Success; // tipos no soportados => no bloquear
            }

            if (toCheck > DateTime.Now)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }

        // Validador cliente
        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-fechanofutura", ErrorMessage ?? "La fecha no puede ser futura");
        }

        private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key)) return false;
            attributes.Add(key, value);
            return true;
        }
    }
}
