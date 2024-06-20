using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserScript : MonoBehaviour
{
    private Users users;
    // [SerializeField] private GameObject cardHistory;
    // [SerializeField] private GameObject gameHistory;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform cardList;
    [SerializeField] private RectTransform gameList;
    [SerializeField] private ContentSizeFitter contentSizeFitterCardList;
    [SerializeField] private ContentSizeFitter contentSizeFitterGameList;
    
    private void Start() {
        scrollRect.content = cardList;
    }
    public void CardListOnClick() {
        scrollRect.content = cardList;
    }
    public void GameListOnClick() {
        scrollRect.content = gameList;
    }
}
