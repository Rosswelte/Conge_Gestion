using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestionDeConges.Data.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // ── DbSets ───────────────────────────────────────────────────────────────
    public DbSet<Departement>     Departements     { get; set; }
    public DbSet<Poste>           Postes           { get; set; }
    public DbSet<Employe>         Employes         { get; set; }
    public DbSet<Utilisateur>     Utilisateurs     { get; set; }
    public DbSet<TypeConge>       TypesConges      { get; set; }
    public DbSet<SoldeConge>      SoldesConges     { get; set; }
    public DbSet<DemandeConge>    DemandesConges   { get; set; }
    public DbSet<HistoriqueAction> Historiques     { get; set; }
    public DbSet<Notification>    Notifications    { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ── Département ──────────────────────────────────────────────────────
        mb.Entity<Departement>(e =>
        {
            e.ToTable("departements");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nom).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(255);
            e.HasIndex(x => x.Nom).IsUnique();
        });

        // ── Poste ────────────────────────────────────────────────────────────
        mb.Entity<Poste>(e =>
        {
            e.ToTable("postes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nom).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Departement)
             .WithMany(d => d.Postes)
             .HasForeignKey(x => x.IdDepartement)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Nom, x.IdDepartement }).IsUnique();
            e.HasQueryFilter(x => !x.EstSupprime); // filtre global soft-delete
        });

        // ── Employé ──────────────────────────────────────────────────────────
        mb.Entity<Employe>(e =>
        {
            e.ToTable("employes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nom).HasMaxLength(60).IsRequired();
            e.Property(x => x.Prenom).HasMaxLength(60).IsRequired();
            e.Property(x => x.Email).HasMaxLength(120).IsRequired();
            e.Property(x => x.Telephone).HasMaxLength(20);
            e.Property(x => x.Sexe).HasConversion<string>().HasMaxLength(10);
            e.HasOne(x => x.Poste)
             .WithMany(p => p.Employes)
             .HasForeignKey(x => x.IdPoste)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasQueryFilter(x => !x.EstSupprime);
            e.Ignore(x => x.NomComplet);
            e.Ignore(x => x.Anciennete);
        });

        // ── Utilisateur ──────────────────────────────────────────────────────
        mb.Entity<Utilisateur>(e =>
        {
            e.ToTable("utilisateurs");
            e.HasKey(x => x.Id);
            e.Property(x => x.NomUtilisateur).HasMaxLength(60).IsRequired();
            e.Property(x => x.MotDePasse).HasMaxLength(255).IsRequired();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Employe)
             .WithOne(emp => emp.Utilisateur)
             .HasForeignKey<Utilisateur>(x => x.IdEmploye)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => x.NomUtilisateur).IsUnique();
        });

        // ── TypeConge ────────────────────────────────────────────────────────
        mb.Entity<TypeConge>(e =>
        {
            e.ToTable("types_conges");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Libelle).HasMaxLength(100).IsRequired();
            e.Property(x => x.SexeRequis).HasConversion<string>().HasMaxLength(10);
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ── SoldeConge ───────────────────────────────────────────────────────
        mb.Entity<SoldeConge>(e =>
        {
            e.ToTable("soldes_conges");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Employe)
             .WithMany(emp => emp.Soldes)
             .HasForeignKey(x => x.IdEmploye)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.TypeConge)
             .WithMany(t => t.Soldes)
             .HasForeignKey(x => x.IdTypeConge)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.IdEmploye, x.IdTypeConge, x.Annee }).IsUnique();
            e.Ignore(x => x.Restant);
        });

        // ── DemandeConge ─────────────────────────────────────────────────────
        mb.Entity<DemandeConge>(e =>
        {
            e.ToTable("demandes_conges");
            e.HasKey(x => x.Id);
            e.Property(x => x.Statut).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Motif).HasColumnType("text");
            e.Property(x => x.CommentaireAdmin).HasColumnType("text");
            e.Property(x => x.CheminPreuve).HasMaxLength(500);
            e.HasOne(x => x.Employe)
             .WithMany(emp => emp.Demandes)
             .HasForeignKey(x => x.IdEmploye)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TypeConge)
             .WithMany(t => t.Demandes)
             .HasForeignKey(x => x.IdTypeConge)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Admin)
             .WithMany(u => u.DemandesTraitees)
             .HasForeignKey(x => x.TraitePar)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => !x.EstSupprime);
            e.Ignore(x => x.EstModifiable);
            e.Ignore(x => x.EstEnAttente);
        });

        // ── HistoriqueAction ─────────────────────────────────────────────────
        mb.Entity<HistoriqueAction>(e =>
        {
            e.ToTable("historique_actions");
            e.HasKey(x => x.Id);
            e.Property(x => x.TableCible).HasMaxLength(50);
            e.Property(x => x.Action).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Details).HasColumnType("text");
            e.HasOne(x => x.Utilisateur)
             .WithMany()
             .HasForeignKey(x => x.IdUtil)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Notification ─────────────────────────────────────────────────────
        mb.Entity<Notification>(e =>
        {
            e.ToTable("notifications");
            e.HasKey(x => x.Id);
            e.Property(x => x.Titre).HasMaxLength(150).IsRequired();
            e.Property(x => x.Message).HasColumnType("text").IsRequired();
            e.HasOne(x => x.Employe)
             .WithMany(emp => emp.Notifications)
             .HasForeignKey(x => x.IdEmploye)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>Intercepte SaveChanges pour horodater automatiquement.</summary>
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreeLe = DateTime.UtcNow;
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.ModifieLe = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(ct);
    }
}