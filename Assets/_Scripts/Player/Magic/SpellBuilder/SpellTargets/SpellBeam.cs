using UnityEngine;
using System.Collections;
public class SpellBeam : SpellTarget
{
    public TileInfo targetTile;
        public SpellBeam(Spell spell) : base(spell)
    {
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override IEnumerator CastingSpell()
    {
        var manaCore = ManaCore.Instance;
        manaCore.lineRenderer.enabled = true;
        manaCore.lineRenderer.positionCount = 2;
        manaCore.lineRenderer.startColor = spell.manaCore.GetColorFromSpellColor(spell.manaCore.currentSpell.spellColor);
        manaCore.lineRenderer.endColor = spell.manaCore.GetColorFromSpellColor(spell.manaCore.currentSpell.spellColor);

        while (!Input.GetMouseButtonUp(0))
        {
            manaCore.currentSpell.transform.position = spell.manaCore.transform.position; // Update spell position to the player's position
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            Vector2 direction = (mouseWorld - spell.manaCore.transform.position).normalized;
            RaycastHit2D hits = Physics2D.Raycast(spell.manaCore.transform.position, direction, spell.manaCore.beamLength, LayerMask.GetMask("wall"));
            Vector3 hitPos = hits.collider != null ? hits.point : spell.manaCore.transform.position + Vector3.ClampMagnitude(mouseWorld - spell.manaCore.transform.position, spell.manaCore.beamLength);
            manaCore.lineRenderer.SetPosition(0, spell.manaCore.transform.position);
            manaCore.lineRenderer.SetPosition(1, hitPos);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Casting cancelled");
                manaCore.lineRenderer.enabled = false;
                manaCore.castingCoroutine = null;
                yield break;
            }

            yield return null; // Wait for next frame
        }

        manaCore.lineRenderer.enabled = false; // Turn off visual after casting
        manaCore.currentSpell.Cast();
        manaCore.castingCoroutine = null;
        yield break;
    }
}
