using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public bool ai_game;
    public List<GameObject> CardPool = new List<GameObject>();
    //reference to Card Info Interface
    public GameObject infoInterfaceObj;
    private InfoInterface infoInterface;
    public GameObject generalCanvas; //ui for draw button etc
    public GameObject winUI;
    public GameObject lossUI;
    public GameObject endGameUI;
    public GameObject endGameUText;

    //objects carrying TMP components
    public GameObject playerIndicatiorText;
    public GameObject lifePointsTextPlayer1;
    public GameObject lifePointsTextPlayer2;
    public GameObject infoBox;
    public GameObject drawButton;
    private Color drawButtonOriginalColor;
    private bool canDrawFromDeck = true;
    private bool hasDrawn = false;
    public bool firstTurn = true;

    public GameObject orbTextP1;
    public GameObject orbTextP2;
    public GameObject levelText1;
    public GameObject levelText2;

    private GameObject currentlySelectedCard;
    private GameObject attackingCard;

    private System.Random rand;
    public Color originalBackgroundColor;

    [HideInInspector] public Player activePlayer;
    [HideInInspector] public Player opponent;
    public Player player1;
    public Player player2;

    [HideInInspector] public PassiveAbilityActivator abilityActivator = new PassiveAbilityActivator();

    private BasicAi ai_opponent;


    [Header("Game configuration")]
    public List<GameObject> Deck_player1 = new List<GameObject>(); //teporary setup of the Decks in the inspector
    public List<GameObject> Deck_player2 = new List<GameObject>();
    public List<GameObject> CardSlots_player1 = new List<GameObject>(); // 0-5 cardslots, 6 graveyard, 7 power orbs
    public List<GameObject> CardSlots_player2 = new List<GameObject>();



    [Header("Game play settings")]
    public int starting_hand_size = 6;
    public int max_hand_size = 12;
    public int orb_placements_per_turn = 1;
    public int level_border = 12;

    [Header("Debug")]
    public bool inGraveyard = false;
    public int orbPlacements = 0;
    public int orbsToSpend;
    public List<GameObject> Hand_player1 = new List<GameObject>(); //teporary setup of the Decks in the inspector
    public List<GameObject> Hand_player2 = new List<GameObject>();
    public List<GameObject> Field_player1 = new List<GameObject>(); //teporary setup of the Decks in the inspector
    public List<GameObject> Field_player2 = new List<GameObject>();



    void Awake()
    {
        ai_game = MenuManager.ai_game;
    }
    void Start()
    {
        infoInterface = infoInterfaceObj.GetComponent<InfoInterface>();
        rand = new System.Random();

        //initialize the players
        player1 = new Player(Deck_player1, 1);
        player2 = new Player(Deck_player2, 2);
        activePlayer = player1;
        opponent = player2;

        ai_opponent = new BasicAi(this);



        UpdateLevelText();

        drawButtonOriginalColor = drawButton.GetComponent<Image>().color;

        orbPlacements = orb_placements_per_turn;
        orbsToSpend = player1.orbCount;

        originalBackgroundColor = Camera.main.backgroundColor;

        StartGame();
    }

    void Update()
    {
        //close interface when left click on empty space
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider == null && !EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Ground clicked, no card at this position.");
                infoInterface.HideInterface();
            }
        }

        //temporary key to swap players
        if (Input.GetKeyDown(KeyCode.S))
        {
            EndTurn();
            Debug.Log("end turn");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            foreach (GameObject card in activePlayer.Grave)
            {
                Debug.Log(card.name);
            }

        }

        //conditions for draw to be enabled
        if (activePlayer.Hand.Count > max_hand_size || activePlayer.Deck.Count <= 0 || hasDrawn || firstTurn)
        {
            drawButton.GetComponent<Image>().color = Color.grey;
            canDrawFromDeck = false;
        }
        else
        {
            drawButton.GetComponent<Image>().color = drawButtonOriginalColor;
            canDrawFromDeck = true;
        }

        //objects to set during grave yard move
        if (inGraveyard)
        {
            drawButton.SetActive(false);
        }
        else
        {
            drawButton.SetActive(true);
        }

        //update the orb text
        string text1;
        string text2;
        if (activePlayer.playerNum == 1)
        {
            text1 = orbsToSpend.ToString();
            text2 = player2.orbCount.ToString();
        }
        else
        {
            text1 = player1.orbCount.ToString();
            text2 = orbsToSpend.ToString();
        }
        orbTextP1.GetComponent<TextMeshPro>().text = text1;
        orbTextP2.GetComponent<TextMeshPro>().text = text2;

        //update LifePoint text
        lifePointsTextPlayer1.GetComponent<TextMeshPro>().text = "LP: " + player1.lifePoints.ToString();
        lifePointsTextPlayer2.GetComponent<TextMeshPro>().text = "LP: " + player2.lifePoints.ToString();

        if (player1.lifePoints <= 0 || player2.lifePoints <= 0) GameOver();

        //debug
        Hand_player1 = player1.Hand;
        Hand_player2 = player2.Hand;
        Field_player1 = player1.Field;
        Field_player2 = player2.Field;
    }

    void StartGame()
    {
        //draw starting hand for both players
        for (int i = 0; i <= starting_hand_size; i++)
        {
            //player 1
            int random_index = rand.Next(player1.Deck.Count);
            GameObject new_card = Instantiate(player1.Deck[random_index], new Vector3Int(100, 100, 0), transform.rotation); //get random card from deck and instantiate of screen
            new_card.GetComponent<Card>().player1 = true;
            player1.Hand.Add(new_card); //add to hand
            player1.Deck.RemoveAt(random_index); //remove from deck

            //player 2
            int random_index2 = rand.Next(player2.Deck.Count);
            GameObject new_card2 = Instantiate(player2.Deck[random_index2], new Vector3Int(100, 100, 0), transform.rotation);
            player2.Hand.Add(new_card2);
            new_card2.GetComponent<Card>().player1 = false;
            //new_card2.SetActive(false); //set false because its player1s turn
            player2.Deck.RemoveAt(random_index2);
        }

        UpdateCardPlacementInHand(); //move instantiated cards to hand positions
        playerIndicatiorText.GetComponent<TextMeshProUGUI>().text = "Player 1"; //update player text


    }
    public void DrawCardFromDeck() //for the button
    {
        if (canDrawFromDeck == false) return;

        int random_index = rand.Next(activePlayer.Deck.Count);
        GameObject new_card = Instantiate(activePlayer.Deck[random_index], new Vector3Int(100, 100, 0), transform.rotation);
        if (activePlayer.playerNum == 1) new_card.GetComponent<Card>().player1 = true;
        activePlayer.Hand.Add(new_card);
        activePlayer.Deck.RemoveAt(random_index);

        UpdateCardPlacementInHand();
        hasDrawn = true;
    }

    public void DrawCardFromDeckAnyPlayer(Player player)
    {
        if (player.Deck.Count <= 0) return;

        int random_index = rand.Next(player.Deck.Count);
        GameObject new_card = Instantiate(player.Deck[random_index], new Vector3Int(100, 100, 0), transform.rotation);
        if (player.playerNum == 1) new_card.GetComponent<Card>().player1 = true;
        player.Hand.Add(new_card);
        player.Deck.RemoveAt(random_index);
    }


    //called every time a card is removed or added to the hand
    public void UpdateCardPlacementInHand()
    {

        //bottom left coordinates
        float xpos = -16.4f;
        float ypos = -10f;

        //every 3 cards move one line higher
        for (int i = 1; i < activePlayer.Hand.Count; i++)
        {
            activePlayer.Hand[i].transform.position = new Vector3(xpos, ypos, -1);
            if (i % 3 != 0) xpos += 3f;
            else
            {
                xpos = -16.4f;
                ypos += 4.5f;
            }

        }
    }

    public Vector3 GetGravePosition(bool isplayer1)
    {
        //top left coordinates
        float xpos = 34f;
        float ypos = 4f;

        Player selectedPlayer;
        if (isplayer1) selectedPlayer = player1;
        else
        {
            selectedPlayer = player2;
            xpos += 50f;
        }

        Vector3 nextPos = new Vector3(xpos, ypos, -1);

        for (int i = 1; i < selectedPlayer.Grave.Count + 1; i++)
        {
            nextPos = new Vector3(xpos, ypos, -1);
            if (i % 9 != 0) xpos += 3f;
            else
            {
                if (isplayer1) xpos = 34f;
                else xpos = 34f + 50f;
                ypos += 4.5f;
            }
        }
        return nextPos;

    }


    public void MoveActivePlayerCardFromHandToField(GameObject card)
    {
        //passive abilites on Summon
        foreach (GameObject _card in activePlayer.Field)
        {
            CardProperties prop = _card.GetComponent<CardProperties>();
            CardAbility ability = prop.passive_ability;

            if (ability is IOnSummonFriend trigger)
                trigger.OnSummonFriend(_card, this, card);
        }
        foreach (GameObject _card in opponent.Field)
        {
            CardProperties prop = _card.GetComponent<CardProperties>();
            CardAbility ability = prop.passive_ability;

            if (ability is IOnSummonOpp trigger)
                trigger.OnSummonOpp(_card, this, card);
        }



        activePlayer.Hand.Remove(card);
        activePlayer.Field.Add(card);
        orbsToSpend -= card.GetComponent<CardProperties>().cost;
        UpdateCardPlacementInHand();
        UpdateLevelText();


        //update the on field friends effects
        abilityActivator.UpdateOnFieldsFriendAbility(activePlayer, opponent, card, this);


    }

    public void MoveAnyCardFromFieldToGrave(bool isplayer1, GameObject card)
    {
        if (isplayer1)
        {
            player1.Field.Remove(card);
            player1.Grave.Add(card);
        }
        else
        {
            player2.Field.Remove(card);
            player2.Grave.Add(card);
        }

        UpdateLevelText();
        //update the on field friends effects
        abilityActivator.UpdateOnFieldsFriendAbility(activePlayer, opponent, card, this);
    }
    public void MoveActivePlayerCardFromFieldToGrave(GameObject card)
    {
        activePlayer.Field.Remove(card);
        activePlayer.Grave.Add(card);

        //update the on field friends effects
        abilityActivator.UpdateOnFieldsFriendAbility(activePlayer, opponent, card, this);

    }
    public void MoveOpponentCardFromFieldToGrave(GameObject card)
    {
        opponent.Field.Remove(card);
        opponent.Grave.Add(card);

        //update the on field friends effects
        abilityActivator.UpdateOnFieldsFriendAbility(activePlayer, opponent, card, this);
    }

    public void MoveAnyCardFromHandToGrave(bool isplayer1, GameObject card)
    {
        if (isplayer1)
        {
            player1.Hand.Remove(card);
            player1.Grave.Add(card);
        }
        else
        {
            player2.Hand.Remove(card);
            player2.Grave.Add(card);
        }

        UpdateCardPlacementInHand();
    }

    public int CalculateLevelsOnField(Player player)
    {
        int level = 0;
        foreach (GameObject card in player.Field)
        {
            level += card.GetComponent<CardProperties>().level;
        }
        return level;

    }
    private void UpdateLevelText()
    {
        levelText1.GetComponent<TextMeshPro>().text = CalculateLevelsOnField(player1).ToString() + "/" + level_border;
        levelText2.GetComponent<TextMeshPro>().text = CalculateLevelsOnField(player2).ToString() + "/" + level_border;
    }


    private void SwapPlayer()
    {

        if (activePlayer.playerNum == 1)
        {
            activePlayer = player2;
            opponent = player1;
            playerIndicatiorText.GetComponent<TextMeshProUGUI>().text = "Player 2";
        }
        else
        {
            activePlayer = player1;
            opponent = player2;
            playerIndicatiorText.GetComponent<TextMeshProUGUI>().text = "Player 1";
        }

        foreach (GameObject card in opponent.Hand)
        {
            card.SetActive(false);
        }
        foreach (GameObject card in activePlayer.Hand)
        {
            card.SetActive(true);
        }

        UpdateCardPlacementInHand();
        SwapOnFieldSlots();

    }

    //swap the positons of the slots
    private void SwapOnFieldSlots()
    {
        Vector3 transitionVector;

        for (int i = 0; i < CardSlots_player1.Count; i++) //8 iterations
        {
            transitionVector = CardSlots_player1[i].transform.position;
            CardSlots_player1[i].transform.position = CardSlots_player2[i].transform.position;
            CardSlots_player2[i].transform.position = transitionVector;
        }

    }

    public void EndTurn()
    {
        hasDrawn = false;
        orbPlacements = orb_placements_per_turn; //reset how many orbs where placed in turn before

        foreach (GameObject card in activePlayer.Field)
        {
            card.GetComponent<CardProperties>().ResetAPT();
            card.GetComponent<CardProperties>().ResetActiveAbilityUsesPerTurn();
        }

        if (ai_game == false)
        {
            SwapPlayer();
            orbsToSpend = activePlayer.orbCount; //set next currentOrbCount to opponents before making opponent the active player
        }
        else
        {
            //switch
            activePlayer = player2;
            opponent = player1;
            foreach (GameObject card in opponent.Hand)
            {
                card.SetActive(false);
            }
            foreach (GameObject card in activePlayer.Hand)
            {
                card.SetActive(true);
            }
            //ai
            ai_opponent.PlayTurn();


            //switch back
            activePlayer = player1;
            opponent = player2;
            orbsToSpend = activePlayer.orbCount;
            foreach (GameObject card in opponent.Hand)
            {
                card.SetActive(false);
            }
            foreach (GameObject card in activePlayer.Hand)
            {
                card.SetActive(true);
            }


        }

        infoInterface.HideInterface();

        firstTurn = false;
    }

    public void ActivateAbility() //hooked to button
    {
        CardProperties props = currentlySelectedCard.GetComponent<CardProperties>();
        CardAbility ability = props.active_ability;
        if (props.active_ability_UsesPerTurn <= 0) return;
        if (ability is IActiveAbility activeAbility)
        {
            activeAbility.Activate(currentlySelectedCard, this);
        }

    }

    public void Attack() //hooked to button
    {
        /*
        foreach (GameObject card in opponent.Field)
        {
            Debug.Log("OPPONENT FIELD:   " + card.name);
        }
        */
        attackingCard = currentlySelectedCard;

        if (attackingCard.GetComponent<CardProperties>().attacksPerTurn <= 0) return;
        if (firstTurn) return;

        //attack directly if there arent any enemy creatures in play
        if (opponent.Field.Count <= 0)
        {
            opponent.lifePoints -= attackingCard.GetComponent<CardProperties>().attack;
            infoBox.GetComponent<InfoBox>().DisplayText("opponent damaged");
            attackingCard.GetComponent<CardProperties>().attacksPerTurn -= 1;
            infoInterface.UpdateInfo(currentlySelectedCard);

        }
        //go into target selection and color the background
        else
        {
            StartCoroutine(TargetSelection(OnTargetSelected));
            Camera.main.backgroundColor = Color.red;

            infoBox.GetComponent<InfoBox>().DisplayText("select target");
        }


    }

    public bool CheckIfTargetInAttackRange(GameObject attackedCard, GameObject attackingCard)
    {
        bool attackerBackRow = attackingCard.GetComponent<Card>().currentSlot.backRow;
        bool attackedBackRow = attackedCard.GetComponent<Card>().currentSlot.backRow;
        int attackRange = attackingCard.GetComponent<CardProperties>().attackRange;

        if (attackRange >= 3) return true;
        if (attackRange == 2 && !(attackedBackRow && attackerBackRow)) return true; //not if both of them in back rows
        if (attackRange == 1 && !(attackedBackRow || attackerBackRow)) return true; //noti f one of them in back rows
        else return false;
    }



    public IEnumerator TargetSelection(System.Action<GameObject> onTargetSelected)
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

                if (hit.collider != null && hit.collider.GetComponent<Card>() != null)
                {
                    if (onTargetSelected != null) onTargetSelected.Invoke(hit.collider.gameObject); //Invoke calls the onTargetSelected function
                    yield break; // Exit the coroutine
                }
                else
                {
                    if (onTargetSelected != null) onTargetSelected.Invoke(null);
                    yield break;
                }
            }

            yield return null; // Wait for the next frame
        }
    }

    private void OnTargetSelected(GameObject attackedCard)
    {
        if (attackedCard == null)
        {
            Debug.Log("Attack cancelled");
            Camera.main.backgroundColor = originalBackgroundColor;
            infoBox.GetComponent<InfoBox>().DisplayText("no target found");
            return;
        }

        if (CheckIfTargetInAttackRange(attackedCard, attackingCard) == false)
        {
            Debug.Log("Attack out of range");
            Camera.main.backgroundColor = originalBackgroundColor;
            infoBox.GetComponent<InfoBox>().DisplayText("target out of range");
            return;
        }

        Debug.Log(attackedCard);

        CardProperties attackerProps = attackingCard.GetComponent<CardProperties>();
        CardProperties targetProps = attackedCard.GetComponent<CardProperties>();

        targetProps.life -= attackerProps.attack; //attack life points


        attackingCard.GetComponent<CardProperties>().attacksPerTurn -= 1; //lower attacks per turn
        Camera.main.backgroundColor = originalBackgroundColor; //reset color
        infoInterface.UpdateInfo(currentlySelectedCard); //update card info in idex to update the life count

        infoBox.GetComponent<InfoBox>().DisplayText("target damaged");

        if (targetProps.life <= 0)
        {
            Debug.Log("Move to Grave");
            infoBox.GetComponent<InfoBox>().DisplayText("target killed");
            abilityActivator.triggerOnKillAbility(attackingCard, this);
        }
    }
    public void SetCurrentlySelectedCard(GameObject card)
    {
        if (card.GetComponent<CardProperties>() == null) return;
        //if (card.GetComponent<CardProperties>().inplay == true)

        currentlySelectedCard = card;

    }
    public GameObject GetCurrentlySelectedCard()
    {
        return currentlySelectedCard;
    }

    private void GameOver()
    {
        if (!ai_game)
        {
            string text = "tie I guess?";
            if (player1.lifePoints <= 0) text = "player1 has won";
            if (player2.lifePoints <= 0) text = "player2 has won";

            generalCanvas.SetActive(false);
            endGameUI.SetActive(true);

            endGameUText.GetComponent<TextMeshProUGUI>().text = text;

        }
        else
        {
            generalCanvas.SetActive(false);
            if (player1.lifePoints <= 0) lossUI.SetActive(true);
            if (player2.lifePoints <= 0) winUI.SetActive(true);
        }   
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    

}

public class Player
{
    public int playerNum;
    public int lifePoints;
    public int orbCount = 0;
    public List<GameObject> Deck = new List<GameObject>();
    public List<GameObject> Hand = new List<GameObject>();
    public List<GameObject> Field = new List<GameObject>();
    public List<GameObject> Grave = new List<GameObject>();

    public Player(List<GameObject> _deck, int _playerNum)
    {
        Deck = _deck;
        playerNum = _playerNum;
        lifePoints = 1000;
    }

}

public class PassiveAbilityActivator
{
    public void UpdateOnFieldsFriendAbility(Player activePlayer, Player opponent, GameObject card, Game game)
    {
        foreach (GameObject _card in activePlayer.Field)
        {
            Debug.Log(_card.name);
            CardProperties prop = _card.GetComponent<CardProperties>();
            CardAbility ability = prop.passive_ability;

            if (ability is IOnFieldFriend trigger)
                trigger.OnFieldFriend(_card, game, card, activePlayer);
        }
        foreach (GameObject _card in opponent.Field)
        {
            CardProperties prop = _card.GetComponent<CardProperties>();
            CardAbility ability = prop.passive_ability;

            if (ability is IOnFieldFriend trigger)
                trigger.OnFieldFriend(_card, game, card, opponent);
        }
    }
    public void triggerOnKillAbility(GameObject killer, Game game)
    {
        if (killer.GetComponent<CardProperties>().passive_ability is IOnKill trigger)
            trigger.OnKill(killer, game);
    }
}

public class BasicAi
{
    public Player aiPlayer;
    private Game gameScript;
    PassiveAbilityActivator abilityActivator;
    public bool ignoreLevelRestriction = false;
    public bool ignoreSummoningCost = false;

    public BasicAi(Game _gameScript)
    {

        gameScript = _gameScript;
        aiPlayer = gameScript.player2;
        abilityActivator = new PassiveAbilityActivator();
    }

    public void PlayTurn()
    {
        Debug.Log("NEW TURN NEW TURN NEW TURN NEW TURN NEW TURN NEW TURN NEW TURN NEW TURN");
        Debug.Log("AI DRAWS CARD");
        gameScript.DrawCardFromDeckAnyPlayer(aiPlayer);
        Debug.Log("AI DREW CARD");
        Debug.Log("AI PLACES ORB");
        PlaceOrb();
        Debug.Log("AI PLACED ORB");
        Debug.Log("AI STARTS SUMMONING CREATURES");
        SummonCreatures(aiPlayer.orbCount, aiPlayer.orbCount);
        Debug.Log("AI FINISHED SUMMONING CREATURES");
        Debug.Log("AI Started Using Abilites");
        UseAcitiveAbilities();
        Debug.Log("AI FINISHED using Abilities");
        Debug.Log("AI STARTS ATTACK");
        AttackCratures();
        AttackDirectyWithRemainingCreatursThatCouldntAttackPreviously();
        Debug.Log("AI FINISHED ATTACKING");
        foreach (GameObject card in aiPlayer.Field)
        {
            card.GetComponent<CardProperties>().ResetAPT();
            card.GetComponent<CardProperties>().ResetActiveAbilityUsesPerTurn();
        }
        Debug.Log("AI ENDS TURN");
        //gameScript.EndTurn();
    }
    private void PlaceOrb()
    {
        bool hasOrb = false;
        GameObject orbCard = null;
        foreach (GameObject card in aiPlayer.Hand)
        {
            if (card.GetComponent<CardProperties>().card_type == 1)//orb
            {
                orbCard = card;
                hasOrb = true;
            }
        }
        if (hasOrb)
        {
            aiPlayer.orbCount += 1;
            aiPlayer.Hand.Remove(orbCard);
            GameObject.Destroy(orbCard);
        }
    }
    private void SummonCreatures(int orbsToSpend, int costCheck)
    {
        if (orbsToSpend < 0 || costCheck < 0) return;

        List<GameObject> SlotsRow_1 = new List<GameObject>();
        List<GameObject> SlotsRow_2 = new List<GameObject>();

        //populate Lists with available slots
        foreach (GameObject slot in gameScript.CardSlots_player2)
        {
            if (slot.GetComponent<CardSlot>() != null) //grave and orbstation
            {
                CardSlot cardSlot = slot.GetComponent<CardSlot>();
                if (cardSlot.isOccupied == false && cardSlot.backRow == false)
                    SlotsRow_1.Add(slot);
                if (cardSlot.isOccupied == false && cardSlot.backRow == true)
                    SlotsRow_2.Add(slot);
            }

        }
        if (SlotsRow_1.Count == 0 && SlotsRow_2.Count == 0) return;

        ///card selection 
        foreach (GameObject card in aiPlayer.Hand.ToList())
        {
            CardProperties props = card.GetComponent<CardProperties>();
            if ((props.cost == costCheck && orbsToSpend >= costCheck) || ignoreSummoningCost)
            {
                //consider level cap
                if (ignoreLevelRestriction == false)
                    if (gameScript.CalculateLevelsOnField(aiPlayer) + props.level > gameScript.level_border) break;
                    
                if (props.attackRange <= 2)
                {
                    if (SlotsRow_1.Count != 0)
                    {
                        summon(card, SlotsRow_1[0]);
                        orbsToSpend -= costCheck;
                        break;
                    }
                    else if (props.attackRange == 2)
                    {
                        if (SlotsRow_2.Count != 0)
                        {
                            summon(card, SlotsRow_2[0]);
                            orbsToSpend -= costCheck;
                            break;
                        }
                    }
                }
                else //attackrange
                {
                    if (SlotsRow_2.Count != 0)
                    {
                        summon(card, SlotsRow_2[0]);
                        orbsToSpend -= costCheck;
                        break;
                    }
                    else
                    {
                        if (SlotsRow_1.Count != 0)
                        {
                            summon(card, SlotsRow_1[0]);
                            orbsToSpend -= costCheck;
                            break;
                        }
                    }
                }

            }


        }

        SummonCreatures(orbsToSpend, costCheck - 1);

    }
    private void summon(GameObject creatureToSummon, GameObject slot)
    {
        slot.GetComponent<CardSlot>().PlaceCard(creatureToSummon.GetComponent<Card>());
        Debug.Log(creatureToSummon.name + "WAS SUMMONED BY AI");
    }

    private void UseAcitiveAbilities()
    {
        foreach (GameObject card in aiPlayer.Field.ToList())
        {
            CardProperties prop = card.GetComponent<CardProperties>();
            CardAbility ability = prop.active_ability;
            if (ability is IActiveAbility activeAbility)
            {
                activeAbility.ai_Activate(card, gameScript);
                Debug.Log(card.name + "ABILITY USED");
            }

        }
        //hand missing
    }

    private void AttackCratures()
    {

        GameObject target = null;
        GameObject attackingCreature = null;
        //select the creature with the lowest attack that can attack
        foreach (GameObject card in gameScript.player2.Field.ToList())
        {
            CardProperties props = card.GetComponent<CardProperties>();
            if (props.attacksPerTurn > 0) //check if he can attack
                if (attackingCreature == null)
                    attackingCreature = card;
                else if (props.attack < attackingCreature.GetComponent<CardProperties>().attack) //start with the weakest and end on the strongest
                    attackingCreature = card;

        }
        if (attackingCreature == null) return; //no creature that can attack on field

        int attackPoints = attackingCreature.GetComponent<CardProperties>().attack;

        //if player 1 has no creatures on field
        if (gameScript.player1.Field.ToList().Count <= 0)
        {
            //direct
            gameScript.player1.lifePoints -= attackPoints;
            attackingCreature.GetComponent<CardProperties>().attacksPerTurn -= 1;
            Debug.Log(attackingCreature.name + "ATTACED DIRECTLY");

        }
        else //player 1 has creatures
        {
            //check if there is a p1 creature that this card could take out
            foreach (GameObject card in gameScript.player1.Field.ToList())
            {
                CardProperties props = card.GetComponent<CardProperties>();
                if (props.life <= attackPoints && gameScript.CheckIfTargetInAttackRange(card, attackingCreature) == true)
                {
                    target = card;
                }


            }
            //no creature to take out with attack -> pick at random
            if (target == null)
            {
                List<GameObject> possibleTargets = new List<GameObject>();
                foreach (GameObject card in gameScript.player1.Field.ToList())
                {
                    if (gameScript.CheckIfTargetInAttackRange(card, attackingCreature) == true)
                        possibleTargets.Add(card);
                }
                if (possibleTargets.Count > 0)
                {
                    System.Random rand = new System.Random();
                    int random_index = rand.Next(possibleTargets.Count);
                    target = possibleTargets[random_index];
                }


            }

            if (target != null)
            {
                attack(attackingCreature, target);

            }
            else
            {
                attackingCreature.GetComponent<CardProperties>().attacksPerTurn = 0; //take out of attacking line up since there is no possible target in range
            }


        }

        AttackCratures();

    }

    private void attack(GameObject attacker, GameObject target)
    {
        target.GetComponent<CardProperties>().life -= attacker.GetComponent<CardProperties>().attack;
        attacker.GetComponent<CardProperties>().attacksPerTurn -= 1;
        Debug.Log(attacker.name + "ATTACKED" + target.name);

        if (target.GetComponent<CardProperties>().life <= 0)
        {
            target.GetComponent<Card>().TransferCardToGrave();
            abilityActivator.triggerOnKillAbility(attacker, gameScript);
        } 
    }
    private void AttackDirectyWithRemainingCreatursThatCouldntAttackPreviously()
    {
        if (gameScript.player1.Field.Count > 0) return;

        foreach (GameObject card in gameScript.player2.Field.ToList())
        {
            CardProperties props = card.GetComponent<CardProperties>();
            while (props.attacksPerTurn > 0)
            {
                gameScript.player1.lifePoints -= props.attack;
                props.attacksPerTurn -= 1;
                Debug.Log(card.name + "ATTACED DIRECTLY");
            }
            
        }
    }
}