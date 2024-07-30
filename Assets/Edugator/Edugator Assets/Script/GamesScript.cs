using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GamesScript : MonoBehaviour
{
    GameManagerMainMenu gameManagerMainMenu;
    GameObject gameSelected;
    Transform panelGamesUI;
    private void Start() {
        gameManagerMainMenu = GameObject.Find("GameManager").GetComponent<GameManagerMainMenu>();
        panelGamesUI = GameObject.Find("Panel Games UI").GetComponent<Transform>();
    }

    public void StartGame() => StartCoroutine(Game());
    public IEnumerator Game() {
        // gameManagerMainMenu.loadingUI.Prepare();
        TextMeshProUGUI tokenGobj = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        PlayerPrefs.SetString("tokenSelected", tokenGobj.text);
        gameManagerMainMenu.progressBarGameObject = Resources.Load<GameObject>("DownloadPopup");
        gameManagerMainMenu.progressBarGameObjectClone = Instantiate(gameManagerMainMenu.progressBarGameObject);
        gameManagerMainMenu.url = "https://dev.unimasoft.id/edugator/api/getDataGame/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetString("tokenSelected");
        yield return StartCoroutine(gameManagerMainMenu.StartQuiz());
        GameObject gamesUI = GameObject.Find("Games UI");
        gamesUI.SetActive(false);
    }

    public void SetGame(GameObject game) {
        gameSelected = game;
        TextMeshProUGUI gameToken = game.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        PlayerPrefs.SetString("tokenSelected", gameToken.text);
        print("Game Selected : " + gameSelected);
    }
    public void DeleteGame(GameObject game) {
        SetGame(game);
        GameObject popup = Resources.Load<GameObject>("Popup");
        GameObject popupClone = Instantiate(popup, panelGamesUI);
        popupClone.SetActive(true);
    }
    
    //Animation
    //==============================================================================================================================//
    private IEnumerator SlidingAnimation(Animator action) {
        action.SetBool("activateSlide", true);
        yield return new WaitForSeconds(0.25f);
        action.SetBool("activateSlide", false);
    }

    public void ActivateAnimationSliding(Animator action) {
        StartCoroutine(SlidingAnimation(action));
    }
    //==============================================================================================================================//
}
