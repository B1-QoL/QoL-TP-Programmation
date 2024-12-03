# Bibliothèque de fonctions facilitant les travaux pratiques de programmation.
## Description :
Comprend :
  - Une fonction C# d'affichage.

## Installation de la bibliothèque C# :
1) Télécharger le dossier bin :<br />
   ```git clone -n --depth=1 --filter=tree:0 https://github.com/B1-QoL/QoL-TP-Programmation.git ;```<br />
   ```cd QoL-TP-Programmation ;```<br />
   ```git sparse-checkout set --no-cone /bin ;```<br />
   ```git checkout```
3) Ouvrir son IDE puis son projet ;
4) Ajouter une référence à bin/B1.dll en faisant un clique droit sur "Dependencies" puis en cliquant sur "Reference..." ;
5) Ajouter ```using static B1.Affichage;``` dans Programme.cs.

**ATTENTION : NE PAS INCLURE LA BIBLIOTHEQUE DANS LES AUTRES FICHIERS !**\
La moulinette ne vous autorisera pas leur utilisation.
