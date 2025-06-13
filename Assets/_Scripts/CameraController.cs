using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{

    public static CameraController Instance;
    public GameObject player; // Reference to the player object
    private bool camLock = false;
    public Light2D mainLight; // Reference to the main light in the scene

    public float dragSpeed = 1f;
    private Vector3 dragOrigin;
    private bool isDragging = false;

    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 15f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize camera position
        if (player != null)
        {
            Camera.main.transform.position = player.transform.position + new Vector3(0, 0, -5f);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (!camLock)
            {
                HandleMouseDrag();
            }
        else if (camLock && player != null)
            {
                Vector3 newPos = player.transform.position;
                newPos.z = -5f;
                Camera.main.transform.position = newPos;
            }

        HandleZoom();

    }

    public void LockOnAvatar(GameObject avatar)
    {
        player = avatar;
        camLock = true;
        Camera.main.transform.position = player.transform.position + new Vector3(0, 0, -5f);
        mainLight.shadowIntensity = 0.8f; // Set light type to directional when locking camera
    }

    public void UnlockCamera()
    {
        camLock = false;
        player = null; // Optionally clear the player reference
        mainLight.shadowIntensity = 0.2f;
    }
    

    
    void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 currentMousePos = Input.mousePosition;
            Vector3 difference = Camera.main.ScreenToViewportPoint(dragOrigin - currentMousePos);

            Vector3 move = new Vector3(difference.x * dragSpeed, difference.y * dragSpeed, 0);
            Camera.main.transform.position += move;

            dragOrigin = currentMousePos; // Update for smooth dragging
        }
    }
    
        void HandleZoom()
    {
        float scrollInput = Input.mouseScrollDelta.y;
        if (scrollInput != 0f)
        {
            Camera.main.orthographicSize -= scrollInput * zoomSpeed * Time.deltaTime;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
        }
    }
}
