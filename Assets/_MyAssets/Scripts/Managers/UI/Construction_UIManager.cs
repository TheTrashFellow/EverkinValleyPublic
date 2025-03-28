using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Construction_UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _menuPrincipal = default;
    [SerializeField] private GameObject _inventaire = default;
    [SerializeField] private GameObject _options = default;
    [SerializeField] private GameObject _commandes = default;

    private bool _invOuvert = false;
    private bool _menuOuvert = false;

    // Quitter
    public void Quitter()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    private void Update()
    {
        // Touche 3 ou E
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventaire();
        }

        // Touche 4 ou Escape
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    // Permet d'ouvrir et de fermer l'inventaire avec les touches
    public void ToggleInventaire()
    {
        _invOuvert = !_invOuvert;
        _inventaire.SetActive(_invOuvert);

        if (_inventaire.activeSelf)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Permet de fermer l'inventaire avec le bouton
    public void FermerInventaire()
    {
        _inventaire.SetActive(false);

        if (_inventaire.activeSelf)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Permet d'ouvrir et de fermer le menu avec les touches
    public void ToggleMenu()
    {
        _menuOuvert = !_menuOuvert;
        _menuPrincipal.SetActive(_menuOuvert);

        if (_menuPrincipal.activeSelf)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Permet de fermer le menu avec le bouton 
    public void FermerMenu()
    {
        _menuPrincipal.SetActive(false);


        if (_menuPrincipal.activeSelf)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Permet d'afficher les options avec le bouton dans le menu
    public void AfficherOptions()
    {
        _options.SetActive(true);
        _menuPrincipal.SetActive(false);
    }

    // Permet de fermer les options avec le bouton
    public void FermerOptions()
    {
        _options.SetActive(false);
        _menuPrincipal.SetActive(true);
    }

    // Permet d'afficher les commandes avec le bouton dans le menu
    public void AfficherCommandes()
    {
        _commandes.SetActive(true);
        _menuPrincipal.SetActive(false);
    }

    // Permet de fermer les commandes avec le bouton
    public void FermerCommandes()
    {
        _commandes.SetActive(false);
        _menuPrincipal.SetActive(true);
    }
}
