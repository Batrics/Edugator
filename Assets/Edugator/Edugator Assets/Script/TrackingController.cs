using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using SimpleJSON;
using UnityEngine.Networking;
using EasyUI.Progress;
using UnityEditor.XR.ARSubsystems;
using Unity.VisualScripting;

[RequireComponent(typeof(ARTrackedImageManager))]
public class TrackingController : MonoBehaviour
{
    public List<GameObject> objectsToShow = new List<GameObject>();
    public Dictionary<string, Texture2D> cardReferenceImgae = new Dictionary<string, Texture2D>();
    private Dictionary<string, GameObject> gameObjectDictionary = new Dictionary<string, GameObject>();
    private ARTrackedImageManager trackedImageManager;
    private RuntimeReferenceImageLibrary library;

    [Header("Main")]
    private JSONNode _jsonData;
    private string url;
    private string getDataGamesUrl;
    private bool tracking = false;
    public GameObject ConnectionGameObject;
    // public GameObject cardPopup;
    public GameObject ParticleEffect;
    public GameObject playButton;
    [SerializeField] private RuntimeAnimatorController animatorController;
    
    void Awake()
    {
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        
        GameObject[] prefabs3D;
        prefabs3D = Resources.LoadAll<GameObject>("3dObject");

        Texture2D[] texture2Ds;
        texture2Ds = Resources.LoadAll<Texture2D>("CardImage");

        library = trackedImageManager.CreateRuntimeLibrary();
        trackedImageManager.referenceLibrary = library;

        foreach(Texture2D texture2D in texture2Ds)
        {
            print(texture2D.format);
            cardReferenceImgae.Add(texture2D.name, texture2D);
        }

        foreach(KeyValuePair<string, Texture2D> imageReference in cardReferenceImgae)
        {
            AddImage(imageReference.Key, imageReference.Value);
        }
        print("Card Reference Value : " + trackedImageManager.referenceLibrary[0].texture);

        foreach(GameObject obj in prefabs3D)
        {
            objectsToShow.Add(obj);
        }

        foreach(GameObject prefabs in objectsToShow)
        {
            GameObject newPrefabs = prefabs;
            newPrefabs.name = prefabs.name;
            gameObjectDictionary.Add(prefabs.name, newPrefabs);
        }

        // AddImage(texture2Ds, );
    }

    private void Start()
    {
        trackedImageManager.enabled = true;
        getDataGamesUrl = "https://dev.birosolusi.com/edugator/public/api/getDataGame/a49fdc824fe7c4ac29ed8c7b460d7338/c648b2afe7abb3ab4027edcd8e47eee2";

        PlayerPrefs.SetInt("game_id", 1);
        PlayerPrefs.SetInt("visualEffect", 1);

        foreach (KeyValuePair<string, GameObject> entry in gameObjectDictionary)
        {
            Debug.Log("Key: " + entry.Key + " Value: " + entry.Value);
        }

    }

    private void OnEnable()
    {
        print("Enable");
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        print("Disable");
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach(ARTrackedImage trackedImage in eventArgs.added)
        {
            print("TrackingState : " + tracking);
            getDataGamesUrl = "https://dev.birosolusi.com/edugator/public/api/getDataGame/a49fdc824fe7c4ac29ed8c7b460d7338/c648b2afe7abb3ab4027edcd8e47eee2";
            
            StartCoroutine(FirstTrackedImage(trackedImage));
        }

        foreach(ARTrackedImage trackedImage in eventArgs.updated)
        {
            // print(trackedImage.trackingState);
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                if(tracking == false)
                {
                    tracking = true;
                    StartCoroutine(UpdateImage(trackedImage, true));
                }
            }
            else
            {
                StartCoroutine(UpdateImage(trackedImage, false));
                playButton.SetActive(false);
                tracking = false;
                print("TrackingState : " + tracking);
            }
        }
    }

    private void AddImage(string name, Texture2D imageToAdd)
    {
        // if (!(ARSession.state == ARSessionState.SessionInitializing || ARSession.state == ARSessionState.SessionTracking))
        // {

        //     return; // Session state is invalid
        // }

        if (library is MutableRuntimeReferenceImageLibrary mutableLibrary)
        {
            mutableLibrary.ScheduleAddImageWithValidationJob(imageToAdd, name, 0.5f /* 50 cm */);
        }

        // print("reference Image : " + trackedImageManager.referenceLibrary.count);
        // print("reference Image name : " + trackedImageManager.referenceLibrary[0]);
    }

    private IEnumerator FirstTrackedImage(ARTrackedImage trackedImage)
    {
        url = "https://dev.birosolusi.com/edugator/public/api/getquestions/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetInt("game_id") + "/" + PlayerPrefs.GetInt("number_of_card");

        foreach (KeyValuePair<string, GameObject> go in gameObjectDictionary)
        {
            if (trackedImage.referenceImage.name == go.Key)
            {
                yield return StartCoroutine(GetDataFromAPIAndGetCardId(go));

                yield return StartCoroutine(GetDataFromAPIAndInstantiateObject(go.Value, trackedImage.transform));
            }
            else
            {
                    print("Error");
            }
        }
    }

    private IEnumerator UpdateImage(ARTrackedImage trackedImage, bool setActive)
    {
        foreach (KeyValuePair<string, GameObject> go in gameObjectDictionary)
        {
            if (trackedImage.referenceImage.name == go.Key)
            {
                yield return StartCoroutine(GetDataFromAPIAndGetCardId(go));
                
                for(int i = 0; i < trackedImage.transform.childCount; i++)
                {
                    trackedImage.transform.GetChild(i).gameObject.SetActive(setActive);
                }

                playButton.SetActive(setActive);
            }
            else
            {
                print("Error");
            }
        }

    }

    private void AnimationIn3DObject(GameObject entry, Transform transform)
    {
        GameObject object3d = Instantiate(entry, transform);
        Animator anim3dObject = object3d.AddComponent<Animator>();

        anim3dObject.runtimeAnimatorController = animatorController;
    }

    private IEnumerator GetDataFromAPIAndInstantiateObject(GameObject entry, Transform transform)
    {
        Progress.Show("Please Wait...", ProgressColor.Default);
        using (UnityWebRequest webData = UnityWebRequest.Get(url))
        {
            yield return webData.SendWebRequest();

            if (webData.result == UnityWebRequest.Result.ConnectionError || webData.result == UnityWebRequest.Result.ProtocolError)
            {
                Progress.Hide();
                Debug.Log("Tidak ada Koneksi/Jaringan");
                ConnectionGameObject.SetActive(true);
                yield return new WaitForSeconds(3f);
                ConnectionGameObject.SetActive(false);
            }
            else
            {
                if (webData.isDone)
                {
                    ConnectionGameObject.SetActive(false);
                    Progress.Hide();
                    _jsonData = JSON.Parse(System.Text.Encoding.UTF8.GetString(webData.downloadHandler.data));
                    if (_jsonData == null)
                    {
                        Debug.Log("Json data Kosong");
                    }
                    else
                    {
                        // print(_jsonData);
                        if (PlayerPrefs.GetString("token") != null)
                        {
                            int i = 0;
                            int total_soal_setiap_kartu = 0;

                            while (i < _jsonData["data"].Count)
                            {
                                total_soal_setiap_kartu++;
                                PlayerPrefs.SetInt("total_soal_setiap_kartu", total_soal_setiap_kartu);
                                i++;
                            }

                            if (_jsonData["data"].Count == 0)
                            {
                                // GameObject PopupCard = Instantiate(cardPopup);
                                // PopupCard.transform.SetParent(transform);
                                // PopupCard.transform.localPosition = new Vector3(0f, 0.5f, 0f);

                                Destroy(ParticleEffect);
                                playButton.SetActive(false);
                            }
                            else
                            {

                                if (PlayerPrefs.GetInt("visualEffect") == 1)
                                {
                                    GameObject particle = Instantiate(ParticleEffect);
                                    particle.transform.SetParent(transform);
                                    particle.transform.localPosition = new Vector3(0f, 0.1f, 0f);
                                    particle.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                                }
                                else
                                {
                                    Destroy(ParticleEffect);
                                }

                                AnimationIn3DObject(entry, transform);
                                playButton.SetActive(true);

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

    private IEnumerator GetDataFromAPIAndGetCardId(KeyValuePair<string, GameObject> target)
    {
        Progress.Show("Please Wait...", ProgressColor.Default);
        using (UnityWebRequest webData = UnityWebRequest.Get(getDataGamesUrl))
        {
            print("Waitt...");
            yield return webData.SendWebRequest();

            if (webData.result == UnityWebRequest.Result.ConnectionError || webData.result == UnityWebRequest.Result.ProtocolError)
            {
                Progress.Hide();
                Debug.Log("Tidak ada Koneksi/Jaringan");
                ConnectionGameObject.SetActive(true);
                yield return new WaitForSeconds(3f);
                ConnectionGameObject.SetActive(false);
            }
            else
            {
                if (webData.isDone)
                {
                    print("seccess");
                    ConnectionGameObject.SetActive(false);
                    Progress.Hide();
                    _jsonData = JSON.Parse(System.Text.Encoding.UTF8.GetString(webData.downloadHandler.data));
                    if (_jsonData == null)
                    {
                        Debug.Log("Json data Kosong");
                    }
                    else
                    {
                        if (_jsonData["success"] == true)
                        {
                            print("Search...");
                            for (int i = 0; i < _jsonData["data"]["card"].Count; i++)
                            {
                                print("Card Data : " + _jsonData["data"]["card"][i]);
                                if (target.Key == _jsonData["data"]["card"][i]["nama"])
                                {
                                    PlayerPrefs.SetInt("number_of_card", _jsonData["data"]["card"][i]["id"]);
                                    print("CARD ID " + i + " : " + PlayerPrefs.GetInt("number_of_card"));
                                }
                            }
                            print("Set Card Id Success");
                        }
                        else
                        {
                            print("State : Failed");
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
}
