using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayBtnBS : MonoBehaviour
{
    AudioSource butttonAudio;
    [SerializeField] private AudioSource buttonSE;
    AudioClip[] audioClips;
    public TransitionScript transitionScript;
    public LoadNextScene loadNextScene;
    private void Start() {
        butttonAudio = GetComponent<AudioSource>();
        audioClips = Resources.LoadAll<AudioClip>("Music");
    }
    public void Play() {
        StartCoroutine(QuizAudio());
    }

    private IEnumerator QuizAudio() {
        int rnd = Random.Range(0, 9);
        butttonAudio.clip = audioClips[rnd];
        buttonSE.Play();
        yield return new WaitForSeconds(0.1f);
        butttonAudio.Play();
        yield return new WaitForSeconds(1.5f);
        transitionScript.startTransitionActive();
        yield return new WaitForSeconds(1);
        loadNextScene.GoNextScene();
    }
}
