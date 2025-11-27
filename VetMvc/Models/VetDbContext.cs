using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VetMvc.Models;

public partial class VetDbContext : DbContext
{
    public VetDbContext()
    {
    }

    public VetDbContext(DbContextOptions<VetDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Atencione> Atenciones { get; set; }

    public virtual DbSet<Dueno> Duenos { get; set; }

    public virtual DbSet<Especy> Especies { get; set; }

    public virtual DbSet<Mascota> Mascotas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; } = null!;


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server=DESKTOP-LT67EI7; initial catalog=VetDb; Trusted_connection=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Atencione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Atencion__3214EC07AADDD014");

            entity.Property(e => e.Costo).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Diagnostico).HasMaxLength(500);
            entity.Property(e => e.Motivo).HasMaxLength(160);
            entity.Property(e => e.Tratamiento).HasMaxLength(500);

            entity.HasOne(d => d.Mascota).WithMany(p => p.Atenciones)
                .HasForeignKey(d => d.MascotaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Atenciones_Mascota");
        });

        modelBuilder.Entity<Dueno>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Duenos__3214EC079A3AC84F");

            entity.HasIndex(e => e.Rut, "UQ__Duenos__CAF0366009512CAA").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Apellidos).HasMaxLength(80);
            entity.Property(e => e.Direccion).HasMaxLength(160);
            entity.Property(e => e.Email).HasMaxLength(120);
            entity.Property(e => e.Nombres).HasMaxLength(80);
            entity.Property(e => e.Rut)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(12)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Especy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Especies__3214EC078CB14FB4");

            entity.HasIndex(e => e.Nombre, "UQ__Especies__75E3EFCF2DF5D2E2").IsUnique();

            entity.Property(e => e.Nombre).HasMaxLength(40);
        });

        modelBuilder.Entity<Mascota>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Mascotas__3214EC07FC3CCC10");

            entity.HasIndex(e => e.Chip, "UX_Mascotas_Chip")
                .IsUnique()
                .HasFilter("([Chip] IS NOT NULL)");

            entity.Property(e => e.Chip).HasMaxLength(30);
            entity.Property(e => e.Color).HasMaxLength(40);
            entity.Property(e => e.Nombre).HasMaxLength(60);
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.Property(e => e.PesoKg).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Sexo)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.Dueno).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.DuenoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mascotas_Duenos");

            entity.HasOne(d => d.Especie).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.EspecieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mascotas_Especies");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
