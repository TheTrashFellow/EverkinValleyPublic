using Unity.Netcode;
using UnityEngine;

public class ResourcesProperties : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject _fxDestroyed;
    [SerializeField] private GameObject _fxHit;
    [SerializeField] private GameObject _fxHitBlocks;
    [SerializeField] private int _idRessource;    
    [SerializeField] private int _healthAmount = 20;

    public GameObject FxHit => _fxHit;
    public GameObject FxHitBlocks => _fxHitBlocks;

    private HealthSystem healthSystem;

    private void Awake()
    {
        healthSystem = new(_healthAmount);
        healthSystem.OnDead += HealthSystem_OnDead;
    }

    private void HealthSystem_OnDead(object sender, System.EventArgs e)
    {
        Instantiate(_fxDestroyed, transform.position, transform.rotation);
        ulong objectId = gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        int idPersonnage = GatheringManagerNetwork.Instance.Player.personnageId;
        ulong idClient = GatheringManagerNetwork.Instance.Player.clientId;
        int[] range =  GatheringManagerNetwork.Instance.HandHolding.ItemHand.GetComponent<ToolsProperties>().RangeAmount;
        int quantity = Random.Range(range[0], range[1]);

        RessourceManager.Instance.RequestDespawnRessourcesServerRpc(objectId, _idRessource, quantity,  idClient, idPersonnage);
    }

    public void Damage(int amount)
    {
        healthSystem.Damage(amount);
    }
}
