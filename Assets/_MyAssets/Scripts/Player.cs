using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static Recette_Library;

public class Player : NetworkBehaviour
{
    //BY DEFAULT< EVERYTHING IS SERVER AUTORITATIVE
    [Header("Player Controls")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _speed = 7f;
    [SerializeField] private float _speedRunMultiplicator = 2f;
    [SerializeField] private float _lookSpeed = 3;

    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float gravityMultiplier = 2.5f;

    [Header("Items")]
    [SerializeField] private GameObject _villageItem;
    [SerializeField] private Tools_Library _axes;
    [SerializeField] private Tools_Library _pickaxes;
    [SerializeField] private HandHolding _handHolding;
    private GameObject axeItem;
    private GameObject pickaxeItem;

    [Header("Village")]
    [SerializeField] private GameObject _villagePrefab;
    [SerializeField] private GameObject _villagePlaceholderValidPrefab;
    [SerializeField] private GameObject _villagePlaceholderInvalidPrefab;

    [Header("UIPlayer")]
    [SerializeField] private GameObject _coordonees = default;
    [SerializeField] private TMP_Text _playerPositionText = default;
    //[SerializeField] private TMP_Text _playerVillagePositionText = default;
    [SerializeField] private Image _loadingScreenBackground = default;
    [SerializeField] private GameObject _loadingScreenText = default;
    [SerializeField] private GameObject _skills = default;
    [SerializeField] private GameObject _canvas = default;
    [SerializeField] private GameObject _popUpWindow = default;
    [SerializeField] private TMP_Text _popUpText = default;
    [SerializeField] private TMP_Text _nameTag = default;

    private const float POP_UP_X_ACTIVE = 625;
    private const float POP_UP_X_INACTIVE = 1400;

    [Header("Sounds")]
    [SerializeField] private SoundManager _soundManager = default;

    [Header("Libraries")]
    [SerializeField] private Recette_Library _recettes = default;

    private List<RessourceInfo> ressources;

    private class RessourceInfo
    {
        public int idRessource;
        public int quantite;

        public RessourceInfo(int idRessource, int quantite)
        {
            this.idRessource = idRessource;
            this.quantite = quantite;
        }
    }

    public GameObject VillageItem => _villageItem;
    public GameObject AxeItem => axeItem;
    public GameObject PickaxeItem => pickaxeItem;
    public GameObject VillagePrefab => _villagePrefab;
    public GameObject VillagePlaceholderValidPrefab => _villagePlaceholderValidPrefab;
    public GameObject VillagePlaceholderInvalidPrefab => _villagePlaceholderInvalidPrefab;

    private bool isGrounded;

    private bool lockCamera;

    Rigidbody rb;

    private float lastStepTime = 0f;
    private float stepCooldown = 0.3f;


    //Default network variable. If not explicitly told, only modifiable by server. 
    //(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.-> server or owner)
    private NetworkVariable<int> randomVariable = new NetworkVariable<int>();

    public int personnageId;
    public ulong clientId;

    private bool isTerrainGenerated = false;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        if (IsOwner)
        {
            _canvas.SetActive(true);
            ressources = new();
            StartCoroutine(InitializePlayer());
            return;
        }
        if(IsServer)        
        {    
            _loadingScreenText.SetActive(false);            
            playerCamera.gameObject.SetActive(false);
            return;
        }
        if(IsClient && !IsOwner)
        {
            _loadingScreenText.SetActive(false);   
            playerCamera.gameObject.SetActive(false);
            return;
            //AUTRES CLIENTS DOIVENT FETCH POUR AVOIR LE NAME TAG OU TROUVER UNE AUTRE SOLUTION
        }
    }

    private IEnumerator InitializePlayer()
    {        
        yield return new WaitForSeconds(0.5f);

        clientId = NetworkManager.LocalClientId;
        personnageId = PersonnageId_Reference.PersonnageId;
        Debug.LogError(personnageId);

        OpenWorld_QueryManager.Instance.SetPLayer(this);
        OpenWorld_QueryManager.Instance.GetLastPosition(personnageId);

        ServerSceneManager.Instance.OnClientConnectServerRpc(clientId);
        ServerSceneManager.Instance.SetPLayer(this);

        _handHolding.SetVariables(this, TerrainManager.Instance);

        RessourceManager.Instance.OnClientConnectServerRpc(clientId);

        GatheringManagerNetwork.Instance.SetPlayer(this);


        MainScene_UIManager.Instance.SetPlayer(this);
        Village_UIManager.Instance.DisableCanvas();
       

        OpenWorld_GameManager.Instance.SetPlayer(this);

        playerCamera.gameObject.SetActive(true);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        lockCamera = true;

        yield return new WaitForSeconds(3);
        StartCoroutine(FadeOutLoading());
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        //Debug.LogError(SceneManager.GetActiveScene().name);

        CheckControlsMovement();
        CheckControlsInteractable();
        PositionSystem();
    }

    public void SetNameTag(string _playerName)
    {
        _nameTag.text = _playerName;
    }

    public void SelectSceneMusic(bool isOnStart, string sceneName)
    {
        _soundManager.SelectSceneMusic(false, sceneName);
    }

    private void DisableLoadingScreen()
    {
        _coordonees.SetActive(true);
        _skills.SetActive(true);
        _loadingScreenBackground.gameObject.SetActive(false);
    }

    private void EnableLoadingScreen(string sceneName)
    {
        StartCoroutine(EnableLoadingScreenCoroutine(sceneName));
    }

    private IEnumerator EnableLoadingScreenCoroutine(string sceneName)
    {
        _coordonees.SetActive(false);
        _skills.SetActive(false);
        _loadingScreenText.SetActive(true);
        _loadingScreenBackground.gameObject.SetActive(true);
        _loadingScreenBackground.color = new(1f, 1f, 1f, 1f);

        yield return new WaitForSeconds(2);

        _loadingScreenText.SetActive(false);

        Color originalColor = _loadingScreenBackground.color;

        float fadeDuration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            _loadingScreenBackground.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        _loadingScreenBackground.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        if (sceneName == "OpenWorld")
        {
            DisableLoadingScreen();
        }
    }

    private IEnumerator FadeOutLoading()
    {
        _loadingScreenText.SetActive(false);
        Color originalColor = _loadingScreenBackground.color;

        float fadeDuration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            _loadingScreenBackground.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        _loadingScreenBackground.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        
        DisableLoadingScreen();
        
    }


    private void PositionSystem()
    {
        string x = transform.position.x.ToString("F2");
        string y = transform.position.y.ToString("F2");
        string z = transform.position.z.ToString("F2");
        _playerPositionText.text = "X: " + x + "; Y: " + y + "; Z: " + z + ";";
    }

    public Vector3 GetPosition()
    {
        return gameObject.transform.position;
    }

    public void SetLastPositionLogin(Vector3 _position)
    {
        if (!isTerrainGenerated)
        {
            isTerrainGenerated = true;
            TerrainManager.Instance.AssignPlayer(this.gameObject, _position);
        }
        else
        {
            gameObject.transform.position = _position;
        }            
        
    }     
    
    

    public void SetTools(int hache, int pioche)
    {
        axeItem = _axes.GetToolById(hache);
        pickaxeItem = _pickaxes.GetToolById(pioche);
    }

    private void SetTool(bool isAxe, int idOutil)
    {
        if (isAxe)
            axeItem = _axes.GetToolById(idOutil);
        else
            pickaxeItem= _pickaxes.GetToolById(idOutil);
    }

    public void SetInventory(int idRessource, int quantite, int maximum)
    {
        RessourceInfo thisInfo = new RessourceInfo(idRessource, quantite);
        ressources.Add(thisInfo);
        Debug.LogError("Ressources du joueur : " + thisInfo.idRessource + " : " + thisInfo.quantite);
        Inventaire_UIManager.Instance.AddToInventory(idRessource, quantite, maximum);
    }

    public void ModifyInventory(int idRessource, int quantite, int maximum)
    {
        RessourceInfo thisInfo = ressources.FirstOrDefault(ressource => ressource.idRessource == idRessource);
        if (thisInfo != null)
        {
            thisInfo.quantite = quantite;
        }
        else if(quantite <= 0)
        {
            ressources.Remove(thisInfo);
        }
        else
        {
            RessourceInfo newInfo = new RessourceInfo(idRessource, quantite);
            ressources.Add(newInfo);
        }
        Inventaire_UIManager.Instance.ModifToInventory(idRessource, quantite, maximum);
    }

    public void ResetInventory()
    {
        Inventaire_UIManager.Instance.EmptyUI();
        OpenWorld_QueryManager.Instance.FetchPlayerInventoryServerRpc(personnageId);
    }

    private Coroutine popUpCoroutine;

    public void CheckToCraftRecipe(int idRecette)
    {
        RecetteData thisRecette = _recettes.recettes.FirstOrDefault(recette => recette.idRecette == idRecette);

        bool isCraftable = true;
        string[] missingRessources = new string[thisRecette.ingredients.Count];
        RessourceInfo[] ressourcesForCraft = new RessourceInfo[thisRecette.ingredients.Count];
        int[] quantityForCraft = new int[thisRecette.ingredients.Count];
        int counter = 0;
        foreach(IngredientData ingredientBundle in thisRecette.ingredients)
        {
            RessourceInfo thisRessource = ressources.FirstOrDefault(ressource => ressource.idRessource == ingredientBundle.idRessource);

            if(thisRessource != null)
            {
                if(thisRessource.quantite >= ingredientBundle.quantite)
                {
                    ressourcesForCraft[counter] = thisRessource;
                    quantityForCraft[counter] = ingredientBundle.quantite;                    
                }
                else
                {                   
                    isCraftable = false;
                    missingRessources[counter] = ingredientBundle.nomRessource;
                }
            }
            else
            {
                isCraftable = false;
                missingRessources[counter] = ingredientBundle.nomRessource;
            }
            counter++;
        }

        if (isCraftable)
        {
            counter = 0;
            foreach (RessourceInfo toRemove in ressourcesForCraft)
            {
                toRemove.quantite -= quantityForCraft[counter];

                if(toRemove.quantite == 0)
                {
                    ressources.Remove(toRemove);
                }

                counter++;
            }

            //Village_QueryManager DANS UN ETAT QUI FAIT PITIER, ALORS JE VAIS CENTRALISER SUR OPENWORLD QUERYMANAGER
            OpenWorld_QueryManager.Instance.PlayerCraftRecipeServerRpc(personnageId, thisRecette.idRecette, clientId);
            Debug.LogError("Crafting " + thisRecette.nomRecette);

            if(popUpCoroutine != null) 
                StopCoroutine(popUpCoroutine);
            popUpCoroutine = StartCoroutine(PopUp("Fabrication de " + thisRecette.nomRecette + " réussis !", true));
                        
            if(thisRecette.idOutils != 0)
            {
                SetTool(thisRecette.isAxe, thisRecette.idOutils);
            }
            //return true;
        }
        else
        {
            Debug.LogError("Missing ingredients : " + missingRessources[0] + " " + missingRessources[1] + " " + missingRessources[2]);
            if (popUpCoroutine != null)
                StopCoroutine(popUpCoroutine);
            popUpCoroutine = StartCoroutine(PopUp("Missing ingredients : " + missingRessources[0] + " " + missingRessources[1] + " " + missingRessources[2], false));
            //return false;
        }
    }

    private IEnumerator PopUp(string message, bool isSuccess)
    {        
        _popUpWindow.transform.localPosition = new Vector3(POP_UP_X_INACTIVE, 465, 0);
        _popUpText.text = message;
        if(isSuccess)
            _popUpText.color = Color.green;        
        else
            _popUpText.color = Color.red;

        float slideInDuration = 1.5f;
        float elapsedTime = 0f;
        while (elapsedTime < slideInDuration)
        {
            elapsedTime += Time.deltaTime;
            float positionX = Mathf.Lerp(POP_UP_X_INACTIVE, POP_UP_X_ACTIVE, elapsedTime / slideInDuration);
            _popUpWindow.transform.localPosition = new Vector3(positionX, 465, 0);
            yield return null;
        }
        _popUpWindow.transform.localPosition = new Vector3(POP_UP_X_ACTIVE,465, 0);

        yield return new WaitForSeconds(5);

        float slideOutDuration = 1.5f;
        elapsedTime = 0f;
        while (elapsedTime < slideOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float positionX = Mathf.Lerp(POP_UP_X_ACTIVE, POP_UP_X_INACTIVE, elapsedTime / slideOutDuration);
            _popUpWindow.transform.localPosition = new Vector3(positionX, 465, 0);
            yield return null;
        }
        _popUpWindow.transform.localPosition = new Vector3(POP_UP_X_INACTIVE, 465, 0);
    }

    public void RestartTerrainGeneration(Vector3 position)
    {        
        RestartTerrainGeneration();
    }

    public void LockCamera()
    {
        lockCamera = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void UnlockCamera()
    {
        lockCamera = false;
    }    
    
    private void CheckControlsMovement()
    {
        float _xMov = Input.GetAxisRaw("Horizontal");
        float _zMov = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _xMov;
        Vector3 _moveVertical = transform.forward * _zMov;

        bool isRunning = Input.GetButton("Run");
        float finalSpeed = isRunning ? _speed * _speedRunMultiplicator : _speed;
        //Final mouvement vector
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * finalSpeed;

        float movementSpeed = _velocity.magnitude;
        _animator.SetFloat("Speed", movementSpeed);

        bool isMoving = movementSpeed > 0.1f;
        bool isMovingBack = Input.GetKey(KeyCode.S);
        bool isMovingLeft = Input.GetKey(KeyCode.A);
        bool isMovingRight = Input.GetKey(KeyCode.D);
        bool isMovingFront = Input.GetKey(KeyCode.W);
        bool isJumping = Input.GetKey(KeyCode.Space);

        _animator.SetBool("isMovingBack", isMovingBack);
        _animator.SetBool("isMovingLeft", isMovingLeft);
        _animator.SetBool("isMovingRight", isMovingRight);
        _animator.SetBool("isMovingFront", isMovingFront);
        _animator.SetBool("isJumping", isJumping);

        if (isMoving && isGrounded && Time.time - lastStepTime > stepCooldown)
        {
            lastStepTime = Time.time; // Enregistre le temps du dernier pas

            if (isRunning)
            {
                _soundManager.PlaySFX(SoundManager.SoundCategory.Player, 4, 0.5f);
            }
            else
            {
                _soundManager.PlaySFX(SoundManager.SoundCategory.Player, 3, 0.3f);
            }
        }

        if (_velocity != Vector3.zero && lockCamera)
        {
            
            //transform.position = (transform.position + _velocity * Time.fixedDeltaTime);
            rb.MovePosition(transform.position + _velocity * Time.fixedDeltaTime);
            UpdateServerPositionServerRpc(transform.position);

        }
        
        //Rotates player on Y axis, then rotates CAMERA on x Axis :)
        float _yRot = Input.GetAxisRaw("Mouse X");

        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * _lookSpeed;

        if(_rotation != Vector3.zero && lockCamera)
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(_rotation));
            UpdateServerRotationServerRpc(Quaternion.Euler(_rotation));
        }

        float _xRot = Input.GetAxisRaw("Mouse Y");
        Vector3 _rotationCam = new Vector3(_xRot, 0f, 0f) * _lookSpeed;

        Vector3 currentEuler = playerCamera.transform.rotation.eulerAngles;

        float currentX = (currentEuler.x > 180) ? currentEuler.x - 360 : currentEuler.x;

        float newXRotation = Mathf.Clamp(currentX - _rotationCam.x, -90f, 90f);
        
        if( _rotationCam != Vector3.zero && lockCamera)
        {
            playerCamera.transform.rotation = Quaternion.Euler(newXRotation, currentEuler.y, currentEuler.z);
            UpdateServerHeadRotationServerRpc(Quaternion.Euler(newXRotation, currentEuler.y, currentEuler.z));
        }
                

        // Jumping
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, 0.2f, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, _jumpForce, rb.velocity.z);
            _soundManager.PlaySFX(SoundManager.SoundCategory.Player, 1, 0.5f);
        }

        if (rb.velocity.y < 0)
        {
            rb.velocity += (gravityMultiplier - 1) * Physics.gravity.y * Time.deltaTime * Vector3.up;
        }

    }

    private void CheckControlsInteractable()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            float interactRange = 3f;
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);

            foreach (Collider collider in colliderArray)
            {
                if (collider.TryGetComponent(out NPC_UIManager npcInteractable))
                {
                    npcInteractable.Interact(this);
                }
            }
        }

        // Permet d'activer ou d�sactiver le curseur et la cam�ra
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            lockCamera = false;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            lockCamera = true;
        }
    }

    [ServerRpc]
    private void UpdateServerPositionServerRpc(Vector3 position)
    {
        // Update the player's position on the server
        transform.position = position;

        //MovePlayer(transform.position + _velocity * Time.fixedDeltaTime);
        UpdateClientPositionClientRpc(transform.position);
    }
    
    [ServerRpc]
    private void UpdateServerRotationServerRpc(Quaternion _rotation, ServerRpcParams rpcParams = default)
    {
        // Update the player's position on the server
        transform.rotation = _rotation;
        //RotatePlayer(rb.rotation * Quaternion.Euler(_rotation));
    }

    [ServerRpc]
    private void UpdateServerHeadRotationServerRpc(Quaternion _rotation)
    {
        playerCamera.transform.rotation = _rotation;
    }

    // Server-side function to move the player and sync with clients
    private void MovePlayer(Vector3 newPosition)
    {
        rb.MovePosition(newPosition);
        //transform.position = newPosition;
        //UpdatePositionClientRpc(newPosition);
    }

    
    private void RotatePlayer(Quaternion rotation)
    {
        //rb.MoveRotation(rotation);
        UpdateRotationClientRpc(rotation);
    }

    
    public void MovePlayerInWorldSpace(Vector3 newPosition, string sceneName)
    {
        transform.position = newPosition;

        if (sceneName == "Village_Network")
        {
            MainScene_UIManager.Instance.DisableCanvas();
            Village_UIManager.Instance.EnableCanvas();            
            return;
        }else if (sceneName == "OpenWorld")
        {
            MainScene_UIManager.Instance.EnableCanvas();
            Village_UIManager.Instance.DisableCanvas();
            return;
        }
        else
        {
            MainScene_UIManager.Instance.DisableCanvas();
            Village_UIManager.Instance.DisableCanvas();            
            return ;
        }

    }

    
    public Vector3 RequestQueryLastPosition()
    {
        //CALL QUERY MANAGER POUR LES 3 POSITIONS DU JOUEUR 
        return new Vector3 (0, 0, 0);
    }

    /*
    [ClientRpc]
    public Vector3 SaveLastPositionClientRpc()
    {
        //CALL QUERY MANAGER POUR LES 3 POSITIONS DU JOUEUR 
    }*/

    public void RequestMoveScene(string sceneName)
    {
        EnableLoadingScreen(sceneName);
        ServerSceneManager.Instance.MoveClientToSceneServerRpc(clientId, sceneName);
    }

    public void RequestMoveClientFromPrefabToScene(ulong prefabId)
    {
        ServerSceneManager.Instance.MoveClientFromPrefabToSceneServerRpc(clientId, prefabId);
    }

    public void RequestMoveClientFromSceneToPrefab(ulong sceneId)
    {
        ServerSceneManager.Instance.MoveClientFromSceneToPrefabServerRpc(clientId, sceneId);
    }

    // Synchronize position with all clients
    [ClientRpc]
    private void UpdateClientPositionClientRpc(Vector3 newPosition)
    {        
        if (!IsOwner)
        {
            // Update position on all clients
            transform.position = Vector3.Lerp(transform.position, newPosition, 0.1f);
        }        
    }

    [ClientRpc]
    private void UpdateRotationClientRpc(Quaternion newRotation)
    {
        if (!IsOwner) 
        {
            rb.MoveRotation(newRotation);
        }       
    }

    public NPC_UIManager GetInteractableObject()
    {
        List<NPC_UIManager> NPCInteractableList = new List<NPC_UIManager>();
        float interactRange = 3f;
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);

        foreach (Collider collider in colliderArray)
        {
            if (collider.TryGetComponent(out NPC_UIManager npcInteractable))
            {
                NPCInteractableList.Add(npcInteractable);

            }
        }

        NPC_UIManager closestNPCInteractable = null;
        foreach (NPC_UIManager npcInteractable in NPCInteractableList)
        {
            if (closestNPCInteractable == null)
            {
                closestNPCInteractable = npcInteractable;
            }
            else
            {
                if (Vector3.Distance(transform.position, npcInteractable.transform.position) < (Vector3.Distance(transform.position, closestNPCInteractable.transform.position)))
                {
                    // Closer
                    closestNPCInteractable = npcInteractable as NPC_UIManager;
                }
            }
        }

        return closestNPCInteractable;
    }

    
    public void StopTerrainGeneration()
    {        
        Debug.LogError("Stopping terrain generation");

        TerrainManager.Instance.isHalted = true;
        OpenWorld_QueryManager.Instance.SetLastPosition(GetPosition(), personnageId);
    }

    public void RestartTerrainGeneration()
    {
        TerrainManager.Instance.isHalted = false;
    }

    

}
