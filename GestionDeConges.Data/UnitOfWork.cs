using GestionDeConges.Core.Interfaces;
using GestionDeConges.Data.Context;
using GestionDeConges.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace GestionDeConges.Data;

public class UnitOfWork(AppDbContext ctx) : IUnitOfWork
{
    private readonly AppDbContext _ctx = ctx;
    private IDbContextTransaction? _transaction;

    // ── Repositories (lazy init) ──────────────────────────────────────────────
    private IEmployeRepository?      _employes;
    private IDemandeCongeRepository? _demandes;
    private ISoldeCongeRepository?   _soldes;
    private IPosteRepository?        _postes;
    private IUtilisateurRepository?  _utilisateurs;
    private ITypeCongeRepository?    _typesConges;
    private INotificationRepository? _notifications;

    public IEmployeRepository      Employes      => _employes      ??= new EmployeRepository(_ctx);
    public IDemandeCongeRepository Demandes      => _demandes      ??= new DemandeCongeRepository(_ctx);
    public ISoldeCongeRepository   Soldes        => _soldes        ??= new SoldeCongeRepository(_ctx);
    public IPosteRepository        Postes        => _postes        ??= new PosteRepository(_ctx);
    public IUtilisateurRepository  Utilisateurs  => _utilisateurs  ??= new UtilisateurRepository(_ctx);
    public ITypeCongeRepository    TypesConges   => _typesConges   ??= new TypeCongeRepository(_ctx);
    public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_ctx);

    public async Task<int> SaveChangesAsync() => await _ctx.SaveChangesAsync();

    public async Task BeginTransactionAsync()
        => _transaction = await _ctx.Database.BeginTransactionAsync();

    public async Task CommitAsync()
    {
        if (_transaction is not null) await _transaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        if (_transaction is not null) await _transaction.RollbackAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _ctx.Dispose();
        GC.SuppressFinalize(this);
    }
}