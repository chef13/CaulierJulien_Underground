using UnityEngine;
using UnityEngine.AI;

public class NavMeshTools : MonoBehaviour
{
    static public NavMeshTools Instance;
    NavMeshPlus.Components.NavMeshSurface surface;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}
