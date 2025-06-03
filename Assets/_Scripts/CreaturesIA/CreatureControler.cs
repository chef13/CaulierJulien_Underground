
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
// Add the correct namespace if FactionRelationship is defined elsewhere, for example:

[RequireComponent(typeof(NavMeshAgent))]
public class CreatureController : MonoBehaviour
{
    public string currentIAstate;
    public string currentGoal;
    public Vector2 position;
    [HideInInspector] public CreatureState currentState;
    public int currentHP, currentEnergy, currentHunger, currentResources;
    public float stoppingDistance = 1f, attackTimer = 0f;
    public Vector2 destination, tempDestination;
    public NavMeshAgent agent;
    public CreatureAI creatureAI;
    public CreatureType creatureType;
    public CreatureData data;
    public bool hasDestination = false, sleeping = false, isDead = false, isCorpse = false, basicNeed = false;
    [HideInInspector] public RoomInfo currentRoom, previousRoom;
    public FactionBehaviour currentFaction;
    private Coroutine tileDetectionRoutine, hungerCheckRoutine, energyCheckRoutine;
    [HideInInspector] public TileInfo currentTile, lastCheckedTile;
    [HideInInspector]private List<TileInfo> SurroundingTiles = new List<TileInfo>();
    public List<TileInfo> surroundingTiles {
        get { return surroundingTiles; }
        set { surroundingTiles = value; }
    }

    public int currentTileAround;
    public enum hungerState { Starving, Hungry, Normal, Full }
    public hungerState currentHungerState;
    [SerializeField]private bool foodTarget = false;

    public enum energyState { Exhausted, Tired, Normal, Full }
    public energyState currentEnergyState;
    [SerializeField] private List<CreatureController> creaturesInRange = new List<CreatureController>();
    public List<CreatureController> CreaturesInRange
    {
        get { return creaturesInRange; }
        set { creaturesInRange = value; }
    }

    [HideInInspector] public Animator animator;
    [HideInInspector] public SpriteRenderer spriteRenderer;

    protected virtual void OnEnable()
    {
        creatureAI = GetComponent<CreatureAI>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        StartCoroutine(DelayedInit());

    }

    private System.Collections.IEnumerator DelayedInit()
    {
        yield return null;


        currentEnergy = data.maxEnergy;
        currentHP = data.maxLife;
        currentHunger = data.maxHunger;
        currentHungerState = hungerState.Full;
        currentEnergyState = energyState.Full;
        agent.speed = data.speed;

        CheckCurrentRoom();
        StartAllCoroutine();
        yield break;
    }

    protected virtual void Update()
    {

        // chnange sprite orientation based on movement direction
        if (agent.velocity.x > 0.1f)
        {
            spriteRenderer.flipX = false; // facing right
        }
        else if (agent.velocity.x < -0.1f)
        {
            spriteRenderer.flipX = true; // facing left
        }

        if (agent.velocity.magnitude > 0.1f)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        CurrentTileCheck();
        

        if (hasDestination && agent.remainingDistance <= stoppingDistance)
        {
            hasDestination = false;
        }
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
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

    public void OnHit(CreatureController attacker, int damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            OnDeath(attacker);
        }
        else if (currentHP < data.maxLife / 2)
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

    public virtual void OnDeath(CreatureController attacker)
    {
        if (currentFaction != null && attacker != null && attacker != null)
        {
            if (attacker != null && currentFaction.knownFactions.TryGetValue(attacker.currentFaction, out FactionBehaviour.FactionRelationship relationship))
            {
                relationship -= 2;
            }
        }
        isDead = true;
        isCorpse = true;
        agent.isStopped = true;

    }

    public void StartAllCoroutine()
    {
        if (tileDetectionRoutine == null)
            tileDetectionRoutine = StartCoroutine(TilesDetection());
        if (hungerCheckRoutine == null)
            hungerCheckRoutine = StartCoroutine(CheckHungerRoutine());
        if (energyCheckRoutine == null)
            energyCheckRoutine = StartCoroutine(CheckEnergyRoutine());
    }

    public void StopAllCoroutine()
    {
        if (tileDetectionRoutine != null)
        {
            StopCoroutine(tileDetectionRoutine);
            tileDetectionRoutine = null;
        }
        if (hungerCheckRoutine != null)
        {
            StopCoroutine(hungerCheckRoutine);
            hungerCheckRoutine = null;
        }
        if (energyCheckRoutine != null)
        {
            StopCoroutine(energyCheckRoutine);
            energyCheckRoutine = null;
        }
    }
    IEnumerator TilesDetection()
    {
        while (true)
        {
            RegisterNewTiles();
            CheckSurroundingsCreatures();

            if (!foodTarget  && currentHungerState != hungerState.Full)
            {
                if (data.carnivor)
                {
                    //Debug.Log($"{name}" + " checking for carnivor targets");
                    CheckCarnivor();
                }

                if (!foodTarget  && data.herbivor)
                {
                    //Debug.Log($"{name}" + " checking for herbivor targets");
                    CheckHerbivor();
                }
                
            }
            
             yield return new WaitForSeconds(0.2f);
        }
    }

    private void CurrentTileCheck()
    {
        TileInfo tile;
        Vector3Int pos = Vector3Int.FloorToInt(transform.position);
        pos.z = 0;
        if (DungeonGenerator.Instance.dungeonMap.TryGetValue(pos, out tile))
        {
            if (currentTile != tile)
            {
                tile.creatures.Add(this);
                lastCheckedTile?.creatures.Remove(this);
                currentTile?.creatures.Remove(this);
                //Debug.Log($"{name}: Tile with {tile.creatures.Count} creatures at {tile.position}, leaving last tile with {currentTile?.creatures.Count} creatures.");
                //Debug.Log($"{name}: Tile changed to {tile.position}, isDeadEnd: {tile.isDeadEnd}");
                lastCheckedTile = currentTile;
                currentTile = tile;
                CheckSurroundingsTiles();
                CheckCurrentRoom();
            }
        }
    }
    public void RegisterNewTiles()
    {
        Vector3Int centerCell = Vector3Int.FloorToInt(transform.position);
        centerCell.z = 0;

        int range = Mathf.CeilToInt(data.detectionRange);
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
                        faction.tiles++;
                    }
                    else if (faction.knownTilesDict.TryGetValue(checkPos, out TileInfo knowntile))
                    {
                        if (knowntile != tile)
                            faction.knownTilesDict[checkPos] = tile;
                    }

                }
            }
        }
    }

    private void CheckHerbivor()
    {
        //Debug.Log($"{name}"  + " checking for herbivor targets");
            for (int s = 0; s < SurroundingTiles.Count; s++)
            {
                var tile = SurroundingTiles[s];
                if (tile.objects != null && tile.objects.Count > 0)
                {
                    for (int f = 0; f < tile.objects.Count; f++)
                    {
                        var flaureComp = tile.objects[f].GetComponent<FlaureBehaviour>();
                        if (flaureComp != null && flaureComp.isEdible)
                        {
                            foodTarget = true;
                           // Debug.Log($"{name} found edible Flaure {flaureComp.name} at {tile.position}");
                            StartCoroutine(GoEatTarget(tile.objects[f]));
                            break;
                        }
                        //else
                           // Debug.Log($"{name} found Flaure {flaureComp.name} at {tile.position}, but it is not edible.");
                    }
                }
            }
    }
    private void CheckCarnivor()
    {
            //Debug.Log($"{name}"  + " checking for carnivor targets");
            for (int i = 0; i < creaturesInRange.Count; i++)
            {
                
                var creature = creaturesInRange[i];
                if (creature != null && creature.isDead && creature.isCorpse)
                {
                    //Debug.Log($"Found dead creature {creature.name} at {creature.currentTile.position}");
                    foodTarget = true;
                    StartCoroutine(GoEatTarget(creature.gameObject));
                    break;
                }

            }
    }

    private void CheckSurroundingsCreatures()
    {
        creaturesInRange.Clear();

        int range = Mathf.CeilToInt(data.detectionRange);
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                Vector3Int checkPos = new Vector3Int(currentTile.position.x + dx, currentTile.position.y + dy, 0);

                if (DungeonGenerator.Instance.dungeonMap.TryGetValue(checkPos, out TileInfo tile))
                {
                    foreach (CreatureController creature in tile.creatures)
                    {
                        if (creature != this && !creaturesInRange.Contains(creature))
                        {
                            creaturesInRange.Add(creature);
                        }
                    }
                }
            }
        }
    }

    private void CheckSurroundingsTiles()
    {
        SurroundingTiles.Clear();
        int range = Mathf.CeilToInt(data.detectionRange);
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                Vector3Int checkPos = new Vector3Int(currentTile.position.x + dx, currentTile.position.y + dy, 0);

                RaycastHit2D hit = Physics2D.Raycast(transform.position,
                    checkPos - transform.position,
                    Vector2.Distance((Vector2)transform.position, new Vector2(checkPos.x, checkPos.y)),
                    LayerMask.GetMask("Wall"));
                if (hit.collider == null)
                {
                    if (DungeonGenerator.Instance.dungeonMap.TryGetValue(checkPos, out TileInfo tile))
                    {
                        if (!SurroundingTiles.Contains(tile))
                        {
                            SurroundingTiles.Add(tile);
                            currentTileAround++;
                        }
                    }
                }
            }
        }
    }

    private void CheckCurrentRoom()
    {
        if (currentTile != null && DungeonGenerator.Instance.dungeonMap.TryGetValue(currentTile.position, out TileInfo tile) && tile.room != null)
        {
            if ((currentRoom != null && currentRoom == tile.room) || currentRoom == null)
            {
                currentRoom = tile.room;
            }
            else if (currentRoom != tile.room)
            {
                previousRoom = currentRoom;
                currentRoom = tile.room;
            }
        }
        else
        {
            previousRoom = currentRoom;
            currentRoom = null;
        }

        if (currentRoom != null && currentFaction != null && !currentFaction.knownRoomsDict.ContainsKey(currentRoom.index))
        {
            // check if all tiles in the room are known
            bool allTilesKnown = true;
            foreach (TileInfo roomTile in currentRoom.tiles)
            {
                if (!currentFaction.knownTilesDict.ContainsKey(roomTile.position))
                {
                    allTilesKnown = false;
                    break;
                }
            }
            if (allTilesKnown)
            {
                currentFaction.knownRoomsDict.Add(currentRoom.index, currentRoom);
            }
        }
        else if (currentRoom != null && currentFaction != null && currentFaction.knownRoomsDict.ContainsKey(currentRoom.index))
        {
            if (currentFaction.knownRoomsDict.TryGetValue(currentRoom.index, out RoomInfo knownRoom))
            {
                if (knownRoom != currentRoom)
                {
                    currentFaction.knownRoomsDict[currentRoom.index] = currentRoom;
                }
            }
        }
    }

    private void CheckHunger()
    {
        if (currentHunger <= data.maxHunger * 0.1f)
        {
            currentHungerState = hungerState.Starving;
            currentHP--;
            if (currentHP <= 0)
            {
                OnDeath(null); // No attacker since it's starvation
            }
            currentEnergy -= 1;
            if (creatureAI.currentState is not StateAttack && creatureAI.currentState is not StateFlee)
                creatureAI.SwitchState(new StateNeedFood(creatureAI));
        }
        else if (currentHunger < data.maxHunger * 0.35f)
        {

            currentHungerState = hungerState.Hungry;
            currentEnergy -= 1;
             if (!sleeping && creatureAI.currentState is not StateAttack && creatureAI.currentState is not StateFlee)
                creatureAI.SwitchState(new StateNeedFood(creatureAI));
        }
        else if (currentHunger < data.maxHunger * 0.80f)
        {
            currentHungerState = hungerState.Normal;
            basicNeed = false;
        }
        else
        {
            currentHungerState = hungerState.Full;
            basicNeed = false;
        }
    }

    private IEnumerator CheckHungerRoutine()
    {
        while (true)
        {
            currentHunger -= 1;
            CheckHunger();
            yield return new WaitForSeconds(2f);
        }
    }

    private void CheckEnergy()
    {
        if (currentEnergy <= 0)
        {
            Rest();
        }
        if (currentEnergy <= data.maxEnergy * 0.15f)
        {
            currentEnergyState = energyState.Exhausted;
            if (currentHungerState != hungerState.Starving && creatureAI.currentState is not StateAttack && creatureAI.currentState is not StateFlee)
                creatureAI.SwitchState(new StateNeedRest(creatureAI));
        }
        else if (currentEnergy < data.maxEnergy * 0.35f)
        {
            currentEnergyState = energyState.Tired;
            if (currentHungerState != hungerState.Starving && creatureAI.currentState is not StateAttack && creatureAI.currentState is not StateFlee)
                creatureAI.SwitchState(new StateNeedRest(creatureAI));
        }
        else if (currentEnergy < data.maxEnergy * 0.80f)
        {
            if (currentState is StateNeedRest)
            {
                creatureAI.SwitchState(creatureAI.previousState is StateNeedFood ? new StateIdle(creatureAI) : creatureAI.previousState);
            }
            currentEnergyState = energyState.Normal;
            basicNeed = false;
            }
        else
        {
            if (currentState is StateNeedRest)
            {
                creatureAI.SwitchState(creatureAI.previousState is StateNeedFood ? new StateIdle(creatureAI) : creatureAI.previousState);
            }
            basicNeed = false;
            currentEnergyState = energyState.Full;
        }
    }

    private IEnumerator CheckEnergyRoutine()
    {
        while (true)
        {
            CheckEnergy();
            yield return new WaitForSeconds(1f);
        }
    }


    public float GetPathDistance(NavMeshAgent agent, Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            float distance = 0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return distance;
        }
        return -1; // Path not found
    }

    public IEnumerator GoEatTarget(GameObject target)
    {
        while (foodTarget)
        {
            //Debug.Log($"{name} GoEatTarget coroutine started for {target?.name}");
            Vector2 previousDestination = destination;
            tempDestination = target.transform.position;
            SetDestination(tempDestination);
            //Debug.Log($"{name} is going to eat {target.name} at {tempDestination} from {Vector2.Distance(transform.position, tempDestination)}");
            if (target == null || !target.activeInHierarchy)
            {
                target = null;
                foodTarget = false;

                //Debug.LogWarning($"{name} tried to eat a target that is null or inactive.");
                yield break;
            }
            yield return new WaitUntil(() => !hasDestination || Vector2.Distance(transform.position, tempDestination) <= 1f);

            if (target.GetComponent<FlaureBehaviour>() != null)
            {
                FlaureBehaviour flaure = target.GetComponent<FlaureBehaviour>();
                animator.SetTrigger("Attack");
                agent.isStopped = true; // Stop moving while attacking

                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
                agent.isStopped = false; // Resume moving after attack
                flaure.IsEaten();
                currentHunger += flaure.flaureData.edibleAmount;
                foodTarget = false;
            }
            if (target.GetComponent<CreatureController>() != null)
            {
                CreatureController creature = target.GetComponent<CreatureController>();
                animator.SetTrigger("Attack");
                agent.isStopped = true; // Stop moving while attacking

                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
                agent.isStopped = false; // Resume moving after attack
                creature.IsEaten();
                currentHunger += creature.data.maxLife / 2;
                foodTarget = false;
            }
            tempDestination = Vector2.zero;
            SetDestination(previousDestination);
            if (currentHungerState == hungerState.Normal || currentHungerState == hungerState.Full)
            {
                basicNeed = false;
            }
        }
        yield break;
    }

    public void IsEaten()
    {
        isCorpse = false;
        spriteRenderer.sprite = data.skeletonSprite;
        animator.enabled = false;
    }

    public void Rest()
    {
        if (sleeping) return;
        Debug.Log("Resting");
        sleeping = true;
        StartCoroutine(Sleep());
    }
    public IEnumerator Sleep()
    {
        
        while (sleeping)
        {
            if (creatureAI.currentState is StateNeedRest)
            {
                if (creatureAI.previousState is not StateNeedFood)
                {
                    creatureAI.SwitchState(creatureAI.previousState);
                }
                else
                {
                    creatureAI.SwitchState(new StateIdle(creatureAI));
                }
            }
            agent.isStopped = true;
            yield return new WaitForSeconds(0.5f);
            currentEnergy += 1;

            if (currentHungerState == hungerState.Starving)
            {
                sleeping = false;
                agent.isStopped = false;
                creatureAI.SwitchState(new StateNeedFood(creatureAI));
                yield break;
            }

            if (currentHungerState == hungerState.Normal)
            {
                currentEnergy += 1;
            }
            if (currentHungerState == hungerState.Full)
            {
                currentEnergy += 2;
            }
            if (currentEnergy >= data.maxEnergy - 1)
            {
                currentEnergy = data.maxEnergy;
                CheckEnergy();
                sleeping = false;
                basicNeed = false;
                agent.isStopped = false;
                if (creatureAI.previousState is not StateNeedFood)
                {
                    creatureAI.SwitchState(creatureAI.previousState);
                }
                else
                {
                    creatureAI.SwitchState(new StateIdle(creatureAI));
                }

                yield break;
            }
        }
    }

}
