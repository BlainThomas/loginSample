using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;




public class PhotonControls : MonoBehaviourPunCallbacks
{
        bool match;
        bool initiated;
        string[] friends;

    string roomName;
    List<RoomInfo> createdRooms = new List<RoomInfo>();

    
    void Awake() {
        GameManager.Instance.photonControls = this;
        DontDestroyOnLoad(this);        
    }


    void Update(){
        if (PhotonNetwork.InRoom)
            GameManager.Instance.roomCount = PhotonNetwork.CurrentRoom.PlayerCount;
        else GameManager.Instance.roomCount = 0;
        if (!PhotonNetwork.IsConnected) {
            GameManager.Instance.photonControls.ConnectUp();
    }}
    
    public void ConnectUp()
    {   
            PhotonNetwork.AutomaticallySyncScene = true;
        AuthenticationValues authValues = new AuthenticationValues();
            authValues.UserId = PlayerPrefs.GetString("playFabId");
        PhotonNetwork.AuthValues = authValues;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Disconnect() {
        PhotonNetwork.Disconnect();
    }

    public override void OnConnectedToMaster()
    {
        if (GameManager.Instance.inRoom) {
            PhotonNetwork.ReconnectAndRejoin();
        } else {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }}


    public override void OnJoinedLobby() {
        GameManager.Instance.joiningLobby = false;
        GameManager.Instance.inRoom = false;
        GameManager.Instance.inPrivateRoom = false;
        
        Debug.Log("PUN2 Connected");
            FindFriend();
            if ( SceneManager.GetActiveScene().name != "MenuScene" ) {
                SceneManager.LoadScene("MenuScene");
            } else {
            GameManager.Instance.menuSceneManager.loadingCanvas.SetActive(false);
    }}

    public override void OnLeftRoom(){
        Debug.Log("Left room. Connected to master: " );
    }

    public void FindFriend(){
            if ( GameManager.Instance.friendsIDList.Length > 0) 
        PhotonNetwork.FindFriends( GameManager.Instance.friendsIDList );
    }

    public override void OnFriendListUpdate(List<FriendInfo> friendList ) {
        Debug.Log("Friend status updated");
        GameManager.Instance.onlineFriends = friendList;
    }
    
    


    public void createGame()
        {   
        GameManager.Instance.offlineMode = false;
        roomName = PlayerPrefs.GetString("playFabId");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = (byte)2; //Set any number
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        }

    public void CreatePrivateRoom(){
        GameManager.Instance.inPrivateRoom = true;
        roomName = PlayerPrefs.GetString("playFabId");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = false;
        roomOptions.MaxPlayers = (byte)2; //Set any number
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom() {
        Debug.Log("OnCreatedRoom");
        GameManager.Instance.roomOwner = true;
        GameManager.Instance.gameReady = true;
        Debug.Log("Room owner?: " + GameManager.Instance.roomOwner );
    }

    public override void OnJoinedRoom() {
        if ( PhotonNetwork.CurrentRoom.PlayerCount == 1)
        GameManager.Instance.roomOwner = true;
        GameManager.Instance.gameReady = true;
        if ( GameManager.Instance.roomOwner)
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("Joined room " + PhotonNetwork.CurrentRoom.Name );
        GameManager.Instance.inRoom = true;
        if (GameManager.Instance.menuSceneManager.loadingCanvas.activeSelf){
            GameManager.Instance.menuSceneManager.loadingCanvas.SetActive( false );
        }
        if (GameManager.Instance.inPrivateRoom && GameManager.Instance.roomOwner) {
            if (GameManager.Instance.opponentID == GameManager.Instance.lastOpponentID)
                GameManager.Instance.timesOpponentPlayed += 1;
            GameManager.Instance.messageType = "challenge";
            // GameManager.Instance.photonChatControls.SendDirectMessage( GameManager.Instance.opponentID, GameManager.Instance.messageType);
        }
        // GameManager.Instance.photonListener.SendData();
    }


    public void ChallengeFriend(string username, string ID) {
        GameManager.Instance.tableNumber = 4;
        GameManager.Instance.menuSceneManager.loadingCanvas.SetActive(true);    
        GameManager.Instance.menuSceneManager.matchingCanvas.SetActive(true);      
        GameManager.Instance.opponentName = username;
        GameManager.Instance.opponentID = ID;
        if (GameManager.Instance.lastOpponentID == ID )
            GameManager.Instance.timesOpponentPlayed += 1;
        CreatePrivateRoom();
    }
    

    


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {   
        foreach (var room in roomList)
        Debug.Log( "Room List: " + room );
        createdRooms = roomList;
        Debug.Log("Got Room Lists.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {   

        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + cause.ToString() + " ServerAddress: " + PhotonNetwork.ServerAddress);
    }

    public override void OnCreateRoomFailed(short returnCode, string messageError)
    {
        Debug.Log("OnCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
        Debug.Log("Message: " + messageError + " returnCode: " + returnCode);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("it failed");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("On JoinRandomRoom failed. Creating new Room");
        createGame();
    }

    
 

}
