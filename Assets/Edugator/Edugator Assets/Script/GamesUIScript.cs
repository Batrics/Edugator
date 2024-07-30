using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamesUIScript : MonoBehaviour
{
    public GameManagerMainMenu gameManagerMainMenu;
    private void OnEnable() {
        gameManagerMainMenu.RefreshHistory();
    }
}
