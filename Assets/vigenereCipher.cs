using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class vigenereCipher : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public Renderer LED;
    public KMSelectable[] buttons;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved, incorrect;
    private bool lightsOn;

    private string alphabet = "B45PREL0A6GFDHO8CWMQYSJ2ZTU9I1N3K7VX", textDisplay = "", answer = "", input = "";

    void Awake()
    {
        moduleId = moduleIdCounter++;

        foreach (KMSelectable button in buttons)
        {
            KMSelectable pressedObject = button;
            button.OnInteract += delegate () { keypadPress(pressedObject); return false; };
        }

        GetComponent<KMBombModule>().OnActivate += Activate;
    }

    void Start()
    {
        genRand();
    }

    void Activate()
    {
        lightsOn = true;
    }

    void Update()
    {
        if (incorrect) {
            GetComponent<KMBombModule>().HandleStrike();
            incorrect = false;
        }
    }

    void genRand()
    {
        for (int i = 0; i < 6; i++)
        {
            int index = UnityEngine.Random.Range(0, alphabet.Length);
            textDisplay += alphabet[index];
            answer += alphabet[(index + alphabet.IndexOf(Bomb.GetSerialNumber()[i])) % 36];
        }
        Debug.LogFormat("[Vigenère Cipher #{0}] Answer is {1}", moduleId, answer);
        LED.GetComponentInChildren<TextMesh>().text = textDisplay;
    }

    void keypadPress(KMSelectable obj)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, obj.transform);
        obj.AddInteractionPunch();
        Debug.LogFormat("{0} || {1}", moduleSolved, !lightsOn);
        if (moduleSolved || !lightsOn) {
            return;
        }
        string objtext = obj.GetComponentInChildren<TextMesh>().text;
        Debug.LogFormat("Pressed {0}", objtext);
        if (objtext != "Submit")
        {
            input += objtext;
            Debug.LogFormat("[Vigenère Cipher #{0}] Pressed button {1}. Input is {2}.", moduleId, obj.GetComponentInChildren<TextMesh>().text, input);
        }
        else {
            CheckAns();
            input = "";
        }
    }

    void CheckAns()
    {
        Debug.LogFormat("[Vigenère Cipher #{0}] Submitted {1}, Expected {2}.", moduleId, input, answer);
        if (!input.Equals(answer))
        {
            incorrect = true;
        }
        else {
            moduleSolved = true;
            GetComponent<KMBombModule>().HandlePass();
        }
    }
}
