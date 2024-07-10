using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamesUIScript : MonoBehaviour
{
    GameManagerMainMenu gameManagerMainMenu;
    private void Start() {
        gameManagerMainMenu = GameObject.Find("GameManager").GetComponent<GameManagerMainMenu>();
    }
    private void OnEnable() {
        gameManagerMainMenu.RefreshHistory();
    }
}
