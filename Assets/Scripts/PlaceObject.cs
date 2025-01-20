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
    [SerializeField] private GameObject[] objectToPlace;
    private GameObject selectedGameObject;
    private List<GameObject> spawnedObject = new List<GameObject>();
    private ARRaycastManager arRaycastManager;
    private ARPlaneManager arPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private float initialDistance;
    private Vector3 initialScale;
    private bool isObjectPlaced;
    
    [Header("Object Index")]
    public int objectIndex;

    private void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();
        selectedGameObject = objectToPlace[objectIndex];
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
    
    private void SelectObjectToPlace(int index)
    {
        if (index >= 0 && index < objectToPlace.Length)
        {
            selectedGameObject = objectToPlace[index];
        }
    }
    
    public void OnNextButtonClicked()
    {
        objectIndex++;
        if (objectIndex >= objectToPlace.Length)
        {
            objectIndex = 0;
        }
        SelectObjectToPlace(objectIndex);
    }
    
    public void OnPreviousButtonClicked()
    {
        objectIndex--;
        if (objectIndex < 0)
        {
            objectIndex = objectToPlace.Length - 1;
        }
        SelectObjectToPlace(objectIndex);
    }

    private void OnFingerDown(EnhancedTouchSupport.Finger finger)
    {
        if (finger.index != 0) return;

        if (arRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            if (arRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hits[0].pose;
                var newObject = Instantiate(selectedGameObject, hitPose.position, hitPose.rotation);
                spawnedObject.Add(newObject);

                // Check if the touch is on any spawned object
                Ray ray = Camera.main.ScreenPointToRay(finger.currentTouch.screenPosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    foreach (var obj in spawnedObject)
                    {
                        if (hit.transform == obj.transform)
                        {
                            selectedGameObject = obj;
                            break;
                        }
                    }
                }
            }
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
                selectedGameObject.transform.position = hitPose.position;
            }
        }
        else if (EnhancedTouchSupport.Touch.activeFingers.Count == 2)
        {
            var finger1 = EnhancedTouchSupport.Touch.activeFingers[0];
            var finger2 = EnhancedTouchSupport.Touch.activeFingers[1];

            if (finger1.currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Began || finger2.currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(finger1.screenPosition, finger2.screenPosition);
                initialScale = selectedGameObject.transform.localScale;
            }
            else
            {
                float currentDistance = Vector2.Distance(finger1.screenPosition, finger2.screenPosition);
                if (Mathf.Approximately(initialDistance, 0)) return;

                float scaleFactor = currentDistance / initialDistance;
                selectedGameObject.transform.localScale = initialScale * scaleFactor;
            }
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
