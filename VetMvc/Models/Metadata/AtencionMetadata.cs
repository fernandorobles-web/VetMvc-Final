using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace VetMvc.Models;

[ModelMetadataType(typeof(AtencionMetadata))]
public partial class Atencion { }

public class AtencionMetadata
{
    [Required] public int MascotaId { get; set; }

    [Required, Display(Name = "Fecha y hora")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime FechaHora { get; set; }

    [ScaffoldColumn(false)]
    public DateTime? ProximaCita { get; set; }

    [Required, StringLength(160)] public string Motivo { get; set; } = null!;
    [Range(0, 999999)] public decimal Costo { get; set; }
}
