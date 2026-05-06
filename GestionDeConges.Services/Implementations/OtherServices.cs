using ClosedXML.Excel;
using GestionDeConges.Core.Entities;
using GestionDeConges.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionDeConges.Services.Implementations;


// ── PosteService ──────────────────────────────────────────────────────────────
public class PosteService(IUnitOfWork uow) : IPosteService
{
    private readonly IUnitOfWork _uow = uow;

    public Task<IEnumerable<Poste>> GetActifsAsync() => _uow.Postes.GetActifsAsync();
    public Task<IEnumerable<Poste>> GetSupprimesAsync() => _uow.Postes.GetSupprimesAsync();

    public async Task<ResultatOperation<Poste>> CreerAsync(Poste poste)
    {
        if (string.IsNullOrWhiteSpace(poste.Nom))
            return ResultatOperation<Poste>.Echec("Le nom du poste est requis.");
        if (await _uow.Postes.NomExisteAsync(poste.Nom, poste.IdDepartement))
            return ResultatOperation<Poste>.Echec("Ce poste existe déjà dans ce département.");

        await _uow.Postes.AddAsync(poste);
        await _uow.SaveChangesAsync();
        return ResultatOperation<Poste>.Ok(poste);
    }

    public async Task<ResultatOperation<Poste>> ModifierAsync(Poste poste)
    {
        var existant = await _uow.Postes.GetByIdAsync(poste.Id);
        if (existant is null) return ResultatOperation<Poste>.Echec("Poste introuvable.");
        if (await _uow.Postes.NomExisteAsync(poste.Nom, poste.IdDepartement, poste.Id))
            return ResultatOperation<Poste>.Echec("Ce nom est déjà pris.");

        existant.Nom = poste.Nom;
        existant.IdDepartement = poste.IdDepartement;
        existant.NbMinEmployes = poste.NbMinEmployes;
        await _uow.Postes.UpdateAsync(existant);
        await _uow.SaveChangesAsync();
        return ResultatOperation<Poste>.Ok(existant);
    }

    public async Task<ResultatOperation> SupprimerAsync(int id)
    {
        int nb = await _uow.Postes.GetNbEmployesActifsAsync(id);
        if (nb > 0)
            return ResultatOperation.Echec(
                $"Ce poste a encore {nb} employé(s) actif(s). Réaffectez-les avant de supprimer.");

        await _uow.Postes.SoftDeleteAsync(id);
        await _uow.SaveChangesAsync();
        return ResultatOperation.Ok();
    }

    public async Task<ResultatOperation> RestaurerAsync(int id)
    {
        await _uow.Postes.RestaurerAsync(id);
        await _uow.SaveChangesAsync();
        return ResultatOperation.Ok();
    }
}

// ── SoldeCongeService ─────────────────────────────────────────────────────────
public class SoldeCongeService(IUnitOfWork uow) : ISoldeCongeService
{
    private readonly IUnitOfWork _uow = uow;

    public Task<IEnumerable<SoldeConge>> GetParEmployeAsync(int idEmploye, int annee)
        => _uow.Soldes.GetParEmployeAsync(idEmploye, annee);

    public Task<SoldeConge?> GetAsync(int idEmploye, int idType, int annee)
        => _uow.Soldes.GetAsync(idEmploye, idType, annee);

    public async Task<int> GetSoldeRestantAsync(int idEmploye, int idType, int annee)
    {
        var s = await _uow.Soldes.GetAsync(idEmploye, idType, annee);
        return s?.Restant ?? 0;
    }

    public async Task InitialiserSoldesNouvelAnAsync(int annee)
    {
        var employes = await _uow.Employes.GetActifsAsync();
        foreach (var emp in employes)
            await _uow.Soldes.InitialiserSoldesAsync(emp.Id, annee);
        await _uow.SaveChangesAsync();
    }
}

// ── NotificationService ───────────────────────────────────────────────────────
public class NotificationService(IUnitOfWork uow) : INotificationService
{
    private readonly IUnitOfWork _uow = uow;

    public Task<IEnumerable<Notification>> GetNonLuesAsync(int idEmploye)
        => _uow.Notifications.GetNonLuesAsync(idEmploye);

    public Task<int> CountNonLuesAsync(int idEmploye)
        => _uow.Notifications.CountNonLuesAsync(idEmploye);

    public async Task MarquerLueAsync(int id)
    {
        await _uow.Notifications.MarquerLuAsync(id);
        await _uow.SaveChangesAsync();
    }

    public async Task MarquerToutesLuesAsync(int idEmploye)
    {
        await _uow.Notifications.MarquerToutesLuesAsync(idEmploye);
        await _uow.SaveChangesAsync();
    }

    public async Task EnvoyerAsync(int idEmploye, string titre, string message)
    {
        await _uow.Notifications.AddAsync(new Notification
        {
            IdEmploye = idEmploye,
            Titre = titre,
            Message = message
        });
        await _uow.SaveChangesAsync();
    }
}

// ── RapportService ────────────────────────────────────────────────────────────
public class RapportService(IUnitOfWork uow) : IRapportService
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<byte[]> ExporterCongesExcelAsync(int? annee = null)
    {
        var demandes = await _uow.Demandes.GetToutesAvecDetailsAsync();
        if (annee.HasValue) demandes = demandes.Where(d => d.DateDebut.Year == annee.Value);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Congés");

        string[] headers =
        [
            "ID", "Employé", "Département", "Type",
            "Début", "Fin", "Jours", "Statut",
            "Soumis le", "Traité par", "Commentaire"
        ];
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(29, 158, 117);
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var d in demandes)
        {
            ws.Cell(row, 1).Value = d.Id;
            ws.Cell(row, 2).Value = d.Employe?.NomComplet ?? "";
            ws.Cell(row, 3).Value = d.Employe?.Poste?.Departement?.Nom ?? "";
            ws.Cell(row, 4).Value = d.TypeConge?.Libelle ?? "";
            ws.Cell(row, 5).Value = d.DateDebut.ToDateTime(TimeOnly.MinValue);
            ws.Cell(row, 5).Style.NumberFormat.Format = "dd/MM/yyyy";
            ws.Cell(row, 6).Value = d.DateFin.ToDateTime(TimeOnly.MinValue);
            ws.Cell(row, 6).Style.NumberFormat.Format = "dd/MM/yyyy";
            ws.Cell(row, 7).Value = d.NbJours;
            ws.Cell(row, 8).Value = d.Statut.ToString();
            ws.Cell(row, 9).Value = d.CreeLe;
            ws.Cell(row, 9).Style.NumberFormat.Format = "dd/MM/yyyy";
            ws.Cell(row, 10).Value = d.Admin?.NomUtilisateur ?? "-";
            ws.Cell(row, 11).Value = d.CommentaireAdmin ?? "";

            var fillColor = d.Statut switch
            {
                Core.Enums.StatutDemande.Approuve => XLColor.FromArgb(234, 243, 222),
                Core.Enums.StatutDemande.Refuse => XLColor.FromArgb(252, 235, 235),
                Core.Enums.StatutDemande.EnAttente => XLColor.FromArgb(250, 238, 218),
                _ => XLColor.White
            };
            ws.Row(row).Style.Fill.BackgroundColor = fillColor;
            row++;
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExporterSoldesExcelAsync(int annee)
    {
        var soldes = await _uow.Soldes.GetTousParAnneeAsync(annee);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add($"Soldes {annee}");

        string[] headers = ["Employé", "Type de congé", "Quota", "Pris", "Reporté", "Restant"];
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(29, 158, 117);
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var s in soldes.OrderBy(x => x.Employe?.Nom))
        {
            ws.Cell(row, 1).Value = s.Employe?.NomComplet ?? "";
            ws.Cell(row, 2).Value = s.TypeConge?.Libelle ?? "";
            ws.Cell(row, 3).Value = s.Quota;
            ws.Cell(row, 4).Value = s.Pris;
            ws.Cell(row, 5).Value = s.Reporte;
            ws.Cell(row, 6).Value = s.Restant;
            if (s.Restant < 3)
                ws.Cell(row, 6).Style.Font.FontColor = XLColor.Red;
            row++;
        }
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // CORRECTION : ExporterCongesPdfAsync retournait un tableau vide []
    // Implémentation QuestPDF complète
    public async Task<byte[]> ExporterCongesPdfAsync(int? annee = null)
    {
        var demandes = (await _uow.Demandes.GetToutesAvecDetailsAsync()).ToList();
        if (annee.HasValue)
            demandes = demandes.Where(d => d.DateDebut.Year == annee.Value).ToList();

        var doc = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, QuestPDF.Infrastructure.Unit.Centimetre);

                page.Header()
                    .Text($"Rapport des Congés — {annee ?? DateTime.Now.Year}")
                    .FontSize(18).Bold()
                    .FontColor(QuestPDF.Infrastructure.Color.FromHex("#1D9E75"));

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(col =>
                    {
                        col.RelativeColumn(2f);   // Employé
                        col.RelativeColumn(1.5f); // Type
                        col.RelativeColumn(1.2f); // Début
                        col.RelativeColumn(1.2f); // Fin
                        col.RelativeColumn(0.7f); // Jours
                        col.RelativeColumn(1.2f); // Statut
                    });

                    // En-têtes
                    table.Header(header =>
                    {
                        foreach (var h in new[] { "Employé", "Type", "Début", "Fin", "Jours", "Statut" })
                        {
                            header.Cell()
                                .Background(Colors.Green.Darken2)  // Équivalent du #1D9E75
                                .Padding(5)
                                .Text(h)
                                .FontColor(Colors.White)
                                .Bold();
                        }
                    });

                   

                    foreach (var d in demandes)
                    {
                        table.Cell().Padding(4).Text(d.Employe?.NomComplet ?? "");
                        table.Cell().Padding(4).Text(d.TypeConge?.Libelle ?? "");
                        table.Cell().Padding(4).Text(d.DateDebut.ToString("dd/MM/yy"));
                        table.Cell().Padding(4).Text(d.DateFin.ToString("dd/MM/yy"));
                        table.Cell().Padding(4).Text(d.NbJours.ToString());
                        table.Cell().Padding(4).Text(d.Statut.ToString());
                    }
                });

                page.Footer().AlignRight().Text(txt =>
                {
                    txt.Span("Page ");
                    txt.CurrentPageNumber();
                    txt.Span(" / ");
                    txt.TotalPages();
                });
            });
        });

        using var ms = new MemoryStream();
        doc.GeneratePdf(ms);
        return ms.ToArray();
    }

    public async Task<StatistiquesGlobales> GetStatistiquesAsync(int annee)
    {
        var statuts = await _uow.Demandes.GetStatistiquesStatutsAsync(annee);
        var employes = (await _uow.Employes.GetActifsAsync()).ToList();
        var demandes = (await _uow.Demandes.GetToutesAvecDetailsAsync())
                           .Where(d => d.DateDebut.Year == annee).ToList();

        var parType = demandes
            .GroupBy(d => d.TypeConge?.Libelle ?? "Inconnu")
            .ToDictionary(g => g.Key, g => g.Count());

        var parMois = demandes
            .GroupBy(d => d.DateDebut.Month)
            .ToDictionary(g => $"{g.Key:D2}", g => g.Count());

        int enConge = employes.Count(e =>
            demandes.Any(d =>
                d.IdEmploye == e.Id &&
                d.Statut == Core.Enums.StatutDemande.Approuve &&
                d.DateDebut <= DateOnly.FromDateTime(DateTime.Today) &&
                d.DateFin >= DateOnly.FromDateTime(DateTime.Today)));

        var top = demandes
            .Where(d => d.Statut == Core.Enums.StatutDemande.Approuve)
            .GroupBy(d => new { d.IdEmploye, d.Employe?.NomComplet, Type = d.TypeConge?.Libelle })
            .Select(g => new TopEmployeConge
            {
                Employe = g.Key.NomComplet ?? "",
                JoursPris = g.Sum(x => x.NbJours),
                TypeConge = g.Key.Type ?? ""
            })
            .OrderByDescending(x => x.JoursPris)
            .Take(5)
            .ToList();

        return new StatistiquesGlobales
        {
            Annee = annee,
            TotalDemandes = demandes.Count,
            EnAttente = statuts.GetValueOrDefault(Core.Enums.StatutDemande.EnAttente),
            Approuvees = statuts.GetValueOrDefault(Core.Enums.StatutDemande.Approuve),
            Refusees = statuts.GetValueOrDefault(Core.Enums.StatutDemande.Refuse),
            TotalEmployes = employes.Count,
            EmployesEnConge = enConge,
            DemandesParType = parType,
            DemandesParMois = parMois,
            TopConsommateurs = top
        };
    }
}