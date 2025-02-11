# Fonctions et programmes facilitant les travaux pratiques de programmation C#.
## Description de la bibliothèque :
Comprend :
  - Une fonction C# d'affichage.

## Installation de la bibliothèque :
1) Télécharger le dossier bin :<br />
   ```git clone -n --depth=1 --filter=tree:0 https://github.com/B1-QoL/QoL-TP-Programmation.git ;```<br />
   ```cd QoL-TP-Programmation ;```<br />
   ```git sparse-checkout set --no-cone Bibliothèque/bin ;```<br />
   ```git checkout```
3) Ouvrir son IDE puis son projet ;
4) Ajouter une référence à Bibliothèque/bin/B1.dll en faisant un clique droit sur "Dependencies" puis en cliquant sur "Reference..." ;
5) Ajouter ```using static B1.*Affichage/Parsing*;``` dans les fichiers où vous en avez besoin.

**ATTENTION : NE PAS OUBLIER D'EN RETIRER TOUTE MENTION AVANT DE SOUMETTRE !**\
La moulinette ne vous autorisera pas son utilisation dans les fichiers autres que Program.cs.

# Notes :
Les ```StringBuilder``` peuvent être remplacés par des ```string``` pour des petites chaînes de caractères.
