using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public TextMeshProUGUI finalScoreText;

    MainDataJsonQuestions mainData;
    private string jsonstring;
    private string url;
    public int finalScore;
    private int indexQuestions;
    public string correctAnswer;
    public int score;
    public GameObject transitionQuestion;
    public GameObject connection;
    public AudioSource scoreSFX;
    LoadingUI loadingUI = new LoadingUI();

    private void Awake() {
        loadingUI.Prepare();
    }
    void Start() {
        indexQuestions = 0;
        // PlayerPrefs.SetInt("game_id", 13);
        // PlayerPrefs.SetInt("number_of_card", 41);
        StartCoroutine(GetDataQuestions());
    }

    private IEnumerator GetDataQuestions() {
        url = "https://dev.unimasoft.id/edugator/api/getquestions/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetInt("game_id") + "/" + PlayerPrefs.GetString("number_of_card");
        
        yield return StartCoroutine(CalculatingQuestionsFromAPI());
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

    private void CheckAnswer(string myAnswer) {
        if(myAnswer == correctAnswer) {
            finalScore += score;
            finalScoreText.text = finalScore.ToString();
            PlayerPrefs.SetString("finalScore", finalScoreText.text);
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
            soal.text = mainData.data[index].question;
            pilihanJawaban1.text = mainData.data[index].option1;
            pilihanJawaban2.text = mainData.data[index].option2;
            pilihanJawaban3.text = mainData.data[index].option3;
            correctAnswer = mainData.data[index].answer;
            score = mainData.data[index].score;
        }
        else {
            soalGameObject.SetActive(false);
            scoreSFX.Play();
            resultGameObject.SetActive(true);
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
                    
                    jsonstring = webData.downloadHandler.text;
                    PlayerPrefs.SetString("jsonDataQuestions", jsonstring);
                    mainData = JsonUtility.FromJson<MainDataJsonQuestions>(PlayerPrefs.GetString("jsonDataQuestions"));

                    if(mainData == null) {
                        Debug.Log("Json data Kosong");
                    }
                    else {
                        for(int j = 0; j < mainData.data.Length; j++) {
                            int i = 0;
                            int total_soal_setiap_kartu = 0;
                            while (i < mainData.data.Length) {
                                total_soal_setiap_kartu++;
                                PlayerPrefs.SetInt("total_soal_setiap_kartu", total_soal_setiap_kartu);
                                i++;
                            }
                        }
                        loadQuestion(indexQuestions);
                    }
                }
                else {
                    Debug.LogError("Error Detail: " + webData.error);
                }
            }
        }
    }
}
