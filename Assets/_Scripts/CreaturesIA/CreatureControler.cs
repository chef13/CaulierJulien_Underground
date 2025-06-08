
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
// Add the correct namespace if FactionRelationship is defined elsewhere, for example:

[RequireComponent(typeof(NavMeshAgent))]
public class CreatureController : MonoBehaviour
{
    public string currentIAstate;
    public enum CreatureGoal
    {
        None,
        RecoltFood,
        FindHQ,
        Wander,
        Patrol
    }
    public CreatureGoal currentGoal;
    public int currentGoalPriority;
    public bool lookingForRessource = false;
    public Vector2 position;
    [HideInInspector] public CreatureState currentState;
    public int currentHP, currentEnergy, currentHunger, currentResources;
    public float stoppingDistance = 1f, attackTimer = 0f;
    public Vector2 destination, tempDestination;
    public NavMeshAgent agent;
    public CreatureAI creatureAI;
    public CreatureType currentCreatureType;
    public CreatureData data;
    public bool hasDestination = false, sleeping = false, isDead = false, isCorpse = false, basicNeed = false;
    [HideInInspector] public RoomInfo currentRoom, previousRoom;
    public FactionBehaviour currentFaction;
    public Coroutine tileDetectionRoutine, hungerCheckRoutine, energyCheckRoutine, goEatTargetRoutine;
    public float coroutineDelay = 2f;
    [HideInInspector] public TileInfo currentTile, lastCheckedTile;
    [HideInInspector]private List<TileInfo> __surroundingTiles = new List<TileInfo>();
    public List<TileInfo> _surroundingTiles {
        get { return __surroundingTiles; }
        set { __surroundingTiles = value; }
    }

    public int currentTileAround;
    public enum hungerState { Starving, Hungry, Normal, Full }
    public hungerState currentHungerState;
    [SerializeField]public bool foodTarget = false;
    public bool recoltTarget = false;
    public GameObject currentRecoltTarget;
    public GameObject currentFoodTarget;

    public enum energyState { Exhausted, Tired, Normal, Full }
    public energyState currentEnergyState;
    [SerializeField] private List<CreatureController> creaturesInRange = new List<CreatureController>();
    [SerializeField] private List<FlaureBehaviour> flaureInRange = new List<FlaureBehaviour>();
    public int numberOfFlaureInRange = 0;
    public List<CreatureController> CreaturesInRange
    {
        get { return creaturesInRange; }
        set { creaturesInRange = value; }
    }

    [HideInInspector] public Animator animator;
    [HideInInspector] public SpriteRenderer spriteRenderer;

    private float staticCheckDelay = 5f;

    protected virtual void OnEnable()
    {
        creatureAI = GetComponent<CreatureAI>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        StartCoroutine(DelayedInit());
        CurrentTileCheck();

    }

    private IEnumerator DelayedInit()
    {
        yield return null;

        currentCreatureType = GetCreatureTypeInstance();
        currentEnergy = data.maxEnergy;
        currentHP = data.maxLife;
        currentHunger = data.maxHunger;
        currentHungerState = hungerState.Full;
        currentEnergyState = energyState.Full;
        agent.speed = data.speed;
        animator.enabled = true;
        animator.runtimeAnimatorController = data.animator;
        spriteRenderer.sprite = data.sprite;
        this.name = data.name;
        agent.enabled = true;
        agent.isStopped = false;
        lookingForRessource = false;
        hasDestination = false;
        isDead = false;
        isCorpse = false;
        currentTile = null;
        lastCheckedTile = null;
        currentRoom = null;
        previousRoom = null;
        currentTileAround = 0;
        foodTarget = false;
        recoltTarget = false;
        currentGoalPriority = 0;
        CurrentTileCheck();
        //CheckCurrentRoom();
        StartAllCoroutine();
        yield break;
    }

    public CreatureType GetCreatureTypeInstance()
    {
        switch (data.CreatureType)
        {
            case CreatureData.CreatureTypeEnum.Goblin:
                return new GobelinType(this);
            case CreatureData.CreatureTypeEnum.Lezard:
                return new LezardType(this);
            case CreatureData.CreatureTypeEnum.Champi:
                return new ChampiType(this);
            default:
                return null;
        }
    }

    protected virtual void Update()
    {
        if (coroutineDelay >= 0)
        {
            coroutineDelay -= Time.deltaTime;
        }

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

        


        //CurrentTileCheck();

            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                if (hasDestination && agent.remainingDistance <= stoppingDistance)
                {
                    hasDestination = false;
                }
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                }
            }
            
        


    }

    void FixedUpdate()
    {
       /*if (agent.velocity.magnitude < 0.00001 & !sleeping)
        {
            if (staticCheckDelay > 0)
            {
                staticCheckDelay -= Time.deltaTime;
            }
            else
            {
                staticCheckDelay =5f; // Reset the delay
                hasDestination = false; // Reset hasDestination if agent is not moving
            }
        }*/
    }

    public void SetFaction(FactionBehaviour faction)
    {
        currentFaction = faction;
    }

    public virtual void SetDestination(Vector2 destination)
    {
        hasDestination = true;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
        else
        {
            //Debug.LogWarning($"{name}: Tried to SetDestination but agent is not active or not on NavMesh!");
            hasDestination = false;
        }
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
            currentCreatureType.OnDeath(attacker);
        }
        else if (currentHP < data.maxLife / 4)
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

        if (sleeping)
        {
            sleeping = false;
            animator.SetBool("Sleep", false);
            agent.isStopped = false;
            creatureAI.SwitchState(new StateAttack(creatureAI,attacker));
        }
    }

    



    public void StartAllCoroutine()
    {
        /*if (tileDetectionRoutine == null)
            tileDetectionRoutine = StartCoroutine(TilesDetection());
        if (hungerCheckRoutine == null)
            hungerCheckRoutine = StartCoroutine(CheckHungerRoutine());
        if (energyCheckRoutine == null)
            energyCheckRoutine = StartCoroutine(CheckEnergyRoutine());*/
    }

    public void StopAllCoroutine()
    {
        /*if (tileDetectionRoutine != null)
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
        }*/
    }
    /*IEnumerator TilesDetection()
    {
        while (true)
        {
            if (!sleeping)
            {
                CheckSurroundingsCreatures();
                CheckSurroundingsFlaures();

                if (goEatTargetRoutine == null && currentFoodTarget == null && currentHungerState != hungerState.Full)
                {
                    if (data.carnivor)
                    {
                        //Debug.Log($"{name}" + " checking for carnivor targets");
                        CheckCarnivor();
                    }

                    if (!foodTarget && data.herbivor)
                    {
                        //Debug.Log($"{name}" + " checking for herbivor targets");
                        CheckHerbivor();
                    }
                }
            }
            
             yield return new WaitForSeconds(0.2f);
        }
    }*/

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
                RegisterNewTiles();
                
            }
        }
    }
    
    private void RegisterNewTiles()
    {
        var faction = currentFaction;

        for (int t = 0; t < _surroundingTiles.Count; t++)
        {
            Vector3Int checkPos = new Vector3Int(_surroundingTiles[t].position.x, _surroundingTiles[t].position.y, 0);

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

    private void CheckHerbivor()
    {
        //Debug.Log($"{name}"  + " checking for herbivor targets");
        for (int i = 0; i < flaureInRange.Count; i++)
        {

            var flaure = flaureInRange[i];
           // Debug.Log($"{name} checking for herbivor targets in range: {flaureInRange.Count}");
            if (flaure != null && flaure.isActiveAndEnabled && flaure.isEdible)
            {
                if (currentRecoltTarget != null)
                {
                    currentRecoltTarget = null;
                }  
               // Debug.Log($"{name} found a flaure target: {flaure.gameObject.name}");
                currentFoodTarget = flaure.gameObject;
                foodTarget = true;
                goEatTargetRoutine = StartCoroutine(GoEatTarget(currentFoodTarget));
                return;
            }
        }
    }
    private void CheckCarnivor()
    {
            //Debug.Log($"{name}"  + " checking for carnivor targets");
            for (int i = 0; i < creaturesInRange.Count; i++)
            {
                
                var creature = creaturesInRange[i];
                if (creature != null && creature.isActiveAndEnabled && creature.isDead && creature.isCorpse)
                {
                    if (currentRecoltTarget != null )
                    {
                    currentRecoltTarget = null;
                    }
                    currentFoodTarget = creature.gameObject;
                    foodTarget = true;
                    goEatTargetRoutine = StartCoroutine(GoEatTarget(currentFoodTarget));
                    return;
                }

            }
    }

    private void CheckSurroundingsFlaures()
    {
        flaureInRange.Clear();
        numberOfFlaureInRange = 0;

        for (int i = 0; i < _surroundingTiles.Count; i++)
        {
            TileInfo tile = _surroundingTiles[i];
            if (tile.objects != null && tile.objects.Count > 0)
            {
                foreach (GameObject flaure in tile.objects)
                {
                    var flaureBehaviour = flaure.GetComponent<FlaureBehaviour>();
                    if (flaureBehaviour != null)
                    {
                        flaureInRange.Add(flaureBehaviour);
                        numberOfFlaureInRange++;
                    }
                }
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
        _surroundingTiles.Clear();
        currentTileAround = 0;
        int range = data.detectionRange;
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                Vector3Int checkPos = new Vector3Int(currentTile.position.x + dx, currentTile.position.y + dy, 0);
                Vector2 start = transform.position;
                Vector2 end = new Vector2(checkPos.x, checkPos.y);
                Vector2 direction = (end - start).normalized;
                float distance = Vector2.Distance(start, end);
                RaycastHit2D hit = Physics2D.Raycast(
                    start,
                    direction,
                    distance,
                    LayerMask.GetMask("Wall"));
                if (hit.collider == null)
                {
                    if (DungeonGenerator.Instance.dungeonMap.TryGetValue(checkPos, out TileInfo tile))
                    {
                        if (tile.isFloor && !_surroundingTiles.Contains(tile))
                        {
                            _surroundingTiles.Add(tile);
                            currentTileAround++;
                        }
                    }
                }
            }
        }
    }

    public void AllCheckForCoroutine()
    {
        if (!sleeping)
        {
            CurrentTileCheck();
            //RegisterNewTiles();
        }
        CheckEnergy();
        if (!sleeping)   // Optional: move logic out of coroutine
            CheckHunger();
        CheckSurroundingsCreatures();
        if (!sleeping && creatureAI.currentState is not StateAttack && creatureAI.currentState is not StateFlee)
        {

            CheckSurroundingsFlaures();

            if (currentFoodTarget == null && currentHungerState != hungerState.Full)
            {
                if (data.carnivor)
                {
                    //Debug.Log($"{name}" + " checking for carnivor targets");
                    CheckCarnivor();
                }

                if (!foodTarget && data.herbivor)
                {
                    //Debug.Log($"{name}" + " checking for herbivor targets");
                    CheckHerbivor();
                }
            }

            if (currentFoodTarget == null && creatureAI.currentState is StateRecolt)
            {
                if (data.carnivor)
                {
                    creatureAI.currentState.CheckCarnivor();
                }
                if (data.herbivor && !recoltTarget)
                {
                    creatureAI.currentState.CheckHerbivor();
                }
            }
        }
        
        if (currentHP < data.maxLife / 4 && creatureAI.currentState is not StateFlee && creatureAI.currentState is not StateAttack)
        {
            creatureAI.SwitchState(new StateNeedRest(creatureAI));
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


        if (currentRoom != null && currentRoom.faction == currentFaction)
        {
            if (currentResources != 0)
            {
                currentFaction.foodResources += currentResources;
                currentResources = 0;
                hasDestination = false;
            }

        }

    }

    private void CheckHunger()
    {
        ChangeHunger(-1);
        if (currentRoom != null && currentRoom.faction != null && currentRoom.faction == currentFaction)
        {
            if (currentHungerState != hungerState.Full && currentFaction.foodResources > 0)
            {

                currentFaction.foodResources -= 2;
                ChangeHunger(+2);
            }
        }

            if (currentHunger <= data.maxHunger * 0.1f)
        {
            currentHungerState = hungerState.Starving;
            currentHP--;
            if (currentHP <= 0)
                currentCreatureType.OnDeath(null);
            ChangeEnergy(-1);
            if ((data.carnivor || data.herbivor) && creatureAI.currentState is not StateAttack && creatureAI.currentState is not StateFlee && creatureAI.currentState is not StateNeedFood)
                creatureAI.SwitchState(new StateNeedFood(creatureAI));
        }
        else if (currentHunger < data.maxHunger * 0.35f)
        {

            currentHungerState = hungerState.Hungry;
            ChangeEnergy(-1);
            if ((data.carnivor || data.herbivor) && !sleeping && creatureAI.currentState is not StateAttack && creatureAI.currentState is not StateFlee)
                creatureAI.SwitchState(new StateNeedFood(creatureAI));
        }
        else if (currentHunger < data.maxHunger * 0.80f)
        {
            currentHungerState = hungerState.Normal;
            basicNeed = false;
        }
        else if (currentHunger >= data.maxHunger * 0.80f)
        {
            currentHungerState = hungerState.Full;
            basicNeed = false;
        }
        else if (currentHunger > data.maxHunger)
        {
            currentHungerState = hungerState.Full;
            basicNeed = false;
            ChangeEnergy(-1);
        }
    }

    public void ChangeHunger(int amount)
    {
        if (currentHunger < 0 && amount < 0) return;
        currentHunger += amount;
    }
    
        public void ChangeEnergy(int amount)
    {
        if (currentEnergy < 0 && amount < 0) return;
        currentEnergy += amount;
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
            if (currentHungerState != hungerState.Starving && creatureAI.currentState is not StateAttack && creatureAI.currentState is not StateFlee && creatureAI.currentState is not StateNeedFood && creatureAI.currentState is not StateNeedRest)
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
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        return -1f; // Agent not ready

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
        return -1f; // Path not found
    }

    public bool CheckOntargetValidity(GameObject target)
    {
        if (target == null || !target.activeInHierarchy)
            return false;

        FlaureBehaviour flaure = target.GetComponent<FlaureBehaviour>();
        if (flaure != null && flaure.gameObject.activeInHierarchy && !flaure.isEdible)
            return false;

        CreatureController creature = target.GetComponent<CreatureController>();
        if (creature != null && creature.gameObject.activeInHierarchy && creature.isDead && !creature.isCorpse)
            return false;

        return true;
    }
    public IEnumerator GoEatTarget(GameObject target)
    {
        if (target == null) yield break;

        Vector2 previousDestination = destination;
        tempDestination = target.transform.position;
        SetDestination(tempDestination);

        while (target != null && target.activeInHierarchy)
        {
            float distance = Vector2.Distance(transform.position, tempDestination);

            // Reacquire target position and check if still valid
            tempDestination = target.transform.position;
            if (!hasDestination || distance <= 1.5f)
            {
                agent.isStopped = true;
                animator.SetTrigger("Attack");
                yield return new WaitForSeconds(1f);
                agent.isStopped = false;

                if (target.TryGetComponent(out FlaureBehaviour flaure) && flaure.isEdible)
                {
                    flaure.IsEaten();
                    currentHunger += flaure.flaureData.edibleAmount;
                }
                else if (target.TryGetComponent(out CreatureController corpse) && corpse.isCorpse)
                {
                    corpse.currentCreatureType.IsEaten();
                    currentHunger += corpse.data.maxLife / 2;
                }

                break;
            }

            // If target is no longer valid mid-walk
            if (!CheckOntargetValidity(target))
            {
                break;
            }

            // Reissue destination if needed
            if (!hasDestination)
            {
                SetDestination(tempDestination);
            }

            yield return null;
        }

        // Final cleanup
        SetDestination(previousDestination);

        if (currentHungerState == hungerState.Normal || currentHungerState == hungerState.Full)
        {
            basicNeed = false;
        }

        foodTarget = false;
        currentFoodTarget = null;
        goEatTargetRoutine = null;
    }

    public void Rest()
    {
        if (sleeping) return;
        //Debug.Log("Resting");
        sleeping = true;
        animator.SetBool("Sleep", true);
        StartCoroutine(Sleep());
    }
    public IEnumerator Sleep()
    {
        
        while (sleeping)
        {/*
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
            }*/
            agent.isStopped = true;
            yield return new WaitForSeconds(0.5f);
            currentEnergy += 1;
            currentHP += 1;
            if (currentHungerState == hungerState.Normal)
            {
                currentHP += 2;
                currentEnergy += 1;
            }
            if (currentHungerState == hungerState.Full)
            {
                currentHP += 2;
                currentEnergy += 2;
            }
            if (currentEnergy >= data.maxEnergy - 1 && currentHP >= data.maxLife - 1)
            
            {
                currentEnergy = data.maxEnergy;
                CheckEnergy();
                animator.SetBool("Sleep", false);
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
