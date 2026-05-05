namespace GestionDeConges.Core.Enums;

public enum StatutDemande
{
    EnAttente,
    Approuve,
    Refuse,
    Annule
}

public enum RoleUtilisateur
{
    Admin,
    Employe
}

public enum Sexe
{
    M,
    F,
    Autre
}

public enum SexeRequisConge
{
    M,
    F,
    Tous
}

public enum TypeAction
{
    Insertion,
    Modification,
    Suppression,
    Restauration
}