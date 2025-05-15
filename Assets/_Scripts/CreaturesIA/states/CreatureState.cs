
using UnityEngine;

public abstract class CreatureState
{
    protected CreatureAI creature;
    
    public CreatureState(CreatureAI creature)
    {
        this.creature = creature;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }

    public virtual void SetNewDestination() { }

    public GameObject DetectEnemy(out GameObject target)
    {
        // Example: detect enemies within a radius using Physics.OverlapSphereNonAlloc
        float detectionRadius = 5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(creature.transform.position, detectionRadius);
        int hitCount = hits.Length; // Get the number of hits

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Blop"))
            {
                target = hit.gameObject;
                if (target.activeInHierarchy)
                {return target;}
                else
                {target = null;
                return target;}
            }
        }
        target = null;
        return target;
    }
}