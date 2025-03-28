using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RecetteLibrary", menuName = "Game/Recette Library")]
public class Recette_Library : ScriptableObject
{
    [System.Serializable]
    public class IngredientData
    {
        public int idRessource;  // ID de la ressource
        public string nomRessource; // Nom de l'ingr�dient
        public int quantite;     // Quantit� requise
        public Sprite iconRessource;    // Image associ�e � la ressource
    }

    [System.Serializable]
    public class RecetteData
    {
        public int idRecette; // ID unique de la recette
        public string nomRecette; // Nom de la recette
        public Sprite iconRecette;    // Image associ�e � la recette
        public int idBatiment; // ID batiment
        public int idOutils;
        public bool isAxe;
        public List<IngredientData> ingredients = new List<IngredientData>(); // Liste d'ingr�dients
    }

    public List<RecetteData> recettes = new List<RecetteData>(); // Liste de toutes les recettes
}

