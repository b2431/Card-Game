using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoInterface : MonoBehaviour
{
    //Objects carrying sprite, text and button components
    public GameObject spriteDisplay;
    public GameObject nameDisplay;
    public GameObject lifeDisplay;
    public GameObject attackDisplay;
    public GameObject costDisplay;
    public GameObject passiveAbilityDisplay;
    public GameObject activeAbilityDisplay;

    public GameObject attackButton;
    public GameObject abilityButton;

    public GameObject game;

    private Color originalAttackButtonColor;
    private Color originalAbilityButtonColor;

    private bool opponentCard;

    //public Sprite defaultSprite;
    void Start()
    {
        originalAttackButtonColor = attackButton.GetComponent<Image>().color;
        originalAbilityButtonColor = abilityButton.GetComponent<Image>().color;
    }

    void Update()
    {
    }
    /// functions accesed from the card script

    //get the properties from the card and copy them into the sprite/text components of the interface objects
    public void UpdateInfo(GameObject card)
    {
        //if (card.GetComponent<CardProperties>().card_type == 1) return; //dont display anything if its a power orb

        //let the game script know which card is currently selected and displayed in the interface
        game.GetComponent<Game>().SetCurrentlySelectedCard(card);


        //determine if received card belongs to op
        if (card.GetComponent<Card>().currentSlot != null)
        {
            if (card.GetComponent<Card>().currentSlot.player1 == true && game.GetComponent<Game>().activePlayer.playerNum == 1
            || card.GetComponent<Card>().currentSlot.player1 == false && game.GetComponent<Game>().activePlayer.playerNum == 2)
                opponentCard = false;
            else opponentCard = true;
        }
        else opponentCard = false;

        Sprite cardSprite = card.GetComponent<SpriteRenderer>().sprite; //get the sprite
        spriteDisplay.GetComponent<SpriteRenderer>().sprite = cardSprite; //set the sprite

        CardProperties props = card.GetComponent<CardProperties>(); //reference to the properties script
        //set the texts
        if (!opponentCard) nameDisplay.GetComponent<TextMeshPro>().text = props.cardName;
        else nameDisplay.GetComponent<TextMeshPro>().text = props.cardName + " (Opp)";
        lifeDisplay.GetComponent<TextMeshPro>().text = "Life: " + props.life.ToString();
        attackDisplay.GetComponent<TextMeshPro>().text = "Attack: " + props.attack.ToString() + "  (R: " + props.attackRange.ToString() + ")";
        costDisplay.GetComponent<TextMeshPro>().text = "Cost: " + props.cost.ToString();

        if (props.passive_ability_text == "") passiveAbilityDisplay.GetComponent<TextMeshPro>().text = "";
        else passiveAbilityDisplay.GetComponent<TextMeshPro>().text = "Passive: " + "\n" + props.passive_ability_text;
        if (props.active_ability_text == "") activeAbilityDisplay.GetComponent<TextMeshPro>().text = "";
        else activeAbilityDisplay.GetComponent<TextMeshPro>().text = "Active: " + "\n" + props.active_ability_text;

        //make sure the attack buttons are only active if the card is yours
        if (opponentCard == false)
        {
            attackButton.SetActive(card.GetComponent<Card>().inplay);
            if(!(props.active_ability_text == "")) //only if card has an active ability
                abilityButton.SetActive(card.GetComponent<Card>().inplay);
        }
        else
        {
            attackButton.SetActive(false);
            abilityButton.SetActive(false);
        }


        //change color of button depending on availability of attack
        if (attackButton.activeSelf)
        {
            if (props.attacksPerTurn > 0 && !game.GetComponent<Game>().firstTurn)
            {
                attackButton.GetComponent<Image>().color = originalAttackButtonColor;
            }
            else
            {
                attackButton.GetComponent<Image>().color = Color.grey;
            }
        }
        //change color of ability button depending on availability
        if (abilityButton.activeSelf)
        {
            if (props.active_ability_UsesPerTurn > 0)
                abilityButton.GetComponent<Image>().color = originalAbilityButtonColor;
            else
                abilityButton.GetComponent<Image>().color = Color.grey;
        }

        if (card.GetComponent<CardProperties>().card_type == 1) //orb
            DisplayOrb(cardSprite);

        this.gameObject.SetActive(true);

    }

    //close 
    public void HideInterface()
    {
        spriteDisplay.GetComponent<SpriteRenderer>().sprite = null;
        nameDisplay.GetComponent<TextMeshPro>().text = "";
        lifeDisplay.GetComponent<TextMeshPro>().text = "";
        attackDisplay.GetComponent<TextMeshPro>().text = "";
        costDisplay.GetComponent<TextMeshPro>().text = "";

        passiveAbilityDisplay.GetComponent<TextMeshPro>().text = "";
        activeAbilityDisplay.GetComponent<TextMeshPro>().text = "";

        attackButton.SetActive(false);
        abilityButton.SetActive(false);
    }

    public void DisplayOrb(Sprite sprite)//only show the sprite
    {
        spriteDisplay.GetComponent<SpriteRenderer>().sprite = sprite;
        nameDisplay.GetComponent<TextMeshPro>().text = "Power Orb";
        lifeDisplay.GetComponent<TextMeshPro>().text = "";
        attackDisplay.GetComponent<TextMeshPro>().text = "";
        costDisplay.GetComponent<TextMeshPro>().text = "";

        passiveAbilityDisplay.GetComponent<TextMeshPro>().text = "";
        activeAbilityDisplay.GetComponent<TextMeshPro>().text = "";

        attackButton.SetActive(false);
        abilityButton.SetActive(false);
    }
}
