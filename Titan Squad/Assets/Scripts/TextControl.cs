using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextControl : MonoBehaviour
{
    public static TextControl instance;

    [SerializeField]
    private Text speaker = null;
    [SerializeField]
    private Text textToDisplay = null;

    [SerializeField]
    private Canvas textBox = null;

    bool nextLineReady = false;

    string currSpeaker;
    string[] speakerlines;
    int currLine = 0;

    public bool readingLines = false;

    void Start()
    {
        //If there are no instances of TextControl, instantiate it
        if (instance == null)
            instance = this;
        //If there is more than one instance of TextControl, destroy and reset
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }

        textBox.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!textBox.enabled)
            return;


        if (Input.anyKeyDown && currSpeaker != null && !nextLineReady)
        {
            skipToEnd();
        }
        else if (currSpeaker != null && nextLineReady && Input.anyKeyDown)
        {
            currLine++;
            if (currLine >= speakerlines.Length)
                endSpeech();
            else
                StartCoroutine(writeText(speakerlines[currLine]));
        }
    }

    public void beginSpeech(string desiredSpeaker, string[] lines)
    {
        readingLines = true;

        currLine = 0;
        currSpeaker = desiredSpeaker;
        speakerlines = lines;
        speaker.text = desiredSpeaker;
        nextLineReady = true;
        textToDisplay.text = "";
        StartCoroutine(writeText(speakerlines[currLine]));

        textBox.enabled = true;
    }

    void endSpeech()
    {
        currSpeaker = null;
        speakerlines = null;
        currLine = 0;

        textBox.enabled = false;

        readingLines = false;
    }

    IEnumerator writeText(string writeText)
    {
        nextLineReady = false;

        string currDisplayText = "";

        yield return null;

        foreach (char c in writeText)
        {
            textToDisplay.text = currDisplayText;
            currDisplayText += c;
            yield return new WaitForSecondsRealtime(0.04f);
        }
        textToDisplay.text = writeText;

        nextLineReady = true;
    }

    void skipToEnd()
    {
        textToDisplay.text = speakerlines[currLine];
        StopAllCoroutines();
        nextLineReady = true;
    }
}
