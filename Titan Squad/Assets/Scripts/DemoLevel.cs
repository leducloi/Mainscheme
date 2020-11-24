using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoLevel : Level
{

    string[] allSpeeches;

    // Start is called before the first frame update
    override protected
    void Start()
    {
        activeObjectives = new List<GameObject>();
        activeObjectives.Add(objectives[0]);

        allSpeeches = levelScript.text.Split('-');

        base.Start();
    }

    // Update is called once per frame
    override protected
    void Update()
    {
        base.Update();
    }

    override
    protected void levelCompleted()
    {
        base.levelCompleted();
    }


    override
    public IEnumerator cutscene()
    {

        yield return null;
        
        for (int index = 1; index < allSpeeches.Length - 1; index++)
        {
            string[] allLines = allSpeeches[index].Split('.');
            string speaker = allLines[0];
            if (speaker.Contains("Pan"))
                continue;

            string[] linesToSpeak = new string[allLines.Length - 2];
            for (int x = 0; x < linesToSpeak.Length; x++)
            {
                linesToSpeak[x] = allLines[x + 1];
                linesToSpeak[x] = linesToSpeak[x].Trim();
                linesToSpeak[x] += ".";
            }

            TextControl.instance.beginSpeech(speaker, linesToSpeak);
            while (TextControl.instance.readingLines)
                yield return null;
        }
        
    }
}
