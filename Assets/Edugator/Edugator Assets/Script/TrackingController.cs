using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Loading.UI;
using Unity.Collections;
using TMPro;
using Unity.VisualScripting;

public class TrackingController : MonoBehaviour
{
    public List<GameObject> objectsToShow = new List<GameObject>();
    public Dictionary<string, Texture2D> cardReferenceImgae = new Dictionary<string, Texture2D>();    private Dictionary<string, GameObject> gameObjectDictionary = new Dictionary<string, GameObject>();
    private ARTrackedImageManager trackedImageManager;
    private RuntimeReferenceImageLibrary library;
    public XRReferenceImageLibrary referenceImagesLibrary;
    private string cardName;

    [Header("Main")]
    MainDataJson mainData;
    private string url;
    private string getDataGamesUrl;
    private bool tracking = false;
    public GameObject ConnectionGameObject;
    public GameObject ParticleEffect;
    public GameObject playButton;
    [SerializeField] private RuntimeAnimatorController animatorController;
    private LoadingUI loadingUI = new LoadingUI();
    AddReferenceImageJobState referenceImageJobState;
    [SerializeField] List<GameObject> prefabs3D = new List<GameObject>();
    [SerializeField] List<Texture2D> textures2D = new List<Texture2D>();
    GameObject model;
    GameObject particleParent;
    Transform particleChild1;
    Transform particleChild2;
    Transform particleChild3;
    public TextMeshProUGUI infoForDev;

    void Awake() {
        AssetBundle.UnloadAllAssetBundles(true);
        
        trackedImageManager = gameObject.AddComponent<ARTrackedImageManager>();

        library = trackedImageManager.CreateRuntimeLibrary();
        trackedImageManager.referenceLibrary = library;
        

        loadingUI.Prepare();
        // PlayerPrefs.SetString("token", "fe0e50396723b6dbe04c21afff6349c7");
        // PlayerPrefs.SetInt("game_id", 13);
        // PlayerPrefs.SetInt("visualEffect", 1);
    }

    private IEnumerator Start() {

        getDataGamesUrl = "https://dev.unimasoft.id/edugator/api/getDataGame/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetString("token");
        
        yield return StartCoroutine(CheckFiles());
        
        foreach(Texture2D texture2D in textures2D) {
            cardReferenceImgae.Add(texture2D.name, texture2D);
            if(cardReferenceImgae != null) {                
            }
            print(texture2D);
        }

        foreach(KeyValuePair<string, Texture2D> imageReference in cardReferenceImgae) {
           if (imageReference.Value != null) {
                NativeArray<byte> imageBytes =  new NativeArray<byte>(imageReference.Value.GetRawTextureData(), Allocator.Persistent);

                var aspectRatio = (float)imageReference.Value.width / (float)imageReference.Value.height;
                var sizeInMeters = new Vector2(imageReference.Value.width, imageReference.Value.width * aspectRatio);

                StartCoroutine(AddImages(imageReference.Key, imageReference.Value));
            }
            else {
                Debug.LogError("Failed to load image from Resources");
            }
            

        }

        foreach(GameObject obj in prefabs3D) {
            objectsToShow.Add(obj);
        }

        foreach(GameObject prefabs in objectsToShow) {
            GameObject newPrefabs = prefabs;
            newPrefabs.name = prefabs.name;
            gameObjectDictionary.Add(prefabs.name, newPrefabs);
        }
        
        trackedImageManager.enabled = true;

    }

    private void OnEnable() {
        print("Enable");
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable() {
        print("Disable");
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        foreach(ARTrackedImage trackedImage in eventArgs.added) {
            print("Reference Image : " + trackedImage.referenceImage);
            getDataGamesUrl = "https://dev.unimasoft.id/edugator/api/getDataGame/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetString("token");
            
            FirstTrackedImage(trackedImage);
            playButton.SetActive(true);
            print("Reference Image Name : " + trackedImage.referenceImage.name);
            print("Reference Image Size : " + trackedImage.referenceImage.size);
            
            // modelScale = particleParent.transform.localScale / 5;
            infoForDev.text = "First tracked";
        }

        foreach(ARTrackedImage trackedImage in eventArgs.updated) {
            print(eventArgs);
            print(trackedImage.trackingState);
            // print(trackedImage.transform.GetChild(0));
            infoForDev.text = trackedImage.ToString();

            if (trackedImage.trackingState == TrackingState.Tracking) {
                if(tracking == false) {
                    UpdateImage(trackedImage);
                    playButton.SetActive(true);
                    // infoForDev.text = "First tracked\nPlay button Active";
                }
                tracking = true;
            }
            else {
                tracking = false;
                playButton.SetActive(false);
                ResetData(trackedImage);
                // infoForDev.text = "First tracked\nPlay button InActive";
            }

            if (PlayerPrefs.GetInt("visualEffect") == 1) {
                particleParent.transform.localScale = new Vector3(trackedImage.referenceImage.size.x, trackedImage.referenceImage.size.x, trackedImage.referenceImage.size.x);
                // particleParent.transform.localScale = new Vector3(1f, 1f, 1f);
                particleChild1.localScale = particleParent.transform.localScale;
                particleChild2.localScale = particleParent.transform.localScale;
                particleChild3.localScale = particleParent.transform.localScale;
            }
            
            model.transform.localScale = new Vector3(trackedImage.referenceImage.size.x, trackedImage.referenceImage.size.x, trackedImage.referenceImage.size.x); 
            infoForDev.text = trackedImage.referenceImage.name + " : " + trackedImage.referenceImage.size + "\n" + particleParent.name + " : " + particleParent.transform.localScale.ToString() + "\n" + model.name + " : " + model.transform.localScale.ToString();
            // model.transform.localScale = new Vector3(1f, 1f, 1f);
        }

    }
    private IEnumerator AddImages(string imageName, Texture2D imageToAdd) {
        yield return null;

        if(trackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary) {
            infoForDev.text = "1";
            
            yield return referenceImageJobState = mutableLibrary.ScheduleAddImageWithValidationJob(imageToAdd, imageName, 0.21f);

            infoForDev.text = "1\n2";

            referenceImageJobState.jobHandle.Complete();
            infoForDev.text = "1\n2\nReference Image JobState : " + referenceImageJobState;

            print("Reference Image JobState : " + referenceImageJobState);
        }
        else {
            Debug.LogError("Reference library is not MutableRuntimeReferenceImageLibrary.");
        }        

    }

    private void FirstTrackedImage(ARTrackedImage trackedImage) {
        url = "https://dev.unimasoft.id/edugator/api/getquestions/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetInt("game_id") + "/" + PlayerPrefs.GetInt("number_of_card");
        print("URL : " + url);
        print(trackedImage.gameObject);
        foreach (KeyValuePair<string, GameObject> go in gameObjectDictionary) {
            if (trackedImage.referenceImage.name == go.Key) {
                GetDataFromAPIAndGetCardId(go);
                Instantiate3DObject(go.Value, trackedImage.transform);
                InstantiateParticleSystem(trackedImage);

                particleChild1 = particleParent.transform.GetChild(0).GetComponent<Transform>();
                particleChild2 = particleParent.transform.GetChild(1).GetComponent<Transform>();
                particleChild3 = particleParent.transform.GetChild(2).GetComponent<Transform>();
                
            }
            else {
                    print("Error");
            }
        }
    }

    private void UpdateImage(ARTrackedImage trackedImage) {
        foreach (KeyValuePair<string, GameObject> go in gameObjectDictionary) {
            if (trackedImage.referenceImage.name == go.Key) {
                GetDataFromAPIAndGetCardId(go);
                
                for(int i = 0; i < trackedImage.transform.childCount; i++) {
                    trackedImage.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
            else {
                print("Error");
            }
        }
    }

    private void ResetData(ARTrackedImage trackedImage) {
        foreach (KeyValuePair<string, GameObject> go in gameObjectDictionary) {
            if (trackedImage.referenceImage.name == go.Key) {
                for(int i = 0; i < trackedImage.transform.childCount; i++) {
                    trackedImage.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            else {
                print("Error");
            }
        }
    }
    private void Instantiate3DObject(GameObject entry, Transform transform) {
        model = Instantiate(entry, transform);
        Animator anim3dObject = model.AddComponent<Animator>();

        anim3dObject.runtimeAnimatorController = animatorController;
    }

    private IEnumerator CheckFiles() {

        string filePath;

        mainData = JsonUtility.FromJson<MainDataJson>(PlayerPrefs.GetString("jsonData"));

        for(int j = 0; j < mainData.data.cards.Length ; j++) {
            cardName = mainData.data.cards[j].name;
            
            //Harus diganti ke path Local
            filePath = Application.persistentDataPath + "/AssetsBundle/" + cardName + " " +  "(model)";

            AssetBundle bundleModel = AssetBundle.LoadFromFile(filePath);
            GameObject model = bundleModel.LoadAsset<GameObject>(cardName + ".fbx");
            prefabs3D.Add(model);

            filePath = Application.persistentDataPath + "/AssetsBundle/" + cardName + " " + "(card)";

            AssetBundle bundleCard = AssetBundle.LoadFromFile(filePath);
            Texture2D card = null;
            if(card == null) {
                card = bundleCard.LoadAsset<Texture2D>(cardName + ".png");
                if(card == null) {
                    card = bundleCard.LoadAsset<Texture2D>(cardName + ".jpg");
                    if(card == null) {
                        card = bundleCard.LoadAsset<Texture2D>(cardName + ".jpeg");
                        print("File Extention is jpeg");
                    }
                }
            }
            yield return card;
            textures2D.Add(card);
            
        }
    }
    private void GetDataFromAPIAndGetCardId(KeyValuePair<string, GameObject> target) {

        mainData = JsonUtility.FromJson<MainDataJson>(PlayerPrefs.GetString("jsonData"));

        if (mainData == null) {
            Debug.Log("Json data Kosong");
        }
        else {
            if (mainData.success == true) {
                print("Search...");
                for (int i = 0; i < mainData.data.cards.Length; i++) {
                    if (target.Key == mainData.data.cards[i].name) {
                        print("CARD ID JSON : " + mainData.data.cards[i].id);
                        PlayerPrefs.SetString("number_of_card", mainData.data.cards[i].id);
                        print("CARD ID " + i + " : " + PlayerPrefs.GetString("number_of_card"));
                    }
                }
                print("Set Card Id Success");
            } else {
                print("State : Failed");
            }
        }
    }

    private void InstantiateParticleSystem(ARTrackedImage trackedImage) {
        particleParent = Instantiate(ParticleEffect, trackedImage.transform);

        if (PlayerPrefs.GetInt("visualEffect") == 1) {
            particleParent.SetActive(true);
            print($"GAMEOBJECT {particleParent} ACtive");
        } else {
            // particleParent.SetActive(false);
            print($"GAMEOBJECT {particleParent} inACtive");
            Destroy(particleParent);
        }
    }

    public void Information() {
        print(model);
        print(particleParent);
        print(particleChild1);
        print(particleChild2);
        print(particleChild3);
    }
}
