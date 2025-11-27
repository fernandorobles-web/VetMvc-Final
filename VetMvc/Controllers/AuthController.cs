using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VetMvc.DTOs;
using VetMvc.Models;
using VetMvc.Services;

namespace VetMvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // GET: /Auth/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginDto());
        }

        // POST: /Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid) return View(dto);

            try
            {
                var usuario = await _authService.ValidarCredenciales(dto.NombreUsuario, dto.Password);
                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos");
                    return View(dto);
                }

                // Claims de la sesión
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol),
                    new Claim("NombreCompleto", usuario.NombreCompleto)
                };

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);

                var props = new AuthenticationProperties
                {
                    IsPersistent = dto.Recordarme,
                    ExpiresUtc = dto.Recordarme
                        ? DateTimeOffset.UtcNow.AddDays(30)
                        : DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync("CookieAuth", principal, props);

                _logger.LogInformation("Login OK: {Usuario}", usuario.NombreUsuario);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al iniciar sesión");
                return View(dto);
            }
        }

        // GET: /Auth/Registro
        [AllowAnonymous]
        public IActionResult Registro() => View(new RegistroDto());

        // POST: /Auth/Registro
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var (exito, mensaje, usuario) = await _authService.RegistrarUsuario(dto);
                if (!exito)
                {
                    ModelState.AddModelError(string.Empty, mensaje);
                    return View(dto);
                }

                TempData["Success"] = "Registro exitoso. Inicia sesión.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al registrar el usuario");
                return View(dto);
            }
        }

        // POST: /Auth/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var nombreUsuario = User.Identity?.Name ?? "(desconocido)";
            await HttpContext.SignOutAsync("CookieAuth");
            _logger.LogInformation("Logout de {Usuario}", nombreUsuario);
            TempData["Info"] = "Sesión cerrada exitosamente";
            return RedirectToAction(nameof(Login));
        }

        // GET: /Auth/AccesoDenegado
        [AllowAnonymous]
        public IActionResult AccesoDenegado() => View();

        // GET: /Auth/Perfil
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var userId))
                return Unauthorized();

            var usuario = await _authService.ObtenerUsuarioPorId(userId);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // GET: /Auth/CambiarPassword
        [Authorize]
        public IActionResult CambiarPassword() => View();

        // POST: /Auth/CambiarPassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var userId))
                    return Unauthorized();

                var (exito, mensaje) = await _authService.CambiarPassword(userId, dto.PasswordActual, dto.NuevaPassword);
                if (!exito)
                {
                    ModelState.AddModelError(string.Empty, mensaje);
                    return View(dto);
                }

                TempData["Success"] = mensaje;
                return RedirectToAction(nameof(Perfil));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al cambiar la contraseña");
                return View(dto);
            }
        }
    }
}
