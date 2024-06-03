using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

public class LoginScript : MonoBehaviour
{
    public Users users;
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TMP_Text name1;
    [SerializeField] private TMP_Text email1;
    [SerializeField] private TMP_Text name2;
    [SerializeField] private TMP_Text email2;
    [SerializeField] private GameObject guesUI;
    [SerializeField] private GameManagerMainMenu gameManagerMainMenu;
    public GameObject loginFailed;
    // public GameObject loginUI;
    void Start(){
        PlayerPrefs.SetString("Json_Users", "{\"success\":true,\"data\":[{\"username\":\"Bagas\",\"password\":\"123\",\"email\":\"example@gmail.com\",\"id\":13},{\"username\":\"sando\",\"password\":\"123\",\"email\":\"masandofami@gmail.com\",\"id\":13}]}");
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
                        gameObject.SetActive(false);
                        guesUI.SetActive(false);
                        User(i);
                        gameManagerMainMenu.RefreshUserAccountBtn();
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

    public void User(int i) {
        print("User : " + i);
        name1.text = users.data[i].username;
        email1.text = users.data[i].email;
        name2.text = users.data[i].username;
        email2.text = users.data[i].email;
    }
}
