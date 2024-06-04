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
    [SerializeField] private ContentSizeFitter contentSizeFitterCardList;
    [SerializeField] private ContentSizeFitter contentSizeFitterGameList;
    
    private void Start() {
        scrollRect.content = cardList;
        
        if(cardList.childCount >= 6)
            contentSizeFitterCardList.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        else
            contentSizeFitterCardList.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        if(gameList.childCount >= 5)
            contentSizeFitterGameList.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        else
            contentSizeFitterGameList.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
    }
    public void CardListOnClick() {
        scrollRect.content = cardList;
        if(cardList.childCount >= 6)
            contentSizeFitterCardList.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        else
            contentSizeFitterCardList.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
    }
    public void GameListOnClick() {
        scrollRect.content = gameList;
        if(gameList.childCount >= 5)
            contentSizeFitterGameList.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        else
            contentSizeFitterGameList.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
    }
    
}
