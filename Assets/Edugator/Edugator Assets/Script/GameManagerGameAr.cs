using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using EasyUI.Progress;


public class GameManagerGameAr : MonoBehaviour
{
    private JSONNode _jsonData;
    private string url;
    public GameObject ConnectionGameObject;
    public GameObject cardPopup;
    public GameObject cardObject;
    public GameObject ParticleEffect;
    public GameObject playButton;
    public AudioSource audioSource;
    private GameObject parentObject3d;
    public void setCardId(int id)
    {
        Progress.Show("Please Wait...", ProgressColor.Default);

        PlayerPrefs.SetInt("number_of_card", id);

        url = "https://dev.unimasoft.id/edugator/api/getquestions/a49fdc824fe7c4ac29ed8c7b460d7338/" + PlayerPrefs.GetInt("game_id") + "/" + PlayerPrefs.GetInt("number_of_card");

        StartCoroutine(GetDataFromAPI());
    }
    public void SetCardParent(GameObject _object)
    {
        parentObject3d = _object;
    }
    public void SetCardObject(GameObject _object)
    {
        cardObject = _object;
    }
    public void DestroyCardObject()
    {
        GameObject[] allGameObjectsClone = FindObjectsOfType<GameObject>();
        
        foreach(GameObject gameObj in allGameObjectsClone)
        {
            if (gameObj.name.Contains("(Clone)"))
            {
                Destroy(gameObj);
            }
        }
    }

    private IEnumerator GetDataFromAPI()
    {
        using(UnityWebRequest webData = UnityWebRequest.Get(url))
        {
            webData.SendWebRequest();

            while(!webData.isDone)
            {
                yield return null;
            }

            if(webData.result == UnityWebRequest.Result.ConnectionError || webData.result == UnityWebRequest.Result.ProtocolError)
            {
                Progress.Hide();
                Debug.Log("Tidak ada Koneksi/Jaringan");
                ConnectionGameObject.SetActive(true);
                yield return new WaitForSeconds(3f);
                ConnectionGameObject.SetActive(false);
            }
            else
            {
                if(webData.isDone)
                {
                    ConnectionGameObject.SetActive(false);
                    Progress.Hide();
                    _jsonData = JSON.Parse(System.Text.Encoding.UTF8.GetString(webData.downloadHandler.data));
                    if(_jsonData == null)
                    {
                        Debug.Log("Json data Kosong");
                    }
                    else
                    {
                        if (PlayerPrefs.GetString("token") != null)
                        {
                            int i = 0;
                            int total_soal_setiap_kartu = 0;

                            while(i < _jsonData["data"].Count)
                            {
                                total_soal_setiap_kartu++;
                                PlayerPrefs.SetInt("total_soal_setiap_kartu", total_soal_setiap_kartu);
                                i++;
                            }

                            if(_jsonData["data"].Count == 0)
                            {
                                GameObject PopupCard = Instantiate(cardPopup);
                                PopupCard.transform.SetParent(parentObject3d.transform);
                                PopupCard.transform.localPosition = new Vector3(0f, 0.5f, 0f);

                                Destroy(cardObject);
                                Destroy(ParticleEffect);
                                playButton.SetActive(false);
                            }
                            else
                            {
                                Destroy(cardPopup);

                                if (PlayerPrefs.GetInt("visualEffect") == 1)
                                {
                                    GameObject particle = Instantiate(ParticleEffect);
                                    particle.transform.SetParent(parentObject3d.transform);
                                    particle.transform.localPosition = new Vector3(0f, 0.1f, 0f);
                                    particle.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                                }
                                else
                                {
                                    Destroy(ParticleEffect);
                                }
                                yield return new WaitForSeconds(0.7f);
                                
                                audioSource.Play();
                                Instantiate(cardObject).transform.SetParent(parentObject3d.transform);
                                playButton.SetActive(true);
                            }

                            print("isi : " + PlayerPrefs.GetString("nullValue"));
                            print("jumlah data : " + _jsonData["data"].Count);
                            print("jumlah soal : " + PlayerPrefs.GetInt("total_soal_setiap_kartu"));
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
