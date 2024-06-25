using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Loading.UI;
using System.IO;
using System.IO.Compression;
using System;

public class HistoryScript : MonoBehaviour
{
    MainDataJson mainData;
    private string jsonstring;
    private string url;
    public string token;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI GameOwnerText;
    private GameObject koneksi;
    public GameObject gameSelected;
    public GameObject popup;
    public Transform PanelHistoryUI;
    private LoadingUI loadingUI = new LoadingUI();
    public GameObject progressBarGameObject;
    public GameObject progressBarGameObjectClone;
    public GameManagerMainMenu gameManagerMainMenu;

    private void Start() {
        GameObject canvas = gameObject.transform.parent.parent.parent.parent.parent.parent.parent.gameObject;
        GameObject go = GameObject.Find("Canvas");
        print(go);

        titleText = GameObject.Find("MainTitle Text").GetComponent<TextMeshProUGUI>();
        GameOwnerText = GameObject.Find("GameOwnerText").GetComponent<TextMeshProUGUI>();
        koneksi = GameObject.Find("Panel Koneksi");
        PanelHistoryUI = GameObject.Find("Games UI").transform;
        popup = Resources.Load<GameObject>("Popup");

        gameManagerMainMenu = GameObject.Find("GameManager").GetComponent<GameManagerMainMenu>();
        string allGames = PlayerPrefs.GetString("history");
        string[] allGamesArr = allGames.Split(";");

        Debug.Log("All Game : " + allGames);
        Debug.Log("All Game : " + allGamesArr[0]);
        Debug.Log("Game Selected : " + gameSelected);
        
        AssetBundle.UnloadAllAssetBundles(true);
    }

    public void StartQuiz() {
        loadingUI.Prepare();
        progressBarGameObject = Resources.Load<GameObject>("DownloadPopup");
        progressBarGameObjectClone = Instantiate(progressBarGameObject);
        url = "https://dev.unimasoft.id/edugator/api/getDataGame/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetString("tokenSelected");
        StartCoroutine(GetDataFromAPI());
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

                    jsonstring = webData.downloadHandler.text;
                    PlayerPrefs.SetString("jsonData", jsonstring);

                    mainData = JsonUtility.FromJson<MainDataJson>(PlayerPrefs.GetString("jsonData"));
                    print(mainData.data.cards[0].id);

                    if(mainData == null) {
                        print("JSON Data Null");
                    }
                    else {
                        if(mainData.success == true) { 
                            if (token == mainData.data.token) {

                                // //Download Assets
                                progressBarGameObjectClone.SetActive(true);
                                string urlDownloadModel = "https://dev.unimasoft.id/edugator/api/downloadBundle/a49fdc824fe7c4ac29ed8c7b460d7338/";
                                
                                string path = Application.persistentDataPath + "/AssetsBundle/";
                                print("Dec Var");
                                yield return StartCoroutine(DownloadFileLogic(urlDownloadModel, path, ".zip"));
                                if (DownloadPopup.userCancelledDownload == true) {
                                    progressBarGameObjectClone.SetActive(false);
                                    DownloadPopup.userCancelledDownload = false;
                                    yield break;
                                }

                                loadingUI.Hide();

                                PlayerPrefs.SetString("token", token);
                                PlayerPrefs.SetInt("game_id", mainData.data.id);

                                print("Jumlah Card : " + mainData.data.cards.Length);
                                // gameManagerMainMenu.CreateScoreHistory(mainData.data.cards.Length, mainData, PlayerPrefs.GetString("finalScore"));

                                titleText.text = mainData.data.name;
                                GameOwnerText.text = "Created By : " + mainData.data.author;

                                progressBarGameObjectClone.SetActive(false);
                                transform.parent.parent.parent.parent.parent.gameObject.SetActive(false);
                                
                            }
                            else {
                                print("Token Anda salah");
                            }
                        }
                        else {
                            Debug.LogError("Error Detail: " + webData.error);
                        }
                    }
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
    string cardId;
    // List<string> files = new List<string>();
    public List<GameObject> files3D = new List<GameObject>();
    public List<Texture2D> filesCard = new List<Texture2D>();
    private const string _BUNDLETYPEMODEL = " (model)";
    private const string _BUNDLETYPECARD = " (card)";
    private IEnumerator DownloadFileLogic(string URLWithoutCardId, string savePath, string extention) {
        string[] files;

        files = Directory.GetFiles(Application.persistentDataPath + "/AssetsBundle/");

        //==================================================================================

        print("Dec Var\nDec Var in Function");

        bool fileIsAvailable;
        
        print("Dec Var\nDec Var in Function\nLanjut");

        yield return null;
        if(files.Length == 0) {
            print("Dec Var\nDec Var in Function\nif");

            // loadingUI.Show("Download Assets...");
            for(int j = 0; j < mainData.data.cards.Length; j++) {
                cardName = mainData.data.cards[j].name;
                cardId = mainData.data.cards[j].id;
                
                yield return StartCoroutine(DownloadFile(URLWithoutCardId, cardName, cardId, extention, savePath));
                if (DownloadPopup.userCancelledDownload == true) {
                    yield break;
                }
                yield return StartCoroutine(ExtractFile());
                DeleteZipFile();
                yield return StartCoroutine(InitializationBundleToObject());
            }
        }
        else {
            print("Dec Var\nDec Var in Function\nelse");
            for(int j = 0; j < mainData.data.cards.Length; j++) {
                cardName = mainData.data.cards[j].name;
                cardId = mainData.data.cards[j].id;
                fileIsAvailable = false;

                foreach(string file in files) {
                    if(file.Contains(cardName.ToLower())) {

                        fileIsAvailable = true;
                    }
                }

                if(fileIsAvailable == true) {
                    Debug.Log("File is Available");
                    yield return StartCoroutine(InitializationBundleToObject());
                }
                else {
                    // loadingUI.Show("Download Assets...");
                    yield return StartCoroutine(DownloadFile(URLWithoutCardId, cardName, cardId, extention, savePath));
                    if (DownloadPopup.userCancelledDownload == true) {
                        yield break;
                    }
                    yield return StartCoroutine(ExtractFile());
                    DeleteZipFile();
                    yield return StartCoroutine(InitializationBundleToObject());
                    print("CName : " + cardName);
                }
            }
        }
    }

    private IEnumerator InitializationBundleToObject() {
        string filePathModel = Application.persistentDataPath + "/AssetsBundle/" + cardName + _BUNDLETYPEMODEL;
        string filePathCard = Application.persistentDataPath + "/AssetsBundle/" + cardName + _BUNDLETYPECARD;
        
        AssetBundle bundleCard = AssetBundle.LoadFromFile(filePathCard);
        Texture2D card = bundleCard.LoadAsset<Texture2D>(cardName + ".png");
        card.name = cardName;
        yield return card;
        filesCard.Add(card);

        AssetBundle bundleModel = AssetBundle.LoadFromFile(filePathModel);
        GameObject model = bundleModel.LoadAsset<GameObject>(cardName + ".fbx");
        model.name = cardName;
        yield return model;
        files3D.Add(model);

    }
    //==============================================================================================================================//
    //Extract and Delete File
    //==============================================================================================================================//

    private IEnumerator ExtractFile() {
        string filePath = Application.persistentDataPath + "/AssetsBundle/";

        string[] zipFiles = Directory.GetFiles(filePath, "*.zip");

        try
        {
            foreach(string zipFile in zipFiles) {
                print("ZIPFILE : " + zipFile);
                ZipFile.ExtractToDirectory(zipFile, filePath);
            }
            print("Zip file extracted successfully.");
        }
        catch (Exception ex) {
            print($"Error extracting zip file: {ex.Message}");
        }

        yield return null;
    }

    private void DeleteZipFile() {
        string filePath = Application.persistentDataPath + "/AssetsBundle/";

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

    //Download
    //==============================================================================================================================//
    /// <summary>
    /// URL for edugator = https://dev.unimasoft.id/edugator/api/downloadBundle/a49fdc824fe7c4ac29ed8c7b460d7338/
    /// </summary>
    private string downloadFileURL;
    public string filePath;
    // private GameManagerMainMenu gameManagerMainMenu;
    private UnityEngine.UI.Image progressBarFilled;
    UnityWebRequest www;
    public IEnumerator DownloadFile(string URLWithoutCardId, string fileName, string cardId, string extention, string savePath) {
        yield return null;
        
        progressBarFilled = progressBarGameObjectClone.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<UnityEngine.UI.Image>();

        downloadFileURL = URLWithoutCardId + cardId;
        
        Debug.Log("URLFILE : " + downloadFileURL);

        filePath = savePath + fileName + extention;
        
        www = UnityWebRequest.Get(downloadFileURL);

        yield return new WaitForSeconds(0.1f);

        www.SendWebRequest();

        while(!www.isDone) {
            if (DownloadPopup.userCancelledDownload) {
                www.Abort(); // Batalkan unduhan jika user meminta
                Debug.Log("Download dibatalkan.");
                yield break; // Keluar dari coroutine
            }
            progressBarFilled.fillAmount = www.downloadProgress;
            yield return null;
        }
        if(www.isDone) {
            print("Is Done");
        }

        if (www.result != UnityWebRequest.Result.Success) {
            print("WebRequest Failed");
        }
        else {
            byte[] data = www.downloadHandler.data;
            try {
                File.WriteAllBytes(filePath, data);
                Debug.Log("File berhasil diunduh dan disimpan di " + filePath);
            }
            catch(Exception ex) {
                print(ex);
            }

        }
    }
    //==============================================================================================================================//
}
