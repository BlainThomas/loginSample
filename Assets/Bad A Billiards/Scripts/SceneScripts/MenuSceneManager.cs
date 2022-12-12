using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;



public class MenuSceneManager : MonoBehaviour
{
    
    [SerializeField] public GameObject menuCanvas, leaderboardCanvas, loadingCanvas, friendCanvas, matchingCanvas, insuficientCoinsCanvas, addFriend, removeFriend, errorText, pFPCanvas;

    public bool friendTagCreated;


    void Start() {
        GameManager.Instance.menuSceneManager = this;
        if (!PhotonNetwork.IsConnected) {
            loadingCanvas.SetActive( true );
            GameManager.Instance.photonControls.ConnectUp();
        }
        if ( GameManager.Instance.registering ) {
            pFPCanvas.SetActive( true );
        } else if ( GameManager.Instance.loading || GameManager.Instance.joiningLobby) {
            loadingCanvas.SetActive( true );
        } else if (!GameManager.Instance.loading) {
        GameManager.Instance.menuSceneManager.loadingCanvas.SetActive(false);
        GameManager.Instance.menuSceneManager.menuCanvas.SetActive(true);
        } else {
            menuCanvas.SetActive( true );
        }    
    }  

    public void BuyPoint() {
        GameManager.Instance.playFabControls.BuyPoint();
    }


    public void LoadLeaderBoard(){
        loadingCanvas.SetActive( true );
        GameManager.Instance.playFabControls.GetStatistics();
    }

    public void Logout(){
        GameManager.Instance.Logout();
    }

    public void LoginScene(){
        SceneManager.LoadScene("LoginScene");
    }

    public void RemoveFriend(){
        GameManager.Instance.playFabControls.confirmRemoveFriend();
    }

    public void cancelRemoveFriend() {
        GameManager.Instance.playFabControls.cancelRemoveFriend();
    }

    // Links

    public void Website()
    {
        Application.OpenURL("https://www.badabilliards.com/");
    }

}