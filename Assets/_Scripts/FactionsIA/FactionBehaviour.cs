using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
public class FactionBehaviour : MonoBehaviour
{
    static public FactionType currentFactionType;
    public string factionName;
    public List<GameObject> members = new List<GameObject>();
    public Dictionary<Vector3Int, TileInfo> knownTilesDict = new Dictionary<Vector3Int, TileInfo>();
    public Dictionary<Vector3Int, RoomInfo> knownRoomsDict = new Dictionary<Vector3Int, RoomInfo>();
    public RoomInfo currentHQ;


    public void SwitchType(FactionType newType)
    {
        currentFactionType?.Exit();
        currentFactionType = newType;
        currentFactionType?.Enter();
    }

    void Awake()
    {
        Vector2Int startPos = Vector2Int.RoundToInt(transform.position);
        Collider2D[] hits = Physics2D.OverlapCircleAll(startPos, 5, 15);
        SwitchType(new GobFaction(this));
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (GameObject member in members)
        {
            member.GetComponent<CreatureAI>().controller.currentFaction = this;
            member.GetComponent<CreatureAI>().SwitchState(new StateExplore(member.GetComponent<CreatureAI>()));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHQ == null && knownRoomsDict.Count > 0)
        {
            foreach (RoomInfo room in knownRoomsDict.Values)
            {
                if (currentFactionType.PotencialHQ(room))
                {
                    currentHQ = room;
                    break;
                }
            }
        }

    }

    public void AskedForState(GameObject unit)
    {
        currentFactionType.AskForState();
    }

    internal void AskedForState(Transform transform)
    {
        throw new NotImplementedException();
    }
    
}
