using UnityEngine;

public class GreyBlop : BlopType
{
    private GameObject target;

    override public int maxHP { get; set; } = 80;
    override public int damage { get; set; } = 10;
    override public float speed { get; set; } = 40f;
    override public float attackRange { get; set; } = 0.5f;

    public GreyBlop(BlopBehaviour blop) : base(blop)
    {
        
    }

      public override void Enter()
    {
        Color color= Color.grey;
        blop.GetComponent<SpriteRenderer>().color = color;
    }
    public override void Attack(GameObject target)
    {
        // Logique d’attaque
        var CreatureController = target.GetComponent<CreatureController>();
       // CreatureController.OnHit(blop.gameObject, blop.damage);
    }

    public override void OnHit(GameObject attacker)
    {
        blop.target = attacker;
    }

    private bool LowHealth()
    {
        // Remplacer par ton système de vie
        return false;
    }
}
