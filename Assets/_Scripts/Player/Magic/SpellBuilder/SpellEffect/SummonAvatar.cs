using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using System.Collections;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(menuName = "Magic/Effects/SummnAvatar")]
public class SummnAvatar : SpellEffectSO
{
    public float summoningTime = 2f;
    public GameObject avatarPrefab; // Prefab of the avatar to summon
    public GameObject avatar;

    public Material blueAvatar;
    public Material greenAvatar;
    public Material whiteAvatar;
    public Material blackAvatar;
    public Material brownAvatar;

    public override void ApplyEffect(Spell caster, GameObject target = null, int rangelevel = 1, int powerlevel = 1, int durationlevel = 1)
    {
        var manaCore = ManaCore.Instance;

        Debug.Log("🧙 Summoning avatar...");
        ManaCore.Instance.summonAvatar(avatarPrefab, summoningTime);

    }
    

    public IEnumerator SummoningAvatar(GameObject avatar, float summoningTime, Spell caster)
    {
        var manaCore = ManaCore.Instance;
        while (true)
        {

            var spawnPos = manaCore.transform.position + Vector3.right; // Spawn beside caster
            //spawnPos.z = 0f; // Ensure the avatar is on the same plane
            //manaCore.currentAvatar = Instantiate(avatar, spawnPos, Quaternion.identity);
            CreatureSpawner.Instance.SpawnCreatureInRoom(spawnPos, avatar, manaCore.dungeonFaction);
            manaCore.SetControlledAvatar(manaCore.currentAvatar); // Optional link for spellcasting
            GameObject currentAvatar = manaCore.currentAvatar;

            yield return new WaitForEndOfFrame(); // Optional: delay before summon starts
            manaCore.avatarController = currentAvatar.GetComponent<CreatureController>();
            manaCore.avatarController.currentFaction = manaCore.dungeonFaction; // Set the faction for the new avatar
            // Lock the camera on the new avatar
            CameraController.Instance?.LockOnAvatar(currentAvatar);

            
            SpriteRenderer avatarRenderer = currentAvatar.GetComponent<SpriteRenderer>();

            Material material = avatarRenderer.material;
            yield return new WaitForEndOfFrame();
            if (caster != null)
            {
                switch (caster.spellColor)
                {
                    case Spell.SpellColors.Blue:
                        avatarRenderer.material = AvatarGenerator.Instance.blueAvatar;
                        break;
                    case Spell.SpellColors.Green:
                        avatarRenderer.material = AvatarGenerator.Instance.greenAvatar;
                        break;
                    case Spell.SpellColors.White:
                        avatarRenderer.material = AvatarGenerator.Instance.whiteAvatar;
                        break;
                    case Spell.SpellColors.Black:
                        avatarRenderer.material = AvatarGenerator.Instance.blackAvatar;
                        break;
                    case Spell.SpellColors.Brown:
                        avatarRenderer.material = AvatarGenerator.Instance.brownAvatar;
                        break;
                }
            }
            else
            {
                Debug.LogWarning("Caster is null, using default material.");
                avatarRenderer.material = brownAvatar;
            }
            Debug.Log("Assigning material: " + avatarRenderer.material.name);
            yield return new WaitForEndOfFrame();
            // Always update the reference after assignment!
            material = avatarRenderer.material;


            yield return new WaitForEndOfFrame(); // Optional: delay before starting the dissolve effect
            float elapsedTime = 0f;
            while (elapsedTime < summoningTime)
            {
                elapsedTime += Time.deltaTime;
                float dissolveValue = Mathf.Lerp(1.1f, 0f, elapsedTime / summoningTime);
                material.SetFloat("_dissolveAmount", dissolveValue);
                yield return null; // Frame-based update
            }

            // Ensure final state
            material.SetFloat("_dissolveAmount", 0f);

            manaCore.summonAvatarCoroutine = null; // Reset coroutine reference
            break; // Exit the loop after summoning

        }
        yield break;
    }


}
