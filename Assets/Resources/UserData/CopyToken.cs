using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CopyToken : MonoBehaviour
{
    [SerializeField] private TMP_Text tokenValue;
    public void CopyText()
    {
        string textToCopy = tokenValue.text;
        GUIUtility.systemCopyBuffer = textToCopy;
        Debug.Log("Teks dari InputField telah disalin ke clipboard: " + textToCopy);
    }
}
