using System.Collections.Generic;
using UnityEngine;

public class CardSlot : MonoBehaviour
{
    [Header("Slot Properties")]
    public bool isOccupied = false;
    public bool player1; //is for player 1
    public bool backRow; 

    [Header("Debug")]
    public Card currentCard = null;
    /*
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;
    public Color invalidColor = Color.red;
    */

    private SpriteRenderer spriteRenderer;

    private GameObject game;
    private Game gameScript;
    private InfoBox infoBox;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        game = GameObject.Find("Game");
        gameScript = game.GetComponent<Game>();
        infoBox = GameObject.Find("InfoBox").GetComponent<InfoBox>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }
    
    public bool CanPlaceCard(GameObject card)
    {
        if (isOccupied) return false;
        //filter for opponet space
        if (gameScript.activePlayer.playerNum == 1 && player1 == false) return false;
        if (gameScript.activePlayer.playerNum == 2 && player1 == true) return false;
        //filter powerorbs
        if (card.GetComponent<CardProperties>().card_type == 1)
        {
            infoBox.DisplayText("Cant be placed here");
            return false;
        }

        //if card comes from hand check for orb and level restrictions
        if (card.GetComponent<Card>().inHand)
        {
            int cost = card.GetComponent<CardProperties>().cost;
            if (cost > gameScript.orbsToSpend)
            {
                infoBox.DisplayText("Not enough orbs");
                return false;
            }

            int level = card.GetComponent<CardProperties>().level;
            if (gameScript.CalculateLevelsOnField(gameScript.activePlayer) + level > gameScript.level_border)
            {
                infoBox.DisplayText("Too high of a level");
                return false;
            }
        }
        else if (card.GetComponent<CardProperties>().attacksPerTurn <= 0)
        {
            infoBox.DisplayText("Cant change position now");
            return false; //restriction on repositioning (cant move if you already attacked)
        } 

        
        return true;
    }
    
    public void PlaceCard(Card card)
    {
        // Remove card from previous slot if it had one
        if (card.currentSlot != null)
        {
            card.currentSlot.RemoveCard();
        }
        
        // Set this slot as occupied
        isOccupied = true;
        currentCard = card;

        

        //set this as the parent
        card.transform.SetParent(transform);

        // Update card's position and slot reference
        card.transform.position = transform.position;
        card.currentSlot = this;

        card.OnCardPlacedToField();
        
        // Trigger any placement effects
        OnCardPlaced(card);
    }

    
    public void RemoveCard()
    {
        if (currentCard != null)
        {
            currentCard.transform.SetParent(null);
            currentCard.currentSlot = null;
            currentCard = null;
        }
        isOccupied = false;

        OnCardRemoved();
    }

    
    
    // Events that can be overridden or subscribed to
    protected virtual void OnCardPlaced(Card card)
    {
        // Override this method to add custom behavior when a card is placed
        //Debug.Log($"Card {card.name} placed in slot {gameObject.name}");
    }
    
    protected virtual void OnCardRemoved()
    {
        // Override this method to add custom behavior when a card is removed
        //Debug.Log($"Card removed from slot {gameObject.name}");
    }
    
    // Helper methods for condition checking (implement these based on your game logic)
    /*
    private bool IsCardTypeValid(Card card)
    {
        // Implement card type validation logic
        return true;
    }
    
    private bool HasEnoughResources(Card card)
    {
        // Implement resource checking logic
        return true;
    }
    
    private bool IsGameStateValid(Card card)
    {
        // Implement game state validation logic
        return true;
    }
    
    private bool CanPlayerPlaceCard(Card card)
    {
        // Implement player permission logic
        return true;
    }
    */
}