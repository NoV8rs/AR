using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouchSupport = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class PlaceObject : MonoBehaviour
{
    private GameObject selectedGameObject;
    private GameObject spawnedObject;
    private ARRaycastManager arRaycastManager;
    private ARPlaneManager arPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private float initialDistance;
    private Vector3 initialScale;
    private float initialRotation;
    private bool isObjectPlaced;

    private void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.EnhancedTouchSupport.Enable();
        EnhancedTouchSupport.TouchSimulation.Enable();
        EnhancedTouchSupport.Touch.onFingerDown += OnFingerDown;
        EnhancedTouchSupport.Touch.onFingerMove += OnFigureMoveObject;
        EnhancedTouchSupport.Touch.onFingerUp += OnFigureUp;
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.EnhancedTouchSupport.Disable();
        EnhancedTouchSupport.TouchSimulation.Disable();
        EnhancedTouchSupport.Touch.onFingerDown -= OnFingerDown;
        EnhancedTouchSupport.Touch.onFingerMove -= OnFigureMoveObject;
        EnhancedTouchSupport.Touch.onFingerUp -= OnFigureUp;
    }

    private void OnFingerDown(EnhancedTouchSupport.Finger finger)
    {
        if (finger.index != 0 || EventSystem.current.IsPointerOverGameObject(finger.index)) return;

        if (arRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
            }
            spawnedObject = Instantiate(selectedGameObject, hitPose.position, hitPose.rotation);
        }
    }

    private void OnFigureMoveObject(EnhancedTouchSupport.Finger finger)
    {
        if (spawnedObject == null) return;

        if (EnhancedTouchSupport.Touch.activeFingers.Count == 1)
        {
            if (arRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hits[0].pose;
                spawnedObject.transform.position = hitPose.position;
            }
        }
        else if (EnhancedTouchSupport.Touch.activeFingers.Count == 2)
        {
            var finger1 = EnhancedTouchSupport.Touch.activeFingers[0];
            var finger2 = EnhancedTouchSupport.Touch.activeFingers[1];

            if (finger1.currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Began || finger2.currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(finger1.screenPosition, finger2.screenPosition);
                initialScale = spawnedObject.transform.localScale;
                initialRotation = Vector2.SignedAngle(finger1.screenPosition, finger2.screenPosition);
            }
            else
            {
                float currentDistance = Vector2.Distance(finger1.screenPosition, finger2.screenPosition);
                if (Mathf.Approximately(initialDistance, 0)) return;

                float scaleFactor = currentDistance / initialDistance;
                spawnedObject.transform.localScale = initialScale * scaleFactor;
            }
            
            float currentRotation = Vector2.SignedAngle(finger1.screenPosition,finger2.screenPosition);
            float rotationFactor = currentRotation - initialRotation;
            spawnedObject.transform.Rotate(Vector3.up, rotationFactor);
            initialRotation = currentRotation;
        }
    }
    
    private void OnFigureUp(EnhancedTouchSupport.Finger finger)
    {
        if (EnhancedTouchSupport.Touch.activeFingers.Count < 2)
        {
            initialDistance = 0;
        }
        isObjectPlaced = false;
    }
}
