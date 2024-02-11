using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))][ExecuteAlways]
public class CameraResize : MonoBehaviour
{
    [SerializeField]
    private Camera GameCamera;
    
    private Vector2 targetAspectRatio = new(1,1);
    private Vector2 rectCenter = new(0.5f, 0.5f);
    
    private Vector2 lastResolution;   

    private void OnValidate()
    {
        GameCamera ??= GetComponent<Camera>();
    }

    public void LateUpdate()
    {        
        var currentScreenResolution = new Vector2(Screen.width, Screen.height);
 
        // Don't run all the calculations if the screen resolution has not changed
        if (lastResolution != currentScreenResolution)
        {
            CalculateCameraRect(currentScreenResolution);
        }
 
        lastResolution = currentScreenResolution;
    }
 
    private void CalculateCameraRect(Vector2 currentScreenResolution)
    {
        var normalizedAspectRatio = targetAspectRatio / currentScreenResolution;
        var size = normalizedAspectRatio / Mathf.Max(normalizedAspectRatio.x, normalizedAspectRatio.y);
        GameCamera.rect = new Rect(default, size) { center = rectCenter };
    }
}
