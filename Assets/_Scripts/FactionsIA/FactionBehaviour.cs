using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Unity.VisualScripting;

public class FactionBehaviour : MonoBehaviour
{
    public FactionData factionData;
    public int rooms, tiles, foodResources;
    static public FactionType currentFactionType;
    public string factionName;
    public int numberOfHQ = 0;
    public List<GameObject> members = new List<GameObject>();
    public Dictionary<Vector3Int, TileInfo> knownTilesDict = new Dictionary<Vector3Int, TileInfo>();
    public Dictionary<Vector2Int, RoomInfo> knownRoomsDict = new Dictionary<Vector2Int, RoomInfo>();
    public Dictionary<FactionBehaviour, FactionRelationship> knownFactions = new Dictionary<FactionBehaviour, FactionRelationship>();
    [SerializeField] public List<RoomInfo> currentHQ = new List<RoomInfo>();
    public enum FactionRelationship { Enemy = -10, Hostile = -5, Neutral = 0, Friendly = 5, Ally = 10 }
    public GameObject prefabCreature;
    private Coroutine RegisterNewRoomsCoroutine;



    public void SwitchType(FactionType newType)
    {
        currentFactionType?.Exit();
        currentFactionType = newType;
        currentFactionType?.Enter();
    }


    void Awake()
    {
       
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
        


    }
    void OnEnable()
    {
        StartCoroutine(DelayedInit());

    }

    private System.Collections.IEnumerator DelayedInit()
    {
        // Wait until factionData is set by the spawner
        while (factionData == null)
            yield return null; // Wait one frame

        // Now safe to use factionData
        currentFactionType = GetFactionTypeInstance();
        factionName = factionData.factionName;
        prefabCreature = factionData.prefabCreature[0];
        SwitchType(currentFactionType);

        StartAllCoroutines();

        for (int i = 0; i < factionData.startingMembers; i++)
        {
            StartCoroutine(SpawnCreatureInRoom(transform.position, prefabCreature));
        }
        yield break;
    }

    // Update is called once per frame
    void Update()
    {

        currentFactionType?.Update();
        if (currentHQ == null && knownRoomsDict.Count > 0)
        {
            foreach (RoomInfo room in knownRoomsDict.Values)
            {
                if (currentFactionType.PotencialHQ(room))
                {
                    currentHQ.Add(room);
                    break;
                }
            }
        }
    }

    public void AskedForState(CreatureController unit)
    {
        currentFactionType.AskForState();
    }

    public System.Collections.IEnumerator SpawnCreatureInRoom(Vector2 pos, GameObject creaturePrefab)
    {

        // Instantiate creature
        GameObject creatureGO = GameObject.Instantiate(creaturePrefab, pos, Quaternion.identity);
        creatureGO.SetActive(false);
        creatureGO.transform.SetParent(transform);
        CreatureController controller = creatureGO.GetComponent<CreatureController>();
        controller.currentFaction = this;
        members.Add(creatureGO); // optional: track units
        yield return new WaitForSeconds(0.1f); // wait a bit to ensure the creature is fully initialized
        creatureGO.SetActive(true);
    }

    public void RegisterKnownFaction(FactionBehaviour otherFaction, FactionRelationship relationship)
    {
        if (otherFaction == this) return; // Don't register self
        if (!knownFactions.ContainsKey(otherFaction))
            knownFactions.Add(otherFaction, relationship);
        else
            knownFactions[otherFaction] = relationship;
    }


    private void StartAllCoroutines()
    {
        StartCoroutine(currentFactionType.MainCoroutineForGoals());
        //if (RegisterNewRoomsCoroutine == null)
        //   RegisterNewRoomsCoroutine = StartCoroutine(RegisterNewRooms());
    }
    
    

}
