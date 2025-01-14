using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouchSupport = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class PlaceObject : MonoBehaviour
{
    [SerializeField] public Slider scaleSlider;
    [SerializeField] public Slider rotateSlider;
    
    [SerializeField] private GameObject objectToPlace;
    private int scaleObject;
    private int rotateObject;
    
    private ARRaycastManager arRaycastManager;
    private ARPlaneManager arPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    
    private void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();
    }
    
    private void OnEnable()
    {
        EnhancedTouchSupport.EnhancedTouchSupport.Enable();
        EnhancedTouchSupport.TouchSimulation.Enable();
        EnhancedTouchSupport.Touch.onFingerDown += OnFigureDown;
    }
    
    private void OnDisable()
    {
        EnhancedTouchSupport.EnhancedTouchSupport.Disable();
        EnhancedTouchSupport.TouchSimulation.Disable();
        
        scaleSlider.onValueChanged.RemoveListener(delegate { ScaleObject(); });
        rotateSlider.onValueChanged.RemoveListener(delegate { RotateObject(); });
    }
    
    private void OnFigureDown(EnhancedTouchSupport.Finger finger)
    {
        if (finger.index != 0) return;
        
        if (arRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (var hit in hits)
            {
                var hitPose = hit.pose;
                Instantiate(objectToPlace, hitPose.position, hitPose.rotation);
            }
        }
        
        scaleSlider.onValueChanged.AddListener(delegate { ScaleObject(); });
        rotateSlider.onValueChanged.AddListener(delegate { RotateObject(); });
    }
    
    private void ScaleObject()
    {
        scaleObject = (int) scaleSlider.value;
        objectToPlace.transform.localScale = new Vector3(scaleObject, scaleObject, scaleObject);
    }
    
    private void RotateObject()
    {
        rotateObject = (int) rotateSlider.value;
        objectToPlace.transform.rotation = Quaternion.Euler(0, rotateObject, 0);
    }
}
