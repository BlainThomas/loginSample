using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;



public class PhotonListener : MonoBehaviour 
{

    
    void Start(){
        if ( GameManager.Instance.photonListener == null) {
        GameManager.Instance.photonListener = this;
    } else {
        Destroy(this);
    }
    }

   private void OnEnable(){
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }
    
    private void OnDisable(){
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }


    private void NetworkingClient_EventReceived(EventData photonEvent) {
        byte eventCode = photonEvent.Code;
       string sender = photonEvent.Sender.ToString();
        Debug.Log("Received Event Code " + eventCode);

        if ( eventCode == 226 && PhotonNetwork.InLobby) {
                GameManager.Instance.photonControls.FindFriend();
        } else if ( eventCode == 230 ) {
            GameManager.Instance.photonControls.FindFriend();
            if (PhotonNetwork.InRoom) {
        GameManager.Instance.roomCount = PhotonNetwork.CurrentRoom.PlayerCount;
            }
        }
    }
}