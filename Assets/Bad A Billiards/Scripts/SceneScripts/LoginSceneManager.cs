using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginSceneManager : MonoBehaviour
{

    [SerializeField] public GameObject loadingCanvas, resetPasswordCanvas, messageCanvas, registerErrorMessage, resetErrorMessage;

    void Start() {
        GameManager.Instance.loginSceneManager = this;
        if (PlayerPrefs.HasKey("userName") && PlayerPrefs.HasKey("userPassword"))
        {     
            GameManager.Instance.playFabControls.Login();             
        }
    }

    public void ClearLogin()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("LoginScene");
    }

}
