using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NPC_UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _menu = default;
    [SerializeField] private string interactText;

    [SerializeField] private Recettes_QueryManager _queryManager;
    

    private Player player;
    public bool _isMenuOpen => _menu.activeSelf;
    private Animator animator;

    [Header("Son du NPC")]
    public bool isMale;

    //[System.Serializable]
    //public class CraftingItem
    //{
    //    // Nom de l'item
    //    public string nom;
    //    // Panel des ingrédients
    //    public GameObject ingredients; 
    //}    

    private void Start()
    {
        animator = GetComponent<Animator>();
        player = FindAnyObjectByType<Player>();
    }

    // Liste des objets craftables
    //public CraftingItem[] craftingItems;
    // Référence du pannel affiché
    private GameObject currentPanel = null;

    //public void ToolPanel (int index)
    //{
    //    if (currentPanel == craftingItems[index].ingredients)
    //    {
    //        // Referme le panel si on reclique dessus
    //        currentPanel.SetActive(false);
    //        currentPanel = null;
    //    }
    //    else
    //    {
    //        // Ferme le panel actuel s'il y en a un déjà ouvert
    //        if(currentPanel != null)
    //        {
    //            currentPanel.SetActive(false);
    //        }
    //    }

    //    // Active le nouveau panel et stock comme actuel
    //    currentPanel = craftingItems[index].ingredients;
    //    currentPanel.SetActive(true);
    //}  

    // Permet de fermer le menu outils
    public void FermerMenu()
    {
        _menu.SetActive(false);        
        player.LockCamera();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        /*
        if (_menu.activeSelf)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }*/
    }

    // Ouvrir le menu avec le NPC
    public void Interact(Player _player)
    {
        player = _player;

        if(this.gameObject.tag == "NPC")
        {
            _queryManager.SetPlayer(_player);
            _menu.SetActive(!_menu.activeSelf);
            if (_menu.activeSelf)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                _player.UnlockCamera();
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                _player.LockCamera();
            }
            return;
        }
        
      
        
        if(gameObject.tag == "Sortie")
        {
            Debug.Log("Going back to scene : OpenWorld");
            Village_UIManager.Instance.DisableCanvas();
            MainScene_UIManager.Instance.EnableCanvas();
            _player.RequestMoveScene("OpenWorld");
            return;
        }

        if(gameObject.tag == "Entree_Village")
        {
            Debug.LogError("Entering scene : Village_network");
            Village_UIManager.Instance.EnableCanvas();
            MainScene_UIManager.Instance.DisableCanvas();
            _player.RequestMoveScene("Village_Network");            
            return;
        }

        if(gameObject.tag == "Entree_Village_Player")
        {
            ulong prefabId = gameObject.GetComponentInParent<NetworkObject>().NetworkObjectId;
            //Debug.LogError("Id Personnage du village : " + idPersonnage);
            //_player.RequestMoveScenePrefab(idPersonnage);
            Village_UIManager.Instance.EnableCanvas();
            MainScene_UIManager.Instance.DisableCanvas();
            _player.RequestMoveClientFromPrefabToScene(prefabId);
            return;
        }

        if(gameObject.tag == "Sortie_Village_Player")
        {
            ulong sceneId = gameObject.GetComponentInParent<NetworkObject>().NetworkObjectId;
            //int prefabIdPersonnage = gameObject.GetComponent<VillagePlayer_InfoTag>().prefabIdPersonnage;
            //_player.RequestMoveScenePrefab(prefabIdPersonnage);
            Village_UIManager.Instance.DisableCanvas();
            MainScene_UIManager.Instance.EnableCanvas();
            _player.RequestMoveClientFromSceneToPrefab(sceneId);
            return;
        }

        if(gameObject.tag == "Chest")
        {
            _menu.SetActive(!_menu.activeSelf);
            if (_menu.activeSelf)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                _player.UnlockCamera();
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                _player.LockCamera();
            }
            return;
        }
        
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
