using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GamesScript : MonoBehaviour
{
    GameManagerMainMenu gameManagerMainMenu;
    private void Start() {
        gameManagerMainMenu = GameObject.Find("GameManager").GetComponent<GameManagerMainMenu>();
    }
    public void StartQuiz() {
        // gameManagerMainMenu.loadingUI.Prepare();
        TextMeshProUGUI tokenGobj = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        PlayerPrefs.SetString("tokenSelected", tokenGobj.text);
        gameManagerMainMenu.progressBarGameObject = Resources.Load<GameObject>("DownloadPopup");
        gameManagerMainMenu.progressBarGameObjectClone = Instantiate(gameManagerMainMenu.progressBarGameObject);
        gameManagerMainMenu.url = "https://dev.unimasoft.id/edugator/api/getDataGame/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetString("tokenSelected");
        StartCoroutine(gameManagerMainMenu.StartQuiz());
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
