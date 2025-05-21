using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.AI;

public class BlopBehaviour : MonoBehaviour
{
    static public BlopType currentblopType;

    public int maxHP, currentHP, damage;
    public float attackRange, attackSpeed, attackTimer, detectionRange, speed;

    public GameObject target;
    public Rigidbody2D rb;
    CircleCollider2D circleCollider;
    Animator animator;
    SpriteRenderer spriteRenderer;
    public LineRenderer lineRenderer;
    // Replace 'object' with the actual type of 'blops' if known, and ensure 'blops' is defined somewhere
    public Vector2 direction;

    /*public enum blopType
    { grey,blue,red,green,black }
    [SerializeField] public blopType type;
    [SerializeField] Color color;*/
    public void SwitchType(BlopType newType)
    {
        currentblopType?.Exit();
        currentblopType = newType;
        currentblopType?.Enter();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SwitchType(new GreyBlop(this));
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        lineRenderer = GetComponent<LineRenderer>();
        RandomizeDirection();
        
    }
    void OnEnable()
    {
        maxHP = currentblopType.maxHP;
        currentHP = maxHP;
        damage = currentblopType.damage;
        attackRange = currentblopType.attackRange;
        attackSpeed = currentblopType.attackSpeed;
        speed = currentblopType.speed;
        detectionRange = currentblopType.attackRange * 2;
        attackTimer = 0;
        target = null;
        RandomizeDirection();
    }
    

    // Update is called once per frame
    void Update()
    {

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
        else
        {
            attackTimer = 0;
        }
        
        if (target != null)
        {
            Attack();
        }
        else
        {
            Move();
        }
            
    }


    public void Move()
    {
        
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("MoveBlop"))
        rb.linearVelocity = direction * speed * Time.deltaTime;
        else
        rb.linearVelocity = Vector2.zero;
        
    }

    public void Attack()
    {
     float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance <= attackRange && attackTimer <= 0)
        {
            currentblopType.Attack(target);
            attackTimer = attackSpeed;
        }
        else if (distance > attackRange)
        {
            direction = (target.transform.position - transform.position).normalized;
            Move();
        }
        
        else if (distance > detectionRange || target.activeInHierarchy == false) 
        {
            target = null;
            RandomizeDirection();
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        RandomizeDirection();
    }


    public void RandomizeDirection()
    {
        direction = Vector2.zero;
        
            List<Vector2Int> potenitalDirection = new List<Vector2Int>();
        foreach (var neighbour in Direction2D.eightDirectionsList)
            {
                Vector2Int neighbourPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y) + neighbour;
                if (IsWalkable(new Vector3(neighbourPosition.x, neighbourPosition.y, 0)))
                {
                    potenitalDirection.Add(neighbour);
                }
            }
            if (potenitalDirection.Count > 0)
            {
                int randomIndex = Random.Range(0, potenitalDirection.Count);
                direction = new Vector2(potenitalDirection[randomIndex].x, potenitalDirection[randomIndex].y);
            }
            else
            {
                Debug.LogWarning("No valid directions found for blop movement.");
            }
    }

    private bool IsWalkable(Vector2 position)
    {
        // Use Physics2D to check if the position is walkable (no collider at that point)
        Collider2D hit = Physics2D.OverlapCircle(position, 0.1f, LayerMask.GetMask("Wall"));
        return hit == null;
    }

    public void OnDeath()
    {
        BlopSpawner.Instance.blopList.Add(gameObject);
        BlopSpawner.Instance.blopSpawnedList.Remove(gameObject);
        gameObject.SetActive(false);
        transform.position = new Vector3(-100, -100, 0);
    }

    public void OnHit(int damage, GameObject attacker)
    {
        currentHP -= damage;
        currentblopType.OnHit(attacker);
        if (currentHP <= 0)
        {
            OnDeath();
        }
        
    }

       private GameObject DetectEnemy(out GameObject target)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        int hitCount = hits.Length; // Get the number of hits

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Mobs"))
            {
                var hitControler = hit.GetComponent<CreatureController>();
                target = hit.gameObject;
                if (target.activeInHierarchy && hitControler.creatureAI.target == this.gameObject)
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
