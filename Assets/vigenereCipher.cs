using System.Collections;
using System.Text.RegularExpressions;
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

    Coroutine flashCoroutine;
    TextMesh ledText;

    void Awake()
    {
        moduleId = moduleIdCounter++;

        foreach (KMSelectable button in buttons)
        {
            KMSelectable pressedObject = button;
            button.OnInteract += delegate () { keypadPress(pressedObject); return false; };
        }

        GetComponent<KMBombModule>().OnActivate += Activate;
        ledText = LED.GetComponentInChildren<TextMesh>();
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
        ledText.text = textDisplay;
    }

    void keypadPress(KMSelectable obj)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, obj.transform);
        obj.AddInteractionPunch();
        if (moduleSolved || !lightsOn) {
            return;
        }
        string objtext = obj.GetComponentInChildren<TextMesh>().text;
        if (objtext != "Submit")
        {
            if (input.Length < 6)
            {
                input += objtext;
                Debug.LogFormat("[Vigenère Cipher #{0}] Pressed button {1}. Input is {2}.", moduleId, obj.GetComponentInChildren<TextMesh>().text, input);
                ledText.text = input;
            }
        }
        else {
            CheckAns();
        }
    }

    bool CheckAns()
    {
        Debug.LogFormat("[Vigenère Cipher #{0}] Submitted {1}, Expected {2}.", moduleId, input, answer);
        if (!input.Equals(answer))
        {
            incorrect = true;
            flashCoroutine = StartCoroutine(Flash(input, textDisplay, new Color(1.0f, 0.0f, 0.0f), new Color(0.0f, 1.0f, 0.0f)));
            input = "";
            return false;
        }
        else {
            moduleSolved = true;
            flashCoroutine = StartCoroutine(Flash(input, input, new Color(0.0f, 1.0f, 0.0f), new Color(0.0f, 1.0f, 0.0f)));
            GetComponent<KMBombModule>().HandlePass();
            return true;
        }
    }

    IEnumerator Flash(string curr, string final, Color colorStart, Color colorFinish) {
        FlashHelper(curr, colorStart);
        yield return new WaitForSeconds(.5f);
        FlashHelper("", new Color(0.0f, 0.0f, 0.0f));
        yield return new WaitForSeconds(.5f);
        FlashHelper(curr, colorStart);
        yield return new WaitForSeconds(.5f);
        FlashHelper("", new Color(0.0f, 0.0f, 0.0f));
        yield return new WaitForSeconds(.5f);
        FlashHelper(curr, colorStart);
        yield return new WaitForSeconds(.75f);
        FlashHelper(final, colorFinish);
        StopCoroutine(flashCoroutine);
    }

    void FlashHelper(string text, Color color) {
        ledText.color = color;
        ledText.text = text;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} submit|solve|s ABC123";
#pragma warning restore 414

    public KMSelectable[] ProcessTwitchCommand(string command) {
        command = command.ToLowerInvariant();

        var submit = Regex.Match(command, @"^\s*(?:submit|solve|s)\s+([A-Za-z0-9]*)\s*$", RegexOptions.IgnoreCase);
        if (submit.Success) {
            string tpInput = submit.Groups[1].Value;
            Debug.LogFormat("[Vigenère Cipher #{0}] Twitch Plays submitted {1}, Expected {2}.", moduleId, tpInput.ToUpperInvariant(), answer);
            KMSelectable[] submitButtons = new KMSelectable[tpInput.Length + 1];
            for (int i = 0; i < tpInput.Length; i++) {
                foreach (KMSelectable kMSelectable in buttons) {
                    if (kMSelectable.GetComponentInChildren<TextMesh>().text.Length == 1 && kMSelectable.GetComponentInChildren<TextMesh>().text.ToLowerInvariant()[0] == tpInput[i]) {
                        submitButtons[i] = kMSelectable;
                    }
                }
            }
            foreach (KMSelectable kMSelectable2 in buttons)
            {
                if (kMSelectable2.GetComponentInChildren<TextMesh>().text == "Submit")
                {
                    submitButtons[submitButtons.Length-1] = kMSelectable2;
                }
            }
            return submitButtons;
        }
        return null;
    }
}
