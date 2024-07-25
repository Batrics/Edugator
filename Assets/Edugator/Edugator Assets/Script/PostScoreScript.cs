using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PostScoreScript : MonoBehaviour
{
    private void Start() {
        StartCoroutine(SendPostRequest(PlayerPrefs.GetString("finalScore")));
    }
    private void OnEnable() {
        StartCoroutine(SendPostRequest(PlayerPrefs.GetString("finalScore")));
    }
    private IEnumerator SendPostRequest(string data)
    {
        // URL yang akan dituju
        string url = "https://dev.unimasoft.id/edugator/api/getquestions/a49fdc824fe7c4ac29ed8c7b460d7338/13/41";

        // Data yang akan dikirim (contoh data form)
        WWWForm form = new WWWForm();
        form.AddField("final_score", data);

        // Membuat UnityWebRequest dengan metode POST
        UnityWebRequest www = UnityWebRequest.Post(url, form);

        // Mengirim permintaan dan menunggu hingga selesai
        yield return www.SendWebRequest();

        // Memeriksa jika ada kesalahan
        if (www.result != UnityWebRequest.Result.Success){
            Debug.Log("Error: " + www.error);
        }
        else {
            Debug.Log("Response: " + www.downloadHandler.text);
        }
    }
}
