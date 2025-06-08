using UnityEngine;
using System.Collections;
public class SpellOnDungeon : SpellTarget
{
    public TileInfo targetTile;
        public SpellOnDungeon(Spell spell) : base(spell)
    {
    }
        public override IEnumerator CastingSpell()
    {
        yield break;
    }
}
