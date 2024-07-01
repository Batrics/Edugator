using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreHistoryScript : MonoBehaviour
{
    public List<Image> cardImages = new List<Image>();
    public List<string> cardNames = new List<string>();
    public List<GameObject> scoreGos = new List<GameObject>();
    public GameObject CardScoreHistory;
    private void OnEnable() {
        UpdateDataHistory();
    }
    private void UpdateDataHistory() {
        GameObject history = gameObject.transform.GetChild(0).GetChild(2).GetChild(0).GetChild(0).gameObject;
        List<double> averageScores = new List<double>();
        List<int> dontHaveCardId = new List<int>();

        void cardScoreListBtn(GameObject scoreGo) {
            TMP_Text cardName = CardScoreHistory.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
            Image cardImage = CardScoreHistory.transform.GetChild(0).GetChild(2).GetComponent<Image>();

            Image image = scoreGo.transform.GetChild(0).GetComponent<Image>();
            TMP_Text name = scoreGo.transform.GetChild(1).GetComponent<TMP_Text>();

            cardImage.sprite = image.sprite;
            cardName.text = name.text;

            CardScoreHistory.SetActive(true);
        }

        for(int i = 1; i < history.transform.childCount; i += 2) {
            List<int> scores = new List<int>();
            GameObject cardGame = history.transform.GetChild(i).gameObject;
            if(cardGame.transform.childCount > 0) {
                for(int j = 0; j < cardGame.transform.childCount; j++) {
                    GameObject scoreGo = cardGame.transform.GetChild(j).gameObject;
                    Button ScoreGoBtn = scoreGo.GetComponent<Button>();
                    Image cardImage = scoreGo.transform.GetChild(0).GetComponent<Image>();
                    TMP_Text cardName = scoreGo.transform.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text scoreValueString = scoreGo.transform.GetChild(2).GetComponent<TMP_Text>();

                    ScoreGoBtn.onClick.AddListener(delegate { cardScoreListBtn(scoreGo); } );
                    
                    int scoreValue = Int32.Parse(scoreValueString.text);
                    scoreGos.Add(scoreGo);
                    cardImages.Add(cardImage);
                    cardNames.Add(cardName.text);
                    scores.Add(scoreValue);
                }
                double averageScore = scores.Average();
                averageScores.Add(averageScore);
            }
            else {
                dontHaveCardId.Add(i - 3);
            }
        }
        
        int averageScoresIndex = 0;
        for(int i = 0; i < history.transform.childCount; i += 2) {
            GameObject mainHistory = history.transform.GetChild(i).gameObject;
            TMP_Text score = mainHistory.transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<TMP_Text>();
            Image averageScoreGo = mainHistory.transform.GetChild(0).GetChild(2).GetComponent<Image>();
            if(dontHaveCardId.Contains(averageScoresIndex)) {
                print("Dont Have Card");
                score.text = "0";
                averageScoreGo.fillAmount = 0;
            }
            else {
                score.text = averageScores[averageScoresIndex].ToString();
                averageScoreGo.fillAmount = (float)averageScores[averageScoresIndex] / 100;
                averageScoresIndex++;
            }
        }
        
    }
}
