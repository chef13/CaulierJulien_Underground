using UnityEngine;
using System.Collections;
public class SpellOnRoom : SpellTarget
{
    public TileInfo targetTile;
        public SpellOnRoom(Spell spell) : base(spell)
    {
    }
        public override IEnumerator CastingSpell(Spell spell)
    {
        yield break;
    }
}
