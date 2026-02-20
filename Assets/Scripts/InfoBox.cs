using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoBox : MonoBehaviour
{
    public float displayTime = 3f;
    private bool newText;
    void Start()
    {
        this.gameObject.GetComponent<TextMeshPro>().text = "";
    }

    // Update is called once per frame
    void Update()
    {
        //remove info text after 3 seconds
        if (newText)
        {
            if (displayTime > 0) displayTime -= Time.deltaTime;
            else
            {
                displayTime = 3f;
                this.gameObject.GetComponent<TextMeshPro>().text = "";
                newText = false;
            }
        }

    }

    public void DisplayText(string text)
    {
        this.gameObject.GetComponent<TextMeshPro>().text = text;
        newText = true;
    }
    
}
