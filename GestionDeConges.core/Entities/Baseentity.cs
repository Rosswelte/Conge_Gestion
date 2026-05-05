namespace GestionDeConges.Core.Entities;

/// <summary>Entité de base avec audit et soft-delete.</summary>
public abstract class BaseEntity
{
    public int      Id         { get; set; }
    public DateTime CreeLe     { get; set; } = DateTime.UtcNow;
    public DateTime ModifieLe  { get; set; } = DateTime.UtcNow;
    public bool     EstSupprime { get; set; } = false;
    public DateTime? SupprimeLe { get; set; }
}