// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Networking;
// using System.IO;
// public class CardScript : MonoBehaviour
// {
//     private string fbxUrl = "https://cace-103-165-41-34.ngrok-free.app/edugator/public/api/downloadModel/a49fdc824fe7c4ac29ed8c7b460d7338/1";
//     private string savePath = "Assets/3D Object/3D/a.fbx"; // Ubah path sesuai dengan kebutuhan Anda.

//     IEnumerator Start()
//     {

//         UnityWebRequest www = UnityWebRequest.Get(fbxUrl);
//         yield return www.SendWebRequest();

//         if (www.result != UnityWebRequest.Result.Success)
//         {
//             Debug.LogError("Gagal mengunduh file: " + www.error);
//         }
//         else
//         {
//             byte[] data = www.downloadHandler.data;

//             // Simpan data ke dalam file di folder "Assets".
//             File.WriteAllBytes(savePath, data);
            
//             UnityEditor.AssetDatabase.Refresh();
//             Debug.Log("File berhasil diunduh dan disimpan di " + savePath);
//         }
//     }
// }
