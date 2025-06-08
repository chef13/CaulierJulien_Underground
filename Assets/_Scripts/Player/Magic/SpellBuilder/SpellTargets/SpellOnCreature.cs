using UnityEngine;
using System.Collections;
public class SpellOnCreature : SpellTarget
{
    public TileInfo targetTile;
        public SpellOnCreature(Spell spell) : base(spell)
    {
    }
    public override IEnumerator CastingSpell()
    {
        yield break;
    }
}
