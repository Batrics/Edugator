using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using TMPro;
using Loading.UI;
using Download.file;
using System.IO;
using System.IO.Compression;
using System;

public class HistoryScript : MonoBehaviour
{
    private JSONNode _jsonData;
    private string url;
    public string token;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI GameOwnerText;
    private GameObject koneksi;
    public GameObject gameSelected;
    public GameObject popup;
    public Transform PanelHistoryUI;
    private LoadingUI loadingUI = new LoadingUI();
    private DownloadFile downloadFile = new DownloadFile();

    private void Awake() {
        loadingUI.Prepare();
    }
    private void Start() {
        titleText = gameObject.transform.parent.parent.parent.parent.GetChild(1).GetComponent<TextMeshProUGUI>();
        GameOwnerText = gameObject.transform.parent.parent.parent.parent.GetChild(2).GetComponent<TextMeshProUGUI>();
        koneksi = gameObject.transform.parent.parent.parent.parent.GetChild(10).gameObject;
        // popup = gameObject.transform.parent.parent.GetChild(5).gameObject;
        PanelHistoryUI = gameObject.transform.parent.parent.parent;
        string allGames = PlayerPrefs.GetString("history");
        string[] allGamesArr = allGames.Split(";");

        Debug.Log("All Game : " + allGames);
        Debug.Log("All Game : " + allGamesArr[0]);
        Debug.Log("Game Selected : " + gameSelected);

        url = "https://dev.unimasoft.id/edugator/api/getAllGames/a49fdc824fe7c4ac29ed8c7b460d7338";
    }

    public void StartQuiz() {
        StartCoroutine(GetDataFromAPI());
    }

    private void CallAllCoroutine() {
        StartCoroutine(GetDataFromAPI());
        // yield return StartCoroutine(DownloadingModel());
    }
    private IEnumerator GetDataFromAPI() {
        using(UnityWebRequest webData = UnityWebRequest.Get(url)) {
            loadingUI.Show("Please Wait...");
            webData.SendWebRequest();

            while(!webData.isDone) {
                yield return null;
            }

            if(webData.result == UnityWebRequest.Result.ConnectionError || webData.result == UnityWebRequest.Result.ProtocolError) {
                loadingUI.Hide();
                StartCoroutine(NoConnection());
                Debug.Log("tidak ada Koneksi/Jaringan"); 
            }
            else {
                if(webData.isDone) {
                    loadingUI.Hide();
                    _jsonData = JSON.Parse(System.Text.Encoding.UTF8.GetString(webData.downloadHandler.data));
                    if(_jsonData == null) {
                        Debug.Log("Json data Kosong");
                    }
                    else {
                        for (int i = 0; i < _jsonData["data"].Count; i++) {
                            if (token == _jsonData["data"][i]["token"]) {
                                PlayerPrefs.SetString("token", token);
                                PlayerPrefs.SetInt("game_id", _jsonData["data"][i]["id"]);

                                print("Jumlah Card : " + _jsonData["data"][i]["cards"].Count);

                                titleText.text = _jsonData["data"][i]["name"];
                                GameOwnerText.text = "Created By : " + _jsonData["data"][i]["author"];

                                // //Download Assets
                                string urlDownloadModel = "https://dev.unimasoft.id/edugator/api/downloadModel/a49fdc824fe7c4ac29ed8c7b460d7338/";
                                
                                string path = Application.persistentDataPath + "/3dObject/";
                                // print("FILE EXIST ) : " + File.Exists(path + "Gold Fish.fbx"));
                                print("Dec Var");
                                yield return StartCoroutine(DownloadFileLogic(urlDownloadModel, path, ".zip", i, "/3dObject/"));

                                path = Application.persistentDataPath + "/Card/";
                                string urlDownloadCard = "https://dev.unimasoft.id/edugator/api/downloadCard/a49fdc824fe7c4ac29ed8c7b460d7338/";
                                // path = "Assets/Resources/CardImage/";
                                yield return StartCoroutine(DownloadFileLogic(urlDownloadCard, path, ".jpg", i, "/Card/"));

                                yield return StartCoroutine(ExtractFile());
                                DeleteZipFile();
                                RefreshDirectory();

                                loadingUI.Hide();

                                transform.parent.parent.parent.gameObject.SetActive(false);
                                
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

    public void SetToken(TextMeshProUGUI tokenText) {
        token = tokenText.text;
    }

    private IEnumerator NoConnection() {
        koneksi.SetActive(true);
        yield return new WaitForSeconds(3f);
        koneksi.SetActive(false);
    }

    public void InstantiatePopup() {
        Instantiate(popup, PanelHistoryUI);
        // popup.SetActive(true);
    }

    public void SetGame(GameObject game) {
        gameSelected = game;
        PlayerPrefs.SetString("tokenSelected", token);
        print("Game Selected : " + gameSelected);
        print("Token Selected : " + PlayerPrefs.GetString("tokenSelected"));

    }

    void RefreshDirectory() {

        string directoryPath = Application.persistentDataPath; // Contoh path, sesuaikan dengan kebutuhan Anda

        // Create a DirectoryInfo object for the specified directory
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

        try {
            // Refresh the directory information
            directoryInfo.Refresh();

            // // Get the updated list of files and subdirectories
            // FileInfo[] files = directoryInfo.GetFiles();
            // DirectoryInfo[] subdirectories = directoryInfo.GetDirectories();

            // Debug.Log("Files:");
            // foreach (var file in files)
            // {
            //     Debug.Log(file.Name);
            // }

            // Debug.Log("\nSubdirectories:");
            // foreach (var subdirectory in subdirectories)
            // {
            //     Debug.Log(subdirectory.Name);
            // }
            print("Refresh Complete");
        }
        catch (Exception ex) {
            Debug.LogError($"An error occurred: {ex.Message}");
        }
    }

    //Animation
    //==============================================================================================================================//
    
    private IEnumerator SlidingAnimation(Animator action) {
        action.SetBool("activateSlide", true);
        yield return new WaitForSeconds(0.25f);
        action.SetBool("activateSlide", false);
    }

    public void ActivateAnimationSliding(Animator action) {
        StartCoroutine(SlidingAnimation(action));
    }

    //==============================================================================================================================//
    //Download File
    //==============================================================================================================================//
    string cardName;
    int cardId;
    // public List<string> files = new List<string>();
    string[] files;
    private IEnumerator DownloadFileLogic(string URLWithoutCardId, string savePath, string extention, int indexGame, string directorySavePath) {
        // string file;
        string directoryPath = Application.persistentDataPath + directorySavePath;
        
        if (Directory.Exists(directoryPath)) {
            files = Directory.GetFiles(directoryPath);
        }
        else {
            Console.WriteLine("Direktori tidak ditemukan.");
        }

        print("Dec Var\nDec Var in Function");

        bool fileIsAvailable;
        
        print("Dec Var\nDec Var in Function\nLanjut");

        // yield return null;
        if(files.Length == 0) {
            print("Dec Var\nDec Var in Function\nif");

            loadingUI.Show("Download Assets...");
            for(int j = 0; j < _jsonData["data"][indexGame]["cards"].Count; j++) {
                cardName = _jsonData["data"][indexGame]["cards"][j]["name"];
                cardId = _jsonData["data"][indexGame]["cards"][j]["id"];
                
                yield return StartCoroutine(downloadFile.Download(URLWithoutCardId, cardName, cardId, extention, savePath));
                // print("FI NAME : " + file);
            }
        }
        else {
            print("Dec Var\nDec Var in Function\nelse");
            for(int j = 0; j < _jsonData["data"][indexGame]["cards"].Count; j++) {
                cardName = _jsonData["data"][indexGame]["cards"][j]["name"];
                cardId = _jsonData["data"][indexGame]["cards"][j]["id"];
                fileIsAvailable = false;

                foreach(string file in files) {
                    if(file.Contains(cardName)) {
                        fileIsAvailable = true;
                    }
                }

                // if(cardName == fileName) {
                //     fileIsAvailable = true;
                // }

                if(fileIsAvailable == true)
                    Debug.Log("File is Available");
                else {
                    loadingUI.Show("Download Assets...");
                    yield return StartCoroutine(downloadFile.Download(URLWithoutCardId, cardName, cardId, extention, savePath));
                    // print("FI NAME : " + file);
                }
            }
        }

    }
    //==============================================================================================================================//
    //Extract and Delete File
    //==============================================================================================================================//

    private IEnumerator ExtractFile() {
        string filePath = Application.persistentDataPath + "/3dObject/";

        string[] zipFiles = Directory.GetFiles(filePath, "*.zip");

        try
        {
            foreach(string zipFile in zipFiles) {
                print("ZIPFILE : " + zipFile);
                ZipFile.ExtractToDirectory(zipFile, filePath);
            }

            // Extract the contents of the zip file

            print("Zip file extracted successfully.");
        }
        catch (Exception ex) {
            print($"Error extracting zip file: {ex.Message}");
        }

        yield return null;
    }

    private void DeleteZipFile() {
        string filePath = Application.persistentDataPath + "/3dObject/";

        string[] zipFiles = Directory.GetFiles(filePath, "*.zip");

        try
        {
            foreach(string zipFile in zipFiles) {
                print("ZIPFILE : " + zipFile);
                File.Delete(zipFile);
            }

            print("Zip file Deleted successfully.");
        }
        catch (Exception ex) {
            print($"Error Deleted zip file: {ex.Message}");
        }
    }

    //==============================================================================================================================//
}
