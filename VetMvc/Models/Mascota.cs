using System;
using System.Collections.Generic;

namespace VetMvc.Models;

public partial class Mascota
{
    public int Id { get; set; }

    public int DuenoId { get; set; }

    public int EspecieId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Sexo { get; set; } = null!;

    public DateOnly FechaNacimiento { get; set; }

    public decimal PesoKg { get; set; }

    public bool Esterilizado { get; set; }

    public string? Chip { get; set; }

    public string? Color { get; set; }

    public string? Observaciones { get; set; }

    public virtual ICollection<Atencione> Atenciones { get; set; } = new List<Atencione>();

    public virtual Dueno Dueno { get; set; } = null!;

    public virtual Especy Especie { get; set; } = null!;
}
