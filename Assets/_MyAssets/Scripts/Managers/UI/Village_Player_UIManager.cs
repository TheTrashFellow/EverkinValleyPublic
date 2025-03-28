using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Village_Player_UIManager : MonoBehaviour
{
    public static Village_Player_UIManager Instance { get; private set; }

    [SerializeField] private GameObject _menuPrincipal = default;
    [SerializeField] private GameObject _options = default;
    [SerializeField] private GameObject _commandes = default;
    [SerializeField] private Canvas _canvas = default;

    private bool _menuOuvert = false;

    private Player _player;

    // Quitter
    public void Quitter()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

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

    private void Start()
    {
        _player = FindAnyObjectByType<Player>();
    }

    private void Update()
    {
        // Touche 1 ou Escape
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Escape))
        {            
            ToggleMenu();
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
            _player.UnlockCamera();
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            _player.LockCamera();
        }
    }

    // Permet de fermer le menu avec le bouton 
    /*public void FermerMenu()
    {
        _menuOuvert = !_menuOuvert;
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
    }*/

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
    public void DisableCanvas()
    {
        _canvas.gameObject.SetActive(false);
    }

    public void EnableCanvas()
    {
        _canvas.gameObject.SetActive(true);
    }
}
