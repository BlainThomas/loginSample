using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;



public class PlayFabControls : MonoBehaviour
{ 
    string removeFriendID;


    public void Start(){
        GameManager.Instance.playFabControls = this;
        DontDestroyOnLoad(this);
    }


    // Login Sceen
    string encryptedPassword( string pass ){

        System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] bs = System.Text.Encoding.UTF8.GetBytes( pass );
        bs = x.ComputeHash( bs );
        System.Text.StringBuilder s = new System.Text.StringBuilder();
        foreach(byte b in bs){
            s.Append(b.ToString("x2").ToLower());
        }
        return s.ToString();
    }

    public void Login() {

        if (SceneManager.GetActiveScene().name == "LoginScene")
            GameManager.Instance.loginSceneManager.loadingCanvas.SetActive( true );
        if (SceneManager.GetActiveScene().name == "MenuScene")
            GameManager.Instance.menuSceneManager.loadingCanvas.SetActive( true );
        string loginUserName;
        string loginPassword;

        if (SceneManager.GetActiveScene().name == "LoginScene") {
            GameObject.Find("loadingText").GetComponent<Text>().text = "Checking Bad A Rosters";
        }

        if (PlayerPrefs.HasKey( "userName") && PlayerPrefs.HasKey("userPassword")){
            loginUserName = PlayerPrefs.GetString( "userName" );
            loginPassword = PlayerPrefs.GetString( "userPassword" );
        } else {
            loginUserName = GameObject.Find( "userInputName" ).GetComponent<Text>().text;
            loginPassword = GameObject.Find( "userInputPassword" ).GetComponent<Text>().text;
        }

        LoginWithPlayFabRequest request = new LoginWithPlayFabRequest() {
            TitleId = PlayFabSettings.TitleId,
            Username = loginUserName,
            Password = encryptedPassword( loginPassword ),
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams() {
                GetPlayerProfile = true,
                GetPlayerStatistics = true,
                GetUserAccountInfo = true,
                GetUserInventory = true,
                GetUserVirtualCurrency = true,
                GetUserData = true,
                GetUserReadOnlyData = true,
                GetCharacterInventories = false,
                GetCharacterList = false,
                GetTitleData = false,
                ProfileConstraints = new PlayerProfileViewConstraints() {
                    ShowDisplayName = true,
                    ShowAvatarUrl = true,
                    
                }
            }
        };

        PlayFabClientAPI.LoginWithPlayFab( request, ( result ) => {
            GameManager.Instance.loggedIn = true;
            PlayerPrefs.SetString( "playFabId", result.PlayFabId );
            if (result.InfoResultPayload.PlayerProfile != null)
                if ( result.InfoResultPayload.PlayerProfile.DisplayName != null )
                    PlayerPrefs.SetString("userName", result.InfoResultPayload.PlayerProfile.DisplayName );
                PlayerPrefs.SetString("userPassword", loginPassword );
                if (result.InfoResultPayload.PlayerProfile.AvatarUrl != null)
                    GameManager.Instance.userPFPURL = result.InfoResultPayload.PlayerProfile.AvatarUrl;
                    GameManager.Instance.userPFP = SetPFP(result.InfoResultPayload.PlayerProfile.AvatarUrl);
                if (result.InfoResultPayload.UserData != null){
                    if (result.InfoResultPayload.UserData.ContainsKey( "CurrentPFPCount" ))
                        PlayerPrefs.SetInt("userPFPCount", Int32.Parse( result.InfoResultPayload.UserData["CurrentPFPCount"].Value ));
                    if (result.InfoResultPayload.UserData.ContainsKey( "CurrentCueStick" ))
                        PlayerPrefs.SetInt("userCueStick", Int32.Parse( result.InfoResultPayload.UserData["CurrentCueStick"].Value ));
                }
            GameManager.Instance.coinsCount = result.InfoResultPayload.UserVirtualCurrency["BC"];
            PlayerPrefs.Save();
            Debug.Log( "Logged in" );
            GameManager.Instance.loading = true;
            if (SceneManager.GetActiveScene().name == "LoginScene")
                SceneManager.LoadScene( "MenuScene" );
            if (SceneManager.GetActiveScene().name == "MenuScene")
                GameManager.Instance.photonControls.ConnectUp();
        }, ( error ) =>
            {
            DisplayError( error );
            });
    }

    public void Register() {
        
        string loginUserName = GameObject.Find("userInputName").GetComponent<Text>().text;
        string loginPassword = GameObject.Find("userInputPassword").GetComponent<Text>().text;
        string loginEmail = GameObject.Find("userInputEmail").GetComponent<Text>().text;

        if (Regex.IsMatch(loginEmail, @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$") && loginPassword.Length >= 6 && loginUserName.Length > 0) {
              

            RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest() {
            TitleId = PlayFabSettings.TitleId,
            Username = loginUserName,
            Password = encryptedPassword( loginPassword ),
            DisplayName = loginUserName,
            Email = loginEmail,
            RequireBothUsernameAndEmail = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams() {
                    GetUserAccountInfo = true,
                    GetUserInventory = true,
                    GetUserVirtualCurrency = true,
                    GetUserData = true,
                    GetUserReadOnlyData = false,
                    GetCharacterInventories = false,
                    GetCharacterList = false,
                    GetTitleData = false,
                    GetPlayerStatistics = true
                }
            };

            PlayFabClientAPI.RegisterPlayFabUser( request, ( result ) => { 
                if (SceneManager.GetActiveScene().name == "LoginScene")
                    GameManager.Instance.loginSceneManager.loadingCanvas.SetActive( true );
                GameObject.Find("loadingText").GetComponent<Text>().text = "Adding To Bad A Rosters";
                GameManager.Instance.loggedIn = true;
                PlayerPrefs.SetString( "playFabId", result.PlayFabId );
                PlayerPrefs.SetString( "userName", loginUserName );
                PlayerPrefs.SetString("userPassword", loginPassword );
                GameManager.Instance.registering = true;
                GetInventoryCurrency();
                PlayerPrefs.Save();
                Debug.Log( "Registered" );
                GameManager.Instance.loading = true;
                SceneManager.LoadScene( "MenuScene" );
            }, ( error ) => {
                DisplayRegisterError( error );
            });
        } else if ( loginEmail.Length == 0 && loginPassword.Length >= 6 && loginUserName.Length > 6 ) {

            RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest() {
            TitleId = PlayFabSettings.TitleId,
            Username = loginUserName,
            Password = encryptedPassword( loginPassword ),
            DisplayName = loginUserName,
            RequireBothUsernameAndEmail = false,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams() {
                    GetUserAccountInfo = true,
                    GetUserInventory = true,
                    GetUserVirtualCurrency = true,
                    GetUserData = true,
                    GetUserReadOnlyData = false,
                    GetCharacterInventories = false,
                    GetCharacterList = false,
                    GetTitleData = false,
                    GetPlayerStatistics = true
                }
            };

            PlayFabClientAPI.RegisterPlayFabUser( request, ( result ) => { 
                if (SceneManager.GetActiveScene().name == "LoginScene")
                    GameManager.Instance.loginSceneManager.loadingCanvas.SetActive( true );
                GameObject.Find("loadingText").GetComponent<Text>().text = "Adding To Bad A Rosters";
                GameManager.Instance.loggedIn = true;
                PlayerPrefs.SetString( "playFabId", result.PlayFabId );
                PlayerPrefs.SetString( "userName", loginUserName );
                PlayerPrefs.SetString("userPassword", loginPassword );
                GameManager.Instance.registering = true;
                GetInventoryCurrency();
                PlayerPrefs.Save();
                Debug.Log( "Registered" );
                GameManager.Instance.loading = true;
                SceneManager.LoadScene( "MenuScene");
            }, (error) => {
                DisplayRegisterError( error );
            });
        } else if (loginEmail.Length > 0 && !Regex.IsMatch(loginEmail, @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$")) {
            GameManager.Instance.loginSceneManager.registerErrorMessage.SetActive(true);
            GameObject.Find("errorMessage").GetComponent<Text>().text = "Must put in a valid email or no email at all";
        } else {
            GameManager.Instance.loginSceneManager.registerErrorMessage.SetActive(true);
            GameObject.Find("errorMessage").GetComponent<Text>().text = "UserName and Password must each be 6 characters long";
        }  
    }
    void GetInventoryCurrency(){
        PlayFabClientAPI.GetUserInventory( new GetUserInventoryRequest(), ( result ) => {
        GameManager.Instance.coinsCount = result.VirtualCurrency["BC"];
        }, OnError);
    }
    void DisplayError(PlayFabError error) {
        Debug.Log(error.ErrorMessage.ToString());
        if (SceneManager.GetActiveScene().name == "LoginScene") {
            GameManager.Instance.loginSceneManager.messageCanvas.SetActive(true);
        if (error.ErrorMessage.ToString() == "Invalid input parameters")
            GameObject.Find("errorMessage").GetComponent<Text>().text = "error.ErrorMessage.ToString()";
        GameObject.Find("errorMessage").GetComponent<Text>().text = error.ErrorMessage.ToString();
        }
    }

    void DisplayRegisterError(PlayFabError error) {
        Debug.Log(error.ErrorMessage.ToString());
        if (GameManager.Instance.loginSceneManager.registerErrorMessage.activeSelf){
            GameManager.Instance.loginSceneManager.registerErrorMessage.SetActive( true );
            GameManager.Instance.loginSceneManager.registerErrorMessage.GetComponent<Text>().text = error.ErrorMessage.ToString();
        } else if (GameManager.Instance.loginSceneManager.resetPasswordCanvas.activeSelf) {
            GameManager.Instance.loginSceneManager.resetErrorMessage.SetActive( true );
        } else if ( error.ErrorMessage.ToString() == "Invalid input parameters" ) {
            GameObject.Find("errorMessage").GetComponent<Text>().text = "Invalid input";
            GameObject.Find("errorMessage").GetComponent<Text>().text = error.ErrorMessage.ToString();
        }
    }

    public void ResetPassword() {
        SendAccountRecoveryEmailRequest request = new SendAccountRecoveryEmailRequest() {
            TitleId = PlayFabSettings.TitleId,
            Email = GameObject.Find("userInputEmail").GetComponent<Text>().text
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request, (result) => {
            if (SceneManager.GetActiveScene().name == "LoginScene")
                GameManager.Instance.loginSceneManager.messageCanvas.SetActive(true);
            GameObject.Find("ErrorMessage").GetComponent<Text>().text = "Email sent to your address. Check your inbox";
        }, (error) => {
            DisplayRegisterError( error );
        });
    }
    
    public void DeleteAccount() {
        if (SceneManager.GetActiveScene().name == "LoginScene")
            GameManager.Instance.loginSceneManager.loadingCanvas.SetActive( true );
        GameObject.Find("loadingText").GetComponent<Text>().text = "Removing Bad A Rosters";
        LoginWithPlayFabRequest request = new LoginWithPlayFabRequest() {
            TitleId = PlayFabSettings.TitleId,
            Username = GameObject.Find( "userInputName" ).GetComponent<Text>().text,
            Password = encryptedPassword( GameObject.Find( "userInputPassword" ).GetComponent<Text>().text ),
        };

        PlayFabClientAPI.LoginWithPlayFab( request, ( result ) => {
            
            PlayerPrefs.SetString( "playFabId", result.PlayFabId );
            PlayerPrefs.SetString("userName", GameObject.Find("userInputName").GetComponent<Text>().text );
            
            GameManager.Instance.emailFactory.SendEmailDeleteAccount();
        }, (error) => {
            DisplayError(error);
        });
    }



    // Menu Sceen

    public void GetStatistics() {
        GameManager.Instance.menuSceneManager.leaderboardCanvas.SetActive( true );
        var request = new GetLeaderboardRequest {
            StatisticName = "points",
            StartPosition = 0,
            MaxResultsCount = 10,
            ProfileConstraints = new PlayerProfileViewConstraints {
                ShowAvatarUrl = true,
                ShowDisplayName = true
            }
        };
        PlayFabClientAPI.GetLeaderboard( request, ( result ) => {
            Debug.Log("Got Leaderboard data");
            foreach ( var contender in result.Leaderboard ){
                string spot = (contender.Position + 1).ToString();
                GameObject.Find(spot + "Name").GetComponent<Text>().text = contender.Profile.DisplayName;
                GameObject.Find(spot + "PFP").GetComponent<Image>().sprite = SetPFP(contender.Profile.AvatarUrl);
                GameObject.Find(spot + "Points").GetComponent<Text>().text = contender.StatValue.ToString();
            };
            Debug.Log("Leaderboard Updated");
            GameManager.Instance.menuSceneManager.loadingCanvas.SetActive( false );
        }, OnError);
    }
    
    public void confirmRemoveFriend() {
        PlayFabClientAPI.RemoveFriend(new RemoveFriendRequest {
            FriendPlayFabId = removeFriendID
        }, result => {
        for ( var i = 0; i <= GameManager.Instance._friends.Count; i++ ) {
            Destroy(GameObject.Find(removeFriendID));
            if ( GameManager.Instance._friends[i].FriendPlayFabId == removeFriendID) {
                GameManager.Instance._friends.Remove(GameManager.Instance._friends[i]);
                i += GameManager.Instance._friends.Count;
            }
        }
        for ( var j = 0; j <= GameManager.Instance.onlineFriends.Count; j++ ) {
            if ( GameManager.Instance.onlineFriends[j].UserId == removeFriendID) {
                GameManager.Instance.onlineFriends.Remove(GameManager.Instance.onlineFriends[j]);
                j += GameManager.Instance.onlineFriends.Count;
            }
        }
        
            Debug.Log("PlayfabremoveID " + removeFriendID + " removed from friends");
            removeFriendID = null;
        }, OnError);
    }


public void removeFriend(string ID) {
        removeFriendID = ID;
        GameManager.Instance.removefriendID = ID;
        GameManager.Instance.menuSceneManager.removeFriend.SetActive(true);
    }

public void cancelRemoveFriend() {
    removeFriendID = null;
    }




    // Finish Sceen

    public void UpdateGamesPlayed(){
        var request = new UpdatePlayerStatisticsRequest {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate {
                    StatisticName = "totalGamesPlayed",
                    Value = 1
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, ( result ) => {
            
        var request = new UpdatePlayerStatisticsRequest {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate {
                    StatisticName = "weeklyGamesPlayed",
                    Value = 1
                },
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderBoardUpdate, OnError);
        }
        , OnError);
    }


     
    void OnLeaderBoardUpdate(UpdatePlayerStatisticsResult result){
        Debug.Log("Leaderboard updated");
        // Debug.Log(result.ToString());
    }

    public void BuyPoint(){
        if (GameManager.Instance.coinsCount <50) {
            GameManager.Instance.menuSceneManager.insuficientCoinsCanvas.SetActive(true);
        } else {
        GameManager.Instance.menuSceneManager.loadingCanvas.SetActive(true);
        var request = new SubtractUserVirtualCurrencyRequest {
            VirtualCurrency = "BC",
            Amount = 50,
        };
        PlayFabClientAPI.SubtractUserVirtualCurrency(request, ( result ) => {
            GameManager.Instance.coinsCount = result.Balance;
             var request = new UpdatePlayerStatisticsRequest {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate {
                    StatisticName = "points",
                    Value = 1
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderBoardUpdate, OnError);
        GameManager.Instance.menuSceneManager.loadingCanvas.SetActive(false);
        }
        , OnError);
    }}




    public void UpdatePlayFabPFP(int i){
        GameManager.Instance.userPFPURL = SetURL(i);
                    GameManager.Instance.userPFP = SetPFP(SetURL(i));
        var request = new UpdateAvatarUrlRequest {
            ImageUrl = SetURL(i),
        };
        PlayFabClientAPI.UpdateAvatarUrl(request, OnPFPUpdateSuccess, OnError);
    }


    void OnPFPUpdateSuccess(EmptyResponse result) {
        Debug.Log("PFP Updated!");
        GameManager.Instance.registering = false;
        GameManager.Instance.menuSceneManager.pFPCanvas.SetActive(false);
        GameManager.Instance.menuSceneManager.menuCanvas.SetActive(true);
    }


    void GetUserData(){
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }

    void OnDataRecieved(GetUserDataResult result){
        Debug.Log("Data Recieved!");
        if (result.Data != null){
        if (result.Data.ContainsKey("CurrentPFPCount")){
            PlayerPrefs.SetInt("userPFPCount", Int32.Parse(result.Data["CurrentPFPCount"].Value));
        } if (result.Data.ContainsKey("CurrentCueStick")){
            PlayerPrefs.SetInt("userCueStick", Int32.Parse(result.Data["CurrentCueStick"].Value));
        }
        }
    }


    void SetUserData(){
        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"CurrentPFPCount", PlayerPrefs.GetInt("userPFPCount").ToString()},
                {"CurrentCueStick", PlayerPrefs.GetInt("userCueStick").ToString()}
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnDataUpdated, OnError);
        }

    void OnDataUpdated(UpdateUserDataResult result) {
        Debug.Log("Data Updated!");
    }


    // Setting Profile
    public Sprite SetPFP( string URL ){
        if (URL == "https://ssl.gstatic.com/docs/common/profile/elephant_lg.png"){
             return GameManager.Instance.dataRender.DownloadedPFP[1];
        } else if (URL == "https://ssl.gstatic.com/docs/common/profile/giraffe_lg.png"){
             return GameManager.Instance.dataRender.DownloadedPFP[2];
        } else if (URL == "https://ssl.gstatic.com/docs/common/profile/ibex_lg.png"){
             return GameManager.Instance.dataRender.DownloadedPFP[3];
        } else if (URL == "https://ssl.gstatic.com/docs/common/profile/jackalope_lg.png"){
             return GameManager.Instance.dataRender.DownloadedPFP[4];
        } else if (URL == "https://ssl.gstatic.com/docs/common/profile/koala_lg.png"){
             return GameManager.Instance.dataRender.DownloadedPFP[5];
        } else if (URL == "https://ssl.gstatic.com/docs/common/profile/lemur_lg.png"){
             return GameManager.Instance.dataRender.DownloadedPFP[6];
        } else if (URL == "https://ssl.gstatic.com/docs/common/profile/liger_lg.png"){
             return GameManager.Instance.dataRender.DownloadedPFP[7];
        } else if (URL == "https://ssl.gstatic.com/docs/common/profile/panda_lg.png"){
             return GameManager.Instance.dataRender.DownloadedPFP[8];
        } else if (URL == "https://ssl.gstatic.com/docs/common/profile/turtle_lg.png"){
             return GameManager.Instance.dataRender.DownloadedPFP[9];
        } else {
             return GameManager.Instance.dataRender.DownloadedPFP[0];
        }
    }

    public string SetURL( int i ){
        if (i == 1){
             return "https://ssl.gstatic.com/docs/common/profile/elephant_lg.png";
        } else if (i == 2){
             return "https://ssl.gstatic.com/docs/common/profile/giraffe_lg.png";
        } else if (i == 3){
             return "https://ssl.gstatic.com/docs/common/profile/ibex_lg.png";
        } else if (i == 4){
             return "https://ssl.gstatic.com/docs/common/profile/jackalope_lg.png";
        } else if (i == 5){
             return "https://ssl.gstatic.com/docs/common/profile/koala_lg.png";
        } else if (i == 6){
             return "https://ssl.gstatic.com/docs/common/profile/lemur_lg.png";
        } else if (i == 7){
             return "https://ssl.gstatic.com/docs/common/profile/liger_lg.png";
        } else if (i == 8){
             return "https://ssl.gstatic.com/docs/common/profile/panda_lg.png";
        } else if (i == 9){
             return "https://ssl.gstatic.com/docs/common/profile/turtle_lg.png";
        } else {
             return "https://ssl.gstatic.com/docs/common/profile/dinosaur_lg.png";
        }
    }

    IEnumerator DownloadImage( string URL) {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(URL);
        yield return www.SendWebRequest();
        Texture2D myTexture2 = (Texture2D) DownloadHandlerTexture.GetContent(www);
        Sprite spritePFP = Sprite.Create(myTexture2, new Rect(0, 0, myTexture2.width, myTexture2.height), new Vector2(0.5f, 0.5f), 32);
        yield return spritePFP;
    }

    // Error

    void OnError(PlayFabError error){
        Debug.Log("Error " + error.ErrorMessage);
    }

}




