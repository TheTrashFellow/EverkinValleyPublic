using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDev : MonoBehaviour
{
    //BY DEFAULT< EVERYTHING IS SERVER AUTORITATIVE
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _speed = 5;
    [SerializeField] private float _lookSpeed = 3;

    Rigidbody rb;

    //Default network variable. If not explicitly told, only modifiable by server. 
    //(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.-> server or owner)
    private NetworkVariable<int> randomVariable = new NetworkVariable<int>();

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Masquer le curseur et le verrouiller en début de partie
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

/*
public override void OnNetworkSpawn()
{
    rb = GetComponent<Rigidbody>();
    if (IsOwner)
    {
        playerCamera.gameObject.SetActive(true);
        //GameObject.Find("Main Camera").SetActive(false);
        //GameObject.Find("Canvas").SetActive(false);
        TerrainManager terrainManager = FindAnyObjectByType<TerrainManager>();
        terrainManager.AssignPlayer(this.gameObject);
    }
    else if (IsHost)
    {
        playerCamera.gameObject.SetActive(true);
        GameObject.Find("Main Camera").SetActive(false);            
    }
    else
    {
        playerCamera.gameObject.SetActive(false);
    }
}*/

// Update is called once per frame
void Update()
    {
        //if (!IsOwner) return;

        CheckControls();

        // Ajout pour les interactions
        if (Input.GetKeyDown(KeyCode.R))
        {
            float interactRange = 3f;
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);

            foreach (Collider collider in colliderArray)
            {
                if (collider.TryGetComponent(out NPC_UIManager npcInteractable))
                {
                    //FOR TESTING. CAN REMOVE THIS LATER ??? npcInteractable.Interact(this);
                }
            }
        }

        // À Réactiver pour le gathering 
        // Fais buguer pour le moment vu qu'il n'y a pas de changement de scène
        //GatheringManager.Instance.HandleAttack();
    }

    private void CheckControls()
    {
        // Si le menu est ouvert, le curseur n'est pas verouillé et on ne traite pas la rotation
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        // Déplacement
        float _xMov = Input.GetAxisRaw("Horizontal");
        float _zMov = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _xMov; 
        Vector3 _moveVertical = transform.forward * _zMov;

        //Final mouvement vector
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * _speed;

        // Animations
        // Avancer
        float movementSpeed = _velocity.magnitude;
        _animator.SetFloat("Speed", movementSpeed);
        
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

        if (_velocity != Vector3.zero)
        {

            //transform.position = (transform.position + _velocity * Time.fixedDeltaTime);
            rb.MovePosition(transform.position + _velocity * Time.fixedDeltaTime);
           // UpdateServerPositionServerRpc(transform.position);

        }
        
        //Rotates player on Y axis, then rotates CAMERA on x Axis :)
        float _yRot = Input.GetAxisRaw("Mouse X");

        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * _lookSpeed;

        if(_rotation != Vector3.zero)
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(_rotation));
            //UpdateServerRotationServerRpc(Quaternion.Euler(_rotation));
        }

        float _xRot = Input.GetAxisRaw("Mouse Y");
        Vector3 _rotationCam = new Vector3(_xRot, 0f, 0f) * _lookSpeed;

        Vector3 currentEuler = playerCamera.transform.rotation.eulerAngles;

        float currentX = (currentEuler.x > 180) ? currentEuler.x - 360 : currentEuler.x;

        float newXRotation = Mathf.Clamp(currentX - _rotationCam.x, -90f, 90f);
        
        if( _rotationCam != Vector3.zero)
        {
            playerCamera.transform.rotation = Quaternion.Euler(newXRotation, currentEuler.y, currentEuler.z);
            //UpdateServerHeadRotationServerRpc(Quaternion.Euler(newXRotation, currentEuler.y, currentEuler.z));
        }
        
    }

    //[ServerRpc]
    private void UpdateServerPositionServerRpc(Vector3 position)
    {
        // Update the player's position on the server
        transform.position = position;

        //MovePlayer(transform.position + _velocity * Time.fixedDeltaTime);
        //UpdateClientPositionClientRpc(transform.position);
    }
    
    //[ServerRpc]
    private void UpdateServerRotationServerRpc(Quaternion _rotation, ServerRpcParams rpcParams = default)
    {
        // Update the player's position on the server
        transform.rotation = _rotation;
        //RotatePlayer(rb.rotation * Quaternion.Euler(_rotation));
    }

    //[ServerRpc]
    private void UpdateServerHeadRotationServerRpc(Quaternion _rotation)
    {
        playerCamera.transform.rotation = _rotation;
    }

    // Server-side function to move the player and sync with clients
    /*private void MovePlayer(Vector3 newPosition)
    {
        rb.MovePosition(newPosition);
        //transform.position = newPosition;
        //UpdatePositionClientRpc(newPosition);
    }*/


    /* private void RotatePlayer(Quaternion rotation)
     {
         //rb.MoveRotation(rotation);
         UpdateRotationClientRpc(rotation);
     }*/


    // Synchronize position with all clients
    /*[ClientRpc]
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
        rb.MoveRotation(newRotation);
    }*/
    
    #region
    // Affichage de la bulle d'interaction NPC
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
            if(closestNPCInteractable == null)
            {
                closestNPCInteractable = npcInteractable;
            }
            else
            {
                if(Vector3.Distance(transform.position, npcInteractable.transform.position) < (Vector3.Distance(transform.position, closestNPCInteractable.transform.position))) 
                {
                    // Closer
                    closestNPCInteractable = npcInteractable as NPC_UIManager;
                }
            }
        }

        return closestNPCInteractable;
    }
    #endregion

}
