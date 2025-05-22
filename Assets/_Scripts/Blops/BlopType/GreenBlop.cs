
using UnityEngine;

public class GreenBlop : BlopType
{
    private GameObject target;

    override public int maxHP { get; set; } = 60;
    override public int damage { get; set; } = 5;
    override public float speed { get; set; } = 60f;
    override public float attackRange { get; set; } = 1f;
    

    public GreenBlop(BlopBehaviour blop) : base(blop)
    {
        
    }

      public override void Enter()
    {
        Color color= Color.green;
        blop.GetComponent<SpriteRenderer>().color = color;
    }
    public override void Attack(GameObject target)
    {
        blop.direction = (blop.transform.position - target.transform.position).normalized;
        blop.rb.AddForce(blop.direction * blop.speed, ForceMode2D.Impulse);
        // Logique d’attaque
        var CreatureController = target.GetComponent<CreatureController>();
        CreatureController.OnHit(blop.gameObject, blop.damage);
    }

    public override void OnHit(GameObject attacker)
    {
         if (blop.currentHP <= blop.maxHP / 4)
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
}
