using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;
using UnityEngine.SceneManagement;


public class FriendScript : MonoBehaviour {

    [SerializeField] public GameObject list, friendPrefab;
    private enum FriendIdType { PlayFabId, Username, Email, DisplayName };
    private int friendCount;
    private List<FriendInfo> _friends = null;

    void Start(){
        GetFriends();
    }

    public void GetFriends(){
         PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false,
            XboxToken = null,
            ProfileConstraints = new PlayerProfileViewConstraints() {
            ShowAvatarUrl = true,
            ShowDisplayName = true,
            }
        }, result => {
            _friends = result.Friends;
            GameManager.Instance.friendsIDList = new string[_friends.Count];
            for ( var i = 0; i < _friends.Count; i++ ) {
                GameManager.Instance.friendsIDList[i] = _friends[i].Profile.PlayerId;
            }
            Debug.Log("Got friends");
        }, DisplayPlayFabError);
    }

    void UpdateFriendStatus(){
        GameManager.Instance.photonControls.FindFriend();
    }

    public void DestroyFriends(){
        foreach ( var friendID in GameManager.Instance._friends) {
            Destroy(GameObject.Find(friendID.Profile.PlayerId));
        }
        GameManager.Instance.menuSceneManager.friendTagCreated = false;
        if (GameManager.Instance.menuSceneManager.addFriend.activeSelf) {
            GameManager.Instance.menuSceneManager.addFriend.SetActive(false);
        }
    }


    public void showFriends() { 
        GameManager.Instance.menuSceneManager.friendCanvas.SetActive( true );
        GameObject.Find("playerID").GetComponent<Text>().text = "ID# " + PlayerPrefs.GetString("playFabId");
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false,
            XboxToken = null,
            ProfileConstraints = new PlayerProfileViewConstraints() {
            ShowAvatarUrl = true,
            ShowDisplayName = true,
            }
        }, result => {
            _friends = result.Friends;
            if (GameManager.Instance.menuSceneManager.friendTagCreated)
                DestroyFriends();
            GameManager.Instance.friendsIDList = new string[_friends.Count];
            for ( var i = 0; i < _friends.Count; i++ ) {
                GameManager.Instance.friendsIDList[i] = _friends[i].Profile.PlayerId;
            }
    	    foreach (var resultFriend in _friends ) {
    		    CreteFriendTag(resultFriend.Profile.DisplayName, resultFriend.Profile.AvatarUrl , resultFriend.Profile.PlayerId); 
            }
            GameManager.Instance.menuSceneManager.friendTagCreated =true;
            GameManager.Instance._friends = _friends;
            UpdateFriendStatus();
            Debug.Log("Created Friends");
        }, DisplayPlayFabError);
    }

    public void CreteFriendTag(string username, string AvatarUrl, string PlayfabID){
        GameObject friend = Instantiate(friendPrefab, Vector3.zero ,Quaternion.identity) as GameObject;
        friend.name = PlayfabID;
    	friend.transform.Find ("FriendName").name = PlayfabID + "Name";
    	friend.transform.Find ("FriendPFP").name = PlayfabID + "PFP";
    	friend.transform.Find ("FriendStatus").name = PlayfabID + "Status";
    	friend.transform.Find ("FriendStatusIcon").name = PlayfabID + "StatusIcon";
        
        friend.transform.Find (PlayfabID + "Name").GetComponent <Text>().text = username;
        friend.transform.Find (PlayfabID + "Status").GetComponent <Text>().text = "Offline";
    	friend.transform.Find (PlayfabID + "PFP").GetComponent<Image>().sprite = GameManager.Instance.playFabControls.SetPFP(AvatarUrl);
    	friend.transform.Find (PlayfabID + "StatusIcon").GetComponent<Image>().color = Color.red;
        friend.transform.Find("InviteFriendButton").GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.photonControls.ChallengeFriend(username ,PlayfabID));
        friend.transform.Find("RemoveFriendButton").GetComponent<Button>().onClick.AddListener(() => removeFriend(username ,PlayfabID));
    	friend.transform.SetParent(list.transform);
    	friend.GetComponent <RectTransform>().localScale = new Vector3(1f, 1f, 1.0f);
    }

    public void removeFriend(string removePlayer, string removePlayerID){
        GameManager.Instance.removefriendID = removePlayer;
        GameManager.Instance.playFabControls.removeFriend( removePlayerID );
    }

    public void AddFriend(){
        FriendIdType idType = FriendIdType.Username;
        if ( friendCount == 1 )
        idType = FriendIdType.DisplayName;
        if ( friendCount == 2 )
        idType = FriendIdType.PlayFabId;
        friendCount += 1;
        string friendId = GameObject.Find("FriendData").GetComponent<Text>().text.ToString();
        PlayFabAddFriend( idType, friendId);
    }

    void PlayFabAddFriend(FriendIdType idType, string friendId) {
        var request = new AddFriendRequest();
        switch (idType) {
            case FriendIdType.PlayFabId:
                request.FriendPlayFabId = friendId;
            break;
            case FriendIdType.Username:
                request.FriendUsername = friendId;
            break;
            case FriendIdType.Email:
                request.FriendEmail = friendId;
            break;
            case FriendIdType.DisplayName:
                request.FriendTitleDisplayName = friendId;
            break;
        }
        PlayFabClientAPI.AddFriend(request, result => {
        friendCount = 0;
        Debug.Log("Friend added successfully!");
        GameObject.Find("AddFreindCanvas").SetActive( false );
        showFriends();
        }, (error) => {
            Debug.Log( "Attempt " + friendCount + " of 3  Error Message - " + error.ToString());
            if (friendCount < 3) {
                AddFriend();
            } else {
                DisplayPlayFabError(error);
            }
        });
    }


    void DisplayPlayFabError(PlayFabError error) { 
        Debug.Log(error.GenerateErrorReport());
        if (SceneManager.GetActiveScene().name == "MenuScene")
            GameManager.Instance.menuSceneManager.errorText.SetActive(true);
    }    

}