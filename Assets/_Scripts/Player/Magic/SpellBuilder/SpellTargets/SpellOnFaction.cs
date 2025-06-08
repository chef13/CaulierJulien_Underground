using UnityEngine;
using System.Collections;
public class SpellOnFaction : SpellTarget
{
    public TileInfo targetTile;
        public SpellOnFaction(Spell spell) : base(spell)
    {
    }
    public override IEnumerator CastingSpell()
    {
        yield break;
    }
}
