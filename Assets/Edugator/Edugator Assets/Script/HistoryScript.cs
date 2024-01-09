using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using TMPro;
using EasyUI.Progress;
using System.IO;

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
    

    private void Start()
    {
        cardIndex = 0;
        // fbxUrl = "https://cace-103-165-41-34.ngrok-free.app/edugator/public/api/downloadModel/a49fdc824fe7c4ac29ed8c7b460d7338/" + cardIdList[cardIndex].ToString();

        // print("Card Index : " + cardIdList[0]);
        print("FBXUrl : " + fbxUrl);

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

        url = "https://dev.birosolusi.com/edugator/public/api/getAllGames/a49fdc824fe7c4ac29ed8c7b460d7338";
    }

    public void StartQuiz()
    {
        StartCoroutine(GetDataFromAPI());
    }

    private void CallAllCoroutine()
    {
        StartCoroutine(GetDataFromAPI());
        // yield return StartCoroutine(DownloadingModel());
    }
    private IEnumerator GetDataFromAPI()
    {
        using(UnityWebRequest webData = UnityWebRequest.Get(url))
        {
            Progress.Show("Please Wait...", ProgressColor.Default);
            webData.SendWebRequest();

            while(!webData.isDone)
            {
                yield return null;
            }

            if(webData.result == UnityWebRequest.Result.ConnectionError || webData.result == UnityWebRequest.Result.ProtocolError)
            {
                Progress.Hide();
                StartCoroutine(NoConnection());
                Debug.Log("tidak ada Koneksi/Jaringan"); 
            }
            else
            {
                if(webData.isDone)
                {
                    Progress.Hide();
                    _jsonData = JSON.Parse(System.Text.Encoding.UTF8.GetString(webData.downloadHandler.data));
                    if(_jsonData == null)
                    {
                        Debug.Log("Json data Kosong");
                    }
                    else
                    {
                        for (int i = 0; i < _jsonData["data"].Count; i++)
                        {
                            if (token == _jsonData["data"][i]["token"])
                            {
                                PlayerPrefs.SetString("token", token);
                                PlayerPrefs.SetInt("game_id", _jsonData["data"][i]["id"]);

                                print("Jumlah Card : " + _jsonData["data"][i]["card"].Count);

                                // AddCardInArray(i);

                                titleText.text = _jsonData["data"][i]["nama"];
                                GameOwnerText.text = "Created By : " + _jsonData["data"][i]["pembuat"];

                                // yield return StartCoroutine(DownloadingModel());
                                
                                transform.parent.parent.parent.gameObject.SetActive(false);
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

    public void SetToken(TextMeshProUGUI tokenText)
    {
        token = tokenText.text;
    }

    private IEnumerator NoConnection()
    {
        koneksi.SetActive(true);
        yield return new WaitForSeconds(3f);
        koneksi.SetActive(false);
    }

    public void InstantiatePopup()
    {
        Instantiate(popup, PanelHistoryUI);
        // popup.SetActive(true);
    }

    public void SetGame(GameObject game)
    {
        gameSelected = game;
        PlayerPrefs.SetString("tokenSelected", token);
        print("Game Selected : " + gameSelected);
        print("Token Selected : " + PlayerPrefs.GetString("tokenSelected"));

    }

    //Download Model
    //==============================================================================================================================//
    private string fbxUrl;
    private string savePath;
    public List<int> cardIdList = new List<int>();
    public List<string> cardName = new List<string>();
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
    //         Progress.Hide();
    //         Debug.LogError("Gagal mengunduh file: " + www.error);
    //     }
    //     else
    //     {
    //         byte[] data = www.downloadHandler.data;

    //         // Simpan data ke dalam file di folder "Assets".
    //         File.WriteAllBytes(savePath, data);
            
    //         UnityEditor.AssetDatabase.Refresh();
    //         Progress.Hide();
    //         Debug.Log("File berhasil diunduh dan disimpan di " + savePath);
    //     }
    // }

    // private IEnumerator DownloadingModel()
    // {
    //     for(int i = 0; i < cardIdList.Count; i++)
    //     {
    //         fbxUrl = "https://dev.birosolusi.com/edugator/public/api/downloadModel/a49fdc824fe7c4ac29ed8c7b460d7338/" + cardIdList[cardIndex];
            
    //         savePath = $"Assets/3D Object/3D/{cardName[cardIndex]}.fbx";

    //         Progress.Show("Downloading...", ProgressColor.Default);
    //         yield return StartCoroutine(DownloadModel());
    //         cardIndex++;

    //         print("download 3d ke-" + i);
    //     }
    //     Progress.Hide();
    // }

    //==============================================================================================================================//
}
