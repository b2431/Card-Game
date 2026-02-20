using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Graveyard : MonoBehaviour
{
    public GameObject game;
    public GameObject mainCam;
    public GameObject infoInterface;
    public GameObject backButton;
    private Vector3 infoInterfaceOriginalPos;
    private Vector3 mainCamOriginalPos;
    public bool player1;

    private Color originalBackgroundColor;


    void Start()
    {
        infoInterfaceOriginalPos = infoInterface.transform.position;
        mainCamOriginalPos = mainCam.transform.position;
        originalBackgroundColor = Camera.main.backgroundColor;

    }

    void Update()
    {

    }

    void OnMouseDown()
    {
        //move the camera and the info interface to the graveyard positions
        if (player1)
        {
            mainCam.transform.position = new Vector3(mainCam.transform.position.x + 50, mainCam.transform.position.y, mainCam.transform.position.z);
            infoInterface.transform.position = new Vector3(infoInterface.transform.position.x + 50, infoInterface.transform.position.y, infoInterface.transform.position.z);
        }
        else
        {
            mainCam.transform.position = new Vector3(mainCam.transform.position.x + 100, mainCam.transform.position.y, mainCam.transform.position.z);
            infoInterface.transform.position = new Vector3(infoInterface.transform.position.x + 100, infoInterface.transform.position.y, infoInterface.transform.position.z);
        }
        backButton.SetActive(true);
        Camera.main.backgroundColor = Color.grey;

        game.GetComponent<Game>().inGraveyard = true;
        
    }


    public void BackToGameButton()
    {
        //move them back to the playing field
        mainCam.transform.position = mainCamOriginalPos;
        infoInterface.transform.position = infoInterfaceOriginalPos;
        backButton.SetActive(false);
        Camera.main.backgroundColor = originalBackgroundColor;

        game.GetComponent<Game>().inGraveyard = false;
    }


}
