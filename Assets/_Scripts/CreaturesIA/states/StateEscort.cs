using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using CrashKonijn.Goap.Runtime;

public class StateEscort : CreatureState
{
    ManaCore manaCore;
    GameObject avatar;
    bool avatarFound = false;
    float distance = 0f;
    Vector2 directionToTarget = Vector2.zero;
    CreatureController target;
    public StateEscort(CreatureAI creature) : base(creature) { }

    public override void Enter()
    {
        manaCore = ManaCore.Instance;
        if (manaCore.currentAvatar != null)
        {
            avatar = manaCore.currentAvatar;
            avatarFound = true;
            Controller.SetDestination(avatar.transform.position);
        }
        else
        {
            Controller.SetDestination(manaCore.transform.position);
        }
        
    }

    public override void Update()
    {

        if (DetectTreat(out target))
        {
            creature.SwitchState(new StateAttack(creature, target));
            return;
        }

        avatarFound = manaCore.currentAvatar != null;

        if (avatarFound)
        {
            distance = Vector3.Distance(Controller.transform.position, avatar.transform.position);
        }
        else
        {
            distance = Vector3.Distance(Controller.transform.position, manaCore.transform.position);
        }

        if (!Controller.hasDestination && avatarFound && distance > Controller.data.detectionRange)
        {
            Controller.SetDestination(avatar.transform.position);
        }
        else if (!Controller.hasDestination && avatarFound && distance < Controller.data.detectionRange)
        {
            directionToTarget = (avatar.transform.position - Controller.transform.position).normalized;
            
                // Already in reasonable range — maybe strafe?
                Vector2 strafeDir = Vector2.Perpendicular(directionToTarget).normalized;
                Vector2 strafePos = (Vector2)creature.transform.position + strafeDir * 1f;
                Controller.SetDestination(strafePos);

        }
        else if (!Controller.hasDestination && !avatarFound)
        {
            Vector3 randomPosAroundCore = manaCore.transform.position + Random.insideUnitSphere * Controller.data.detectionRange;
            Controller.SetDestination(randomPosAroundCore);
        }
    

    }

}
