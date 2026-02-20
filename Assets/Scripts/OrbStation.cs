using System.Collections.Generic;
using UnityEngine;

public class OrbStation : MonoBehaviour
{
    public GameObject game;
    private Game gameScript;

    private InfoBox infoBox;

    void Start()
    {
        gameScript = game.GetComponent<Game>();
        infoBox = GameObject.Find("InfoBox").GetComponent<InfoBox>();
    }


    public bool CanPlaceCard()
    {
        if (gameScript.orbPlacements <= 0)
        {
            infoBox.DisplayText("Cant place any more orbs");
            return false;
        } 
        return true;
    }
    public void PlaceOrb(GameObject orb)
    {
        Destroy(orb);
        gameScript.activePlayer.orbCount += 1; //constatn orb count
        gameScript.orbsToSpend += 1; //orb count per turn
        gameScript.activePlayer.Hand.Remove(orb);

        gameScript.orbPlacements -= 1;

        gameScript.UpdateCardPlacementInHand();
        infoBox.DisplayText("Orb placed");
    }
}
