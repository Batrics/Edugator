using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

public class LoginScript : MonoBehaviour
{
    private Users users;
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private GameObject loginFailed;
    void Start(){
        PlayerPrefs.SetString("Json_Users", "{\"success\":true,\"data\":[{\"username\":\"bagas\",\"password\":\"123\",\"id\":13},{\"username\":\"sando\",\"password\":\"123\",\"id\":13}]}");
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
                foreach(var user in users.data) {
                    if(username.text == user.username) {
                        if(password.text == user.password) {
                            gameObject.SetActive(false);
                            yield return null;
                        }
                        else {
                            loginFailed.SetActive(true);
                        }
                    }
                    else {
                        loginFailed.SetActive(true);
                    }
                }
            }
        }
    }
}
