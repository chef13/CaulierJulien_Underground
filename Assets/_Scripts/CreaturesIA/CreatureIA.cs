using UnityEngine;

[RequireComponent(typeof(CreatureController))]
public class CreatureAI : MonoBehaviour
{
    private CreatureState currentState;

    [HideInInspector] public CreatureController controller;

    void Awake()
    {
        controller = GetComponent<CreatureController>();
    }

    void Start()
    {
        SwitchState(new StateExplore(this));
    }

    void Update()
    {
        currentState?.Update();
    }

    public void SwitchState(CreatureState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
}
