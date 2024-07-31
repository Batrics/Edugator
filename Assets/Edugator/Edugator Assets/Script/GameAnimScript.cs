using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAnimScript : MonoBehaviour
{
    public Transform table;
    private void OnEnable() {
        print("ASD " + table.childCount);
        StartCoroutine(FindGameObject());
    }
    private void OnDisable() {
        for(int i = 0; i < table.childCount; i++) {
            table.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    private IEnumerator FindGameObject() {
        for(int i = table.childCount-1; i >= 0; i--) {
            yield return new WaitForSeconds(0.25f);
            GameObject game = table.transform.GetChild(i).gameObject;
            game.SetActive(true);
            print("OKEHH" + game + i);
        }
    }
    
}
