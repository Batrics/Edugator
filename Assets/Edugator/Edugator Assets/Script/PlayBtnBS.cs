using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayBtnBS : MonoBehaviour
{
    public AudioSource butttonAudio;
    public AudioSource buttonSE;
    AudioClip[] audioClips;
    public TransitionScript transitionScript;
    public LoadNextScene loadNextScene;
    private void Start() {
        audioClips = Resources.LoadAll<AudioClip>("Music");
    }

    public IEnumerator QuizAudio() {
        int rnd = Random.Range(0, 9);
        butttonAudio.clip = audioClips[rnd];
        buttonSE.Play();
        yield return new WaitForSeconds(0.5f);
        butttonAudio.Play();
        yield return new WaitForSeconds(2);
        transitionScript.startTransitionActive();
        loadNextScene.GoNextScene();
    }

    public void Play() {
        StartCoroutine(QuizAudio());
    }
}
