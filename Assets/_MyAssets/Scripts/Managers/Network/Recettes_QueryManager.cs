using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class Recettes_QueryManager : MonoBehaviour
{    

    [Header("Références UI")]
    [SerializeField] private Transform recipeListContent; // Conteneur des recettes
    [SerializeField] private GameObject recipeButtonPrefab; // Préfab du bouton de recette
    [SerializeField] private Transform ingredientListContent; // Conteneur des ingrédients
    [SerializeField] private GameObject ingredientItemPrefab; // Préfab d'un ingrédient    

    private int craftRecipeId;

    [Header("Données des recettes")]
    [SerializeField] private Recette_Library recettes; // ScriptableObject contenant les recettes
    [SerializeField] private bool isBlacksmithUI; // True = Forgeron, False = Charpentier

    private List<Recette_Library.RecetteData> recettesDisponibles = new List<Recette_Library.RecetteData>();

    private Player _player;

    private void Start()
    {
        Debug.Log("Chargement des recettes depuis le ScriptableObject...");
        LoadRecipes();
    }

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    private void LoadRecipes()
    {
        // Filtrer les recettes selon le type de NPC
        recettesDisponibles = isBlacksmithUI
            ? recettes.recettes.FindAll(r => r.idBatiment == 0) // Forgeron (idBatiment null)
            : recettes.recettes.FindAll(r => r.idBatiment != 0); // Charpentier (idBatiment non-null)

        Debug.Log("Recettes filtrées (" + (isBlacksmithUI ? "Forgeron" : "Charpentier") + ") : " + recettesDisponibles.Count);
        PopulateRecipeList();
    }

    private void PopulateRecipeList()
    {
        Debug.Log("Début de PopulateRecipeList() - Nombre de recettes : " + recettesDisponibles.Count);

        // Supprime les anciens boutons
        foreach (Transform child in recipeListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var recette in recettesDisponibles)
        {
            GameObject newButton = Instantiate(recipeButtonPrefab, recipeListContent);
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                buttonText.text = recette.nomRecette;
                Debug.Log("Texte du bouton défini : " + buttonText.text);
            }
            else
            {
                Debug.LogError("TextMeshProUGUI introuvable dans le prefab du bouton !");
            }

            Button buttonComponent = newButton.GetComponentInChildren<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() => OnRecipeButtonClicked(recette));
                Debug.Log("Bouton créé pour la recette : " + recette.nomRecette);
            }
            else
            {
                Debug.LogError("Le prefab du bouton ne contient pas de Button !");
            }
        }
    }

    private void OnRecipeButtonClicked(Recette_Library.RecetteData recette)
    {
        Debug.Log("Bouton cliqué pour la recette : " + recette.nomRecette);
        craftRecipeId = recette.idRecette;
        PopulateIngredientList(recette.ingredients);
    }

    private void PopulateIngredientList(List<Recette_Library.IngredientData> ingredients)
    {
        Debug.Log("Début de PopulateIngredientList() - Nombre d'ingrédients : " + ingredients.Count);

        // Nettoyer les anciens ingrédients
        foreach (Transform child in ingredientListContent)
        {
            Destroy(child.gameObject);
        }

        // Ajouter les nouveaux ingrédients
        foreach (var ingredient in ingredients)
        {
            GameObject newIngredient = Instantiate(ingredientItemPrefab, ingredientListContent);

            TextMeshProUGUI nameText = newIngredient.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI quantityText = newIngredient.transform.Find("Qte")?.GetComponent<TextMeshProUGUI>();
            Image spriteImage = newIngredient.transform.Find("Icon")?.GetComponent<Image>();

            if (nameText != null)
                nameText.text = ingredient.nomRessource;
            else
                Debug.LogError("'Name' introuvable dans 'ingredientItemPrefab' !");

            if (quantityText != null)
                quantityText.text = "x" + ingredient.quantite;
            else
                Debug.LogError("'Qte' introuvable dans 'ingredientItemPrefab' !");

            if (spriteImage != null && ingredient.iconRessource != null)
                spriteImage.sprite = ingredient.iconRessource;
        }


    }

    
    public void RequestCraftRecipe()
    {
        Debug.LogError("Attempting craft : " + craftRecipeId);

        _player.CheckToCraftRecipe(craftRecipeId);

        //Village_QueryManager.Instance.RequestCraftRecipeServerRpc(_player.personnageId, craftRecipeId);
    }
}
