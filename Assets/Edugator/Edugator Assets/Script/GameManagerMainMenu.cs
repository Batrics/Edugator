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
using Unity.VisualScripting;

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
    public TextMeshProUGUI infoForDev;
    //Main Menu Script
    //==============================================================================================================================//\
    private void Awake() {

        AssetBundle.UnloadAllAssetBundles(true);

        loadingUI.Prepare();

        print(Application.persistentDataPath);
        string directoryPath = Path.Combine(Application.persistentDataPath, "3dObject");

        // Check if the directory already exists
        if (!Directory.Exists(directoryPath)) {
            // Create the directory
            Directory.CreateDirectory(directoryPath);
            Debug.Log("Directory created successfully.");
        }
        else {
            Debug.Log("Directory already exists.");
        }
        print("D : " + directoryPath);

        directoryPath = Path.Combine(Application.persistentDataPath, "CardImage");

        // Check if the directory already exists
        if (!Directory.Exists(directoryPath)) {
            // Create the directory
            Directory.CreateDirectory(directoryPath);
            Debug.Log("Directory created successfully.");
        }
        else {
            Debug.Log("Directory already exists.");
        }
        print("D2 : " + directoryPath);

        infoForDev.text = Application.persistentDataPath + "\n" + Application.streamingAssetsPath;
    }

    void Start() {
        // PlayerPrefs.DeleteAll();
        // print("Delete Prefabs Success");
        // RefreshDirectory();
        // print(PlayerPrefs.GetString("history"));
        PlayerPrefs.DeleteKey("token");
        historyBtn.text = "History";
        url = "https://dev.unimasoft.id/edugator/api/getAllGames/a49fdc824fe7c4ac29ed8c7b460d7338";
    }

    public void StartGame() {
        StartCoroutine(StartQuiz());
    }

    public IEnumerator StartQuiz() {
        loadingUI.Show("Please Wait...");
        yield return StartCoroutine(GetDataFromAPI());
        RefreshHistory();
    }

    public void ExitGame() {
        Application.Quit();
    }

    public void Playbtn() {
        if(PlayerPrefs.GetString("token") == "") {
            inputTokenUI.SetActive(true);
        }
        else {
            inputTokenUI.SetActive(false);
            transitionScript.startTransitionActive();
            loadNextScene.GoNextScene();
        }
    }

    public void AddGame() {
        inputTokenUI.SetActive(true);
        inputToken.text = "";
        info.text = "";
    }

    public void CheckVisualEffect(bool checkVisualEffect) {
        UpdateToggle(checkVisualEffect);

        PlayerPrefs.SetInt("visualEffect", checkVisualEffectInt);
        Debug.Log("Bool : " + checkVisualEffectInt);
    }

    public void RefreshToggle() {
        int visualEffect = PlayerPrefs.GetInt("visualEffect");
        print(visualEffect);

        if (visualEffect == 1) {
            UpdateToggle(true);
        }
        else {
            UpdateToggle(false);
        }
    }

    public void UpdateToggle(bool is_on) {
        if (is_on) {
            checkVisualEffectInt = 1;
            VFXAnim.SetBool("toggle", true);
        }
        else {
            checkVisualEffectInt = 0;
            VFXAnim.SetBool("toggle", false);
        }
    }
    private IEnumerator GetDataFromAPI() {

        using(UnityWebRequest webData = UnityWebRequest.Get(url)) {
            webData.SendWebRequest();

            while(!webData.isDone) {
                yield return null;
            }

            if(webData.result == UnityWebRequest.Result.ConnectionError || webData.result == UnityWebRequest.Result.ProtocolError) {
                loadingUI.Hide();
                Debug.Log("tidak ada Koneksi/Jaringan"); 
                info.text = "tidak ada Koneksi/Jaringan";
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

                            if (inputToken.text == _jsonData["data"][i]["token"]) {
                                PlayerPrefs.SetString("token", inputToken.text);
                                PlayerPrefs.SetInt("game_id", _jsonData["data"][i]["id"]);
                                titleText.text = _jsonData["data"][i]["name"];
                                GameOwnerText.text = "Created By : " + _jsonData["data"][i]["author"];
                                print(PlayerPrefs.GetInt("game_id"));
                                inputTokenUI.SetActive(false);

                                //Update History Data
                                string storage = PlayerPrefs.GetString("history");

                                if(storage != "") {
                                    List<string> allHistory = new List<string>(storage.Split(";"));

                                    bool ketemu = false;
                                    foreach(string history in allHistory) {
                                        string[] historyArr = history.Split(",");

                                        if(inputToken.text == historyArr[0]) {
                                            ketemu = true;
                                            break;
                                        }
                                    }

                                    if(!ketemu) {
                                        allHistory.Insert(0, inputToken.text + "," + _jsonData["data"][i]["name"]);
                                        storage = string.Join(";", allHistory);
                                        PlayerPrefs.SetString("history", storage);
                                    }
                                }
                                else {
                                    PlayerPrefs.SetString("history", inputToken.text + "," + _jsonData["data"][i]["name"]);
                                }

                                // //Download Assets
                                string urlDownloadModel = "https://dev.unimasoft.id/edugator/api/downloadModel/a49fdc824fe7c4ac29ed8c7b460d7338/";
                                string path = Path.Combine(Application.persistentDataPath, "3dObject");
                                // print("FILE EXIST ) : " + File.Exists(path + "Gold Fish.fbx"));
                                infoForDev.text = "Dec Var";
                                yield return StartCoroutine(DownloadFileLogic(urlDownloadModel, path, "model", i, "3dObject"));

                                string urlDownloadCard = "https://dev.unimasoft.id/edugator/api/downloadCard/a49fdc824fe7c4ac29ed8c7b460d7338/";
                                path = Path.Combine(Application.persistentDataPath, "CardImage");
                                yield return StartCoroutine(DownloadFileLogic(urlDownloadCard, path, "card", i, "CardImage"));

                                yield return StartCoroutine(ExtractFile());
                                DeleteZipFile();
                                RefreshDirectory();

                                loadingUI.Hide();

                            }
                            else if (inputToken.text == "") {
                                info.text = "Masukkan Token";
                            }
                            else {
                                info.text = "Token anda salah";
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

    public void RefreshHistory() {
        for(int i = 0; i < table.childCount; i++) {
            Destroy(table.transform.GetChild(i).gameObject);
        }
        string storage = PlayerPrefs.GetString("history");
        if(storage != "") {
            List<string> allHistory = new List<string>(storage.Split(";"));

            foreach(string history in allHistory) {
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



    //==============================================================================================================================//

    //Download Model
    //==============================================================================================================================//
    string cardName;
    int cardId;
    public List<GameObject> files3D = new List<GameObject>();
    public List<Texture2D> filesCard = new List<Texture2D>();
    private IEnumerator DownloadFileLogic(string URLWithoutCardId, string savePath, string bundleType, int indexGame, string directoryPath) {
        
        string filePath;
        
        for(int j = 0; j < _jsonData["data"][indexGame]["cards"].Count ; j++) {
            cardName = _jsonData["data"][indexGame]["cards"][j]["name"];

            filePath = $"Assets/AssetBundles/Android/{cardName} ({bundleType})";

            switch(bundleType) {
                case "model" :
                    AssetBundle bundleModel = AssetBundle.LoadFromFile(filePath);
                    GameObject model = bundleModel.LoadAsset<GameObject>(cardName + ".fbx");
                    files3D.Add(model);
                    break;
                case "card" : 
                    AssetBundle bundleCard = AssetBundle.LoadFromFile(filePath);
                    Texture2D card = bundleCard.LoadAsset<Texture2D>(cardName + ".jpg");
                    yield return card;
                    filesCard.Add(card);
                    break;
            }
        }

        //==================================================================================

        infoForDev.text = "Dec Var\nDec Var in Function";

        bool fileIsAvailable;
        
        infoForDev.text = "Dec Var\nDec Var in Function\nLanjut";

        // fileName = Path.GetFileName(savePath);
        yield return null;
        if(files3D.Count == 0 && filesCard.Count == 0) {
            infoForDev.text = "Dec Var\nDec Var in Function\nif";

            loadingUI.Show("Download Assets...");
            for(int j = 0; j < _jsonData["data"][indexGame]["cards"].Count; j++) {
                cardName = _jsonData["data"][indexGame]["cards"][j]["name"];
                cardId = _jsonData["data"][indexGame]["cards"][j]["id"];
                
                yield return StartCoroutine(downloadFile.Download(URLWithoutCardId, cardName, cardId, bundleType, savePath));
            }
        }
        else {
            infoForDev.text = "Dec Var\nDec Var in Function\nelse";
            for(int j = 0; j < _jsonData["data"][indexGame]["cards"].Count; j++) {
                cardName = _jsonData["data"][indexGame]["cards"][j]["name"];
                cardId = _jsonData["data"][indexGame]["cards"][j]["id"];
                fileIsAvailable = false;
                
                if(directoryPath == "3dObject") {
                    foreach(GameObject file in files3D) {
                        if(file.name.Contains(cardName)) {
                            fileIsAvailable = true;
                        }
                    }
                }
                else if(directoryPath == "CardImage") {
                    foreach(Texture2D file in filesCard) {
                        if(file.name.Contains(cardName)) {
                            fileIsAvailable = true;
                        }
                    }
                }

                if(fileIsAvailable == true)
                    Debug.Log("File is Available");
                else {
                    loadingUI.Show("Download Assets...");
                    yield return StartCoroutine(downloadFile.Download(URLWithoutCardId, cardName, cardId, bundleType, savePath));
                    print("CName : " + cardName);
                }
            }
        }

    }

    public void CheckFile(int i)
    {
        
        // Memuat Asset Bundle yang telah dibangun
        // AssetBundle loadedBundle = AssetBundle.LoadFromFile("Assets/AssetBundles/Android/3dObject");
        
        // // Mengambil prefab dari Asset Bundle
        // GameObject myPrefab = loadedBundle.LoadAsset<GameObject>("PrefabName"); // Ganti "PrefabName" dengan nama prefab Anda
        
        // Instansiasi prefab dari Asset Bundle
        // Instantiate(myPrefab);

        
        // files3D = Resources.LoadAll<GameObject>(Application.persistentDataPath + "/3dObject");
        // filescCard = Resources.LoadAll<Texture2D>(Application.persistentDataPath + "/CardImage");
        // Texture2D file = Resources.Load<Texture2D>(filePath);

        // string filePath;
        
        // for(int j = 0; j < _jsonData["data"][indexGame]["cards"].Count; j++) {
        //     cardName = _jsonData["data"][indexGame]["cards"][j]["name"];

        //     filePath = $"Assets/AssetBundles/Android/{cardName}";

        //     AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
        //     bundles.Append<AssetBundle>(bundle);

            // if (bundle != null) {
            //     Texture2D loadedObject = bundle.LoadAsset<Texture2D>("Fire Extingusher.jpg");

            //     if (loadedObject != null) {
            //         filescCard.Append(loadedObject);
            //         Debug.Log("File FBX berhasil dimuat dan diinstansiasi sebagai GameObject");
            //     }
            //     else Debug.LogError("Gagal memuat GameObject dari bundle");

            //     bundle.Unload(false);
            // } 
            // else Debug.LogError("Gagal memuat AssetBundle dari file: " + filePath);
        // }

        // string filePath = "Assets/AssetBundles/Android/fire hydrant";
        // AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
        // bundles.Add(bundle);

        // filePath = "Assets/AssetBundles/Android/fire extingusher";
        // bundle = AssetBundle.LoadFromFile(filePath);
        // bundles.Add(bundle);

        // print(bundle);
        // print(bundles[1]);
        // if (bundle != null) {
        //     GameObject loadedObject = bundle.LoadAsset<GameObject>("fire extingusher");

        //     if (loadedObject != null)
        //     {
        //         print(loadedObject.GetType());
        //         Debug.Log("File FBX berhasil dimuat dan diinstansiasi sebagai GameObject");
        //         Instantiate(loadedObject);
        //     }
        //     else
        //     {
        //         Debug.LogError("Gagal memuat GameObject dari bundle");
        //     }
        //     bundle.Unload(false);
        // } 
        // else Debug.LogError("Gagal memuat AssetBundle dari file: " + filePath);
    }
    //==============================================================================================================================//
    
    //Music
    //==============================================================================================================================//
    public void SetVoume(float volume) {
        Debug.Log(volume);
        if(volume <= -20f && volume >= -30) {
            volume = -80f;
        }
        else if(volume > -80 && volume <= -70) {
            volume = -20f;
        }

        audioMixer.SetFloat("volume", volume);
    }

    public void SetVFX(float volume) {
        Debug.Log(volume);
        if(volume <= -20f && volume >= -30) {
            volume = -80f;
        }
        else if(volume > -80 && volume <= -70) {
            volume = -20f;
        }

        audioMixer.SetFloat("sfx", volume);
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

namespace Download.file
{
    public class DownloadFile : MonoBehaviour
    {
        /// <summary>
        /// URL for edugator = https://dev.unimasoft.id/edugator/api/downloadModel/a49fdc824fe7c4ac29ed8c7b460d7338/
        /// </summary>
        private string downloadFileURL;
        public string filePath;

        public IEnumerator Download(string URLWithoutCardId, string fileName, int cardId, string extention, string savePath) {
            downloadFileURL = URLWithoutCardId + cardId;
            
            Debug.Log("URLFILE : " + downloadFileURL);
            //Create "Resources" Folder First
            filePath = savePath + fileName + extention;

            
            
            UnityWebRequest www = UnityWebRequest.Get(downloadFileURL);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.LogError("Gagal mengunduh file: " + www.error);
            }
            else {
                byte[] data = www.downloadHandler.data;

                // Simpan data ke dalam file di folder "Assets".
                // GameManagerMainMenu gameManagerMainMenu;
                // gameManagerMainMenu = GetComponent<GameManagerMainMenu>();
                try {
                    File.WriteAllBytes(filePath, data);
                    Debug.Log("File berhasil diunduh dan disimpan di " + filePath);
                    // gameManagerMainMenu.infoForDev.text = "File berhasil diunduh dan disimpan di " + filePath;
                }
                catch(Exception ex) {
                    // gameManagerMainMenu.infoForDev.text = ex.ToString();
                    print(ex);
                }

            }
        }
    }
}
