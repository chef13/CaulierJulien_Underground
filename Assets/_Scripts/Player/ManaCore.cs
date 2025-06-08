using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using System;

[RequireComponent(typeof(LineRenderer))]
public class ManaCore : MonoBehaviour
{
    public List<GameObject> spellObjects = new();
    public Spell currentSpell;
    public SpriteRenderer renderer;
    public int beamLength = 10; // Length of the spell beam
    private Color spellColor; // Color of the currently selected spell
    public LineRenderer lineRenderer; // Line renderer for visual effects
    public int mana = 100; // Initial mana value
    public int maxMana = 500; // Maximum mana value
    public int manaRegenRate = 1; // Mana regeneration rate per second
    public float manaRegenInterval = 1f; // Time interval for mana regeneration
    public Coroutine castingCoroutine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (spellObjects.Count > 0)
            currentSpell = spellObjects[0].GetComponent<Spell>();
        lineRenderer = GetComponent<LineRenderer>();
        renderer = GetComponent<SpriteRenderer>();
        foreach (GameObject spellObject in spellObjects)
        {
            Spell spell = spellObject.GetComponent<Spell>();
            if (spell != null)
            {
                spell.manaCore = this; // Assign the ManaCore to each spell
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && castingCoroutine == null) // Example input to cast a spell
        {
            currentSpell.Build(gameObject); // Build the spell with the current target
            Debug.Log("Casting spell: " + currentSpell.gameObject.name);
            castingCoroutine = StartCoroutine(currentSpell.targetHandler.CastingSpell());
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            int currentIndex = spellObjects.FindIndex(obj => obj.GetComponent<Spell>() == currentSpell);
            if (currentIndex != -1)
            {
                int nextIndex = (currentIndex + 1) % spellObjects.Count;
                currentSpell = spellObjects[nextIndex].GetComponent<Spell>();
                Debug.Log("Current spell: " + currentSpell.gameObject.name);
                UpdateSpellColorVisual();

            }
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            int currentIndex = spellObjects.FindIndex(obj => obj.GetComponent<Spell>() == currentSpell);
            if (currentIndex != -1)
            {
                int prevIndex = (currentIndex - 1 + spellObjects.Count) % spellObjects.Count;
                currentSpell = spellObjects[prevIndex].GetComponent<Spell>();
                Debug.Log("Current spell: " + currentSpell.gameObject.name);
                UpdateSpellColorVisual();
            }
        }
    }

    /*private IEnumerator CastingSpell()
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.startColor = GetColorFromSpellColor(currentSpell.spellColor);
        lineRenderer.endColor = GetColorFromSpellColor(currentSpell.spellColor);

        while (!Input.GetMouseButtonUp(0))
        {
            currentSpell.transform.position = transform.position; // Update spell position to the player's position
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            Vector2 direction = (mouseWorld - transform.position).normalized;
            RaycastHit2D hits = Physics2D.Raycast(transform.position, direction, beamLength, LayerMask.GetMask("wall"));
            Vector3 hitPos = hits.collider != null ? hits.point : transform.position + Vector3.ClampMagnitude(mouseWorld - transform.position, beamLength);
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hitPos);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Casting cancelled");
                lineRenderer.enabled = false;
                castingCoroutine = null;
                yield break;
            }

            yield return null; // Wait for next frame
        }

        lineRenderer.enabled = false; // Turn off visual after casting
        currentSpell.Cast();
        castingCoroutine = null;
        yield break;
    }*/

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
                return Color.black;
            case Spell.SpellColors.Brown:
                return new Color(0.6f, 0.4f, 0.2f); // Brown color
            default:
                return Color.white;
        }
    }
        

        public void UpdateSpellColorVisual()
    {
        Color hdrColor = GetColorFromSpellColor(currentSpell.spellColor) * 3f; // Increase intensity for HDR bloom
        renderer.material.SetColor("_EmissionColor", hdrColor);
        renderer.material.EnableKeyword("_EMISSION"); // Required for Unity standard shader
    }
}
