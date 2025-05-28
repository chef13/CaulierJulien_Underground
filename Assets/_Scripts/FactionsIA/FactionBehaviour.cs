using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
public class FactionBehaviour : MonoBehaviour
{
    public FactionData factionData;
    public int rooms;
    public int tiles;
    static public FactionType currentFactionType;
    public string factionName;
    public List<GameObject> members = new List<GameObject>();
    public Dictionary<Vector3Int, TileInfo> knownTilesDict = new Dictionary<Vector3Int, TileInfo>();
    public Dictionary<Vector2Int, RoomInfo> knownRoomsDict = new Dictionary<Vector2Int, RoomInfo>();
    public RoomInfo currentHQ;

    public GameObject prefabCreature;



    public void SwitchType(FactionType newType)
    {
        currentFactionType?.Exit();
        currentFactionType = newType;
        currentFactionType?.Enter();
    }

    void Awake()
    {   
        GetFactionTypeInstance();
        factionName = factionData.factionName;

        Vector2Int startPos = Vector2Int.RoundToInt(transform.position);
        Collider2D[] hits = Physics2D.OverlapCircleAll(startPos, 5, 15);
        SwitchType(new GobFaction(this));
    }

        public FactionType GetFactionTypeInstance()
    {
        switch (factionData.factionTypeEnum)
        {
            case FactionData.FactionTypeEnum.Goblin:
                return new GobFaction(this);
            case FactionData.FactionTypeEnum.Lezard:
                return new LezardFaction(this);
            default:
                return null;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < factionData.startingMembers; i++)
        {
            SpawnCreatureInRoom(transform.position, factionData.prefabCreature[Random.Range(0, factionData.prefabCreature.Length)]);
        }
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

        rooms = knownRoomsDict.Count;
        tiles = knownTilesDict.Count;
    }

    public void AskedForState(CreatureController unit)
    {
        currentFactionType.AskForState();
    }

    internal void AskedForState(Transform transform)
    {
        throw new NotImplementedException();
    }
    

    public void SpawnCreatureInRoom(Vector2 pos, GameObject creaturePrefab)
    {
        
        // Instantiate creature
        GameObject creatureGO = GameObject.Instantiate(creaturePrefab, pos, Quaternion.identity);
        creatureGO.transform.SetParent(transform);
        CreatureController controller = creatureGO.GetComponent<CreatureController>();
        controller.currentFaction = this.GetComponent<FactionBehaviour>();
        
        // Assign faction

        GetComponent<FactionBehaviour>().members.Add(creatureGO); // optional: track units
    }
}
