using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.Networking;
using Loading.UI;
using System.IO;

public class LoginScript : MonoBehaviour
{
    public Users users;
    public AllGamesJson allGames;
    private GameObject progressBarGameObject;
    private GameObject progressBarGameObjectClone;
    [SerializeField] private GameObject cardHistory;
    [SerializeField] private GameObject gameHistory;
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TMP_Text name1;
    [SerializeField] private TMP_Text email1;
    [SerializeField] private TMP_Text name2;
    [SerializeField] private TMP_Text email2;
    [SerializeField] private GameObject guesUI;
    [SerializeField] private GameManagerMainMenu gameManagerMainMenu;
    LoadingUI loadingUI;
    public GameObject loginFailed;
    public List<string> gameNameList = new List<string>();
    public List<string> gameTokenList = new List<string>();
    public List<string> cardIdList = new List<string>();
    public List<Sprite> sprites = new List<Sprite>();
    // public GameObject loginUI;
    private void Awake() {
        PlayerPrefs.SetString("Json_Users", "{\"success\":true,\"data\":[{\"id\":1,\"username\":\"Bagas\",\"password\":\"123\",\"email\":\"example@gmail.com\",\"user_id\":8},{\"id\":2,\"username\":\"AnakPandai\",\"password\":\"SehatSehatCees\",\"email\":\"sayasiap432@gmail.com\",\"user_id\":16}]}");
        progressBarGameObject = Resources.Load<GameObject>("DownloadPopup");
        cardHistory = Resources.Load<GameObject>("UserData/Card");
        gameHistory = Resources.Load<GameObject>("UserData/GameInProfile");
        print("A : " + cardIdList);
        // print(cardIdList[0]);
    }
    private void Start(){
        loadingUI = gameManagerMainMenu.loadingUI;
        progressBarGameObjectClone =  Instantiate(progressBarGameObject);
        // loadingUI.Prepare();
    }
    public void loginBtn() => StartCoroutine(Login_Coroutine());
    private IEnumerator Login_Coroutine() {
        yield return null;
        users = JsonUtility.FromJson<Users>(PlayerPrefs.GetString("Json_Users"));
        if(users == null){
            print("Json data null");
        }
        else {
            if(users.success == true){
                for(int i = 0; i < users.data.Length; i++) {
                    if(username.text == users.data[i].username && password.text == users.data[i].password) {
                        PlayerPrefs.SetString("Login_State", "success");
                        PlayerPrefs.SetString("username", username.text);
                        PlayerPrefs.SetString("password", password.text);
                        PlayerPrefs.SetString("email", users.data[i].email);
                        PlayerPrefs.SetInt("user_id", users.data[i].user_id);
                        MainUserInfo();
                        // yield return StartCoroutine(User_Coroutine());
                        loadingUI.Show("Please Wait...");
                        StartCoroutine(gameManagerMainMenu.LoginSuccess());
                        gameManagerMainMenu.RefreshUserAccountBtn();
                        guesUI.SetActive(false);
                        gameObject.SetActive(false);
                        loadingUI.Hide();
                        yield return null;
                    }
                    else {
                        PlayerPrefs.SetString("Login_State", "failed");
                        loginFailed.SetActive(true);
                    }
                }
            }
        }
    }
    public void MainUserInfo() {
        name1.text = PlayerPrefs.GetString("username");
        email1.text = PlayerPrefs.GetString("email");
        name2.text = PlayerPrefs.GetString("username");
        email2.text = PlayerPrefs.GetString("email");
    }

    // public void User() => StartCoroutine(User_Coroutine());
    public IEnumerator User_Coroutine() {
        string getAllGames = "https://dev.unimasoft.id/edugator/api/getAllGames/a49fdc824fe7c4ac29ed8c7b460d7338";
        print("UserCoroutine");
        using(UnityWebRequest webData = UnityWebRequest.Get(getAllGames)) {
            // loadingUI.Show("Please Wait...");
            yield return webData.SendWebRequest();

            if(webData.result == UnityWebRequest.Result.ConnectionError || webData.result == UnityWebRequest.Result.ProtocolError) {
                // loadingUI.Hide();
                StartCoroutine(gameManagerMainMenu.NoConnection());
                Debug.Log("tidak ada Koneksi/Jaringan"); 
                
            }
            else {
                if(webData.isDone) {
                    // loadingUI.Hide();
                    string jsonString = webData.downloadHandler.text;
                    PlayerPrefs.SetString("AllGames", jsonString);
                    allGames = JsonUtility.FromJson<AllGamesJson>(PlayerPrefs.GetString("AllGames"));
                    users = JsonUtility.FromJson<Users>(PlayerPrefs.GetString("Json_Users"));

                    if(allGames == null) {
                        Debug.Log("Data Json null");
                    }
                    else {
                        print(allGames.data.Length);
                        int cardLength = 0;
                        int numberOfCard = 0;
                        // int numberOfCardid = 0;
                        for(int i = 0; i < allGames.data.Length; i++) {
                            if(allGames.data[i].user_id == PlayerPrefs.GetInt("user_id")){
                                gameNameList.Add(allGames.data[i].name);
                                gameTokenList.Add(allGames.data[i].token);
                                foreach(var card in allGames.data[i].cards){
                                    cardLength++;
                                    PlayerPrefs.SetInt("AllgamesDataLength", allGames.data.Length);
                                    PlayerPrefs.SetString("cardName" + numberOfCard, card.name);
                                    PlayerPrefs.SetString("cardId" + numberOfCard, card.id);
                                    cardIdList.Add(card.id);
                                    print("Card Name" + numberOfCard +  " : " + PlayerPrefs.GetString("cardName" + numberOfCard));
                                    print("Card Id" + numberOfCard +  " : " + PlayerPrefs.GetString("cardId" + numberOfCard));

                                    progressBarGameObjectClone.SetActive(false);
                                    numberOfCard++;
                                }
                            }
                        }
                        // Download Assets
                        loadingUI.Hide();

                        // progressBarGameObjectClone.SetActive(true);
                        string urlDownloadCardImage = "https://dev.unimasoft.id/edugator/api/downloadBundle/a49fdc824fe7c4ac29ed8c7b460d7338/";
                        string path = Application.persistentDataPath + "/AssetsBundle/";
                        yield return StartCoroutine(DownloadFileLogic(urlDownloadCardImage, path, ".zip"));
                        
                        if (DownloadPopup.userCancelledDownload == true) {
                            DownloadPopup.userCancelledDownload = false;
                            progressBarGameObjectClone.SetActive(false);
                        }
                        else {
                            foreach(var textureCard in gameManagerMainMenu.filesCard){
                                sprites.Add(TextureToSprite(textureCard, textureCard.name));
                            }
                        }
                    }
                }
            }
        }
    }

    public IEnumerator DownloadFileLogic(string URLWithoutCardId, string savePath, string extention) {
        string[] files;

        files = Directory.GetFiles(Application.persistentDataPath + "/AssetsBundle/");
        

        bool fileIsAvailable;
                
        string cardName;
        string cardId;

        // fileName = Path.GetFileName(savePath);
        yield return null;
        if(files.Length == 0) {            
            // loadingUI.Show("Download Assets...");
            for(int i = 0; i < cardIdList.Count; i++) {
                cardName = PlayerPrefs.GetString("cardName" + i);
                cardId = PlayerPrefs.GetString("cardId" + i);
                yield return StartCoroutine(gameManagerMainMenu.DownloadFile(URLWithoutCardId, cardName, cardId, extention, savePath));
                if (DownloadPopup.userCancelledDownload == true) {
                    yield break;
                }
                yield return StartCoroutine(gameManagerMainMenu.ExtractFile());
                gameManagerMainMenu.DeleteZipFile();
                yield return StartCoroutine(InitializationBundleToObject(cardName));
            }
        }
        else {
            for (int i = 0; i < cardIdList.Count; i++) {
                cardName = PlayerPrefs.GetString("cardName" + i);
                cardId = PlayerPrefs.GetString("cardId" + i);
                fileIsAvailable = false;

                foreach(string file in files) {
                    if(file.Contains(cardName.ToLower())) {
                        fileIsAvailable = true;
                    }
                }

                if(fileIsAvailable == true) {
                    Debug.Log("File is Available");
                    yield return StartCoroutine(InitializationBundleToObject(cardName));
                }
                else {
                    // loadingUI.Show("Download Assets...");
                    yield return StartCoroutine(gameManagerMainMenu.DownloadFile(URLWithoutCardId, cardName, cardId, extention, savePath));
                    if (DownloadPopup.userCancelledDownload == true) {
                        yield break;
                    }
                    yield return StartCoroutine(gameManagerMainMenu.ExtractFile());
                    gameManagerMainMenu.DeleteZipFile();
                    yield return StartCoroutine(InitializationBundleToObject(cardName));
                    print("CName : " + cardName);
                }
            }
        }
    }

    public Sprite TextureToSprite(Texture2D texture, string name)
    {
        // Check if the texture is not null
        if (texture == null)
        {
            Debug.LogError("Texture2D is null!");
            return null;
        }

        // Create a new sprite from the texture
        Sprite newSprite = Sprite.Create(
            texture, 
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), 
            100.0f
        );

        newSprite.name = name;
        return newSprite;
    }

    public IEnumerator InitializationBundleToObject(string cardName) {
        string filePathModel = Application.persistentDataPath + "/AssetsBundle/" + cardName + gameManagerMainMenu.BUNDLETYPEMODEL;
        string filePathCard = Application.persistentDataPath + "/AssetsBundle/" + cardName + gameManagerMainMenu.BUNDLETYPECARD;
        
        AssetBundle bundleCard = AssetBundle.LoadFromFile(filePathCard);
        Texture2D card = bundleCard.LoadAsset<Texture2D>(cardName + ".png");
        card.name = cardName;
        yield return card;
        gameManagerMainMenu.filesCard.Add(card);

        AssetBundle bundleModel = AssetBundle.LoadFromFile(filePathModel);
        GameObject model = bundleModel.LoadAsset<GameObject>(cardName + ".fbx");
        model.name = cardName;
        yield return model;
        gameManagerMainMenu.files3D.Add(model);
    }
    public void CardList() {
        Transform cardList = GameObject.Find("CardList").GetComponent<Transform>();
        if(cardList.childCount != 0){
            for(int i = 0; i < cardList.childCount; i++) {
                Destroy(cardList.GetChild(i).gameObject);
            }
            for(int i = 0; i < sprites.Count; i++) {
                GameObject card = Instantiate(cardHistory, cardList);
                UnityEngine.UI.Image cardImage = card.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
                TMP_Text cardName = card.transform.GetChild(1).GetComponent<TMP_Text>();
                cardImage.sprite = sprites[i];
                cardName.text = sprites[i].name;
            }
        }
        else {
            for(int i = 0; i < sprites.Count; i++) {
                GameObject card = Instantiate(cardHistory, cardList);
                UnityEngine.UI.Image cardImage = card.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
                TMP_Text cardName = card.transform.GetChild(1).GetComponent<TMP_Text>();
                cardImage.sprite = sprites[i];
                cardName.text = sprites[i].name;
            }
        }
    }
    public void GameList() {
        Transform gameList = GameObject.Find("GameList").GetComponent<Transform>();
        if(gameList.childCount != 0) {
            for(int i = 0; i < gameList.childCount; i++) {
                Destroy(gameList.GetChild(i).gameObject);
            }
            for(int i = 0; i < gameNameList.Count; i++) {
                GameObject game = Instantiate(gameHistory, gameList);
                TMP_Text gameName = game.transform.GetChild(2).GetComponent<TMP_Text>();
                TMP_Text gametoken = game.transform.GetChild(4).GetComponent<TMP_Text>();
                gameName.text = gameNameList[i];
                gametoken.text = gameTokenList[i];
            }
        }
        else {
            for(int i = 0; i < gameNameList.Count; i++) {
                GameObject game = Instantiate(gameHistory, gameList);
                TMP_Text gameName = game.transform.GetChild(2).GetComponent<TMP_Text>();
                TMP_Text gametoken = game.transform.GetChild(4).GetComponent<TMP_Text>();
                gameName.text = gameNameList[i];
                gametoken.text = gameTokenList[i];
            }
        }
    }
}
