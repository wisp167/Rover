using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera[] cameras;
    private int currentCameraIndex = 0;

    void Start()
    {
        // Set all cameras to low priority except the first one
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].depth = i == 0 ? 0 : -1;
        }
    }
    void Update()
    {
        // Switch camera on key press (for testing)
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchToNextCamera();
        }
    }

    public void SwitchToNextCamera()
    {
        if (cameras.Length == 0) return;
        
        // Set current camera to low priority
        cameras[currentCameraIndex].depth = -1;
        
        // Move to next index
        currentCameraIndex = (currentCameraIndex + 1) % cameras.Length;
        
        // Set new camera to high priority
        cameras[currentCameraIndex].depth = 0;
    }
}