using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StudentPortal.API.Infrastructure;

public partial class DBContext : DbContext
{
    public DBContext()
    {
    }

    public DBContext(DbContextOptions<DBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Acceso> Accesos { get; set; }

    public virtual DbSet<Estudiante> Estudiantes { get; set; }

    public virtual DbSet<InscripcionesEstudiante> InscripcionesEstudiantes { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Materia> Materias { get; set; }

    public virtual DbSet<ProfesorMateria> ProfesorMaterias { get; set; }

    public virtual DbSet<Profesore> Profesores { get; set; }

    public virtual DbSet<Programa> Programas { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<TokensExpirado> TokensExpirados { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VwCompanerosClase> VwCompanerosClases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Acceso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Accesos__3214EC074712088F");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Contraseña).HasMaxLength(250);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Sitio).HasMaxLength(50);
        });

        modelBuilder.Entity<Estudiante>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Estudian__3214EC07AA9D55BC");

            entity.HasIndex(e => e.UsuarioId, "UQ__Estudian__2B3DE7B9F54FF88C").IsUnique();

            entity.HasIndex(e => e.Identificacion, "UQ__Estudian__D6F931E575CC7374").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Carrera).HasMaxLength(100);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Identificacion).HasMaxLength(50);
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.Programa).WithMany(p => p.Estudiantes)
                .HasForeignKey(d => d.ProgramaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Estudiant__Progr__75A278F5");

            entity.HasOne(d => d.Usuario).WithOne(p => p.Estudiante)
                .HasForeignKey<Estudiante>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Estudiant__Usuar__74AE54BC");
        });

        modelBuilder.Entity<InscripcionesEstudiante>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inscripc__3214EC074776F6B7");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("TR_ValidarMaximoMaterias");
                    tb.HasTrigger("TR_ValidarProfesorDiferente");
                });

            entity.HasIndex(e => new { e.EstudianteId, e.MateriaId }, "UQ__Inscripc__6FA69B07172F87D5").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Calificacion).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .HasDefaultValue("Inscrito");
            entity.Property(e => e.FechaInscripcion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Estudiante).WithMany(p => p.InscripcionesEstudiantes)
                .HasForeignKey(d => d.EstudianteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inscripci__Estud__7C4F7684");

            entity.HasOne(d => d.Materia).WithMany(p => p.InscripcionesEstudiantes)
                .HasForeignKey(d => d.MateriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inscripci__Mater__7D439ABD");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Logs__3214EC07EC9A7E27");

            entity.Property(e => e.Accion).HasMaxLength(200);
            entity.Property(e => e.Fecha).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Ip).HasMaxLength(45);
            entity.Property(e => e.Tipo).HasMaxLength(50);

            entity.HasOne(d => d.Usuario).WithMany(p => p.Logs)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Logs__UsuarioId__5165187F");
        });

        modelBuilder.Entity<Materia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Materias__3214EC07C84301BF");

            entity.HasIndex(e => e.Codigo, "UQ__Materias__06370DAC8289BEA4").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Codigo).HasMaxLength(20);
            entity.Property(e => e.Creditos).HasDefaultValue(3);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(d => d.CreadoPor).WithMany(p => p.MateriaCreadoPors)
                .HasForeignKey(d => d.CreadoPorId)
                .HasConstraintName("FK__Materias__Creado__5FB337D6");

            entity.HasOne(d => d.ModificadoPor).WithMany(p => p.MateriaModificadoPors)
                .HasForeignKey(d => d.ModificadoPorId)
                .HasConstraintName("FK__Materias__Modifi__60A75C0F");
        });

        modelBuilder.Entity<ProfesorMateria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Profesor__3214EC07881383F9");

            entity.HasIndex(e => new { e.ProfesorId, e.MateriaId }, "UQ__Profesor__4D23E917172AF95B").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Materia).WithMany(p => p.ProfesorMateria)
                .HasForeignKey(d => d.MateriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProfesorM__Mater__6E01572D");

            entity.HasOne(d => d.Profesor).WithMany(p => p.ProfesorMateria)
                .HasForeignKey(d => d.ProfesorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProfesorM__Profe__6D0D32F4");
        });

        modelBuilder.Entity<Profesore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Profesor__3214EC0707753FC5");

            entity.HasIndex(e => e.UsuarioId, "UQ__Profesor__2B3DE7B9DA93E326").IsUnique();

            entity.HasIndex(e => e.Identificacion, "UQ__Profesor__D6F931E55DE57CCC").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Departamento).HasMaxLength(100);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Identificacion).HasMaxLength(50);
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.Usuario).WithOne(p => p.Profesore)
                .HasForeignKey<Profesore>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Profesore__Usuar__6754599E");
        });

        modelBuilder.Entity<Programa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Programa__3214EC079EEF0AFF");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.CreditosMaximos).HasDefaultValue(9);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(d => d.CreadoPor).WithMany(p => p.ProgramaCreadoPors)
                .HasForeignKey(d => d.CreadoPorId)
                .HasConstraintName("FK__Programas__Cread__5812160E");

            entity.HasOne(d => d.ModificadoPor).WithMany(p => p.ProgramaModificadoPors)
                .HasForeignKey(d => d.ModificadoPorId)
                .HasConstraintName("FK__Programas__Modif__59063A47");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC070C77B6B4");

            entity.HasIndex(e => e.Nombre, "UQ__Roles__75E3EFCF18783A31").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tokens__3214EC07F2AF1AFE");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Ip).HasMaxLength(45);
            entity.Property(e => e.Observacion).HasMaxLength(200);
            entity.Property(e => e.Token1)
                .HasMaxLength(1000)
                .HasColumnName("Token");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tokens__UsuarioI__49C3F6B7");
        });

        modelBuilder.Entity<TokensExpirado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TokensEx__3214EC07DE781090");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Ip).HasMaxLength(45);
            entity.Property(e => e.Observacion).HasMaxLength(200);
            entity.Property(e => e.Token).HasMaxLength(1000);

            entity.HasOne(d => d.Usuario).WithMany(p => p.TokensExpirados)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TokensExp__Usuar__4D94879B");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC078464B346");

            entity.HasIndex(e => e.NombreUsuario, "UQ__Usuarios__6B0F5AE09D599C06").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Usuarios__A9D10534C4D23E82").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Contraseña).HasMaxLength(250);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.NombreUsuario).HasMaxLength(100);

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios__RolId__44FF419A");
        });

        modelBuilder.Entity<VwCompanerosClase>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VW_CompanerosClase");

            entity.Property(e => e.CompaneroNombre).HasMaxLength(201);
            entity.Property(e => e.MateriaNombre).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
