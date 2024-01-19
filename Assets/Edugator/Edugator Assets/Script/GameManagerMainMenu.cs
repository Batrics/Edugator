using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using TMPro;

using UnityEngine.Audio;
using Loading.UI;
using System.IO;


public class GameManagerMainMenu : MonoBehaviour
{
    private JSONNode _jsonData;
    private string url;
    public TMP_InputField inputToken;
    public TextMeshProUGUI info;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI GameOwnerText;
    public Animator VFXAnim;
    public LoadNextScene loadNextScene;
    public TransitionScript transitionScript;
    public AudioMixer audioMixer;
    private int checkVisualEffectInt = 1;
    public TextMeshProUGUI historyBtn;
    public Transform panel;

    [Header("GameObject")]
    public GameObject inputTokenUI;
    public GameObject visualEffectToggle;
    public GameObject startTransitionObject;
    public Transform table;
    public GameObject historyValue;
    
    [Space]
    LoadingUI loadingUI = new LoadingUI();
    //Main Menu Script
    //==============================================================================================================================//\
    private void Awake()
    {
        loadingUI.Prepare();
    }
    void Start()
    {
        // PlayerPrefs.DeleteAll();
        print(PlayerPrefs.GetString("history"));
        PlayerPrefs.DeleteKey("token");
        historyBtn.text = "History";
        Debug.Log("Value : " + PlayerPrefs.GetString("token"));
        url = "https://dev.unimasoft.id/edugator/api/getAllGames/a49fdc824fe7c4ac29ed8c7b460d7338";
    }
    public void StartGame()
    {
        StartCoroutine(StartQuiz());
    }
    public IEnumerator StartQuiz()
    {
        // loadingUI.Show("Please Wait...");
        loadingUI.Show("Please Wait...");
        yield return StartCoroutine(GetDataFromAPI());
        // yield return StartCoroutine(DownloadingModel());
        refreshHistory();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void Playbtn()
    {
        if(PlayerPrefs.GetString("token") == "")
        {
            inputTokenUI.SetActive(true);
            // Instantiate(inputTokenUI, panel);
        }
        else
        {
            inputTokenUI.SetActive(false);
            // Destroy(inputTokenUI);
            transitionScript.startTransitionActive();
            loadNextScene.GoNextScene();
        }
    }

    public void AddGame()
    {
        inputTokenUI.SetActive(true);
        inputToken.text = "";
        info.text = "";
    }
    // public void DestroyGame()
    // {
    //     Destroy(inputTokenUI);
    // }

    public void CheckVisualEffect(bool checkVisualEffect)
    {
        UpdateToggle(checkVisualEffect);

        PlayerPrefs.SetInt("visualEffect", checkVisualEffectInt);
        Debug.Log("Bool : " + checkVisualEffectInt);
    }

    public void RefreshToggle()
    {
        int visualEffect = PlayerPrefs.GetInt("visualEffect");
        print(visualEffect);

        if (visualEffect == 1)
        {
            UpdateToggle(true);
        }
        else
        {
            UpdateToggle(false);
        }
    }

    public void UpdateToggle(bool is_on)
    {
        if (is_on)
        {
            checkVisualEffectInt = 1;
            VFXAnim.SetBool("toggle", true);
        }
        else
        {
            checkVisualEffectInt = 0;
            VFXAnim.SetBool("toggle", false);
        }
    }
    private IEnumerator GetDataFromAPI()
    {
        using(UnityWebRequest webData = UnityWebRequest.Get(url))
        {
            webData.SendWebRequest();

            while(!webData.isDone)
            {
                yield return null;
            }

            if(webData.result == UnityWebRequest.Result.ConnectionError || webData.result == UnityWebRequest.Result.ProtocolError)
            {
                // loadingUI.Hide();
                loadingUI.Hide();
                Debug.Log("tidak ada Koneksi/Jaringan"); 
                info.text = "tidak ada Koneksi/Jaringan";
            }
            else
            {
                if(webData.isDone)
                {
                    // loadingUI.Hide();
                    loadingUI.Hide();
                    _jsonData = JSON.Parse(System.Text.Encoding.UTF8.GetString(webData.downloadHandler.data));
                    if(_jsonData == null)
                    {
                        Debug.Log("Json data Kosong");
                    }
                    else
                    {
                        for (int i = 0; i < _jsonData["data"].Count; i++)
                        {

                            if (inputToken.text == _jsonData["data"][i]["token"])
                            {
                                PlayerPrefs.SetString("token", inputToken.text);
                                PlayerPrefs.SetInt("game_id", _jsonData["data"][i]["id"]);
                                titleText.text = _jsonData["data"][i]["nama"];
                                GameOwnerText.text = "Created By : " + _jsonData["data"][i]["pembuat"];
                                print(PlayerPrefs.GetInt("game_id"));
                                // Destroy(inputTokenUI);
                                inputTokenUI.SetActive(false);

                                print("data : " + _jsonData["data"]);

                                // AddCardInArray(i);

                                //Update History Data
                                string storage = PlayerPrefs.GetString("history");

                                if(storage != "")
                                {
                                    List<string> allHistory = new List<string>(storage.Split(";"));

                                    bool ketemu = false;
                                    foreach(string history in allHistory)
                                    {
                                        string[] historyArr = history.Split(",");

                                        if(inputToken.text == historyArr[0])
                                        {
                                            ketemu = true;
                                            break;
                                        }
                                    }

                                    if(!ketemu)
                                    {
                                        allHistory.Insert(0, inputToken.text + "," + _jsonData["data"][i]["nama"]);
                                        storage = string.Join(";", allHistory);
                                        PlayerPrefs.SetString("history", storage);
                                    }
                                }
                                else
                                {
                                    PlayerPrefs.SetString("history", inputToken.text + "," + _jsonData["data"][i]["nama"]);
                                }     
                                historyBtn.text = "New Game";

                            }
                            else if (inputToken.text == "")
                            {
                                info.text = "Masukkan Token";
                            }
                            else
                            {
                                info.text = "Token anda salah";
                            }

                        }
                    }
                }
                else
                {
                    Debug.LogError("Error Detail: " + webData.error);
                }
            }
        }
    }

    public void refreshHistory()
    {
        for(int i = 0; i < table.childCount; i++)
        {
            Destroy(table.transform.GetChild(i).gameObject);
        }
        string storage = PlayerPrefs.GetString("history");
        if(storage != "")
        {
            List<string> allHistory = new List<string>(storage.Split(";"));

            foreach(string history in allHistory)
            {
                string[] historyArr = history.Split(",");

                string token = historyArr[0];
                string name = historyArr[1];

                Instantiate(historyValue, table);

                GameObject titleGame = historyValue.transform.GetChild(0).gameObject;
                GameObject tokenGame = historyValue.transform.GetChild(1).gameObject;
                TextMeshProUGUI titleGameText = titleGame.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI tokenGameText = tokenGame.GetComponent<TextMeshProUGUI>();
                titleGameText.text = name;
                tokenGameText.text = token;
            }
        }
    }


    //==============================================================================================================================//

    //Download Model
    //==============================================================================================================================//
    private string fbxUrl;
    private string savePath;
    private List<int> cardIdList = new List<int>();
    private List<string> cardName = new List<string>();
    private int cardIndex;

    private void AddCardInArray(int i)
    {
        for(int j = 0; j < _jsonData["data"][i]["card"].Count; j++)
        {
            cardIdList.Add(_jsonData["data"][i]["card"][j]["id"]);
            cardName.Add(_jsonData["data"][i]["card"][j]["nama"]);
            print("card id " + j + " = " + cardIdList[j]);
            print("card name " + j + " = " + cardName[j]);
        }
    }

    // private   IEnumerator DownloadModel()
    // {
    //     Progress.Show("Downloading...", ProgressColor.Default);
    //     UnityWebRequest www = UnityWebRequest.Get(fbxUrl);
    //     yield return www.SendWebRequest();

    //     if (www.result != UnityWebRequest.Result.Success)
    //     {
    //         loadingUI.Hide();
    //         Debug.LogError("Gagal mengunduh file: " + www.error);
    //     }
    //     else
    //     {
    //         byte[] data = www.downloadHandler.data;

    //         // Simpan data ke dalam file di folder "Assets".
    //         File.WriteAllBytes(savePath, data);
            
    //         UnityEditor.AssetDatabase.Refresh();
    //         loadingUI.Hide();
    //         Debug.Log("File berhasil diunduh dan disimpan di " + savePath);
    //     }
    // }

    // private IEnumerator DownloadingModel()
    // {
    //     for(int i = 0; i < cardIdList.Count; i++)
    //     {
    //         fbxUrl = "https://dev.unimasoft.id/edugator/api/downloadModel/a49fdc824fe7c4ac29ed8c7b460d7338/" + cardIdList[cardIndex];
            
    //         savePath = $"Assets/3D Object/3D/{cardName[cardIndex]}.fbx";

    //         Progress.Show("Downloading...", ProgressColor.Default);
    //         yield return StartCoroutine(DownloadModel());
    //         cardIndex++;

    //         print("download 3d ke-" + i);
    //     }
    //     loadingUI.Hide();
    // }

    //==============================================================================================================================//
    
    //Music
    //==============================================================================================================================//
    public void SetVoume(float volume)
    {
        Debug.Log(volume);
        if(volume <= -20f && volume >= -30)
        {
            volume = -80f;
        }
        else if(volume > -80 && volume <= -70)
        {
            volume = -20f;
        }

        audioMixer.SetFloat("volume", volume);
    }

    public void SetVFX(float volume)
    {
        Debug.Log(volume);
        if(volume <= -20f && volume >= -30)
        {
            volume = -80f;
        }
        else if(volume > -80 && volume <= -70)
        {
            volume = -20f;
        }

        audioMixer.SetFloat("sfx", volume);
    }
    //==============================================================================================================================//
}
