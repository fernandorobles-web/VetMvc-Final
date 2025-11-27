using System;
using System.Collections.Generic;

namespace VetMvc.Models;

public partial class Atencione
{
    public int Id { get; set; }

    public int MascotaId { get; set; }

    public DateTime FechaHora { get; set; }

    public string Motivo { get; set; } = null!;

    public string? Diagnostico { get; set; }

    public string? Tratamiento { get; set; }

    public decimal Costo { get; set; }

    public DateTime? ProximaCita { get; set; }

    public virtual Mascota Mascota { get; set; } = null!;
}
