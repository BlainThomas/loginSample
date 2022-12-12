using System.ComponentModel;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EmailFactory : MonoBehaviour
{
    
    void Awake() {
        GameManager.Instance.emailFactory = this;
    }

    public void SendEmailDeleteAccount() {
        SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
        client.Credentials = new System.Net.NetworkCredential(
            "badbilliardservices@gmail.com",
            "tjsjbphgaxqwpngq");
        client.EnableSsl = true;
        MailAddress from = new MailAddress(
            "badbilliardservices@gmail.com",
            "Billiard Services",
            System.Text.Encoding.UTF8);
        MailAddress to = new MailAddress("blainthomas12@gmail.com");
        MailMessage message = new MailMessage(from, to);

        message.Body = "There has been requested to delete an account for " + 
        PlayerPrefs.GetString("userName") + System.Environment.NewLine + 
        System.Environment.NewLine + System.Environment.NewLine + "User name " + 
        System.Environment.NewLine + PlayerPrefs.GetString("userName") + 
        System.Environment.NewLine + System.Environment.NewLine + 
        System.Environment.NewLine + "PlayfabID " + System.Environment.NewLine + 
        PlayerPrefs.GetString("playFabId");

        message.BodyEncoding = System.Text.Encoding.UTF8;
        message.Subject = "Delete account for " + PlayerPrefs.GetString("userName");
        message.SubjectEncoding = System.Text.Encoding.UTF8;
        client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
        string userState = "test message1";
        client.SendAsync(message, userState);
    }

    private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e) {
        string token = (string)e.UserState;
        if (SceneManager.GetActiveScene().name == "LoginScene")
            GameManager.Instance.loginSceneManager.messageCanvas.SetActive(true);
            
        if (e.Cancelled) {
            GameObject.Find("ErrorMessage").GetComponent<Text>().text = e.Cancelled.ToString();
            Debug.Log("Send canceled "+ token);
        } if (e.Error != null) {
            GameObject.Find("ErrorMessage").GetComponent<Text>().text = e.Error.ToString();
            Debug.Log("[ "+token+" ] " + " " + e.Error.ToString());
        } else {
            Debug.Log("Message sent.");
        }
    }
     
}