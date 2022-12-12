using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class DataRender : MonoBehaviour
{

   public Sprite [] DownloadedPFP;
   public bool rendering = false;
   Sprite randSprite;

     void Start() {
        GameManager.Instance.dataRender = this;
        DontDestroyOnLoad(this);
   }

   public void LoggingOut()
    {
        Destroy(this);   
    }

   void Update(){
      if (SceneManager.GetActiveScene().name == "MenuScene"){
         if ( GameManager.Instance.menuSceneManager.menuCanvas.activeSelf ) {
               GameObject.Find("userCoins").GetComponent<Text>().text = GameManager.Instance.coinsCount.ToString();
               GameObject.Find("userName").GetComponent<Text>().text = PlayerPrefs.GetString("userName");
            
            if (GameObject.Find("userPFP").GetComponent<Image>().sprite != GameManager.Instance.userPFP) {
               GameObject.Find("userPFP").GetComponent<Image>().sprite = GameManager.Instance.userPFP;
            }}
        

         if ( GameManager.Instance.menuSceneManager.friendCanvas.activeSelf && GameManager.Instance.menuSceneManager.friendTagCreated ) {
            foreach ( var friend in GameManager.Instance._friends) {
               GameObject.Find( friend.FriendPlayFabId + "PFP").GetComponent<Image>().sprite = GameManager.Instance.playFabControls.SetPFP(friend.Profile.AvatarUrl);    
            }
            if (GameManager.Instance.onlineFriends != null){
            foreach ( var friendStatus in GameManager.Instance.onlineFriends ){
               if ( friendStatus.IsInRoom ){
                  GameObject.Find( friendStatus.UserId + "Status").GetComponent<Text>().text = "In Game";
                  GameObject.Find( friendStatus.UserId + "StatusIcon").GetComponent<Image>().color = Color.blue;
               } else if ( friendStatus.IsOnline ){
                  GameObject.Find( friendStatus.UserId + "Status").GetComponent<Text>().text = "Online";
                  GameObject.Find( friendStatus.UserId + "StatusIcon").GetComponent<Image>().color = Color.green;
               } else {
                  GameObject.Find( friendStatus.UserId + "Status").GetComponent<Text>().text = "Offline";
                  GameObject.Find( friendStatus.UserId + "StatusIcon").GetComponent<Image>().color = Color.red;
               }
            }}
            if ( GameManager.Instance.menuSceneManager.removeFriend.activeSelf ) {
               GameObject.Find("removeFriendText").GetComponent<Text>().text = "Are you sure you want to remove " + GameManager.Instance.removefriend + " as a friend?";
            }
         }
      }
   }

}
