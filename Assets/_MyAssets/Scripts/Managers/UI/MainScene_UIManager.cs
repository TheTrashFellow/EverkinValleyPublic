using UnityEngine;


public class MainScene_UIManager : MonoBehaviour
{
    public static MainScene_UIManager Instance { get; private set; }

    [SerializeField] private GameObject _menuPrincipal = default;
    [SerializeField] private GameObject _inventaire = default;
    [SerializeField] private GameObject _options = default;
    [SerializeField] private GameObject _commandes = default;

    private bool _invOuvert = false;
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

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    private void Update()
    {
        // Touche 4 ou E
        if (!_menuOuvert && (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.E)))
        {
            ToggleInventaire();
        }

        // Touche 5 ou Escape
        if (!_invOuvert && (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Escape)))
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
            _player.UnlockCamera();
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            _player.LockCamera();
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
            _options.SetActive(false);
            _commandes.SetActive(false);
            _player.LockCamera();
        }
        
        
    }

    // Permet de fermer le menu avec le bouton 
    public void FermerMenu()
    {
        _menuPrincipal.SetActive(false);
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

    public void DisableCanvas()
    {
       this.gameObject.SetActive(false);
    }

    public void EnableCanvas()
    {
        this.gameObject.SetActive(true);
    }
}
