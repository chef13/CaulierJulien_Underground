using System.Collections;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.AI;

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
    
    public FactionBehaviour currentFaction;

    protected virtual void Awake()
    {
        creatureAI = GetComponent<CreatureAI>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        currentFaction = GetComponentsInParent<FactionBehaviour>()[0];
        roomGenerator = GameObject.FindFirstObjectByType<RoomFirstDungeonGenerator>();
        StartCoroutine("RegisterUnknownTile");
    }

    protected virtual void Update()
    {
        if (hasDestination && agent.remainingDistance <= stoppingDistance)
        {
            hasDestination = false;
            CheckCurrentRoom();
            //OnDestinationReached();
        }
        if(attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        if (currentEnergy < maxEnergy/4)
        {
            creatureAI.SwitchState(new StateReccover(creatureAI));
        }

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

    public void CheckCurrentRoom()
    {
        foreach (var room in roomGenerator.dgRoomslist)
        {
            // Check if the room is within the explore range
            if (Vector2.Distance(transform.position, room.transform.position) < detectionRange)
            {
                RoomInfo roomInfo = room.GetComponent<RoomComponent>().roomInfo; // Make sure RoomInfo inherits from MonoBehaviour
                foreach (var tile in roomInfo.positions)
                {
                    if (tile.position == Vector2Int.RoundToInt(transform.position))
                    {
                        // If the tile is in the room, set it as the current room
                        currentRoom = room;
                        break;
                    }
                }
                if (currentRoom != null)
                {
                    break;
                }
            }
        }
    }

    IEnumerator RegisterUnknownTile()
    {
        

        
        Vector2 center = transform.position;
        int tileLayer = 15;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, detectionRange, tileLayer);

        foreach (var hit in hits)
        {
            GameObject tile = hit.gameObject;
            TileComponent tileComp = hit.GetComponent<TileComponent>();
            TileInfo tileInfo = tileComp.tileInfo;
            if (!currentFaction.knownTiles.Contains(tile))
            {
                currentFaction.knownTiles.Add(tile);
            }
        }

        yield return new WaitForSeconds(1f);
        StartCoroutine("RegisterUnknownTile");
    }
}
