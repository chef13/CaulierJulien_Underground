using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class testmobscript : MonoBehaviour
{
    private NavMeshAgent agent;
    public NavMeshPlus.Components.NavMeshSurface surface;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {   
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        agent.SetDestination(mousePosition);
    }
}
