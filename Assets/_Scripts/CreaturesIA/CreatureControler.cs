using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CreatureController : MonoBehaviour
{
    public int maxHP, currentHP, damage;
    public float attackRange, attackSpeed, attackTimer, detectionRange;
    public float stoppingDistance = 1f;
    public Vector2 destination;
    protected NavMeshAgent agent;
    public CreatureAI creatureAI;
    public bool hasDestination = false;

    protected virtual void Awake()
    {
        creatureAI = GetComponent<CreatureAI>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

    }

    protected virtual void Update()
    {
        if (hasDestination && agent.remainingDistance <= stoppingDistance)
        {
            hasDestination = false;
            OnDestinationReached();
        }
        if(attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

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
}
