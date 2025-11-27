using System;
using System.Collections.Generic;

namespace VetMvc.Models;

public partial class Dueno
{

    public int Id { get; set; }

    public string Rut { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string? Email { get; set; }

    public string Telefono { get; set; } = null!;

    public string? Direccion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Mascota> Mascota { get; set; } = new List<Mascota>();
}
