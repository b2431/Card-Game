using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    private CardProperties cardProperties;
    public CardSlot currentSlot = null;
    
    [Header("Drag Settings")]
    public float dragOffset = 0.1f; // Z offset while dragging to appear above other objects
    public bool canBeDragged = true; //redundant?
    
    private Vector3 originalPosition; //to return to when drag doest result in placement in accessible slot
    private Vector3 originalScale; //scale is being alterd with during drag process (why?)

    //back to original parent and sibling index when unsucessful (maybe redundant)
    public Transform originalParent; 
    private bool isDragging = false;
    private Vector3 worldOffset; //offest between the mouse and the card object to factor in when draging the card (so it doesnt teleport to the mouse)
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;


    private GameObject infoInterfaceObj;
    private InfoInterface cardInfoInterface;

    private GameObject game;
    private Game gameScript;
    private GameObject infoBox;

    [Header("Debug")]
    public bool player1;
    public bool inplay = false; 
    public bool inHand = true; //to detect if the card is summond from the hand when moved to a card slot (initiated as true because card starts of in hand)
    public bool inGrave = false;


    void Update()
    {
        //check if targeted card is on opposing field
        if (currentSlot != null)
        {
            if (currentSlot.player1 == false && game.GetComponent<Game>().activePlayer.playerNum == 1) canBeDragged = false;
            else if (currentSlot.player1 == true && game.GetComponent<Game>().activePlayer.playerNum == 2) canBeDragged = false;
            else canBeDragged = true;
        }

        if (this.gameObject.GetComponent<CardProperties>().life <= 0 && inGrave == false) //killed
            TransferCardToGrave();  
        
        
    }
    public void TransferCardToGrave()
    {

        //update the lists of the player objects
        if (inplay) gameScript.MoveAnyCardFromFieldToGrave(player1, this.gameObject);
        else if (inHand) gameScript.MoveAnyCardFromHandToGrave(player1, this.gameObject);

        //set the boleans of the card right
        inGrave = true;
        inplay = inHand = false;

        //move the card to the graveyard position (graveyard is 50 (p1) /100 (p2) in x direction)
        transform.position = gameScript.GetGravePosition(player1);

        // Update original position to current slot position
        originalPosition = transform.position;
        originalParent = null;

        transform.parent = null;
        currentSlot.RemoveCard();
        currentSlot = null;

        Debug.Log(this.name + "died and moved to grave");
        
    }
    void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        cardProperties = GetComponent<CardProperties>();

        infoInterfaceObj = GameObject.Find("CardInfo_Interface");
        cardInfoInterface = infoInterfaceObj.GetComponent<InfoInterface>();
        //cardInfoInterface.CloseInterface(); //has to be open at the start for the reference

        game = GameObject.Find("Game");
        gameScript = game.GetComponent<Game>();
        infoBox = GameObject.Find("InfoBox");
        
        // Store original values
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalParent = transform.parent;
    }

    void OnMouseDown() //called when clicked on this card
    {
        //update the interface
        cardInfoInterface.UpdateInfo(this.gameObject);

        if (!canBeDragged)
        {
            return;
        }
            


        // Calculate offset between card position and mouse position
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;
        worldOffset = transform.position - mouseWorldPos;
    }
    
    void OnMouseDrag()
    {
        if (!canBeDragged) return;
        
        if (!isDragging)
        {
            // First frame of dragging
            OnBeginDrag();
        }
        
        // Update position
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;
        transform.position = mouseWorldPos + worldOffset;
    }
    
    void OnMouseUp()
    {
        if (!canBeDragged) return;
        
        
        if (isDragging)
        {
            OnEndDrag();
        }
    }
    
    private void OnBeginDrag()
    {
        isDragging = true;
        
        
        // Store original values
        originalPosition = transform.position;
        originalParent = transform.parent;
        
        // Visual changes while dragging
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0.8f;
            spriteRenderer.color = color;
        }
        
        // Adjust Z position to appear above other objects
        Vector3 pos = transform.position;
        pos.z -= dragOffset;
        transform.position = pos;
        
        // Slightly scale up for visual feedback
        transform.localScale = originalScale * 1.05f;
        
        //OnDragStarted();
    }
    
    private void OnEndDrag()
    {
        isDragging = false;
        
        
        // Reset visual changes
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
        
        transform.localScale = originalScale;
        
        // Reset Z position
        Vector3 pos = transform.position;
        pos.z += dragOffset;
        transform.position = pos;
        
        // Check if we dropped on a valid slot
        CheckForSlotDrop();
        
        //OnDragEnded();
    }
    
    private void CheckForSlotDrop()
    {
        // Get all colliders at the card's current position
        Vector2 cardPos = transform.position;
        Collider2D[] hits = Physics2D.OverlapPointAll(cardPos);
        


        foreach (Collider2D hit in hits)
        {
            // Skip our own collider
            if (hit.gameObject == gameObject) continue;

            //creature card
            if (this.gameObject.GetComponent<CardProperties>().card_type != 1)
            {
                CardSlot slot = hit.GetComponent<CardSlot>();
                if (slot != null)
                {

                    if (slot.CanPlaceCard(this.gameObject))
                    {
                        slot.PlaceCard(this);
                        return;
                    }
                    else
                    {
                        //Debug.Log($"Cannot place {cardProperties.cardName} in slot {slot.name} - conditions not met");
                    }
                }
            }
            else //orb
            {
                OrbStation station = hit.GetComponent<OrbStation>();
                if (station != null)
                {

                    if (station.CanPlaceCard())
                    {
                        station.PlaceOrb(this.gameObject);
                        return;
                    }
                    else
                    {
                        //Debug.Log($"Cannot place {cardProperties.cardName} in slot {station.name} - conditions not met");
                    }
                }
            }
            
        }
        
        /*
        // Also try raycast from mouse position as backup
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        RaycastHit2D rayHit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        
        if (rayHit.collider != null && rayHit.collider.gameObject != gameObject)
        {
            CardSlot slot = rayHit.collider.GetComponent<CardSlot>();
            if (slot != null)
            {
                Debug.Log($"Raycast found slot: {slot.name}");
                
                if (slot.CanPlaceCard(this.gameObject))
                {
                    Debug.Log($"Placing {cardProperties.cardName} in slot {slot.name} via raycast");
                    slot.PlaceCard(this);
                    return;
                }
            }
        }
        */
        
        // If we didn't find a valid slot, return to original position
        //Debug.Log($"No valid slot found for {cardProperties.cardName}, returning to original position");
        ReturnToOriginalPosition();
    }

    public void OnCardPlacedToField()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -1);//move up so it appears above the slot
        // Update original position to current slot position
        originalPosition = transform.position;
        originalParent = transform.parent;
        //update card into (show buttons)
        if(cardInfoInterface != null)
            cardInfoInterface.UpdateInfo(this.gameObject);

        //update lists if coming from hand
        if (inHand)
        {
            if (gameScript == null) Debug.Log("wtf why the fuck??????");
            gameScript.MoveActivePlayerCardFromHandToField(this.gameObject);
            infoBox.GetComponent<InfoBox>().DisplayText(this.gameObject.GetComponent<CardProperties>().cardName + " summoned");
            inHand = false;
        }
        inplay = true;
    }
  
    public void ReturnToOriginalPosition()
    {

        // Return card to its original position and parent
        transform.position = originalPosition;
        transform.localScale = originalScale;

        if (originalParent != null)
        {
            transform.SetParent(originalParent);
        }

        // Reset visual state
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        isDragging = false;

        //OnReturnedToOriginalPosition();
    }

    // Event methods that can be overridden
    /*
    protected virtual void OnDragStarted()
    {
        Debug.Log($"Started dragging {cardProperties.cardName}");
    }
    
    protected virtual void OnDragEnded()
    {
        Debug.Log($"Ended dragging {cardProperties.cardName}");
    }

    protected virtual void OnSuccessfullyPlaced()
    {
        Debug.Log($"{cardProperties.cardName} successfully placed in slot");

    }
    
    protected virtual void OnReturnedToOriginalPosition()
    {
        Debug.Log($"{cardProperties.cardName} returned to original position");
    }
    
    // Public methods for external control
    public void SetDraggable(bool draggable)
    {
        canBeDragged = draggable;
    }
    
    public bool IsDragging()
    {
        return isDragging;
    }
    
    public void RemoveFromSlot()
    {
        if (currentSlot != null)
        {
            currentSlot.RemoveCard();
        }
    }
    */
}