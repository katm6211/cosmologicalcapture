using UnityEngine.InputSystem;
using UnityEngine;

public class TelescopePlacer : MonoBehaviour
{
    public GameObject telescopePrefab;
    public Transform cameraTransform;      // Main Camera (the one PlayerCam is on)
    public float placeDistance = 4f;
    public KeyCode placeKey = KeyCode.J;
    public LayerMask placeableSurfaces = ~0;  // what the placement raycast can hit
    public float cooldown = 0.25f;
   
    float nextPlaceTime;

    void Start()
    {
        if (cameraTransform == null && Camera.main != null) // checks cameratransform slot & presence of Main Camera
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (Keyboard.current.jKey.wasPressedThisFrame && Time.time >= nextPlaceTime)
        {
            nextPlaceTime = Time.time + cooldown;
            Place();
        }   
    }
    
    void Place()
    {
        if (telescopePrefab == null || cameraTransform == null) return;

        Vector3 pos;
        // If we're looking at a surface within range, place on it. Otherwise place in mid-air in front of us.
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward,
                            out RaycastHit hit, placeDistance, placeableSurfaces,
                            QueryTriggerInteraction.Ignore))
        {
            pos = hit.point;
        }
        else
        {
            pos = cameraTransform.position + cameraTransform.forward * placeDistance;
        }

        // Face the same horizontal direction the player is looking
        Quaternion rot = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        Instantiate(telescopePrefab, pos, rot);
    }
}
