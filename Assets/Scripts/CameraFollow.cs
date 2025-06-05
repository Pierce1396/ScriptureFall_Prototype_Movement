using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float minZoom = 3f;
    public float maxZoom = 10f;

    [Header("Pan Settings")]
    public float panSpeed = 0.01f;

    private Camera cam;
    private bool isPanning = false;
    private Vector3 lastMousePosition;
    private bool followPlayer = true;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (target == null)
            target = GameObject.FindWithTag("Player")?.transform;
    }

    void LateUpdate()
    {
        HandleZoom();
        HandlePanning();
        HandleSnapBack();

        if (followPlayer && target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    private void HandlePanning()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isPanning = true;
            followPlayer = false;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            // Convert pixel delta to world units at camera's current orthographic size
            float orthoSize = cam.orthographicSize;
            float screenHeight = Screen.height;

            // Vertical world units per screen pixel
            float unitsPerPixel = (orthoSize * 2) / screenHeight;

            Vector3 move = new Vector3(-delta.x * unitsPerPixel, -delta.y * unitsPerPixel, 0);

            transform.position += move;

            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isPanning = false;
        }
    }

    private void HandleSnapBack()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            followPlayer = true;
        }
    }
}
