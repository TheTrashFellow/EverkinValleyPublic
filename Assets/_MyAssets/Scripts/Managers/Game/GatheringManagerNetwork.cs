using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GatheringManagerNetwork : MonoBehaviour
{
    public static GatheringManagerNetwork Instance { get; private set; }

    [Header("Properties")]
    [SerializeField] private float _criticAttack = 0.8f;
    [SerializeField] private float _maxCollectingDistance = 3f;

    private Animator _animator;

    private HandHolding handHolding;
    public HandHolding HandHolding=> handHolding;

    private bool isChopping = false;
    private Player _player;
    public Player Player => _player;

    private RaycastHit ressource;

    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName != "ChopMiningSystem" && sceneName != "OpenWorld" && sceneName != "TestingRessources")
        {
            Debug.Log("Désactivation de GatheringManager dans cette scène.");
            gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        HandleAttack();
    }

    public void SetPlayer(Player player)
    {
        Debug.Log("PLAYER SET IN GATHERING MANAGER");
        _player = player;

        _animator = _player.gameObject.GetComponent<Animator>();
        handHolding = _player.GetComponentInChildren<HandHolding>();
    }

    public void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && (handHolding.IsAxe || handHolding.IsPickaxe))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, _maxCollectingDistance))
            {
                if ((hit.collider.CompareTag("Tree") && handHolding.IsAxe) || (hit.collider.CompareTag("Rock") && handHolding.IsPickaxe))
                {
                    ressource = hit;
                    CollectResources();
                }
            }
        }
    }

    public void CollectResources()
    {
        if (!isChopping)
        {
            _animator.SetTrigger("Chop");
            FunctionTimer.Create(AnimationEvent, 0.75f);
            StartCoroutine(ChopCooldown());
        }
    }

    private void AnimationEvent()
    {
        if (ressource.collider.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            // Damage popup
            int min = handHolding.ItemHand.GetComponent<ToolsProperties>().Damages.x;
            int max = handHolding.ItemHand.GetComponent<ToolsProperties>().Damages.y;
            int damageAmount = Random.Range(min, max);
            DamagePopup.Create(ressource.point, damageAmount, damageAmount >= Mathf.Floor(min + ((max - min) * _criticAttack)));

            // Damage tree
            damageable.Damage(damageAmount);

            GameObject fxHit = ressource.collider.gameObject.GetComponent<ResourcesProperties>().FxHit;
            GameObject fxHitBlocks = ressource.collider.gameObject.GetComponent<ResourcesProperties>().FxHitBlocks;
            // Spawn FX
            Instantiate(fxHit, ressource.point, Quaternion.identity);
            Instantiate(fxHitBlocks, ressource.point, Quaternion.identity);
        }
    }

    private IEnumerator ChopCooldown()
    {
        isChopping = true;
        yield return new WaitForSeconds(0.75f);
        isChopping = false;
    }
}

// Partie de script pour gérer le gathering dans les scènes
//private void Start()
//{
//    SceneManager.sceneLoaded += OnSceneLoaded;
//    CheckScene(); // Vérifie la scène au démarrage
//}

//private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//{
//    CheckScene(); // Vérifie la scène après chaque chargement
//}

//private void CheckScene()
//{
//    string sceneName = SceneManager.GetActiveScene().name;

//    if (sceneName != "Forêt" && sceneName != "Mine" && sceneName != "OpenWorld")
//    {
//        Debug.Log("Désactivation de GatheringManager car cette scène ne contient pas de ressources.");
//        gameObject.SetActive(false);
//    }
//    else
//    {
//        Debug.Log("GatheringManager activé !");
//        gameObject.SetActive(true);
//    }
//}

//private void OnDestroy()
//{
//    SceneManager.sceneLoaded -= OnSceneLoaded;
//}