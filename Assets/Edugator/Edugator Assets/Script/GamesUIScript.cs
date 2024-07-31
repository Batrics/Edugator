using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamesUIScript : MonoBehaviour
{
    public GameManagerMainMenu gameManagerMainMenu;
    public Transform table;
    private void OnEnable() {
        RefreshGames();
    }
    public void RefreshGames() => StartCoroutine(RefreshGames_Coroutine());
    private IEnumerator RefreshGames_Coroutine() {
        gameManagerMainMenu.RefreshHistory();
        yield return null;
        for(int i = 0; i < table.childCount; i++) {
            GameObject game = table.transform.GetChild(i).gameObject;
            game.SetActive(false);
        }
        StartCoroutine(FindGameObject());
    }
    private IEnumerator FindGameObject() {
        for(int i = table.childCount-1; i >= 0; i--) {
            GameObject game = table.transform.GetChild(i).gameObject;
            game.SetActive(true);
            print("OKEHH" + game + i);
            yield return new WaitForSeconds(0.15f);
        }
    }
}
