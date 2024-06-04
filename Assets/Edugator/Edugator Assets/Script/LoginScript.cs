using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using System.Net;
using UnityEngine.Networking;
using Loading.UI;

public class LoginScript : MonoBehaviour
{
    public Users users;
    public AllGamesJson allGames;
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TMP_Text name1;
    [SerializeField] private TMP_Text email1;
    [SerializeField] private TMP_Text name2;
    [SerializeField] private TMP_Text email2;
    [SerializeField] private GameObject guesUI;
    [SerializeField] private GameManagerMainMenu gameManagerMainMenu;
    LoadingUI loadingUI;
    public GameObject loginFailed;
    // public GameObject loginUI;
    void Start(){
        PlayerPrefs.SetString("Json_Users", "{\"success\":true,\"data\":[{\"id\":1,\"username\":\"Bagas\",\"password\":\"123\",\"email\":\"example@gmail.com\",\"user_id\":8},{\"id\":2,\"username\":\"AnakPandai\",\"password\":\"SehatSehatCees\",\"email\":\"sayasiap432@gmail.com\",\"user_id\":16}]}");
        loadingUI = gameManagerMainMenu.loadingUI;
        // loadingUI.Prepare();
    }
    public void loginBtn() => StartCoroutine(Login_Coroutine());
    private IEnumerator Login_Coroutine() {
        yield return null;
        users = JsonUtility.FromJson<Users>(PlayerPrefs.GetString("Json_Users"));
        if(users == null){
            print("Json data null");
        }
        else {
            if(users.success == true){
                for(int i = 0; i < users.data.Length; i++) {
                    if(username.text == users.data[i].username && password.text == users.data[i].password) {
                        PlayerPrefs.SetString("Login_State", "success");
                        PlayerPrefs.SetString("username", username.text);
                        PlayerPrefs.SetString("password", password.text);
                        PlayerPrefs.SetInt("user_id", users.data[i].user_id);
                        MainUserInfo(i);
                        // yield return StartCoroutine(User_Coroutine());
                        StartCoroutine(User_Coroutine());
                        gameManagerMainMenu.RefreshUserAccountBtn();
                        guesUI.SetActive(false);
                        gameObject.SetActive(false);
                        yield return null;
                    }
                    else {
                        PlayerPrefs.SetString("Login_State", "failed");
                        loginFailed.SetActive(true);
                    }
                }
            }
        }
    }
    public void MainUserInfo(int i) {
        name1.text = users.data[i].username;
        email1.text = users.data[i].email;
        name2.text = users.data[i].username;
        email2.text = users.data[i].email;
    }

    // public void User() => StartCoroutine(User_Coroutine());
    public IEnumerator User_Coroutine() {
        string getAllGames = "https://dev.unimasoft.id/edugator/api/getAllGames/a49fdc824fe7c4ac29ed8c7b460d7338";
        using(UnityWebRequest webData = UnityWebRequest.Get(getAllGames)) {
            // loadingUI.Show("Please Wait...");
            yield return webData.SendWebRequest();

            if(webData.result == UnityWebRequest.Result.ConnectionError || webData.result == UnityWebRequest.Result.ProtocolError) {
                loadingUI.Hide();
                StartCoroutine(gameManagerMainMenu.NoConnection());
                Debug.Log("tidak ada Koneksi/Jaringan"); 
                
            }
            else {
                if(webData.isDone) {
                    loadingUI.Hide();
                    string jsonString = webData.downloadHandler.text;
                    PlayerPrefs.SetString("AllGames", jsonString);
                    allGames = JsonUtility.FromJson<AllGamesJson>(PlayerPrefs.GetString("AllGames"));
                    users = JsonUtility.FromJson<Users>(PlayerPrefs.GetString("Json_Users"));

                    if(allGames == null) {
                        Debug.Log("Data Json null");
                    }
                    else {
                        for(int i = 0; i < allGames.data.Length; i++) {
                            if(allGames.data[i].user_id == PlayerPrefs.GetInt("user_id")){
                                foreach(var card in allGames.data[i].cards){

                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
