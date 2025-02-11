# Fonctions et programmes facilitant les travaux pratiques de programmation C#.
## ArchiBuilder : (bêta)
ArchiBuilder est un programme qui crée un squelette de TP à partir du sujet. Il utilise des identifiants Forge pour cloner le dépôt git et récupérer le sujet.

### Installation :
 ```git clone -n --depth=1 --filter=tree:0 https://github.com/B1-QoL/QoL-TP-Programmation.git ;```<br />
   ```cd QoL-TP-Programmation ;```<br />
   ```git sparse-checkout set --no-cone ArchiBuilder/bin ;```<br />
   ```git checkout```
   
### Utilisation :
```ArchiBuilder *lien Forge du TP*```  \
Il faut mettre le **lien Forge** et non le lien du sujet.

Pour ne pas cloner le dépôt git et créer le TP dans le dossier courant : \
```ArchiBuilder -l *lien Forge du TP*```

Pour ne pas créer le projet et juste ajouter les fichiers manquant dans le dossier courant : \
```ArchiBuilder -d *lien Forge du TP*```

Si un fichier existe déjà, ArchiBuilder demande s'il doit le réécrire ou l'ignorer.

ArchiBuilder a besoin de B1.dll pour fonctionner. Il cherche d'abord dans le dossier courant ou parent QoL-TP-Programmation ; s'il le trouve, il l'inclus dans le projet, sinon, il le clône.

## Description de la bibliothèque :
Comprend :
  - Une fonction d'affichage ;
  - Une fonction de parsing.

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

### Notes :
Les ```StringBuilder``` peuvent être remplacés par des ```string``` pour les petites chaînes de caractères.
