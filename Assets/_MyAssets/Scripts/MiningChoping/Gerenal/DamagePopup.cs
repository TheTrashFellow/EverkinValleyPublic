using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private float disappearTimer;
    private Color textColor;
    private const float DISAPPEAR_TIMER_MAX = 1f;
    private Vector3 moveVector;
    private static int sortingOrder;
    [SerializeField] private TextMeshPro _textMeshPro;   

    // create a damage popup
    public static DamagePopup Create(Vector3 worldPosition, int damageAmount, bool isCrit)
    {
        // Récupérer la caméra du joueur
        Camera playerCamera = Camera.main;
        if (playerCamera == null)
        {
            return null;
        }

        // Positionner le DamagePopup DEVANT la caméra du joueur
        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * 3.0f;

        Transform damagePopupTransform = Instantiate(GameAssets.i.pfDamagePopup, spawnPosition, Quaternion.identity);
        DamagePopup damagePopup = damagePopupTransform.GetComponent<DamagePopup>();
        damagePopup.Setup(damageAmount, isCrit);

        // Faire en sorte que le popup soit TOUJOURS face à la caméra
        damagePopup.transform.LookAt(playerCamera.transform);
        damagePopup.transform.Rotate(0, 180, 0); // Corriger le sens du texte

        return damagePopup;
    }

    private void Awake()
    {
        _textMeshPro = transform.GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount, bool isCrit)
    {
        _textMeshPro.SetText(damageAmount.ToString());

        if (!isCrit)
        {
            _textMeshPro.fontSize = 6;
            // Beige
            textColor = new Color(225f / 255f, 225f / 255f, 190f / 255f);
        }
        else
        {
            _textMeshPro.fontSize = 8;
            // Orange
            textColor = new Color(230f / 255f, 95f / 255f, 20f / 255f);
        }
        _textMeshPro.color = textColor;
        disappearTimer = DISAPPEAR_TIMER_MAX;

        sortingOrder++;
        _textMeshPro.sortingOrder = sortingOrder;
        moveVector = new Vector3(0.7f, 1) * 10f;
    }

    private void Update()
    {

        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

        if (disappearTimer > DISAPPEAR_TIMER_MAX * 0.5f)
        {
            // First half of the popup
            float increaseScaleAmount = 1f;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        }
        else
        {
            // Second half
            float decreaseScaleAmount = 1f;
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            _textMeshPro.color = textColor;

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }


}

    

