using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInteractUI : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private Player player;
    [SerializeField] private TextMeshProUGUI interactText;

    private void Update()
    {
        NPC_UIManager interactableObject = player.GetInteractableObject();


        if (player.GetInteractableObject() != null /*&& !interactableObject._isMenuOpen*/)
        {
            Show(player.GetInteractableObject());            
        }
        else
        {
            Hide();            
        }
    }

    private void Show(NPC_UIManager npcInteractable)
    {
        container.SetActive(true);
        interactText.text = npcInteractable.GetInteractText();
    }

    private void Hide()
    {
        container.SetActive(false);
    }
}
