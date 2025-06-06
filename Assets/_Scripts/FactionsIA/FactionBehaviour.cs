using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using System.Collections;
public class FactionBehaviour : MonoBehaviour
{
    
    public FactionData factionData;
    //public CreatureSpawner creatureSpawner;
    public int rooms, tiles, foodResources;
    static public FactionType currentFactionType;
    public string factionName;
    public int numberOfHQ = 0;
    public List<CreatureController> members = new List<CreatureController>();
    public Dictionary<Vector3Int, TileInfo> knownTilesDict = new Dictionary<Vector3Int, TileInfo>();
    public Dictionary<Vector2Int, RoomInfo> knownRoomsDict = new Dictionary<Vector2Int, RoomInfo>();
    public Dictionary<FactionBehaviour, FactionRelationship> knownFactions = new Dictionary<FactionBehaviour, FactionRelationship>();
    [SerializeField] public List<RoomInfo> currentHQ = new List<RoomInfo>();
    public enum FactionRelationship { Enemy = -10, Hostile = -5, Neutral = 0, Friendly = 5, Ally = 10 }
    public GameObject prefabCreature;
    private Coroutine mainGoalsCoroutine, checkFoodForNewMembersCoroutine;



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
            case FactionData.FactionTypeEnum.Wanderer:
                return new WandererFaction(this);
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

    private IEnumerator DelayedInit()
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

        if (factionData.startingMembers != 0)
        {
            for (int i = 0; i < factionData.startingMembers; i++)
            {
                StartCoroutine(CreatureSpawner.Instance.SpawnCreatureInRoom(transform.position, prefabCreature, this));
            }
            }
        yield break;
    }

    // Update is called once per frame
    void Update()
    {

        currentFactionType?.Update();
        /*if (currentHQ == null && knownRoomsDict.Count > 0)
        {
            foreach (RoomInfo room in knownRoomsDict.Values)
            {
                if (currentFactionType.PotencialHQ(room))
                {
                    currentHQ.Add(room);
                    break;
                }
            }
        }*/
    }

    public void AskedForState(CreatureController unit)
    {
        currentFactionType.AskForState();
    }

    /*public IEnumerator SpawnCreatureInRoom(Vector2 pos, GameObject creaturePrefab)
    {

        // Instantiate creature
        GameObject creatureGO = GameObject.Instantiate(creaturePrefab, pos, Quaternion.identity);
        creatureGO.SetActive(false);
        creatureGO.transform.SetParent(transform);
        CreatureController controller = creatureGO.GetComponent<CreatureController>();
        controller.currentFaction = this;
        members.Add(controller); // optional: track units
        yield return new WaitForSeconds(0.1f); // wait a bit to ensure the creature is fully initialized
        creatureGO.SetActive(true);
    }*/

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
        if (mainGoalsCoroutine == null)
        mainGoalsCoroutine = StartCoroutine(currentFactionType.MainCoroutineForGoals());
        //if (RegisterNewRoomsCoroutine == null)
        //   RegisterNewRoomsCoroutine = StartCoroutine(RegisterNewRooms());
        if (checkFoodForNewMembersCoroutine == null)
        checkFoodForNewMembersCoroutine = StartCoroutine(foodCheckCoroutine());
    }


    private IEnumerator foodCheckCoroutine()
    {
        while (true)
        {
            if (foodResources > members.Count * 30)
            {
                foodResources /= 2;
                RoomInfo randomHQroom = currentHQ[Random.Range(0, currentHQ.Count)];
                TileInfo randomTile = randomHQroom.tiles[Random.Range(0, randomHQroom.tiles.Count)];
                Vector2 spawnPoint = new Vector2(randomTile.position.x, randomTile.position.y);
                StartCoroutine(CreatureSpawner.Instance.SpawnCreatureInRoom(spawnPoint, prefabCreature, this));
            }
            yield return new WaitForSeconds(1f); // Adjust the wait time as needed
        }
    }
}
