using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.IO;
using System.IO.Compression;
using UnityEngine.Audio;
using Loading.UI;
using System;
using UnityEngine.UI;

public class GameManagerMainMenu : MonoBehaviour
{
    public string downloadCount;
    MainDataJson mainData;
    private string jsonstring;
    private string url;
    public TMP_InputField inputToken;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI GameOwnerText;
    public Animator VFXAnim;
    public LoadNextScene loadNextScene;
    public TransitionScript transitionScript;
    public AudioMixer audioMixer;
    private int checkVisualEffectInt;
    public TextMeshProUGUI historyBtn;
    public Transform panel;
    public Transform table;

    [Header("GameObject")]
    public GameObject inputTokenUI;
    public GameObject visualEffectToggle;
    public GameObject startTransitionObject;
    public GameObject historyValue;
    public GameObject connection;
    public GameObject panelToken;
    public GameObject guestUI;
    public GameObject userUI;
    
    [Space]
    public LoadingUI loadingUI;
    private GameObject progressBarGameObject;
    private GameObject progressBarGameObjectClone;
    private Button userAccountButton;
    [SerializeField] private TMP_Text name1;
    [SerializeField] private TMP_Text email1;
    [SerializeField] private TMP_Text name2;
    [SerializeField] private TMP_Text email2;
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    LoginScript loginScript;
    
    //Main Menu Script
    //==============================================================================================================================//\
    private void Awake() {
        // PlayerPrefs.DeleteAll();
        // print("Delete All PlayerPref");
        AssetBundle.UnloadAllAssetBundles(true);
        loadingUI = GetComponent<LoadingUI>();
        loadingUI.Prepare();
        RefreshHistory();

        string directoryPath = Path.Combine(Application.persistentDataPath, "AssetsBundle");

        if (!Directory.Exists(directoryPath)) {
            Directory.CreateDirectory(directoryPath);
            Debug.Log("Assets Bundle Directory created successfully.");
        }
        else {
            Debug.Log("Directory already exists.");
        }
        
        progressBarGameObject = Resources.Load<GameObject>("DownloadPopup");
    }

    void Start() {
        PlayerPrefs.DeleteKey("token");
        historyBtn.text = "History";

        checkVisualEffectInt = 1;
        PlayerPrefs.SetInt("visualEffect", checkVisualEffectInt);

        userAccountButton = GameObject.Find("UserAccountBtn").GetComponent<Button>();
        loginScript = GetComponent<LoginScript>();
        loginScript.users = JsonUtility.FromJson<Users>(PlayerPrefs.GetString("Json_Users"));
        if(PlayerPrefs.GetString("Login_State") == "") {
            PlayerPrefs.SetString("Login_State", "failed");
        }
        loadingUI.Show("Please Wait...");
        RefreshUserAccountBtn();
        if(PlayerPrefs.GetString("Login_State") == "success") {
            LoginSuccess();
        }
        loadingUI.Hide();
    }

    public void Logout() {
        PlayerPrefs.SetString("Login_State", "failed");
        AssetBundle.UnloadAllAssetBundles(true);
        loginScript.sprites.Clear();
        loginScript.cardIdList.Clear();
        files3D.Clear();
        filesCard.Clear();
        userUI.SetActive(false);
        RefreshUserAccountBtn();
    }

    public void RefreshUserAccountBtn() {
        void Failed() {
            userUI.SetActive(false);
            guestUI.SetActive(true);
        }
        void Success() {
            userUI.SetActive(true);
            guestUI.SetActive(false);
        }
        if(PlayerPrefs.GetString("Login_State") == "success") {
            userAccountButton.onClick.AddListener(Success);
            userAccountButton.onClick.RemoveListener(Failed);
        }
        else if(PlayerPrefs.GetString("Login_State") == "failed"){
            userAccountButton.onClick.AddListener(Failed);
            userAccountButton.onClick.RemoveListener(Success);
        }
    }
    public void LoginSuccess() {
        loginScript.MainUserInfo();
        StartCoroutine(loginScript.User_Coroutine());
    }

    public void StartGame() {
        url = "https://dev.unimasoft.id/edugator/api/getDataGame/a49fdc824fe7c4ac29ed8c7b460d7338/" + inputToken.text;
        print(url);
        progressBarGameObjectClone =  Instantiate(progressBarGameObject);
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
        
    }

    public void SetVisualEffect(bool checkVisualEffect) {
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
            PlayerPrefs.SetInt("visualEffect", 1);
            VFXAnim.SetBool("toggle", true);
        }
        else {
            checkVisualEffectInt = 0;
            PlayerPrefs.SetInt("visualEffect", 0);
            VFXAnim.SetBool("toggle", false);
        }
    }

    public IEnumerator NoConnection() {
        connection.SetActive(true);
        yield return new WaitForSeconds(3f);
        connection.SetActive(false);
    }
    private IEnumerator IncorrectToken() {
        panelToken.SetActive(true);
        yield return new WaitForSeconds(3f);
        panelToken.SetActive(false);
    }
    private IEnumerator GetDataFromAPI() {
        using(UnityWebRequest webData = UnityWebRequest.Get(url)) {
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

                    if(mainData == null) {
                        Debug.Log("Json data Kosong");
                    }
                    else {
                        if(mainData.success == true) { 
                            if (inputToken.text == mainData.data.token) {

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
                                        allHistory.Insert(0, inputToken.text + "," + mainData.data.name);
                                        storage = string.Join(";", allHistory);
                                        PlayerPrefs.SetString("history", storage);
                                    }
                                }
                                else {
                                    PlayerPrefs.SetString("history", inputToken.text + "," + mainData.data.name);
                                }

                                // //Download Assets
                                loadingUI.Hide();

                                progressBarGameObjectClone.SetActive(true);
                                string urlDownloadModel = "https://dev.unimasoft.id/edugator/api/downloadBundle/a49fdc824fe7c4ac29ed8c7b460d7338/";
                                string path = Application.persistentDataPath + "/AssetsBundle/";
                                yield return StartCoroutine(DownloadFileLogic(urlDownloadModel, path, ".zip"));
                                if (DownloadPopup.userCancelledDownload == true) {
                                    progressBarGameObjectClone.SetActive(false);
                                    DownloadPopup.userCancelledDownload = false;
                                     
                                }
                                
                                PlayerPrefs.SetString("token", inputToken.text);
                                PlayerPrefs.SetInt("game_id", mainData.data.id);
                                titleText.text = mainData.data.name;
                                GameOwnerText.text = "Created By : " + mainData.data.author;
                                inputTokenUI.SetActive(false);

                                progressBarGameObjectClone.SetActive(false);

                            }
                            else {
                                StartCoroutine(IncorrectToken());
                            }
                        }
                        else {
                             print("API Request Json Success = False");
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

        if(table.childCount >= 6) {
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        else {
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        }
    }

    public void AddAccount() {
        Application.OpenURL("https://dev.unimasoft.id/edugator/signin");
    }

    //==============================================================================================================================//

    //Download Model
    //==============================================================================================================================//
    public string cardName;
    public string cardId;
    // List<string> files = new List<string>();
    public List<GameObject> files3D = new List<GameObject>();
    public List<Texture2D> filesCard = new List<Texture2D>();
    public string BUNDLETYPEMODEL = " (model)";
    public string BUNDLETYPECARD = " (card)";
    public IEnumerator DownloadFileLogic(string URLWithoutCardId, string savePath, string extention) {
        string[] files;

        files = Directory.GetFiles(Application.persistentDataPath + "/AssetsBundle/");
        

        bool fileIsAvailable;
                

        // fileName = Path.GetFileName(savePath);
        yield return null;
        if(files.Length == 0) {
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

    public void CheckFile() {
        string filePath = Application.persistentDataPath + "/AssetsBundle/fire extingusher (model)";

        AssetBundle bundleModel = AssetBundle.LoadFromFile(filePath);
        GameObject model = bundleModel.LoadAsset<GameObject>("Fire Extingusher.fbx");

        Instantiate(model);
        files3D.Add(model);
    }

    public IEnumerator InitializationBundleToObject() {
        string filePathModel = Application.persistentDataPath + "/AssetsBundle/" + cardName + BUNDLETYPEMODEL;
        string filePathCard = Application.persistentDataPath + "/AssetsBundle/" + cardName + BUNDLETYPECARD;
        
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

    public void SetSFX(float volume) {
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

    public IEnumerator ExtractFile() {
        string filePath = Application.persistentDataPath + "/AssetsBundle/";

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

    public void DeleteZipFile() {
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
    UnityWebRequest www;
    public IEnumerator DownloadFile(string URLWithoutCardId, string fileName, string cardId, string extention, string savePath) {
        yield return null;
        
        progressBarGameObjectClone =  Instantiate(progressBarGameObject);
        Image progressBarFilled;
        
        progressBarFilled = progressBarGameObjectClone.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<UnityEngine.UI.Image>();

        downloadFileURL = URLWithoutCardId + cardId;
        
        Debug.Log("URLFILE : " + downloadFileURL);
        //Create "Resources" Folder First
        filePath = savePath + fileName + extention;

        using(UnityWebRequest www = UnityWebRequest.Get(downloadFileURL)) {
            yield return new WaitForSeconds(0.1f);

            www.SendWebRequest();
            progressBarGameObjectClone.gameObject.SetActive(true);

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
            progressBarGameObjectClone.gameObject.SetActive(false);
        }
    }
    //==============================================================================================================================//
}