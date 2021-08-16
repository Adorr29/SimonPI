using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public SimpleButton[] numberButton;
    public SimpleButton commaButton;

    [Space]

    public Text text;
    Color defaultColor;
    public Color errorColor;
    public Color successColor;

    [Space]

    public AudioClip successSound;
    public AudioClip errorSound;
    public AudioClip bigScoreSound;
    public AudioClip[] typingSounds;

    AudioSource audioSource;

    List<SimpleButton> sequencePI;
    int sequenceLength = 0;
    int sequenceIndex = 0;

    Vector2 basePosition;
    bool quickPlay = false;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        basePosition = text.transform.parent.transform.position;
        defaultColor = text.color;

        string strPI = Resources.Load<TextAsset>("PI").text; // don't need to store PI anymore (maybe supr)
        sequencePI = GetSequenceFromString(strPI);

        StartCoroutine(Setup());
    }

    // Update is called once per frame
    void Update()
    {
        float adjust = Mathf.Max(text.preferredWidth - 200, 0);

        float scaleFactor = GameObject.Find("UI").GetComponent<Canvas>().scaleFactor;
        Transform textParent = text.transform.parent;
        Vector2 position = textParent.position;
        position.x = Vector2.Lerp(textParent.position, basePosition + Vector2.left * adjust * scaleFactor, 4 * Time.deltaTime).x; // maybe make a var for speed
        textParent.position = position;
    }

    IEnumerator Setup()
    {
        LockAllButtons(true);

        yield return new WaitForSeconds(1);

        yield return StartCoroutine(ClearText(0.2f));

        LockAllButtons(false);

        yield return new WaitForSeconds(1);

        if (quickPlay == true) // if player have start before the LightUpSequence
            yield break;

        string mode = PlayerPrefs.GetString("Mode");
        if (mode == "Classic")
            SetupClassic();
        else if (mode == "Recite")
            SetupRecite();
    }

    void SetupClassic()
    {
        sequenceLength = 1;

        StartCoroutine(LightUpSequence());
    }

    void SetupRecite()
    {
        sequenceLength = int.MaxValue;
    }

    List<SimpleButton> GetSequenceFromString(string sequenceStr)
    {
        List<SimpleButton> newSequence = new List<SimpleButton>();

        foreach (char c in sequenceStr)
        {
            if (c >= '0' && c <= '9')
                newSequence.Add(numberButton[c - '0']);
            else if (c == ',' || c == '.')
                newSequence.Add(commaButton);
            else
                Debug.LogWarning("\"" + c + "\" don't have a button");
        }

        return newSequence;
    }

    IEnumerator LightUpSequence()
    {
        float lightsUpTime = 0.5f;

        if (quickPlay == false)
            lightsUpTime  = 0.6f / (1 + Mathf.Pow((float)System.Math.E, sequenceLength / 5f - 5)) + 0.2f; // random math formul, but it woks ("f(x) = 0.6 / (1 + ℯ^(x / 5 - 5)) + 0.2" to see in geogebra)

        LockAllButtons(true);

        if (quickPlay == true)
        {
            SimpleButton button = sequencePI[sequenceLength - 1];

            button.LightsUp(lightsUpTime);
        }
        else
        {
            if (text.text.Length > 0)
            {
                yield return StartCoroutine(SmoothClearText(0.2f));

                yield return new WaitForSeconds(0.5f);
            }

            for (int i = 0; i < sequenceLength; i++)
            {
                SimpleButton button = sequencePI[i];

                button.LightsUp(lightsUpTime);

                string buttonText = button.text.text;

                text.text += buttonText;

                yield return new WaitForSeconds(lightsUpTime * 1.5f);
            }
        }

        yield return StartCoroutine(SmoothClearText(0.2f));

        LockAllButtons(false);
    }

    public IEnumerator SmoothClearText(float time)
    {
        Color color = text.color;
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            color.a = Mathf.Lerp(1, 0, elapsedTime / time);

            text.color = color;

            yield return new WaitForEndOfFrame();
        }

        text.text = "";
        text.color = defaultColor;
    }

    public IEnumerator ClearText(float wait)
    {
        while (text.text.Length > 0)
        {
            char c = text.text[text.text.Length - 1];

            text.text = text.text.Substring(0, text.text.Length - 1);

            if (c == ' ')
                continue;

            PlayTypingSound();

            yield return new WaitForSeconds(wait);
        }
    }

    public void LockAllButtons(bool isLock)
    {
        foreach (SimpleButton button in numberButton)
            button.isLock = isLock;

        commaButton.isLock = isLock;
    }

    public void OnButtonPress(SimpleButton pressedButton)
    {
        if (sequenceLength == 0)
            quickPlay = true;

        string buttonText = pressedButton.text.text;

        text.text += buttonText;

        if (pressedButton != sequencePI[sequenceIndex])
        {
            GameOver();
            return;
        }

        sequenceIndex++;

        if (sequenceLength == 0)
            return;

        if (sequenceIndex >= sequenceLength)
        {
            LockAllButtons(true);

            text.color = successColor;

            audioSource.PlayOneShot(successSound);

            StartCoroutine(NextStep());
        }
    }

    IEnumerator NextStep()
    {
        yield return new WaitForSeconds(1);

        sequenceIndex = 0;
        sequenceLength++;

        yield return StartCoroutine(LightUpSequence());
    }

    void GameOver()
    {
        LockAllButtons(true);

        text.color = errorColor;

        StartCoroutine(FlashLightsUp(sequencePI[sequenceIndex], 3));

        string mode = PlayerPrefs.GetString("Mode");
        if (mode == "Classic")
            StartCoroutine(Retry());
        else if (mode == "Recite")
            StartCoroutine(Scoring());

        audioSource.PlayOneShot(errorSound);
    }

    IEnumerator FlashLightsUp(SimpleButton expectedButton, float time)
    {
        yield return new WaitForSeconds(1f);

        const float speed = 1;

        while (time > 0)
        {
            expectedButton.LightsUp(speed / 2f);

            yield return new WaitForSeconds(speed);

            time -= speed;
        }
    }

    IEnumerator Retry()
    {
        yield return new WaitForSeconds(3);

        if (sequenceLength == 0)
            sequenceLength = sequenceIndex + 1;

        sequenceIndex = 0;

        if (quickPlay == true)
        {
            yield return StartCoroutine(SmoothClearText(0.2f));

            LockAllButtons(false);
        }
        else
        {
            yield return new WaitForSeconds(1);

            yield return StartCoroutine(LightUpSequence());
        }
    }

    IEnumerator Scoring()
    {
        yield return new WaitForSeconds(4);

        text.color = defaultColor;

        yield return StartCoroutine(ClearText(0.1f));

        yield return new WaitForSeconds(1);

        foreach (char c in "Score = ")
        {
            if (c == ' ')
            {
                text.text += c;
                continue;
            }

            yield return new WaitForSeconds(0.2f);

            text.text += c;

            PlayTypingSound();
        }

        foreach (char c in sequenceIndex.ToString())
        {
            yield return new WaitForSeconds(0.5f);

            text.text += c;

            PlayTypingSound();
        }

        if (sequenceIndex >= 100)
        {
            yield return new WaitForSeconds(0.5f);

            audioSource.PlayOneShot(bigScoreSound);
        }
    }

    void PlayTypingSound()
    {
        audioSource.PlayOneShot(typingSounds[Random.Range(0, typingSounds.Length)]);
    }
}
