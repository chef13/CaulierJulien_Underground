using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Magic/Effects/BlueRoomRitualEffect")]
public class BlueRoomRitualEffectSO : SpellEffectSO
{

    private static HashSet<TileInfo> waterAffectedTiles = new();

public override void ApplyEffect(Spell caster, GameObject target = null, int rangelevel = 1, int powerlevel = 1, int durationlevel = 1)
{

}

}
