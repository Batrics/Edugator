using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using TMPro;
using System.IO;
using System.IO.Compression;
using UnityEngine.Audio;
using Loading.UI;
using System;
using Download.file;

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
    DownloadFile downloadFile = new DownloadFile();
    //Main Menu Script
    //==============================================================================================================================//\
    private void Awake()
    {
        // ExtractFile();
        // SetNormalMap();

        downloadFile.URLWithoutCardId = "https://dev.unimasoft.id/edugator/api/downloadModel/a49fdc824fe7c4ac29ed8c7b460d7338/";

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
        RefreshHistory();
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
                                titleText.text = _jsonData["data"][i]["name"];
                                GameOwnerText.text = "Created By : " + _jsonData["data"][i]["author"];
                                print(PlayerPrefs.GetInt("game_id"));
                                // Destroy(inputTokenUI);
                                inputTokenUI.SetActive(false);

                                print("data : " + _jsonData["data"]);

                                //Download Assets
                                loadingUI.Show("Download Assets...");
                                for(int j = 0; j < _jsonData["data"][i]["cards"].Count; j++)
                                {
                                    string cardName = _jsonData["data"][i]["cards"][j]["name"];
                                    int cardId = _jsonData["data"][i]["cards"][j]["id"];

                                    downloadFile.extentionFile = ".fbx";
                                    Debug.Log("card NAME : " + cardName);
                                    yield return StartCoroutine(downloadFile.Download(cardName, cardId));
                                }
                                loadingUI.Hide();

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
                                        allHistory.Insert(0, inputToken.text + "," + _jsonData["data"][i]["name"]);
                                        storage = string.Join(";", allHistory);
                                        PlayerPrefs.SetString("history", storage);
                                    }
                                }
                                else
                                {
                                    PlayerPrefs.SetString("history", inputToken.text + "," + _jsonData["data"][i]["name"]);
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

    public void RefreshHistory()
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
    
    //Extract File
    //==============================================================================================================================//

    private void ExtractFile()
    {
        string filePath = "Assets/Resources/3dObject/";

        string[] zipFiles = Directory.GetFiles(filePath, "*.zip");

        try
        {
            foreach(string zipFile in zipFiles)
            {
                print("ZIPFILE : " + zipFile);
                ZipFile.ExtractToDirectory(zipFile, filePath);
            }

            // Extract the contents of the zip file

            print("Zip file extracted successfully.");
        }
        catch (Exception ex)
        {
            print($"Error extracting zip file: {ex.Message}");
        }
    }

    private void SetNormalMap()
    {
        Texture2D[] textureFile = Resources.LoadAll<Texture2D>("3dObject");

        foreach(Texture2D texture in textureFile)
        {
            if(texture.name.Contains("_Normal") || texture.name.Contains("_normal"))
            {
                //...
            }
        }
    }

    //==============================================================================================================================//
}

namespace Download.file
{
    public class DownloadFile : MonoBehaviour
    {
        // private JSONNode _jsonData;

        /// <summary>
        /// URL for edugator = https://dev.unimasoft.id/edugator/api/downloadModel/a49fdc824fe7c4ac29ed8c7b460d7338/
        /// </summary>

        public string URLWithoutCardId;
        public string extentionFile;
        private string savePath;
        private string downloadFileURL;
        // private List<int> cardIdList = new List<int>();
        // private List<string> cardName = new List<string>();
        // private int cardIndex;

        // private void AddCardInArray(int i)
        // {
        //     for(int j = 0; j < _jsonData["data"][i]["cards"].Count; j++)
        //     {
        //         cardIdList.Add(_jsonData["data"][i]["cards"][j]["id"]);
        //         cardName.Add(_jsonData["data"][i]["cards"][j]["name"]);
        //     }
        // }

        // private IEnumerator Downloading()
        // {
        //     UnityWebRequest www = UnityWebRequest.Get(downloadFileURL);
        //     yield return www.SendWebRequest();

        //     if (www.result != UnityWebRequest.Result.Success)
        //     {
        //         Debug.LogError("Gagal mengunduh file: " + www.error);
        //     }
        //     else
        //     {
        //         byte[] data = www.downloadHandler.data;

        //         // Simpan data ke dalam file di folder "Assets".
        //         File.WriteAllBytes(savePath, data);

        //         Debug.Log("File berhasil diunduh dan disimpan di " + savePath);
        //     }
        // }

        public IEnumerator Download(string fileName, int cardId)
        {
            downloadFileURL = URLWithoutCardId + cardId;
            
            Debug.Log("URLFILE : " + downloadFileURL);
            //Create "Resources" Folder First
            savePath = $"Assets/Resources/3dObject/{fileName + extentionFile}";

            
            
            UnityWebRequest www = UnityWebRequest.Get(downloadFileURL);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Gagal mengunduh file: " + www.error);
            }
            else
            {
                byte[] data = www.downloadHandler.data;

                // Simpan data ke dalam file di folder "Assets".
                File.WriteAllBytes(savePath, data);

                Debug.Log("File berhasil diunduh dan disimpan di " + savePath);
            }
        }
    }
}
