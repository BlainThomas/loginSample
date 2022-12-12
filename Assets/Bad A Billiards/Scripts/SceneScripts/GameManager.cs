using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using Photon.Pun;
using ExitGames.Client.Photon;
using PlayFab.ClientModels;


public class GameManager
{

    // Scripts
    public PlayFabControls playFabControls;
    private static GameManager instance;
    public MenuSceneManager menuSceneManager;
    public DataRender dataRender;
    public LoginSceneManager loginSceneManager;
    public PhotonControls photonControls;
    public PhotonListener photonListener;
    public EmailFactory emailFactory;



    // User
    public bool inRoom;
    public bool joiningLobby;
    public bool loading;
    public bool registering = false;
    public bool offlineMode = false;
    public bool loggedIn = false;
    public Int32 coinsCount = 10;
    public Sprite userPFP;
    public string userPFPURL;
    public List<FriendInfo> _friends = null;
    public string removefriendID;
    public string removefriend;
    public string[] friendsIDList;
    public List<Photon.Realtime.FriendInfo> onlineFriends;


    // Opponate
    public string lastOpponentID;
    public string opponentName;
    public string opponentID;
    public int timesOpponentPlayed = 0;


    // Room Settings

    public bool inPrivateRoom = false;
    public int roomCount;
    public int tableNumber = 0;
    public bool roomOwner = false;



    // Messaging
    public string messageType;


    
    public void resetAllData() {
               
        if (joiningLobby){
        PhotonNetwork.LeaveRoom(true);
        }

        lastOpponentID = opponentID;
        messageType = null;
        challenger = false;
        challenge = "none";
        inPrivateRoom = false;
        roomOwner = false;
        inRoom = false;
        PlayerPrefs.DeleteKey( "opponentName" );
    }

    public void Logout() {
        GameManager.instance.photonControls.Disconnect();
        loggedIn = false;
        UnityEngine.Object.Destroy(UnityEngine.GameObject.Find("playFabControls"));  
        UnityEngine.Object.Destroy(UnityEngine.GameObject.Find("components"));   
        UnityEngine.Object.Destroy(UnityEngine.GameObject.Find("photonControls"));
        PlayerPrefs.DeleteAll(); 
        resetAllData();
        Debug.Log("Logged Out");
        GameManager.instance.menuSceneManager.LoginScene();
    }


    private GameManager(){ }

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameManager();
            }
            return instance;
        }
    }
    






    public bool gameReady = true;

    public object challenge = "none";
    public bool challenger = false;





    
}
