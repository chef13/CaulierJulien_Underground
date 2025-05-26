using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))][RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 2f;
    private NavMeshAgent agent;
    private Animator animator;
    private  Vector2 moveDirection = Vector2.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
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
       

        if (moveDirection != Vector2.zero)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }
}
