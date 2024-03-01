using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Networking;
using Loading.UI;
using Unity.Collections;
using TMPro;

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
    private string jsonstring;
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
    Transform model;
    Transform particleParent;
    Transform particleChild1;
    Transform particleChild2;
    Transform particleChild3;

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
        playButton.SetActive(false);
        
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
            // StartCoroutine(FirstTrackedImage(trackedImage));
        }

        foreach(ARTrackedImage trackedImage in eventArgs.updated) {
            if (trackedImage.trackingState == TrackingState.Tracking) {
                if(tracking == false) {
                    tracking = true;

                    UpdateImage(trackedImage);
                    // StartCoroutine(UpdateImage(trackedImage));
                }
            }
            else {
                resetData(trackedImage);
                playButton.SetActive(false);
                tracking = false;
                print("TrackingState : " + tracking);
            }

            if(particleChild1 != null && particleChild2 != null && particleChild3 != null && model != null && particleParent != null) { 
                particleParent.localScale = new Vector3(trackedImage.referenceImage.size.x, trackedImage.referenceImage.size.x, trackedImage.referenceImage.size.x);
                model.localScale = new Vector3(trackedImage.referenceImage.size.x, trackedImage.referenceImage.size.x, trackedImage.referenceImage.size.x);
                particleChild1.localScale = particleParent.localScale;
                particleChild2.localScale = particleParent.localScale;
                particleChild3.localScale = particleParent.localScale;

                if (PlayerPrefs.GetInt("visualEffect") == 1) {
                    particleParent.gameObject.SetActive(true);
                } else {
                    particleParent.gameObject.SetActive(false);
                }
                print("A");
            }
            else {
                print("PARTICLE CHILD = null");
            }
        }

    }
    private IEnumerator AddImages(string imageName, Texture2D imageToAdd) {
        yield return null;
                

        if(trackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary) {
            yield return referenceImageJobState = mutableLibrary.ScheduleAddImageWithValidationJob(imageToAdd, imageName, 0.21f);

            while(!referenceImageJobState.jobHandle.IsCompleted) {                
            }

            referenceImageJobState.jobHandle.Complete();
            print("IMAAAAAGEE : " + mutableLibrary[0]);
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
            print("A : " + trackedImage.referenceImage.name);
            print("B : " + go.Key);
            if (trackedImage.referenceImage.name  == go.Key) {
                GetDataFromAPIAndGetCardId(go);
                Instantiate3dObject(go.Value, trackedImage);

                particleParent = trackedImage.transform.GetChild(0).GetComponent<Transform>();
                model = trackedImage.transform.GetChild(1).GetComponent<Transform>();
                particleChild1 = particleParent.transform.GetChild(0).GetComponent<Transform>();
                particleChild2 = particleParent.transform.GetChild(1).GetComponent<Transform>();
                particleChild3 = particleParent.transform.GetChild(2).GetComponent<Transform>();

                if (PlayerPrefs.GetInt("visualEffect") == 1) {
                    particleParent.gameObject.SetActive(true);
                    print("GAMEOBJECT ACtive");
                } else {
                    particleParent.gameObject.SetActive(false);
                    print("GAMEOBJECT inACtive");
                    // Destroy(ParticleEffect);
                }
            }
            else {
                    print("Error");
            }
        }
    }

    private void UpdateImage(ARTrackedImage trackedImage) {

        foreach (KeyValuePair<string, GameObject> go in gameObjectDictionary) {
            if (trackedImage.referenceImage.name  == go.Key) {
                GetDataFromAPIAndGetCardId(go);
                // yield return StartCoroutine(GetDataFromAPIAndGetCardId(go, trackedImage));
                
                for(int i = 0; i < trackedImage.transform.childCount; i++) {
                    trackedImage.transform.GetChild(i).gameObject.SetActive(true);
                }
                
                if(go.Value.activeSelf == true)
                    playButton.SetActive(true);
            }
            else {
                print("Error");
            }
        }
    }

    private void resetData(ARTrackedImage trackedImage) {
        foreach (KeyValuePair<string, GameObject> go in gameObjectDictionary) {
            if (trackedImage.referenceImage.name  == go.Key) {
                for(int i = 0; i < trackedImage.transform.childCount; i++) {
                    trackedImage.transform.GetChild(i).gameObject.SetActive(false);
                }

                playButton.SetActive(false);
            }
            else {
                print("Error");
            }
        }
    }
    private void AnimationIn3DObject(GameObject entry, Transform transform) {
        GameObject object3d = Instantiate(entry, transform);
        Animator anim3dObject = object3d.AddComponent<Animator>();

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
            Texture2D card = bundleCard.LoadAsset<Texture2D>(cardName + ".jpg");
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

    private void Instantiate3dObject(GameObject target, ARTrackedImage trackedImage) {
        GameObject particle = Instantiate(ParticleEffect);
        particle.transform.SetParent(trackedImage.transform);
        particle.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        particle.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        
        AnimationIn3DObject(target, trackedImage.transform);
        playButton.SetActive(true);
    }

    public void Information() {
        print(model);
        print(particleParent);
        print(particleChild1);
        print(particleChild2);
        print(particleChild3);
    }
}
