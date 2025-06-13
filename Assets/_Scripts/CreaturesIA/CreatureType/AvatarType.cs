
using System.Collections;
using UnityEngine;

public class AvatarType : CreatureType
{
    protected CreatureController Controller;
    //public new List<Coroutine> GoalsCoroutines = new List<Coroutine>();
    public Coroutine deathCoroutine;



    public AvatarType(CreatureController creature) : base(creature)
    {
        this.Controller = creature;
    }

    private void Start()
    {

    }

    public override void Enter()
    {
    }
    public override void Exit()
    {

    }


    public override void Update()
    {

    }


    public override void OnDeath(CreatureController attacker, bool spell = false)
    {
        deathCoroutine = Controller.StartCoroutine(DisolveDestroy());
        Controller.agent.isStopped = true;
        Controller.agent.speed = 0f;
        Controller.currentFaction.members.Remove(Controller);
        CreatureSpawner.Instance.livingCreatures.Remove(Controller);
    }

    public IEnumerator DisolveDestroy()
    {
        float disolveTime = 2f;
        SpriteRenderer avatarRenderer = Controller.GetComponent<SpriteRenderer>();
        Material material = avatarRenderer.material;
        float elapsedTime = 0f;
        while (elapsedTime < disolveTime)
        {
            elapsedTime += Time.deltaTime;
            float dissolveValue = Mathf.Lerp(0f, 1.1f, elapsedTime / disolveTime);
            material.SetFloat("_dissolveAmount", dissolveValue);
            yield return null; // Frame-based update
        }
        ManaCore.Instance.UnsetControlledAvatar();
        CameraController.Instance.UnlockCamera();
        deathCoroutine = null;
        Controller.gameObject.SetActive(false);
        yield break;
    }

    public override void Destroy()
    {
        Controller.gameObject.SetActive(false);
    }
}
