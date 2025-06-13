using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class AvatarGenerator : MonoBehaviour
{
    public static AvatarGenerator Instance;
    public enum SpellColors
    {
        Brown,
        Blue,
        Green,
        White,
        Black
    }
    public Material brownAvatar;
    public Material blueAvatar;
    public Material greenAvatar;
    public Material whiteAvatar;
    public Material blackAvatar;
    public SpellColors currentSpellColor;
    public Color defaultColor = Color.white;
    public Spell avatarSpell;
    public SpellColor colorHandler;
    public GameObject selectorColor; // Reference to the selector color GameObject

    public SpellEffectSO effect;
    public ManaCore manaCore;
    public Coroutine changeAvatarCoroutine;

    public Button blueButton, greenButton, blackButton, whiteButton, brownButton, summondButton;
    private List<Button> colorButtons = new List<Button>();

    public Image avatarImage; // Image to display the avatar spell
    public Material material; // Material for the avatar image
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        manaCore = ManaCore.Instance; // Get the ManaCore instance
        colorButtons = new List<Button> { blueButton, greenButton, blackButton, whiteButton, brownButton };
        material = avatarImage.material; // Assuming you have a material for the avatar image
        blueButton.onClick.AddListener(() => OnButtonClick(blueButton, SpellColors.Blue));
        greenButton.onClick.AddListener(() => OnButtonClick(greenButton, SpellColors.Green));
        blackButton.onClick.AddListener(() => OnButtonClick(blackButton, SpellColors.Black));
        whiteButton.onClick.AddListener(() => OnButtonClick(whiteButton, SpellColors.White));
        brownButton.onClick.AddListener(() => OnButtonClick(brownButton, SpellColors.Brown));
        SetColor(SpellColors.Brown); // Default color
        HighlightSelectedColor(colorButtons, brownButton); // Highlight the default color button
        avatarSpell.Build(Spell.SpellColors.Brown, Spell.SpellTargets.Tile, effect, manaCore);
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SummonAvatar()
    {
            if (manaCore == null)
    {
        manaCore = ManaCore.Instance;
        if (manaCore == null)
        {
            Debug.LogError("manaCore is null in AvatarGenerator! Make sure ManaCore exists in the scene and its Awake() sets Instance.");
            return;
        }
    }

        if (manaCore.currentAvatar == null)
        {
            Debug.Log("Summoning Avatar with color: " + currentSpellColor);
            avatarSpell.Cast();
        }
        else if (manaCore.currentAvatar != null)
        {
            if (changeAvatarCoroutine == null)
            {
                StartCoroutine(ChanchingAvatar()); // Stop any existing change avatar coroutine
            }
        }
    }

    public IEnumerator ChanchingAvatar()
    {
        manaCore.currentAvatar.GetComponent<CreatureController>().currentCreatureType.OnDeath(null);
        yield return new WaitUntil(() => manaCore.currentAvatar == null);
        Debug.Log("Avatar has been dismissed.");
        avatarSpell.Cast();
        yield break;

    }
    
    private void SetColor(SpellColors color)
    {
        currentSpellColor = color;
    }



    private void HighlightSelectedColor(List<Button> buttons, Button selected)
    {
        if (selected == brownButton)
        {
            avatarImage.material = brownAvatar;
            selectorColor.transform.position = brownButton.transform.position; // Move selector to brown button
        }
        else if (selected == blueButton)
        {
            avatarImage.material = blueAvatar;
            selectorColor.transform.position = blueButton.transform.position; // Move selector to blue button
        }
        else if (selected == greenButton)
        {
            avatarImage.material = greenAvatar;
            selectorColor.transform.position = greenButton.transform.position; // Move selector to green button
        }
        else if (selected == blackButton)
        {
            avatarImage.material = blackAvatar;
            selectorColor.transform.position = blackButton.transform.position; // Move selector to black button

        }
        else if (selected == whiteButton)
        {
            avatarImage.material = whiteAvatar;
            selectorColor.transform.position = whiteButton.transform.position; // Move selector to white button
        }
    }
    public void OnButtonClick(Button clickedButton, SpellColors color)
    {
        HighlightSelectedColor(colorButtons, clickedButton);
        SetColor(color);
        avatarSpell.Build((Spell.SpellColors)currentSpellColor, Spell.SpellTargets.Tile, effect, manaCore);
    }

}
