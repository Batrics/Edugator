using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    [SerializeField] GameObject playBtnAnim;
    [SerializeField] GameObject historyBtnAnim;
    [SerializeField] GameObject settingBtnAnim;
    [SerializeField] GameObject quitBtnAnim;
    [SerializeField] GameObject HistoryUI;
    [SerializeField] Animator panelHistoryUIAnim;
    [SerializeField] Animator brightnesAnim;

    private void Awake() {
        // playBtnAnim = HistoryUI.transform.parent.GetChild(4).GetComponent<GameObject>().gameObject;
    }

    private void Start() {
        StartCoroutine(ActivateAnimation());
    }

    private IEnumerator ActivateAnimation() {
        //Wait For Transition
        yield return new WaitForSeconds(0.8f);

        //Activate Animation
        playBtnAnim.SetActive(true);
        yield return new WaitForSeconds(0.175f);
        
        historyBtnAnim.SetActive(true);
        yield return new WaitForSeconds(0.175f);
        
        settingBtnAnim.SetActive(true);
        yield return new WaitForSeconds(0.175f);
        
        quitBtnAnim.SetActive(true);
        yield return new WaitForSeconds(0.175f);
    }

    private IEnumerator SlidingPanelAnimation() {
        panelHistoryUIAnim.SetBool("slideOut", true);
        brightnesAnim.SetBool("activateBrightnes",true);
        yield return new WaitForSeconds(0.5f);
        
        // Transform childHistoryUI;
        
        // for(int i = 0; i < HistoryUI.transform.GetChild(0).GetChild(4).GetChild(0).GetChild(0).childCount; i++) {
        //     childHistoryUI = HistoryUI.transform.GetChild(0).GetChild(4).GetChild(0).GetChild(0).GetChild(i);
        //     childHistoryUI.gameObject.SetActive(false);
        // }
        HistoryUI.SetActive(false);
    }
    public void ActivatePanelAnimation() {
        StartCoroutine(SlidingPanelAnimation());
    }
}
