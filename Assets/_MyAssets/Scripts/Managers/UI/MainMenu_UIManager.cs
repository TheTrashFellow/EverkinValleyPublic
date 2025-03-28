using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;



public class MainMenu_UIManager : MonoBehaviour
{
    [Header("GameManager")]
    [SerializeField] private MainMenu_GameManager gameManager;

    [Header("Views")]
    [SerializeField] private GameObject _viewLogin = default;
    [SerializeField] private GameObject _viewCreation = default;
    [SerializeField] private GameObject _viewLoading = default;
    [SerializeField] private GameObject _messagePopup = default;
    

    [Header("InputFieldsCreation")]
    [SerializeField] private TMP_InputField _inputCourriel = default;
    [SerializeField] private TMP_InputField _inputNomPersonnage = default;
    [SerializeField] private TMP_InputField _inputMotDePasse = default;
    [SerializeField] private TMP_InputField _inputConfirmationMotDePasse = default;

    [Header("ErreursCreation")]
    [SerializeField] private TMP_Text _erreurCourriel = default;
    [SerializeField] private TMP_Text _erreurNomPersonnage = default;
    [SerializeField] private TMP_Text _erreurMotDePasse = default;
    [SerializeField] private TMP_Text _erreurConfirmation = default;

    [Header("InputFieldsConnection")]
    [SerializeField] private TMP_InputField _inputLoginCourriel = default;
    [SerializeField] private TMP_InputField _inputLoginMDP = default;
    

    public void Quitter()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void Btn_Menu_Create()
    {
        _viewLogin.SetActive(false);
        _viewCreation.SetActive(true);
    }

    public void Btn_Return_Login()
    {
        _viewCreation.SetActive(false);
        _viewLogin.SetActive(true);
    }

    public void Btn_Create_Account()
    {
        bool valid = true;

        _erreurCourriel.text = "";
        _erreurNomPersonnage.text = "";
        _erreurMotDePasse.text = "";
        _erreurConfirmation.text = "";

        string courrielRegexString = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$";

        if (_inputMotDePasse.text != _inputConfirmationMotDePasse.text)
        {
            _erreurConfirmation.text = "La confirmation de mot de passe ne coordonne pas avec le mot de passe choisis";
            valid = false;            
        }

        if(string.IsNullOrEmpty(_inputCourriel.text))
        {
            _erreurCourriel.text = "Ce champ est obligatoire";
            valid = false;
        }else if(!Regex.IsMatch(_inputCourriel.text, courrielRegexString))
        {
            _erreurCourriel.text = "Veuillez entrer une adresse courrielle valide";
        }

        if (string.IsNullOrEmpty(_inputNomPersonnage.text))
        {
            _erreurNomPersonnage.text = "Ce champ est obligatoire";
            valid = false;
        }
        if (string.IsNullOrEmpty(_inputMotDePasse.text))
        {
            _erreurMotDePasse.text = "Ce champ est obligatoire";
            valid = false;
        }

        if (valid)
            gameManager.CreateAccount(_inputCourriel.text, _inputMotDePasse.text, _inputNomPersonnage.text);
    }

    public void Show_Loading()
    {
        _viewLoading.SetActive(true);
    }

    public void Close_Loading()
    {
        _viewLoading.SetActive(false);
    }

    public void Show_PopUp_Message(string message, bool succes)
    {
        _messagePopup.SetActive(true);
        Transform result = _messagePopup.transform.Find("Canvas/Result");

        result.GetComponent<TMP_Text>().text = message;

        if (succes)
        {
            Transform ok = _messagePopup.transform.Find("Canvas/Ok");
            Button button = ok.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ClearUI());
        }
    }

    public void ClearUI()
    {
        Btn_Close_Message();
        _inputCourriel.text = "";
        _inputNomPersonnage.text = "";
        _inputMotDePasse.text = "";
        _inputConfirmationMotDePasse.text = "";

        _inputLoginCourriel.text = "";
        _inputLoginMDP.text = "";

        _inputLoginCourriel.gameObject.GetComponent<Image>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
        _inputLoginMDP.gameObject.GetComponent<Image>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f);

        _viewCreation.SetActive(false);
        _viewLogin.SetActive(true);
    }

    public void Btn_Close_Message()
    {
        _messagePopup.SetActive(false);
    }

    public void Btn_Attempt_Login()
    {
        bool valid = true;

        _inputLoginCourriel.gameObject.GetComponent<Image>().color = new Color(255f/255f, 255f/255f, 255f/255f);  
        _inputLoginMDP.gameObject.GetComponent<Image>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f);

        if (string.IsNullOrEmpty(_inputLoginCourriel.text))
        {
            _inputLoginCourriel.gameObject.GetComponent<Image>().color = new Color(255f / 255f, 112f / 255f, 112f / 255f);
            valid = false;
        }

        if (string.IsNullOrEmpty(_inputLoginMDP.text))
        {
            _inputLoginMDP.gameObject.GetComponent<Image>().color = new Color(255f / 255f, 112f / 255f, 112f / 255f);
            valid = false;
        }

        if(valid)
            gameManager.AttemptLogin(_inputLoginCourriel.text, _inputLoginMDP.text);
    }

    public void Set_Errors_Create(string erreurCourriel, string  erreurNomPerso, string erreurMotDePasse)
    {
        if (erreurCourriel != null)
            _erreurCourriel.text = erreurCourriel;
        else
            _erreurCourriel.text = "";

        if(erreurNomPerso != null)
            _erreurNomPersonnage.text = erreurNomPerso;
        else
            _erreurNomPersonnage.text = "";

        if (erreurMotDePasse != null)
            _erreurMotDePasse.text = erreurMotDePasse;
        else
            _erreurMotDePasse.text = "";
    }
}
