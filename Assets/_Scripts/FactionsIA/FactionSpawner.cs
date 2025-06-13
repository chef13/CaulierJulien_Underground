using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
public class FactionSpawner : MonoBehaviour
{
    public static FactionSpawner instance;
    private bool init;

    [SerializeField] private FactionData[] factionData;
    [SerializeField] private FactionData factionWanderer;
    [SerializeField] private FactionData factionDungeon;
    public List<FactionBehaviour> factionsIA = new List<FactionBehaviour>();

    public FactionBehaviour dungeonFaction, wandererFaction, topLeftFaction, topRightFaction, bottomLeftFaction, bottomRightFaction, lezardFactionLeft, lezardFactionRight;
    public GameObject[] spawners;
    public List<GameObject> factions = new List<GameObject>();
    [SerializeField] private GameObject factionPrefab;
    [SerializeField] private GameObject spawnerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        factionsIA = new List<FactionBehaviour>() {
            topLeftFaction,
            topRightFaction,
            bottomLeftFaction,
            bottomRightFaction,
            lezardFactionLeft
            , lezardFactionRight};


         for (int i = 0; i < spawners.Length; i++)

        {
            RoomInfo room;
            if (i == 0)
            {
                DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(1, 1), out room);
            }
            else if (i == 1)
            {
                DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(DungeonGenerator.Instance.roomGrid - 2, 1), out room);
            }
            else if (i == 2)
            {
                DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(DungeonGenerator.Instance.roomGrid - 2, DungeonGenerator.Instance.roomGrid - 2), out room);
            }
            else
            {
                DungeonGenerator.Instance.roomsMap.TryGetValue(new Vector2Int(1, DungeonGenerator.Instance.roomGrid - 2), out room);
            }

            GameObject spawner = Instantiate(spawnerPrefab, room.roomBounds.center, Quaternion.identity);
            spawner.name = "Spawner " + i;

            spawners[i] = spawner;
        }
    }
    void Start()
    {
        int countOffactions = DungeonGenerator.Instance.startingfactionCount;
        int factionIAindex = 0;
        while (countOffactions > 0)
        {
            RoomInfo factionRoomStart = null;
            switch (countOffactions - 1)
            {
                case 0:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(1, 1)];
                    countOffactions--;
                    break;
                case 1:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(DungeonGenerator.Instance.roomGrid - 2, 1)];
                    countOffactions--;
                    break;
                case 2:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(1, DungeonGenerator.Instance.roomGrid - 2)];
                    countOffactions--;
                    break;
                case 3:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(DungeonGenerator.Instance.roomGrid - 2, DungeonGenerator.Instance.roomGrid - 2)];
                    countOffactions--;
                    break;
                case 4:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(1, Mathf.FloorToInt(DungeonGenerator.Instance.roomGrid / 2f))];
                    countOffactions--;
                    break;
                case 5:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(Mathf.FloorToInt(DungeonGenerator.Instance.roomGrid / 2f), 1)];
                    countOffactions--;
                    break;
                case 6:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(DungeonGenerator.Instance.roomGrid - 2, Mathf.FloorToInt(DungeonGenerator.Instance.roomGrid / 2f))];
                    countOffactions--;
                    break;
                case 7:
                    factionRoomStart = DungeonGenerator.Instance.roomsMap[new Vector2Int(Mathf.FloorToInt(DungeonGenerator.Instance.roomGrid / 2f), DungeonGenerator.Instance.roomGrid - 2)];
                    countOffactions--;
                    break;
            }
            var faction = factionData[Random.Range(0, factionData.Length)];
            var pos = new Vector3(factionRoomStart.roomBounds.center.x, factionRoomStart.roomBounds.center.y, 0);

            factionsIA[factionIAindex] = SpawnFaction(faction, pos);
            factionsIA[factionIAindex].currentHQ.Add(factionRoomStart);
            factionRoomStart.faction = factionsIA[factionIAindex];
            foreach (var tile in factionRoomStart.tiles)
            {
                tile.faction = factionsIA[factionIAindex];
            }
            factionsIA[factionIAindex].numberOfHQ++;
            factionIAindex++;
        }

        dungeonFaction = SpawnFaction(factionDungeon, new Vector3(DungeonGenerator.Instance.dungeonWidth / 2f, DungeonGenerator.Instance.dungeonHeight / 2f, 0));
        FlaureSpawner.instance.dungeonFaction = dungeonFaction.GetComponent<FactionBehaviour>();
        dungeonFaction.name = "Dungeon Faction";


        wandererFaction = SpawnFaction(factionWanderer, new Vector3(DungeonGenerator.Instance.dungeonWidth / 2f, DungeonGenerator.Instance.dungeonHeight / 2f, 0));
        FlaureSpawner.instance.wandererFaction = wandererFaction.GetComponent<FactionBehaviour>();
        FlaureSpawner.instance.wandererFaction.name = "Wanderer Faction";

        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public FactionBehaviour SpawnFaction(FactionData factionData, Vector3 spawnTransform)
    {
        GameObject faction = Instantiate(factionPrefab, spawnTransform, Quaternion.identity);
        faction.SetActive(false);
        var factionIa = faction.GetComponent<FactionBehaviour>();
        factionIa.factionData = factionData;
        faction.name = factionData.factionName;
        factions.Add(faction);
        faction.SetActive(true);
        return factionIa;
    }
}
