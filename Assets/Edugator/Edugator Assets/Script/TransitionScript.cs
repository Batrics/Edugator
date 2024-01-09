using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionScript : MonoBehaviour
{
    public GameObject startTransition;
    public GameObject endTransition;

    private void Start() 
    {
        StartCoroutine(endTransitionActive());
    }
    public void startTransitionActive()
    {
        startTransition.SetActive(true);
    }

    IEnumerator endTransitionActive()
    {
        endTransition.SetActive(transform);
        yield return new WaitForSeconds(1.5f);
    }
}
