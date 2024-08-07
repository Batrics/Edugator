using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupScript : MonoBehaviour
{
    public GamesScript gamesScript;
    public GameObject GamesUI;
    // Start is called before the first frame update
    void Start() {
        initializationObject();
        GamesUI = transform.parent.parent.gameObject;
        print("token Selectedd : " + PlayerPrefs.GetString("tokenSelected"));
    }
    
    public void initializationObject() {
        string allGames = PlayerPrefs.GetString("games");
        List<string> allGamesArr = new List<string>(allGames.Split(";"));

        for(int i = 0; i < allGamesArr.Count; i++) {
            string[] GameArr = allGamesArr[i].Split(",");

            if(PlayerPrefs.GetString("tokenSelected") == GameArr[0]) {
                // gamesScript = gameObject.transform.parent.GetChild(0).GetChild(4).GetChild(0).GetChild(0).GetChild(i).GetComponent<GamesScript>();
                // print("Games Script : " + gamesScript);
                // print("inedex : " + i);
            }
        }
    }

    public void DeleteGame() {
        StartCoroutine(DeleteGameWorkFlow());
    }

    public IEnumerator DeleteGameWorkFlow() {
        string allGames = PlayerPrefs.GetString("games");
        List<string> allGamesArr = new List<string>(allGames.Split(";"));

        for(int i = 0; i < allGamesArr.Count; i++) {
            string[] GameArr = allGamesArr[i].Split(",");

            if(PlayerPrefs.GetString("tokenSelected") == GameArr[0]) {
                yield return null;
                // yield return historyScript = gameObject.transform.parent.GetChild(0).GetChild(4).GetChild(i).GetComponent<HistoryScript>();
                // print("History Scripttttt ; " + historyScript.gameObject.name);

                allGamesArr.RemoveAt(i);
                allGames = string.Join(";", allGamesArr);
                PlayerPrefs.SetString("games", allGames);

                Debug.Log("Arr : " + allGames);
                GamesUI.SetActive(false);
            }
        }

        Destroy(this.gameObject);
        // gameObject.SetActive(false);
    }

    public void DestroyGameObject() {
        Destroy(this.gameObject);
    }
}
