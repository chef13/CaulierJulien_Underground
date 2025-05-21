using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BlueBlop : BlopType
{
    private GameObject target;
    override public int maxHP { get; set; } = 50;
    override public int damage { get; set; } = 5;
    override public float speed { get; set; } = 50f;
    override public float attackRange { get; set; } = 5f;
    

    public BlueBlop(BlopBehaviour blop) : base(blop)
    {
        
    }

      public override void Enter()
    {
        Color color= Color.blue;
        blop.GetComponent<SpriteRenderer>().color = color;
        blop.AddComponent<LineRenderer>();
        
        blop.lineRenderer.startColor = Color.blue;
        blop.lineRenderer.endColor = Color.blue;
    }
    public override void Exit()
    {
        
    }
    public override void Attack(GameObject target)
    {
       //Draw a line to the target
        blop.lineRenderer = blop.GetComponent<LineRenderer>();
        blop.lineRenderer.SetPosition(0, blop.transform.position);
        blop.lineRenderer.SetPosition(1, target.transform.position);
        blop.lineRenderer.enabled = true;
         blop.StartCoroutine(DisableLineAfterDelay(blop.lineRenderer, 0.2f));

        var CreatureController = target.GetComponent<CreatureController>();
        CreatureController.OnHit(blop.gameObject, blop.damage);

    }

    public override void OnHit(GameObject attacker)
    {
        if (blop.currentHP <= blop.maxHP / 2)
        {
           blop.direction = (blop.transform.position - attacker.transform.position).normalized;
            blop.rb.AddForce(blop.direction * blop.speed, ForceMode2D.Force);
            blop.target = null; // Ne pas cibler l'attaquant
        }
        else
        {
            blop.target = attacker;
        }
    }
    private bool LowHealth()
    {
        // Remplacer par ton système de vie
        return false;
    }

    private IEnumerator DisableLineAfterDelay(LineRenderer lineRenderer, float delay)
    {
        yield return new WaitForSeconds(delay);
        lineRenderer.enabled = false;
    }
}
