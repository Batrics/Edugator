using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardScoreHistoryscript : MonoBehaviour
{
    public GameObject scoreList;
    public TMP_Text cardName;

    private void OnEnable() {
        UpdateCardScoreHistory();
    }
    private void UpdateCardScoreHistory() {
        for(int i = 0; i < scoreList.transform.childCount; i++) {
            GameObject ScoreGo = scoreList.transform.GetChild(i).gameObject;
            TMP_Text name = ScoreGo.transform.GetChild(0).GetComponent<TMP_Text>();

            name.text = cardName.text;
        }
    }
}
