// Services/AuthService.cs
using Microsoft.EntityFrameworkCore;
using VetMvc.DTOs;
using VetMvc.Models;

namespace VetMvc.Services
{
    /// Servicio para login/registro/cambio de contraseña con BCrypt
    public class AuthService
    {
        private readonly VetDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(VetDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// Valida credenciales (devuelve null si fallan)
        public async Task<Usuario?> ValidarCredenciales(string nombreUsuario, string password)
        {
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Activo &&
                                          u.NombreUsuario.ToLower() == nombreUsuario.Trim().ToLower());

            if (user == null)
            {
                _logger.LogWarning("Login fallido (usuario no existe): {Usuario}", nombreUsuario);
                return null;
            }

            var ok = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!ok)
            {
                _logger.LogWarning("Login fallido (password inválida) para: {Usuario}", nombreUsuario);
                return null;
            }

            user.UltimoAcceso = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Login OK: {Usuario}", nombreUsuario);
            return user;
        }

        /// Registra un nuevo usuario (valida unicidad de usuario/email)
        public async Task<(bool Exito, string Mensaje, Usuario? Usuario)> RegistrarUsuario(RegistroDto dto)
        {
            var username = dto.NombreUsuario.Trim().ToLower();
            var email = dto.Email.Trim().ToLower();

            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario.ToLower() == username))
                return (false, "El nombre de usuario ya está en uso", null);

            if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == email))
                return (false, "El email ya está registrado", null);

            var user = new Usuario
            {
                NombreCompleto = dto.NombreCompleto.Trim(),
                NombreUsuario = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = dto.Rol,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario creado: {Usuario} ({Rol})", user.NombreUsuario, user.Rol);
            return (true, "Usuario registrado exitosamente", user);
        }

        /// Cambia la contraseña (verifica la actual con BCrypt)
        public async Task<(bool Exito, string Mensaje)> CambiarPassword(int usuarioId, string passwordActual, string nuevaPassword)
        {
            var user = await _context.Usuarios.FindAsync(usuarioId);
            if (user == null) return (false, "Usuario no encontrado");

            var ok = BCrypt.Net.BCrypt.Verify(passwordActual, user.PasswordHash);
            if (!ok)
            {
                _logger.LogWarning("Cambio de password fallido (actual incorrecta): {Usuario}", user.NombreUsuario);
                return (false, "La contraseña actual es incorrecta");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Password actualizada: {Usuario}", user.NombreUsuario);
            return (true, "Contraseña cambiada exitosamente");
        }

        public Task<Usuario?> ObtenerUsuarioPorId(int id) => _context.Usuarios.FindAsync(id).AsTask();
        public Task<Usuario?> ObtenerUsuarioPorNombre(string u) =>
            _context.Usuarios.FirstOrDefaultAsync(x => x.NombreUsuario.ToLower() == u.Trim().ToLower());
    }
}
