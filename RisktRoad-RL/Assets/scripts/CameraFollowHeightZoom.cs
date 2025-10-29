using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform truck;
    public float followSpeed = 3f;
    public float minZoom = 5f;
    public float maxZoom = 12f;
    public float zoomSensitivity = 0.2f;
    public float groundLevel = 0f; // the y-level of terrain
    public float topMargin = 2f;   // extra space above truck
    public float zoomSmoothing = 5f;

    public float HorizontalOffSet = 0f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (truck == null) return;

        // Horizontal follow
        Vector3 targetPosition = new Vector3(truck.position.x + HorizontalOffSet, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // Zoom out as truck goes up
        float heightAboveGround = truck.position.y - groundLevel;
        heightAboveGround = Mathf.Max(0, heightAboveGround);

        // Compute ideal zoom to keep truck in view
        float desiredZoom = Mathf.Lerp(minZoom, maxZoom, heightAboveGround * zoomSensitivity);

        // Ensure the top of screen is always above truck
        float cameraY = transform.position.y;
        float maxVisibleY = cameraY + desiredZoom; // top edge of view
        float neededZoom = (truck.position.y + topMargin) - cameraY;
        if (neededZoom > desiredZoom)
            desiredZoom = neededZoom; // expand if needed

        // Clamp zoom so it doesn’t get absurd
        desiredZoom = Mathf.Clamp(desiredZoom, minZoom, maxZoom);

        // Smooth zoom transition
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredZoom, Time.deltaTime * zoomSmoothing);
    }
}
