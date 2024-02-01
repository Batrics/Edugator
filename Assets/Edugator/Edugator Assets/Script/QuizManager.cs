using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using TMPro;

using Loading.UI;

public class QuizManager : MonoBehaviour
{
    public GameObject soalGameObject;
    public GameObject resultGameObject;
    public TextMeshProUGUI soal;
    public TextMeshProUGUI pilihanJawaban1;
    public TextMeshProUGUI pilihanJawaban2;
    public TextMeshProUGUI pilihanJawaban3;
    public TextMeshProUGUI nilaiText;

    private JSONNode _jsonData;
    private string url;
    public int nilai;
    private int indexQuestions;
    public string jawabanBenar;
    public int bobotSoal;
    public GameObject transitionQuestion;
    public GameObject connection;
    public AudioSource scoreSFX;
    LoadingUI loadingUI = new LoadingUI();

    private void Awake() {
        loadingUI.Prepare();
        PlayerPrefs.SetString("token", "44736ebf1ac169b4d5e7d174ca1f8b8e");
    }
    void Start() {
        indexQuestions = 0;
        // print("Link : " + url);
        StartCoroutine(switchLink());
    }

    private IEnumerator switchLink() {
        url = "https://dev.unimasoft.id/edugator/api/getDataGames/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetString("token"); // + PlayerPref.GetString("Token");
        
        yield return StartCoroutine(CalculatingQuestionsFromAPI());
        
        url = "https://dev.unimasoft.id/edugator/api/getquestions/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetInt("game_id") + "/" + PlayerPrefs.GetInt("number_of_card");

        StartCoroutine(GetDataFromAPI());
    }
    private IEnumerator NextQuestionsFunc() {
        indexQuestions++;
        transitionQuestion.SetActive(true);
        yield return new WaitForSeconds(1f);
        loadQuestion(indexQuestions);
        yield return new WaitForSeconds(1.2f);
        transitionQuestion.SetActive(false);
    }

    public void NextQuestions() {
        StartCoroutine(NextQuestionsFunc());
    }

    private void CheckAnswer(string jawabanSaya) {
        if(jawabanSaya == jawabanBenar) {
            nilai += bobotSoal;
            nilaiText.text = nilai.ToString();
        }
    }

    public void answerA() {
        CheckAnswer("1");
    }
    public void answerB() {
        CheckAnswer("2");
    }
    public void answerC() {
        CheckAnswer("3");
    }

    public void loadQuestion(int index) {
        if (index < PlayerPrefs.GetInt("total_soal_setiap_kartu")) {
            soal.text = _jsonData["data"][index]["question"];
            pilihanJawaban1.text = _jsonData["data"][index]["option1"];
            pilihanJawaban2.text = _jsonData["data"][index]["option2"];
            pilihanJawaban3.text = _jsonData["data"][index]["option3"];
            jawabanBenar = _jsonData["data"][index]["answer"];
            bobotSoal = _jsonData["data"][index]["score"];
        }
        else {
            soalGameObject.SetActive(false);
            scoreSFX.Play();
            resultGameObject.SetActive(true);
        }
    }

    private IEnumerator GetDataFromAPI() {
        using(UnityWebRequest webData = UnityWebRequest.Get(url)) {
            // loadingUI.Show("Please Wait...");
            loadingUI.Show("Please Wait...");
            yield return webData.SendWebRequest();
            if(webData.result == UnityWebRequest.Result.ConnectionError || webData.result == UnityWebRequest.Result.ProtocolError) {
                // loadingUI.Hide();
                loadingUI.Hide();
                Debug.Log("tidak ada Koneksi/Jaringan");
            }
            else {
                if(webData.isDone) {
                    loadingUI.Hide();
                    loadingUI.Hide();
                    _jsonData = JSON.Parse(System.Text.Encoding.UTF8.GetString(webData.downloadHandler.data));
                    if(_jsonData == null) {
                        Debug.Log("Json data Kosong");
                    }
                    else {
                        print("Card id : " + PlayerPrefs.GetInt("number_of_card"));
                        print(PlayerPrefs.GetString("token"));
                        loadQuestion(indexQuestions);
                    }
                }
            }
        }
    }

    // MEnghitung Jumlah Soal
    // =========================================================================================================================================================
    public void Refresh() {
        StartCoroutine(CalculatingQuestionsFromAPI());
    }

    private IEnumerator CalculatingQuestionsFromAPI() {
        using(UnityWebRequest webData = UnityWebRequest.Get(url)) {
            // loadingUI.Show("Please Wait...");
            loadingUI.Show("Please Wait...");
            yield return webData.SendWebRequest();

            if(webData.result == UnityWebRequest.Result.ConnectionError || webData.result == UnityWebRequest.Result.ProtocolError) {
                // loadingUI.Hide();
                loadingUI.Hide();
                Debug.Log("tidak ada Koneksi/Jaringan");
                connection.SetActive(true);
                yield return new WaitForSeconds(3f);
                connection.SetActive(false);
            }
            else {
                if(webData.isDone) {
                    // loadingUI.Hide();
                    loadingUI.Hide();
                    _jsonData = JSON.Parse(System.Text.Encoding.UTF8.GetString(webData.downloadHandler.data));
                    if(_jsonData == null) {
                        Debug.Log("Json data Kosong");
                    }
                    else {
                        for(int j = 0; j < _jsonData["data"].Count; j++) {
                            if (PlayerPrefs.GetString("token") == _jsonData["data"][j]["token"]) {
                                Debug.Log("token benar");
                                int i = 0;
                                int total_soal_setiap_kartu = 0;
                                while (i < _jsonData["data"].Count) {
                                    if (_jsonData["data"][j]["cards"][i]["id"] == PlayerPrefs.GetInt("number_of_card")) {
                                        total_soal_setiap_kartu++;
                                        PlayerPrefs.SetInt("total_soal_setiap_kartu", total_soal_setiap_kartu);
                                    }
                                    i++;
                                }
                            }
                        }


                    }
                }
                else {
                    Debug.LogError("Error Detail: " + webData.error);
                }
            }
        }
    }
}
