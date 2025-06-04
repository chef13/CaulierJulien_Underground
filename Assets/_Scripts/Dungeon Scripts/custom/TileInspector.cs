
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Tilemaps;
using TMPro;
public class TileInspector : MonoBehaviour
{
    public GameObject player; // Reference to the player object
    private bool camLock = false;
    public Tilemap tilemap; // Your floor tilemap
    public DungeonGenerator dungeonGenerator; // Holds dungeonMap
    public TMP_Text inspectorTxt;

    public Color highlightColor = Color.yellow; // Color to apply
    public Color defaultColor = Color.white;    // Color to reset

    private Vector3Int? lastTileSelected = null;


    public float dragSpeed = 1f;
    private Vector3 dragOrigin;
    private bool isDragging = false;

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 15f;

    void Start()
    {

    }

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

        if (Input.GetKeyDown(KeyCode.C))
        {
            camLock = !camLock;
            if (camLock && player != null)
            {
                Camera.main.transform.position = player.transform.position;
            }
        }

        if (!camLock && Input.GetMouseButtonDown(0)) // Left-click
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int worldPosInt = new Vector3Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y), 0);
            Vector3Int cellPos = tilemap.WorldToCell(worldPos);
            cellPos.z = 0;

            // Reset color on previous selection
            if (lastTileSelected.HasValue)
            {
                tilemap.SetColor(lastTileSelected.Value, defaultColor);
            }

            // Set new color if tile exists
            if (dungeonGenerator.dungeonMap.TryGetValue(cellPos, out TileInfo tile))
            {
                tilemap.SetColor(cellPos, highlightColor);
                lastTileSelected = cellPos;

                string type = tile.room != null ? "Room" : "Corridor";
                string roomCenter = tile.room != null ? tile.room.index.ToString() : "None";
                Debug.Log($"🟩 Tile at {cellPos} → Type: {type}, Room Center: {roomCenter}");
                inspectorTxt.text = $"tile pos : {tile.position}, floor : {tile.isFloor}, water :{tile.isWater}, nature: {tile.isNature}, dead end : {tile.isDeadEnd} \n";
                if (tile.objects.Count > 0)
                {
                    inspectorTxt.text += $"Objects : {tile.objects.Count} \n";
                    foreach (GameObject obj in tile.objects)
                    {
                        inspectorTxt.text += $"Object : {obj.name} \n";
                    }
                }
                else
                {
                    inspectorTxt.text += "No objects found \n";
                }
                if (tile.room != null)
                {
                    RoomInfo room = tile.room;
                    inspectorTxt.text += $"Room : {room.index}, tiles : {room.tiles.Count}";
                    List<RoomInfo> connection = room.connectedRooms;
                    foreach (RoomInfo r in connection)
                    {
                        inspectorTxt.text += $"connected to : {r.index} \n";
                    }
                }

                if (tile.corridor != null)
                {
                    CorridorInfo corridor = tile.corridor;
                    inspectorTxt.text = inspectorTxt.text + $"Corridor : {corridor.tiles.Count}";
                    List<RoomInfo> connection = corridor.connectedRooms;
                    foreach (RoomInfo r in connection)
                    {
                        inspectorTxt.text = inspectorTxt.text + $"connected to : {r.index} \n";
                    }
                }
            }
            else
            {
                Debug.Log($"❌ No tile info found at {cellPos}");
                inspectorTxt.text = $"no tile found";
            }


        }
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