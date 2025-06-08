using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CreatureController))]
public class CreatureAI : MonoBehaviour
{
    public CreatureState currentState, previousState;
    public CreatureController target, attacker;
    private CreatureController controller;

    void Awake()
    {
        controller = GetComponent<CreatureController>();
    }

    void Start()
    {
        SwitchState(new StateIdle(this));
    }

    void Update()
    {
        if (currentState is StateNeedRest && controller.currentEnergyState == CreatureController.energyState.Full)
        {
            SwitchState(previousState is StateNeedFood ? new StateIdle(this) : previousState);
        }

        currentState?.Update();
    }

    public void OnExit()
    {

    }

    public void SwitchState(CreatureState newState)
    {
        if (newState == null)
            return;

        if (currentState != null && currentState.GetType() == newState.GetType())
            return;
        
        bool isCurrentNeed = currentState is StateNeedFood || currentState is StateNeedRest;
        bool isNewGoal = newState is StateRecolt || newState is StateWander || newState is StatePatrol || newState is StateExplore;
        if (isCurrentNeed && isNewGoal)
        return;

        previousState = currentState;
        currentState?.Exit();
        currentState = newState;
        controller.currentIAstate = currentState.GetType().Name;
        currentState.Controller = controller;
        currentState?.Enter();
    }

    
    private bool LowHealth()
    {
        // Remplacer par ton système de vie
        return false;
    }
}
