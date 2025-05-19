using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class CreatureController : MonoBehaviour
{
    public RoomFirstDungeonGenerator roomGenerator;
    [HideInInspector] public CreatureState currentState;
    public int maxHP, currentHP, damage;
    public float attackRange, attackSpeed, attackTimer, detectionRange, maxEnergy, currentEnergy;
    public float stoppingDistance = 1f;
    public Vector2 destination;
    protected NavMeshAgent agent;
    public CreatureAI creatureAI;
    public bool hasDestination = false;
    public GameObject currentRoom;
    public List<TileInfo> currentRoomTiles;
    public Tilemap floorTileMap;

    public FactionBehaviour currentFaction;
    Grid dgGrid;

    protected virtual void Awake()
    {
        creatureAI = GetComponent<CreatureAI>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        currentFaction = GetComponentsInParent<FactionBehaviour>()[0];
        roomGenerator = GameObject.FindFirstObjectByType<RoomFirstDungeonGenerator>();
        //StartCoroutine("RegisterUnknownTile2");
    }

    protected virtual void Update()
    {
        if (hasDestination && agent.remainingDistance <= stoppingDistance)
        {
            hasDestination = false;
            //CheckCurrentRoom();
            //OnDestinationReached();
        }
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        if (currentEnergy < maxEnergy / 4)
        {
            creatureAI.SwitchState(new StateReccover(creatureAI));
        }

        //RegisterUnknownTileFonc();

    }

    public void SetFaction(FactionBehaviour faction)
    {
        currentFaction = faction;
    }

    public virtual void SetDestination(Vector2 destination)
    {
        hasDestination = true;
        agent.SetDestination(destination);
    }

    public virtual bool HasReachedDestination()
    {
        return !hasDestination;
    }

    protected virtual void OnDestinationReached()
    {
        agent.SetDestination(destination);
    }

    public virtual void OnHit(GameObject attacker, int damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            OnDeath();
        }
        else if (currentHP < maxHP / 2)
        {
            // Change color to red
            creatureAI.attacker = attacker;
            creatureAI.SwitchState(new StateFlee(creatureAI, attacker));
        }
        else
        {
            // Change color to yellow
            creatureAI.target = attacker;
            creatureAI.SwitchState(new StateAttack(creatureAI, attacker));
        }
    }

    public virtual void OnDeath()
    {
        this.gameObject.SetActive(false);
        transform.position = new Vector3(-100, -100, 0);
    }

   /* public void CheckCurrentRoom()
    {
        foreach (var room in roomGenerator.dgRoomslist)
        {
            // Check if the room is within the explore range
            if (Vector2.Distance(transform.position, room.transform.position) < detectionRange)
            {
                RoomInfo roomInfo = room.GetComponent<RoomComponent>().roomInfo; // Make sure RoomInfo inherits from MonoBehaviour
                foreach (var tile in roomInfo.positions)
                {
                    if (tile.position == Vector3Int.RoundToInt(transform.position))
                    {
                        // If the tile is in the room, set it as the current room
                        currentRoom = room;

                    }
                }
                if (currentRoom != null)
                {
                    foreach (var tile in roomInfo.positions)
                    {
                        if (!currentRoomTiles.Contains(tile))
                            currentRoomTiles.Add(tile);
                    }
                    break;
                }


                else if (currentRoom == null)
                {
                    break;
                }
            }
        }
    }*/

    IEnumerator RegisterUnknownTile()
    {
        Vector3Int centerCell = floorTileMap.WorldToCell(transform.position);
        centerCell.z = 0;
        int range = Mathf.RoundToInt(detectionRange);
        for (int x = -range; x < range; x++)
        {
            for (int y = -range; y < range; y++)
            {
                Vector3Int cell = new Vector3Int(centerCell.x, centerCell.y, 0);
                if (roomGenerator.tileInfoDict.TryGetValue(cell, out TileInfo info))
                {
                    Debug.Log(cell);
                    var value = roomGenerator.tileInfoDict[cell];
                    // Update faction's knowledge
                    if (!currentFaction.knowntileInfoDict.ContainsKey(centerCell))
                    {
                        currentFaction.knowntileInfoDict[cell] = info;
                    }
                }
                else
                {
                    // Debug.LogWarning($"Tile NOT found: {cell}");

                    foreach (var key in roomGenerator.tileInfoDict.Keys)
                    {
                        Debug.Log($"Stored key: {key}");
                    }
                }
            }

        }

        yield return new WaitForSeconds(Random.Range(1f, 1.2f));
        StartCoroutine("RegisterUnknownTile");
    }

    public void RegisterUnknownTileFonc()
    {



        Vector3Int centerCell = floorTileMap.WorldToCell(transform.position);

        int range = Mathf.RoundToInt(detectionRange);
        for (int x = -range; x < range; x++)
        {
            for (int y = -range; y < range; y++)
            {
                Vector3Int cell = new Vector3Int(centerCell.x + x, centerCell.y + y, 0);
                if (roomGenerator.tileInfoDict.TryGetValue(cell, out TileInfo tileInfo))
                {
                    Debug.Log(cell);
                    // Update faction's knowledge
                    if (!currentFaction.knowntileInfoDict.ContainsKey(cell))
                    {
                        currentFaction.knowntileInfoDict[cell] = tileInfo;
                    }
                }
                else
                {
                    foreach (var key in roomGenerator.tileInfoDict.Keys)
                    {
                        Debug.Log($"Stored key: {key}");
                    }
                }
            }

        }


    }
    

    IEnumerator RegisterUnknownTile2()
    {
        Vector3Int centerCell = floorTileMap.WorldToCell(transform.position);
        centerCell.z = 0;
        int range = Mathf.RoundToInt(detectionRange);
        for (int x = -range; x < range; x++)
        {
            for (int y = -range; y < range; y++)
            {
                Collider2D hit = Physics2D.OverlapPoint(transform.position, 15);
                if (hit != null)
                {
                    GameObject tile = hit.gameObject;
                    if (roomGenerator.dgTilesList.Contains(tile))
                    {
                        if (!currentFaction.knownTiles.Contains(tile))
                        {
                            currentFaction.knownTiles.Add(tile);
                            currentFaction.knownTilePositions.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }
            
        }

        yield return new WaitForSeconds(Random.Range(1f,1.2f));
        StartCoroutine("RegisterUnknownTile2");
    }
}
