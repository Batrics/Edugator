using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using TMPro;

namespace Loading.UI
{
    public class ProgressData {
        public string title;
    }

    public class LoadingUI : MonoBehaviour {
        private GameObject loadingUIprefabs;
        private TextMeshProUGUI loadingText;
        private GameObject loadingGameObject;

        ProgressData progressData = new ProgressData();

        public void Prepare() {
            loadingUIprefabs = Resources.Load<GameObject>("LoadingUI/Loading UI");
            InstantiateLoadingUI(false, loadingUIprefabs);  
            loadingText = loadingGameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();

        }

        public void InstantiateLoadingUI(bool gameObjectActive, GameObject prefab) {
            loadingGameObject = Instantiate(prefab);
            loadingGameObject.SetActive(gameObjectActive);
        }

        public void DestroyLoadingUI() {
            Destroy(loadingUIprefabs);
        }

        public void Show(string title) {
            progressData.title = title;
            loadingText.text = progressData.title;
            loadingGameObject.SetActive(true);
        }

        public void Hide() {
            loadingGameObject.SetActive(false);

            progressData.title = "";
            loadingText.text = progressData.title;
        }
    }
}