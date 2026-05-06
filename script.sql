-- =============================================================
--  Gestion des Congés — Script SQL complet (corrigé)
--  Compatible XAMPP / MariaDB 10.4+ / MySQL 8+
--  Exécuter dans phpMyAdmin ou MySQL Workbench
-- =============================================================

DROP DATABASE IF EXISTS `gestion_conges`;
CREATE DATABASE `gestion_conges`
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
USE `gestion_conges`;

-- ── Départements ──────────────────────────────────────────────────────────────
CREATE TABLE `departements` (
  `Id`          INT          NOT NULL AUTO_INCREMENT,
  `Nom`         VARCHAR(100) NOT NULL UNIQUE,
  `Description` VARCHAR(255) NULL,
  `EstActif`    TINYINT(1)   NOT NULL DEFAULT 1,
  `CreeLe`      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB;

-- ── Postes ────────────────────────────────────────────────────────────────────
CREATE TABLE `postes` (
  `Id`             INT         NOT NULL AUTO_INCREMENT,
  `Nom`            VARCHAR(100) NOT NULL,
  `IdDepartement`  INT         NOT NULL,
  `NbMinEmployes`  INT         NOT NULL DEFAULT 1,
  `EstActif`       TINYINT(1)  NOT NULL DEFAULT 1,
  `EstSupprime`    TINYINT(1)  NOT NULL DEFAULT 0,
  `SupprimeLe`     DATETIME    NULL,
  `CreeLe`         DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ModifieLe`      DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_poste_dept` (`Nom`, `IdDepartement`),
  CONSTRAINT `fk_poste_dept` FOREIGN KEY (`IdDepartement`)
    REFERENCES `departements`(`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB;

-- ── Employés ──────────────────────────────────────────────────────────────────
CREATE TABLE `employes` (
  `Id`             INT          NOT NULL AUTO_INCREMENT,
  `Nom`            VARCHAR(60)  NOT NULL,
  `Prenom`         VARCHAR(60)  NOT NULL,
  `Email`          VARCHAR(120) NOT NULL UNIQUE,
  `Telephone`      VARCHAR(20)  NULL,
  `DateNaissance`  DATE         NULL,
  `Sexe`           ENUM('M','F','Autre') NOT NULL DEFAULT 'M',
  `IdPoste`        INT          NOT NULL,
  `DateEmbauche`   DATE         NOT NULL DEFAULT (CURDATE()),
  `EstActif`       TINYINT(1)   NOT NULL DEFAULT 1,
  `EstSupprime`    TINYINT(1)   NOT NULL DEFAULT 0,
  `SupprimeLe`     DATETIME     NULL,
  `SupprimePar`    INT          NULL,
  `CreeLe`         DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ModifieLe`      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  CONSTRAINT `fk_employe_poste` FOREIGN KEY (`IdPoste`)
    REFERENCES `postes`(`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB;

-- ── Utilisateurs ──────────────────────────────────────────────────────────────
CREATE TABLE `utilisateurs` (
  `Id`                INT         NOT NULL AUTO_INCREMENT,
  `NomUtilisateur`    VARCHAR(60) NOT NULL UNIQUE,
  `MotDePasse`        VARCHAR(255) NOT NULL,
  `Role`              ENUM('Admin','Employe') NOT NULL DEFAULT 'Employe',
  `IdEmploye`         INT         NULL,
  `EstActif`          TINYINT(1)  NOT NULL DEFAULT 1,
  `DerniereConnexion` DATETIME    NULL,
  `CreeLe`            DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  CONSTRAINT `fk_util_employe` FOREIGN KEY (`IdEmploye`)
    REFERENCES `employes`(`Id`) ON DELETE SET NULL
) ENGINE=InnoDB;

-- ── Types de congé ────────────────────────────────────────────────────────────
CREATE TABLE `types_conges` (
  `Id`              INT         NOT NULL AUTO_INCREMENT,
  `Code`            VARCHAR(20) NOT NULL UNIQUE,
  `Libelle`         VARCHAR(100) NOT NULL,
  `QuotaJours`      INT         NOT NULL DEFAULT 0,
  `EstPaye`         TINYINT(1)  NOT NULL DEFAULT 1,
  `NecessitePreuve` TINYINT(1)  NOT NULL DEFAULT 0,
  `SexeRequis`      ENUM('M','F','Tous') NOT NULL DEFAULT 'Tous',
  `EstActif`        TINYINT(1)  NOT NULL DEFAULT 1,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB;

-- ── Soldes de congés ──────────────────────────────────────────────────────────
CREATE TABLE `soldes_conges` (
  `Id`          INT      NOT NULL AUTO_INCREMENT,
  `IdEmploye`   INT      NOT NULL,
  `IdTypeConge` INT      NOT NULL,
  `Annee`       SMALLINT NOT NULL,
  `Quota`       INT      NOT NULL DEFAULT 0,
  `Pris`        INT      NOT NULL DEFAULT 0,
  `Reporte`     INT      NOT NULL DEFAULT 0,
  `ModifieLe`   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uq_solde` (`IdEmploye`, `IdTypeConge`, `Annee`),
  CONSTRAINT `fk_solde_emp`  FOREIGN KEY (`IdEmploye`)   REFERENCES `employes`(`Id`)    ON DELETE CASCADE,
  CONSTRAINT `fk_solde_type` FOREIGN KEY (`IdTypeConge`) REFERENCES `types_conges`(`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB;

-- ── Demandes de congé ─────────────────────────────────────────────────────────
CREATE TABLE `demandes_conges` (
  `Id`               INT      NOT NULL AUTO_INCREMENT,
  `IdEmploye`        INT      NOT NULL,
  `IdTypeConge`      INT      NOT NULL,
  `DateDebut`        DATE     NOT NULL,
  `DateFin`          DATE     NOT NULL,
  `NbJours`          INT      NOT NULL DEFAULT 0,
  `Motif`            TEXT     NULL,
  `CheminPreuve`     VARCHAR(500) NULL,
  `Statut`           ENUM('EnAttente','Approuve','Refuse','Annule') NOT NULL DEFAULT 'EnAttente',
  `TraitePar`        INT      NULL,
  `TraiteLe`         DATETIME NULL,
  `CommentaireAdmin` TEXT     NULL,
  `EstSupprime`      TINYINT(1) NOT NULL DEFAULT 0,
  `SupprimeLe`       DATETIME NULL,
  `SupprimePar`      INT      NULL,
  `CreeLe`           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ModifieLe`        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  CONSTRAINT `fk_dem_emp`   FOREIGN KEY (`IdEmploye`)   REFERENCES `employes`(`Id`)     ON DELETE RESTRICT,
  CONSTRAINT `fk_dem_type`  FOREIGN KEY (`IdTypeConge`) REFERENCES `types_conges`(`Id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_dem_admin` FOREIGN KEY (`TraitePar`)   REFERENCES `utilisateurs`(`Id`) ON DELETE SET NULL
) ENGINE=InnoDB;

-- ── Historique / Audit ────────────────────────────────────────────────────────
CREATE TABLE `historique_actions` (
  `Id`         INT      NOT NULL AUTO_INCREMENT,
  `TableCible` VARCHAR(50)  NOT NULL,
  `IdEnreg`    INT      NOT NULL,
  `Action`     ENUM('Insertion','Modification','Suppression','Restauration') NOT NULL,
  `Details`    TEXT     NULL,
  `IdUtil`     INT      NULL,
  `RealiseLe`  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  CONSTRAINT `fk_hist_util` FOREIGN KEY (`IdUtil`)
    REFERENCES `utilisateurs`(`Id`) ON DELETE SET NULL
) ENGINE=InnoDB;

-- ── Notifications ─────────────────────────────────────────────────────────────
CREATE TABLE `notifications` (
  `Id`        INT      NOT NULL AUTO_INCREMENT,
  `IdEmploye` INT      NOT NULL,
  `Titre`     VARCHAR(150) NOT NULL,
  `Message`   TEXT     NOT NULL,
  `EstLu`     TINYINT(1) NOT NULL DEFAULT 0,
  `LuLe`      DATETIME NULL,
  `CreeLe`    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  CONSTRAINT `fk_notif_emp` FOREIGN KEY (`IdEmploye`)
    REFERENCES `employes`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB;

-- =============================================================
--  DONNÉES DE TEST
-- =============================================================

-- Départements
INSERT INTO `departements` (`Nom`, `Description`) VALUES
  ('Développement',  'Équipe technique et développement logiciel'),
  ('Ressources Humaines', 'Gestion du personnel'),
  ('Commercial',     'Ventes et relation client'),
  ('Comptabilité',   'Finance et comptabilité');

-- Postes
INSERT INTO `postes` (`Nom`, `IdDepartement`, `NbMinEmployes`) VALUES
  ('Développeur',          1, 2),
  ('Chef de projet',       1, 1),
  ('Responsable RH',       2, 1),
  ('Chargé RH',            2, 1),
  ('Commercial',           3, 2),
  ('Responsable commercial', 3, 1),
  ('Comptable',            4, 1),
  ('Directeur financier',  4, 1);

-- Employés
INSERT INTO `employes` (`Nom`, `Prenom`, `Email`, `Telephone`, `DateNaissance`, `Sexe`, `IdPoste`, `DateEmbauche`) VALUES
  ('Dupont',   'Alice',   'alice.dupont@entreprise.mg',   '034 11 111 11', '1990-03-15', 'F', 1, '2020-01-10'),
  ('Martin',   'Bob',     'bob.martin@entreprise.mg',     '034 22 222 22', '1985-07-22', 'M', 1, '2019-06-01'),
  ('Leroy',    'Camille', 'camille.leroy@entreprise.mg',  '034 33 333 33', '1992-11-08', 'F', 2, '2021-03-15'),
  ('Moreau',   'David',   'david.moreau@entreprise.mg',   '034 44 444 44', '1988-05-30', 'M', 3, '2018-09-01'),
  ('Simon',    'Eva',     'eva.simon@entreprise.mg',      '034 55 555 55', '1995-01-25', 'F', 4, '2022-02-01'),
  ('Laurent',  'Frank',   'frank.laurent@entreprise.mg',  '034 66 666 66', '1987-09-12', 'M', 5, '2017-04-15'),
  ('Petit',    'Grace',   'grace.petit@entreprise.mg',    '034 77 777 77', '1993-06-18', 'F', 5, '2023-01-10'),
  ('Thomas',   'Henri',   'henri.thomas@entreprise.mg',   '034 88 888 88', '1980-12-03', 'M', 7, '2016-07-01');

-- Utilisateurs (mots de passe en clair pour les tests)
-- En production : remplacer par des hash BCrypt
INSERT INTO `utilisateurs` (`NomUtilisateur`, `MotDePasse`, `Role`, `IdEmploye`) VALUES
  ('Administrateur', 'azerty',  'Admin',   NULL),
  ('admin',          '123456',  'Admin',   NULL),
  ('alice',          'pass123', 'Employe', 1),
  ('bob',            'pass123', 'Employe', 2),
  ('camille',        'pass123', 'Employe', 3);

-- Types de congé
INSERT INTO `types_conges` (`Code`, `Libelle`, `QuotaJours`, `EstPaye`, `NecessitePreuve`, `SexeRequis`) VALUES
  ('ANN',  'Congé annuel',              25, 1, 0, 'Tous'),
  ('MAL',  'Congé maladie',             15, 1, 1, 'Tous'),
  ('MAT',  'Congé maternité',           90, 1, 1, 'F'),
  ('PAT',  'Congé paternité',           10, 1, 1, 'M'),
  ('EXC',  'Congé exceptionnel',         5, 1, 1, 'Tous'),
  ('FORM', 'Congé formation',           10, 1, 0, 'Tous'),
  ('SNP',  'Congé sans solde',           0, 0, 0, 'Tous');

-- Soldes de congés (année courante)
SET @annee = YEAR(CURDATE());

INSERT INTO `soldes_conges` (`IdEmploye`, `IdTypeConge`, `Annee`, `Quota`, `Pris`) VALUES
  (1, 1, @annee, 25, 5),  -- Alice : annuel 25j, 5 pris
  (1, 2, @annee, 15, 0),
  (1, 5, @annee,  5, 0),
  (2, 1, @annee, 25, 10), -- Bob : annuel 25j, 10 pris
  (2, 2, @annee, 15, 3),
  (2, 5, @annee,  5, 0),
  (3, 1, @annee, 25, 0),
  (3, 2, @annee, 15, 0),
  (3, 5, @annee,  5, 2),
  (4, 1, @annee, 25, 15),
  (4, 2, @annee, 15, 5),
  (5, 1, @annee, 25, 2),
  (5, 2, @annee, 15, 0),
  (6, 1, @annee, 25, 8),
  (7, 1, @annee, 25, 0),
  (8, 1, @annee, 25, 20);

-- Demandes de congé exemples
INSERT INTO `demandes_conges`
  (`IdEmploye`, `IdTypeConge`, `DateDebut`, `DateFin`, `NbJours`, `Statut`, `Motif`, `TraitePar`, `TraiteLe`) VALUES
  (1, 1, DATE_ADD(CURDATE(), INTERVAL 7  DAY), DATE_ADD(CURDATE(), INTERVAL 16 DAY), 8,  'EnAttente', 'Vacances familiales', NULL, NULL),
  (2, 2, DATE_SUB(CURDATE(), INTERVAL 5  DAY), DATE_SUB(CURDATE(), INTERVAL 3  DAY), 3,  'Approuve',  'Grippe',              1,    DATE_SUB(CURDATE(), INTERVAL 6 DAY)),
  (3, 5, DATE_ADD(CURDATE(), INTERVAL 14 DAY), DATE_ADD(CURDATE(), INTERVAL 15 DAY), 2,  'EnAttente', 'Mariage',             NULL, NULL),
  (4, 1, DATE_SUB(CURDATE(), INTERVAL 30 DAY), DATE_SUB(CURDATE(), INTERVAL 21 DAY), 8,  'Approuve',  NULL,                  1,    DATE_SUB(CURDATE(), INTERVAL 31 DAY)),
  (5, 1, DATE_ADD(CURDATE(), INTERVAL 1  DAY), DATE_ADD(CURDATE(), INTERVAL 3  DAY), 3,  'EnAttente', NULL,                  NULL, NULL),
  (6, 1, DATE_SUB(CURDATE(), INTERVAL 10 DAY), DATE_SUB(CURDATE(), INTERVAL 3  DAY), 6,  'Refuse',    'Période chargée',     1,    DATE_SUB(CURDATE(), INTERVAL 11 DAY)),
  (8, 1, DATE_ADD(CURDATE(), INTERVAL 20 DAY), DATE_ADD(CURDATE(), INTERVAL 29 DAY), 8,  'EnAttente', 'Congé bien mérité',   NULL, NULL);

-- Notification de test
INSERT INTO `notifications` (`IdEmploye`, `Titre`, `Message`) VALUES
  (2, 'Congé approuvé ✅', 'Votre demande de congé maladie a été approuvée.'),
  (6, 'Congé refusé ❌',   'Votre demande de congé annuel a été refusée. Motif : Période chargée');

SELECT 'Base de données créée avec succès !' AS Statut;