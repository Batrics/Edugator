using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserScript : MonoBehaviour
{
    private Users users;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform cardList;
    [SerializeField] private RectTransform gameList;
    
    private void Start() {
        print(scrollRect.content.GetType());
    }
    public void CardListOnClick() {
        scrollRect.content = cardList;
    }
    public void GameListOnClick() {
        scrollRect.content = gameList;
    }
    
}
