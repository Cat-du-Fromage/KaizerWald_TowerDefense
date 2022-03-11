# Cahier des charges : Castle Defense : Real time stratégie



## Le joueur

- Le joueur doit défendre un château au centre du terrain

- Le joueur à a sa disposition des unités qu'il peut contrôle

- Le joueur devra empêcher les ennemies d'atteindre le centre du château

- Il y aura 1-2 couches de portes qui permettront au joueur de temporiser le temps qu'elles soit détruites

  

## Troupes contrôlé par le joueur

* Sont organisées en régiment : une entités qui en régie plusieurs autres

* Démarre la partie avec un nombre de régiments de bases

* Déplacement en groupe (voir mécanique de jeu : Total war)

* Attaque en groupe (une unité engagé en combat met tout le groupe en position de combat)

* les unités tir sur 1 rangée, donc plus le joueur entend ses formations, plus elles feront de dégâts

* pas de tirs à travers les camarades : le joueur ne pourra pas entasser les regiments les uns sur les autres pour maximiser le nombre de tirs

  

* A la fin de chaque Manche : 

  * les unités blessé récupère 20% de leur capacité maximal (donc si 1/10 +(10*0.2) => 3/10)
  *  le joueur pourra dépenser des ressources pour avoir de nouvelles troupes ou ravitailler celles blessées

  

* (SI LE TEMPS)

* Système de promotions : les unités qui survivent gagnent en expérience en fonction du nombre d'ennemies tuées durant la manche

* améliore leur capacité de combat



## Ennemies

* Apparaissent par vagues successives
* Leur objectifs est d'atteindre le centre du château
* Viseront en premier lieu les portes sur leur passages si elles sont encore debout
* Attaquent en priorité les troupes du joueur qui sont sur leur passage
* 2 types d'ennemie
  * distance
  * corps à corps
  * escaladeur : pourront escalader les murs pour atteindre directement le centre
* Opère en groupe mais pas en régiment (pas de notion de formation comme les troupes du joueur)
* Système de Moral : influence les capacités de combats du groupe (uniquement en faveur du joueur)
  * moral baisse en fonction du nombre de troupe encore en vie par rapport au nombre de départ
  * Si le moral est trop bas, le groupe peut partir en déroute la forçant à fuir en direction du bord du terrain
    * En déroute : le groupe ne se défend plus mais peut être achevé(les attaquant ne peuvent pas subir de dégâts d'une unités en déroute)
    * En déroute : Le groupe à une chance de "se ressaisir" et de revenir au combat (incitant le joueur à achever les ennemis en fuite)



## Pathfinding

* HPA : hierarchical pathfinding qui comprendra :

  * Découpage du terrain en une grille partitionnée (Chunk + cellules)

  * Les Chunks contenant l'information suivante : chunks voisins accessibles(non bloqués par des obstacles)

  * Un Flowfield qui permettra aux entité de savoir la direction a prendre en fonction de leur positions sur la grille

    

* (Risque de Difficulté) Steering Behaviour : Système de Leader permettant de guider le régiment durant leur mouvement

* (Risque de Difficulté) Formation de Groupe : les unités du régiment se battent et avancent comme une seul entité

* (Risque de Difficulté : voir continuum-crowds) Adaptation à l'environnement : doivent pouvoir anticiper les collisions avec d'autres entités