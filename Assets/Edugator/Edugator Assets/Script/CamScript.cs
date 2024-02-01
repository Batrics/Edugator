using UnityEngine;

public class CamScript : MonoBehaviour
{
    private Camera _cam;
    // Update is called once per frame
    void Start() {
        _cam = Camera.main;
    }
    void Update() {
        transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
    }
}
