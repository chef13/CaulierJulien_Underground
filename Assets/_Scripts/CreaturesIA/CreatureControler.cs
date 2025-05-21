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
    public bool room;
    [HideInInspector] public CreatureState currentState;
    public int maxHP, currentHP, damage;
    public float attackRange, attackSpeed, attackTimer, detectionRange, maxEnergy, currentEnergy;
    public float stoppingDistance = 1f;
    public Vector2 destination;
    protected NavMeshAgent agent;
    public CreatureAI creatureAI;
    public bool hasDestination = false;
    public RoomInfo currentRoom;
    public FactionBehaviour currentFaction;

    protected virtual void Awake()
    {
        creatureAI = GetComponent<CreatureAI>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        currentFaction = GetComponentsInParent<FactionBehaviour>()[0];
        //StartCoroutine("RegisterUnknownTile2");
        CheckCurrentRoom();
    }

    protected virtual void Update()
    {
        CheckCurrentRoom();

        if (hasDestination && agent.remainingDistance <= stoppingDistance)
        {
            hasDestination = false;

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

    public void RegisterNewTiles()
    {
        Vector3Int centerCell = Vector3Int.FloorToInt(transform.position);
        centerCell.z = 0;

        int range = Mathf.CeilToInt(detectionRange);
        var faction = currentFaction;

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                Vector3Int checkPos = new Vector3Int(centerCell.x + dx, centerCell.y + dy, 0);

                if (DungeonGenerator.Instance.dungeonMap.TryGetValue(checkPos, out TileInfo tile))
                {
                    if (!faction.knownTilesDict.ContainsKey(checkPos))
                    {
                        faction.knownTilesDict[checkPos] = tile;
                        // Optional: notify faction or creature of discovery
                    }
                }
            }
        }
    }

    private void CheckCurrentRoom()
    {
        Vector3Int pos3D = Vector3Int.FloorToInt(transform.position);
        if (DungeonGenerator.Instance.dungeonMap.TryGetValue(pos3D, out TileInfo tile))
            if (tile.room != null)
            {
                currentRoom = tile.room;
                room = true;
            }
            else
            {
                currentRoom = null;
                room = false;
            }
    }
}
