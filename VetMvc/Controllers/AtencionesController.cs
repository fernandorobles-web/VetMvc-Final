using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetMvc.Models;
using QuestPDF.Fluent;

namespace VetMvc.Controllers
{
    [Authorize]
    public class AtencionesController : Controller
    {
        private readonly VetDbContext _context;
        private readonly ILogger<AtencionesController> _logger;

        public AtencionesController(VetDbContext context, ILogger<AtencionesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task PrepararMascotasAsync(Atencione? a = null)
        {
            var mascotas = await _context.Mascotas
                .Include(m => m.Dueno)
                .AsNoTracking()
                .Select(m => new {
                    m.Id,
                    Nombre = m.Nombre + " — " + (m.Dueno != null ? (m.Dueno.Nombres + " " + m.Dueno.Apellidos) : "")
                })
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            ViewData["MascotaId"] = new SelectList(mascotas, "Id", "Nombre", a?.MascotaId);
        }

        // GET: /Atenciones
        public async Task<IActionResult> Index()
        {
            try
            {
                var lista = await _context.Atenciones
                    .Include(a => a.Mascota).ThenInclude(m => m.Dueno)
                    .AsNoTracking()
                    .OrderByDescending(a => a.FechaHora)
                    .ToListAsync();

                return View(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar atenciones");
                TempData["Error"] = "Error al cargar la lista de atenciones.";
                return View(new List<Atencione>());
            }
        }

        // GET: /Atenciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return BadRequest("ID no proporcionado");

            var atencion = await _context.Atenciones
                .Include(a => a.Mascota).ThenInclude(m => m.Dueno)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            return atencion == null ? NotFound() : View(atencion);
        }

        // GET: /Atenciones/Create
        public async Task<IActionResult> Create()
        {
            await PrepararMascotasAsync();

            // Fecha por defecto redondeada a 15 min, sin seg/ms
            var now = DateTime.Now;
            var base15 = new DateTime(now.Year, now.Month, now.Day, now.Hour, (now.Minute / 15) * 15, 0);

            return View(new Atencione { FechaHora = base15, Costo = 0 });
        }



        // POST: /Atenciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
          [Bind("MascotaId,FechaHora,Motivo,Diagnostico,Tratamiento,Costo")]
          Atencione atencion)
        {
            ModelState.Remove("Mascota");

            if (atencion.MascotaId <= 0)
                ModelState.AddModelError("MascotaId", "Seleccione una mascota.");
            if (atencion.Costo < 0)
                ModelState.AddModelError("Costo", "El costo no puede ser negativo.");

            // ← forzar sin segundos/milisegundos
            atencion.FechaHora = new DateTime(
                atencion.FechaHora.Year, atencion.FechaHora.Month, atencion.FechaHora.Day,
                atencion.FechaHora.Hour, atencion.FechaHora.Minute, 0, 0, atencion.FechaHora.Kind);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los campos marcados en rojo.";
                await PrepararMascotasAsync(atencion);
                return View(atencion);
            }

            _context.Add(atencion);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Atención registrada.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Atenciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();
            var a = await _context.Atenciones.FindAsync(id);
            if (a == null) return NotFound();
            await PrepararMascotasAsync(a);
            return View(a);
        }

        // POST: /Atenciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
           [Bind("Id,MascotaId,FechaHora,Motivo,Diagnostico,Tratamiento,Costo")]
           Atencione atencion)
        {
            if (id != atencion.Id) return BadRequest();
            ModelState.Remove("Mascota");

            if (atencion.MascotaId <= 0)
                ModelState.AddModelError("MascotaId", "Seleccione una mascota.");
            if (atencion.Costo < 0)
                ModelState.AddModelError("Costo", "El costo no puede ser negativo.");

            atencion.FechaHora = new DateTime(
                atencion.FechaHora.Year, atencion.FechaHora.Month, atencion.FechaHora.Day,
                atencion.FechaHora.Hour, atencion.FechaHora.Minute, 0, 0, atencion.FechaHora.Kind);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los campos marcados en rojo.";
                await PrepararMascotasAsync(atencion);
                return View(atencion);
            }

            _context.Entry(atencion).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Atención editada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Atenciones/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest("ID no proporcionado");

            var atencion = await _context.Atenciones
                .Include(a => a.Mascota).ThenInclude(m => m.Dueno)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            return atencion == null ? NotFound() : View(atencion);
        }

        // POST: /Atenciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var atencion = await _context.Atenciones.FindAsync(id);
            if (atencion == null) return NotFound();

            _context.Atenciones.Remove(atencion);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Atención eliminada.";
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> ExportExcel()
        {
            var data = await _context.Atenciones
                .AsNoTracking()
                .Include(a => a.Mascota).ThenInclude(m => m.Dueno)
                .OrderByDescending(a => a.FechaHora)
                .Select(a => new
                {
                    Fecha = a.FechaHora,
                    Mascota = a.Mascota != null ? a.Mascota.Nombre : "",
                    Dueno = a.Mascota != null && a.Mascota.Dueno != null ?
                                $"{a.Mascota.Dueno.Nombres} {a.Mascota.Dueno.Apellidos}" : "",
                    RunDueno = a.Mascota != null && a.Mascota.Dueno != null ? a.Mascota.Dueno.Rut : "",
                    Motivo = a.Motivo ?? "",
                    Diagnostico = a.Diagnostico ?? "",
                    Tratamiento = a.Tratamiento ?? "",
                    Costo = a.Costo                  // si tu columna se llama Monto/Precio, cámbiala aquí
                })
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Atenciones");

            ws.Cell(1, 1).Value = "Fecha";
            ws.Cell(1, 2).Value = "Mascota";
            ws.Cell(1, 3).Value = "Dueño";
            ws.Cell(1, 4).Value = "RUN Dueño";
            ws.Cell(1, 5).Value = "Motivo";
            ws.Cell(1, 6).Value = "Diagnóstico";
            ws.Cell(1, 7).Value = "Tratamiento";
            ws.Cell(1, 8).Value = "Costo";
            ws.Range(1, 1, 1, 8).Style.Font.Bold = true;

            var row = 2;
            foreach (var a in data)
            {
                ws.Cell(row, 1).Value = a.Fecha;
                ws.Cell(row, 1).Style.DateFormat.Format = "dd-MM-yyyy HH:mm";
                ws.Cell(row, 2).Value = a.Mascota;
                ws.Cell(row, 3).Value = a.Dueno;
                ws.Cell(row, 4).Value = a.RunDueno;
                ws.Cell(row, 5).Value = a.Motivo;
                ws.Cell(row, 6).Value = a.Diagnostico;
                ws.Cell(row, 7).Value = a.Tratamiento;
                ws.Cell(row, 8).Value = a.Costo;
                ws.Cell(row, 8).Style.NumberFormat.Format = "$ #,##0";
                row++;
            }

            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);

            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"atenciones_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        public async Task<IActionResult> ExportPdf()
        {
            var rows = await _context.Atenciones
                .AsNoTracking()
                .Include(a => a.Mascota).ThenInclude(m => m.Dueno)
                .OrderByDescending(a => a.FechaHora)
                .Select(a => new Reports.AtencionesPdf.Row(
                    a.FechaHora,
                    a.Mascota != null ? a.Mascota.Nombre ?? "" : "",
                    a.Mascota != null && a.Mascota.Dueno != null ? $"{a.Mascota.Dueno.Nombres} {a.Mascota.Dueno.Apellidos}" : "",
                    a.Mascota != null && a.Mascota.Dueno != null ? a.Mascota.Dueno.Rut ?? "" : "",
                    a.Motivo ?? "",
                    a.Diagnostico ?? "",
                    a.Tratamiento ?? "",
                    a.Costo))
                .ToListAsync();

            var doc = new Reports.AtencionesPdf(rows, "Atenciones - VetCare Clínica");
            var pdf = doc.GeneratePdf();
            return File(pdf, "application/pdf", $"atenciones_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
    }

}
