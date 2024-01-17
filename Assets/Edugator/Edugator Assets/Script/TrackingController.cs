using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using SimpleJSON;
using UnityEngine.Networking;
using Loading.UI;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using Unity.Jobs;
using Unity.Collections;
using System;
using Unity.VisualScripting;


struct DeallocateJob : IJob
{
    [DeallocateOnJobCompletion]
    public NativeArray<byte> data;

    public void Execute() { }
    }


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
    public GameObject ParticleEffect;
    public GameObject playButton;
    [SerializeField] private RuntimeAnimatorController animatorController;
    private LoadingUI loadingUI = new LoadingUI();

    void Awake()
    {
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        print("aaa ; " + trackedImageManager.descriptor.supportsMutableLibrary);
        
        GameObject[] prefabs3D;
        prefabs3D = Resources.LoadAll<GameObject>("3dObject");

        Texture2D[] texture2Ds;
        texture2Ds = Resources.LoadAll<Texture2D>("CardImage");

        library = trackedImageManager.CreateRuntimeLibrary();
        trackedImageManager.referenceLibrary = library;

        print("Reference Image Library : " + trackedImageManager.referenceLibrary);

        // string imagePath = "Assets/Resources/CardImage/Gold Fish.jpg";
        // // Texture2D imageTexture = new Texture2D(2, 2);  // Dummy texture, just for size initialization
        // byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
        // texture2Ds[1].LoadImage(imageBytes);

        // // Convert Texture2D to grayscale NativeArray<byte>
        // NativeArray<byte> grayscaleImageBytes = new NativeArray<byte>(texture2Ds[1].GetRawTextureData<byte>(), Allocator.Persistent);

        // // Call the AddImage function
        // AddImage(grayscaleImageBytes, texture2Ds[1].width, texture2Ds[1].height, 0.2f);

        // // Dispose the NativeArray when done
        // grayscaleImageBytes.Dispose();


        // SetNonPowerOf2(texture2Ds[0]);

        // TextureImporter textureImporter;
        // string format;
        // string texturePath;

        

        foreach(Texture2D texture2D in texture2Ds)
        {
            // format = ".jpg";
            // do
            // {
            //     texturePath = $"Assets/Resources/CardImage/{texture2D.name}{format}";
            //     textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            //     if(textureImporter == null && format == ".jpg")
            //     {
            //         format = ".png";
            //     }
            //     else if(textureImporter == null && format == ".png")
            //     {
            //         format = ".jpeg";
            //     }
            //     else
            //     {
            //         TextureImporterSettings texSetting = new TextureImporterSettings();
                    
            //         textureImporter.ReadTextureSettings(texSetting);
            //         texSetting.npotScale = TextureImporterNPOTScale.None;
            //         textureImporter.SetTextureSettings(texSetting);
                    
            //         textureImporter.isReadable = true;
                    
            //         AssetDatabase.ImportAsset(texturePath);
            //     }

            // }while(textureImporter == null);

            // Texture2D newTexture = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBA32, texture2D.mipmapCount > 1);
            // newTexture.name = texture2D.name;
            
            // Graphics.CopyTexture(texture2D, newTexture);

            // print("Texture 2d format : " + texture2D.format);
            // print("Texture 2d format : " + newTexture.format);
            // print("mip Map Count" + texture2D.mipmapCount);
            // print("Texture2d : " + texture2D.GetPixels32(0));
            // print("new Texture : " + newTexture.GetPixels32(0));
            print("Readable : " + texture2D.isReadable);
            
            cardReferenceImgae.Add(texture2D.name, texture2D);
        }

        foreach(KeyValuePair<string, Texture2D> imageReference in cardReferenceImgae)
        {
            AddImages(imageReference.Key, imageReference.Value);
            // print("FORMATTT : " + imageReference.Value.format);
        }

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

        loadingUI.Prepare();
    }

    private void Start()
    {
        trackedImageManager.enabled = true;

        PlayerPrefs.SetInt("game_id", 11);
        PlayerPrefs.SetInt("visualEffect", 1);
        getDataGamesUrl = "https://dev.unimasoft.id/edugator/api/getDataGame/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetString("token");
        PlayerPrefs.SetString("token", "44736ebf1ac169b4d5e7d174ca1f8b8e");

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
            print("Reference Image : " + trackedImage.referenceImage);
            getDataGamesUrl = "https://dev.unimasoft.id/edugator/api/getDataGame/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetString("token");
            
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
                    StartCoroutine(UpdateImage(trackedImage));
                }
            }
            else
            {
                resetData(trackedImage);
                playButton.SetActive(false);
                tracking = false;
                print("TrackingState : " + tracking);
            }
        }
    }

    // private void AddImages(string name, Texture2D imageToAdd)
    // {
    //     if (library is MutableRuntimeReferenceImageLibrary mutableLibrary)
    //     {
    //         mutableLibrary.ScheduleAddImageWithValidationJob(imageToAdd, name, 0.5f);
    //     }

    //     print("jumlah reference Image : " + trackedImageManager.referenceLibrary.count);
    //     print("reference Image name : " + trackedImageManager.referenceLibrary[0]);
    //     print("FORMATTT : " + trackedImageManager.referenceLibrary[0].texture.format);
    // }

    private void AddImages(string imageName, Texture2D imageToAdd)
    {
        if (library is MutableRuntimeReferenceImageLibrary mutableLibrary)
        {
            // Convert Texture2D to NativeSlice<byte>
            NativeSlice<byte> imageBytes = new NativeSlice<byte>(imageToAdd.GetRawTextureData<byte>());

            // Create a Vector2Int for the size of the image
            Vector2Int imageSize = new Vector2Int(imageToAdd.width, imageToAdd.height);

            // Choose the appropriate TextureFormat
            TextureFormat textureFormat = TextureFormat.RGBA32;

            // Create an XRReferenceImage
            XRReferenceImage referenceImage = new XRReferenceImage(
                SerializableGuid.empty,
                SerializableGuid.empty,
                imageSize,    
                imageName,
                imageToAdd
            );

            // Get the JobHandle from the ScheduleAddImageWithValidationJob method
            mutableLibrary.ScheduleAddImageWithValidationJob(imageBytes, imageSize, textureFormat, referenceImage);
        }

        // Print information after adding the image
        print("jumlah reference Image : " + trackedImageManager.referenceLibrary.count);
        print("reference Image name : " + trackedImageManager.referenceLibrary[0]);
        print("reference Image Formattttt : " + trackedImageManager.referenceLibrary[0].texture.graphicsFormat);
    }

    private IEnumerator FirstTrackedImage(ARTrackedImage trackedImage)
    {
        url = "https://dev.unimasoft.id/edugator/api/getquestions/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetInt("game_id") + "/" + PlayerPrefs.GetInt("number_of_card");

        foreach (KeyValuePair<string, GameObject> go in gameObjectDictionary)
        {
            if (trackedImage.referenceImage.name == go.Key)
            {
                // loadingUI.Show("Please Wait...");
                yield return StartCoroutine(GetDataFromAPIAndGetCardId(go));

                StartCoroutine(GetDataFromAPIAndInstantiateObject(go.Value, trackedImage.transform));
            }
            else
            {
                    print("Error");
            }
        }
    }

    private IEnumerator UpdateImage(ARTrackedImage trackedImage)
    {
        foreach (KeyValuePair<string, GameObject> go in gameObjectDictionary)
        {
            if (trackedImage.referenceImage.name == go.Key)
            {
                // loadingUI.Show("Please Wait...");
                yield return StartCoroutine(GetDataFromAPIAndGetCardId(go));
                
                for(int i = 0; i < trackedImage.transform.childCount; i++)
                {
                    trackedImage.transform.GetChild(i).gameObject.SetActive(true);
                }

                playButton.SetActive(true);
            }
            else
            {
                print("Error");
            }
        }
    }

    private void resetData(ARTrackedImage trackedImage)
    {
        foreach (KeyValuePair<string, GameObject> go in gameObjectDictionary)
        {
            if (trackedImage.referenceImage.name == go.Key)
            {
                for(int i = 0; i < trackedImage.transform.childCount; i++)
                {
                    trackedImage.transform.GetChild(i).gameObject.SetActive(false);
                }

                playButton.SetActive(false);
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

        // object3d.SetActive(true);
        anim3dObject.runtimeAnimatorController = animatorController;
    }

    private IEnumerator GetDataFromAPIAndInstantiateObject(GameObject entry, Transform transform)
    {
        // Progress.Show("Please Wait...", ProgressColor.Default);
        loadingUI.Show("Please Wait...");
        using (UnityWebRequest webData = UnityWebRequest.Get(url))
        {
            yield return webData.SendWebRequest();

            if (webData.result == UnityWebRequest.Result.ConnectionError || webData.result == UnityWebRequest.Result.ProtocolError)
            {
                // Progress.Hide();
                loadingUI.Hide();
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
                    // Progress.Hide();
                    loadingUI.Hide();
                    _jsonData = JSON.Parse(System.Text.Encoding.UTF8.GetString(webData.downloadHandler.data));
                    if (_jsonData == null)
                    {
                        Debug.Log("Json data Kosong");
                    }
                    else
                    {
                        print(_jsonData);
                        if (_jsonData["success"] == true)
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

                                // Destroy(ParticleEffect);
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
                                print("INSTANTIATE OBJECT AND ANIMATION");
                                // Instantiate(entry, transform);
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
        // Progress.Show("Please Wait...", ProgressColor.Default);
        loadingUI.Show("Please Wait...");
        using (UnityWebRequest webData = UnityWebRequest.Get(getDataGamesUrl))
        {
            print("Waitt...");
            yield return webData.SendWebRequest();

            if (webData.result == UnityWebRequest.Result.ConnectionError || webData.result == UnityWebRequest.Result.ProtocolError)
            {
                // Progress.Hide();
                loadingUI.Hide();
                Debug.Log("Tidak ada Koneksi/Jaringan");
                ConnectionGameObject.SetActive(true);
                yield return new WaitForSeconds(3f);
                ConnectionGameObject.SetActive(false);
            }
            else
            {
                if (webData.isDone)
                {
                    print("success");
                    ConnectionGameObject.SetActive(false);
                    // Progress.Hide();
                    loadingUI.Hide();
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
                            for (int i = 0; i < _jsonData["data"]["cards"].Count; i++)
                            {
                                // print("Card Data : " + _jsonData["data"]["cards"].Count);
                                // print("target key : " + target.Key);
                                // print("JSON DATA : " + _jsonData["data"]["cards"][i]["name"]);
                                if (target.Key == _jsonData["data"]["cards"][i]["name"])
                                {
                                    PlayerPrefs.SetInt("number_of_card", _jsonData["data"]["cards"][i]["id"]);
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
