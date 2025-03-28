using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Ressource_id_Library;

public class Inventaire_Coffre_UIManager : MonoBehaviour
{
    public static Inventaire_Coffre_UIManager Instance { get; private set; }

    [Header("Pour construire inventaire")]
    [SerializeField] private GameObject _defaultEmpty = default;
    [SerializeField] private GameObject _ligne = default;
    [SerializeField] private GameObject _conteneur = default;
    [SerializeField] private Ressource_id_Library _ressource_Id_Library = default;

    public List<GameObject> listeContenue;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        listeContenue = new List<GameObject>();
        listeContenue.Add(_defaultEmpty);
    }

    // Update is called once per frame
    void Update()
    {
        if (listeContenue.Count > 1)
        {
            listeContenue[0].gameObject.SetActive(false);
        }
        else
        {
            listeContenue[0].gameObject.SetActive(true);
        }
    }

    public void AddToInventory(int _idRessource, int _quantite, int _maximum)
    {
        ResourcesSprite thisRessource = _ressource_Id_Library.GetResourceById(_idRessource);
        GameObject newLine = Instantiate(_ligne);

        newLine.GetComponent<LigneInventaire>().idRessource = _idRessource;
        newLine.GetComponent<LigneInventaire>().name.text = thisRessource.NomRessource;
        newLine.GetComponent<LigneInventaire>().image.sprite = thisRessource._resourcePrefabs;
        newLine.GetComponent<LigneInventaire>().quantity.text = _quantite + " / " + _maximum;

        newLine.transform.parent = _conteneur.transform;

        listeContenue.Add(newLine);
    }

    public void ModifToInventory(int _idRessource)
    {
        GameObject thisItem = listeContenue.FirstOrDefault(item => item.GetComponent<LigneInventaire>().idRessource == _idRessource);

        listeContenue.Remove(thisItem);
        Destroy(thisItem);

    }
}
