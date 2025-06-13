using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;


public class ManaCore : MonoBehaviour
{

    public static ManaCore Instance; // Singleton instance
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // Set the singleton instance
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }
    public SpellBuilder spellBook;
    public List<GameObject> spellObjects = new List<GameObject>();
    public Spell currentSpell;// UI images for spell icons
    public SpriteRenderer renderer;
    [Range(0, 5)]
    public int spellSlotCount = 5; // Number of spell slots for different levels (for UI/Inspector)

    // In Start or Awake, you may want to initialize each slot:
    // for (int i = 0; i < spellSlots.Length; i++) spellSlots[i] = new List<GameObject>();
    public Transform spellOrigin; // Origin point for spell casting
    public GameObject currentAvatar;
    public CreatureController avatarController; // Reference to the avatar controller
    public FactionBehaviour dungeonFaction; // Faction data for the dungeon
    public Coroutine summonAvatarCoroutine; // Coroutine for summoning avatar
    private Color spellColor; // Color of the currently selected spell
    public LineRenderer lineRenderer; // Line renderer for visual effects
    public GameObject tileTargeter;
    public int mana = 100; // Initial mana value
    public int maxMana = 200; // Maximum mana value
    public Slider manaSlider; // UI slider to display mana
    public TMP_Text manaText; // UI text to display mana value
    public int baseManaRegenRate = 1, manaRegenModif = 0, deathAbsorb = 0, spawnAbsorb = 0; // Mana regeneration rate per second
    public float baseManaRegenInterval = 1f; // Time interval for mana regeneration

    public Coroutine castingCoroutine, castingFactionRitual, castingDungeonRitual, castingAreaRitual, manaRegenCoroutine; // Coroutine for casting spells and mana regeneration
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spellBook = SpellBuilder.Instance; // Get the spell book canvas from SpellBuilder
        lineRenderer = GetComponent<LineRenderer>();
        renderer = GetComponent<SpriteRenderer>();
        spellOrigin = this.transform; // Set the spell origin to this object's position
        dungeonFaction = FactionSpawner.instance.dungeonFaction; // Get the dungeon faction from FactionSpawner
        tileTargeter.transform.SetParent(null);

        if (DungeonGenerator.Instance.dungeonMap.TryGetValue(new Vector3Int((int)transform.position.x, (int)transform.position.y, 0), out TileInfo tileInfo))
        {
            tileInfo.room.faction = dungeonFaction; // Set the faction for the room
            dungeonFaction.currentHQ.Add(tileInfo.room); // Add the room to the dungeon faction
            for (int i = 0; i < tileInfo.room.tiles.Count; i++)
            {
                tileInfo.room.tiles[i].faction = dungeonFaction; // Set the faction for each tile in the room
            }
        }
        manaSlider = spellBook.manaSlider; // Get the mana slider from the spell book canvas
        manaText = spellBook.manaText; // Get the mana text from the spell book canvas
        manaSlider.value = (float)mana / maxMana; // Initialize the UI slider with current mana value
        manaText.text = mana.ToString() + "/" + maxMana.ToString(); // Initialize the UI text with current mana value

        manaRegenCoroutine = StartCoroutine(ManaRegenCoroutine()); // Start the mana regeneration coroutine
    }

    // Update is called once per frame
    void Update()
    {

        /*if (Input.GetMouseButtonDown(0) && castingCoroutine == null && !spellBook.gameObject.activeSelf) // Example input to cast a spell
        {

            Debug.Log("Casting spell: " + currentSpell.gameObject.name);

            castingCoroutine = StartCoroutine(currentSpell.targetHandler.CastingSpell());
            if (Input.GetKeyDown(KeyCode.E))
            {
                spellBook.SwitchSpellSelected(true);
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                spellBook.SwitchSpellSelected(false);
            }*/
        if (castingCoroutine == null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && !spellBook.spell1.emptySpell && CheckMana(spellBook.spell1.manaCost))
            {
                castingCoroutine = StartCoroutine(spellBook.spell1.targetHandler.CastingSpell(spellBook.spell1));
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) && !spellBook.spell2.emptySpell && CheckMana(spellBook.spell2.manaCost))
            {
                castingCoroutine = StartCoroutine(spellBook.spell2.targetHandler.CastingSpell(spellBook.spell2));
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) && !spellBook.spell3.emptySpell && CheckMana(spellBook.spell3.manaCost))
            {
                castingCoroutine = StartCoroutine(spellBook.spell3.targetHandler.CastingSpell(spellBook.spell3));
            }
            if (Input.GetKeyDown(KeyCode.Alpha4) && !spellBook.spell4.emptySpell && CheckMana(spellBook.spell4.manaCost))
            {
                castingCoroutine = StartCoroutine(spellBook.spell4.targetHandler.CastingSpell(spellBook.spell4));
            }
            if (Input.GetKeyDown(KeyCode.Alpha5) && !spellBook.spell5.emptySpell && CheckMana(spellBook.spell5.manaCost))
            {
                castingCoroutine = StartCoroutine(spellBook.spell5.targetHandler.CastingSpell(spellBook.spell5));
            }

        }
        CreatureSpawner.Instance.CreatureDeath.AddListener(CreatureDeathInDG);
        CreatureSpawner.Instance.CreatureSpawned.AddListener(CreatureSpawninDG);

        //manaSlider.value = (float)mana / maxMana; // Update the UI slider with current mana value
    }


    public void SetControlledAvatar(GameObject avatar)
    {
        currentAvatar = avatar;
        avatarController = avatar.GetComponent<CreatureController>();
        spellOrigin = avatar.transform; // Set the spell origin to the avatar's position
    }

    public void UnsetControlledAvatar()
    {
        currentAvatar = null;
        avatarController = null;
        spellOrigin = this.transform; // Clear the spell origin when no avatar is controlled
    }

    public void CastCurrentSpellFromAvatar(CreatureController avatar)
    {
        if (currentSpell != null)
        {
            currentSpell.transform.position = avatar.transform.position;
            currentSpell.GetComponent<Spell>().Cast();
        }
    }

    public Color GetColorFromSpellColor(Spell.SpellColors spellColor)
    {
        switch (spellColor)
        {
            case Spell.SpellColors.Blue:
                return Color.blue;
            case Spell.SpellColors.Green:
                return Color.green;
            case Spell.SpellColors.White:
                return Color.white;
            case Spell.SpellColors.Black:
                return Color.darkViolet;
            case Spell.SpellColors.Brown:
                return new Color(0.6f, 0.4f, 0.2f); // Brown color
            default:
                return Color.white;
        }
    }

    public void summonAvatar(GameObject avatar, float summoningTime)
    {
        if (summonAvatarCoroutine != null)
        {
            StopCoroutine(summonAvatarCoroutine); // Stop any existing summoning coroutine
        }
        summonAvatarCoroutine = StartCoroutine(SummoningAvatar(avatar, summoningTime));
    }

    public IEnumerator SummoningAvatar(GameObject avatar, float summoningTime)
    {
        while (true)
        {

            var spawnPos = transform.position + Vector3.right; // Spawn beside caster
            //spawnPos.z = 0f; // Ensure the avatar is on the same plane
            currentAvatar = Instantiate(avatar, spawnPos, Quaternion.identity);
            CreatureSpawner.Instance.SpawnCreatureInRoom(spawnPos, avatar, dungeonFaction);
            SetControlledAvatar(currentAvatar); // Optional link for spellcasting


            yield return new WaitForSeconds(0.1f); // Optional: delay before summon starts
            avatarController = currentAvatar.GetComponent<CreatureController>();
            avatarController.currentFaction = dungeonFaction; // Set the faction for the new avatar
            // Lock the camera on the new avatar
            CameraController.Instance?.LockOnAvatar(currentAvatar);

            SpriteRenderer avatarRenderer = currentAvatar.GetComponent<SpriteRenderer>();

            Material material = avatarRenderer.material;

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

            summonAvatarCoroutine = null; // Reset coroutine reference
            break; // Exit the loop after summoning

        }
        yield break;
    }

    private void CreatureSpawninDG()
    {
        mana += spawnAbsorb; // Absorb mana from creature spawn
        manaSlider.value = (float)mana / maxMana;

    }

    private void CreatureDeathInDG()
    {
        mana += deathAbsorb;
        manaSlider.value = (float)mana / maxMana; // Update the UI slider with current mana value
    }

    public void OnHit(CreatureController hit, int damage)
    {
        mana -= damage; // Absorb mana from creature hit
        spellBook.manaSlider.value = (float)mana / maxMana; // Update the UI slider with current mana value

        if (mana < 0) GameManager.Instance.LooseGame(); // Ensure mana does not go below zero
    }

    public void UseMana(int amount)
    {
        if (mana >= amount)
        {
            mana -= amount; // Deduct mana cost
        manaSlider.value = (float)mana / maxMana; // Initialize the UI slider with current mana value
        manaText.text = mana.ToString() + "/" + maxMana.ToString(); // Initialize the UI text with current mana value
        }
        else
        {
            Debug.LogWarning("Not enough mana to cast the spell.");
        }
    }

    public void GainMana(int amount)
    {
        mana += amount; // Gain mana
        if (mana > maxMana) mana = maxMana; // Ensure mana does not exceed maximum
        manaSlider.value = (float)mana / maxMana; // Initialize the UI slider with current mana value
        manaText.text = mana.ToString() + "/" + maxMana.ToString(); // Initialize the UI text with current mana value
    }

    public bool CheckMana(int amount)
    {
        return mana > amount; // Check if enough mana is available
    }

    public void RegenerateMana()
    {
        if (mana < maxMana)
        {
            mana += baseManaRegenRate + manaRegenModif; // Regenerate mana based on the rate and modifiers
            if (mana > maxMana) mana = maxMana; // Ensure mana does not exceed maximum
        manaSlider.value = (float)mana / maxMana; // Initialize the UI slider with current mana value
        manaText.text = mana.ToString() + "/" + maxMana.ToString(); // Initialize the UI text with current mana value
        }
    }

    private IEnumerator ManaRegenCoroutine()
    {
        while (true)
        {
            RegenerateMana(); // Call the mana regeneration method
            yield return new WaitForSeconds(baseManaRegenInterval); // Wait for the specified interval before regenerating again
        }
    }
}
