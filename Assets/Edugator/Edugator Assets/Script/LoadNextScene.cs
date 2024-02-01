using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextScene : MonoBehaviour
{
    // public Animator transition;
    // Ganti "SceneName" dengan nama scene yang ingin Anda tuju berikutnya
    public string nextSceneName;
    // Fungsi untuk memulai perpindahan ke scene berikutnya
    private IEnumerator NextScene() {
        // transition.SetBool("Start", true);
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(nextSceneName);
        // transition.SetBool("Start", false);
    }

    public void GoNextScene() {
        StartCoroutine(NextScene());
    }
}
