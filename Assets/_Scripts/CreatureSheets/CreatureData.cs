using UnityEngine;

[CreateAssetMenu(fileName = "CreatureData", menuName = "ScriptableObjects/CreatureData", order = 1)]
public class CreatureData : ScriptableObject
{
    public Sprite sprite;
    public string creatureName;
    public int health;
    public int attackPower;
    public float speed;
    public float attackRange;
    public float attackCooldown;

    // Add any other properties you need for your creature data
}
