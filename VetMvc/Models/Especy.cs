using System;
using System.Collections.Generic;

namespace VetMvc.Models;

public partial class Especy
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Mascota> Mascota { get; set; } = new List<Mascota>();
}
