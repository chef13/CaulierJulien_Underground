using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Magic/Effects/SummnAvatar")]
public class SummnAvatar : SpellEffectSO
{
    public GameObject avatarPrefab; // Prefab of the avatar to summon

    public override void ApplyEffect(Spell caster, GameObject target = null, int rangelevel = 1, int powerlevel = 1, int durationlevel = 1)
    {
        var spawnPos = caster.transform.position + Vector3.right; // Spawn beside caster
        GameObject avatar = Object.Instantiate(avatarPrefab, spawnPos, Quaternion.identity);

        if (avatar.TryGetComponent(out CreatureController controller))
        {
            controller.isPlayer = true; // You'll define this flag in CreatureController
            
        }
        ManaCore.Instance.SetControlledAvatar(avatar); // Optional link for spellcasting

        Debug.Log("🧙 Avatar summoned and ready!");
    }
    

}
