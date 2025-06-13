using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))][RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 2f;
    [SerializeField]private NavMeshAgent agent;
    [SerializeField]private Animator animator;
    [SerializeField]private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection = Vector2.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        /*animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();*/
        agent.updateRotation = false;
        agent.updateUpAxis = false;


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            
        }
        else
        {
            moveDirection = Vector2.zero;
        }

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        if (moveDirection != Vector2.zero)
        {
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        }


        if (agent.velocity.magnitude > 0.1f)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);

            
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
    }
}
