using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAnimScript : MonoBehaviour
{
    Transform table;
    public void activateGameObject()
    {
        table = GetComponent<Transform>();
        
        print(table.childCount);
        StartCoroutine(FindGameObject());
    }

    private IEnumerator FindGameObject()
    {
        for(int i = 0; i < table.childCount; i++)
        {
            table.transform.GetChild(i).gameObject.SetActive(true);

            // table.gameObject.SetActive(true);
            print(i);
            yield return new WaitForSeconds(0.225f);
        }
        
    }
    
}
