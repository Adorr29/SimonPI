using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameObject BloomPrefab;

    [Space]

    public Image image;
    public Sprite pressedSprite;

    [Space]

    public Text text;
    public GameObject pressedText;

    [Space]

    public Color defaultColor;
    public Color lightColor;

    [Space]

    public AudioClip pointerDownSound;
    public AudioClip pointerUpSound;
    public AudioClip lightsUpSound;
    public float lightsUpSoundPitch = 1;

    [Space]

    public KeyCode key;

    [Space]

    public UnityEvent onClik;

    AudioSource audioSource;

    [HideInInspector]
    public bool isLock = false;

    Sprite defaultSprite;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        CalcDefaultColor(); // remove this line if you want to set your own defaultColor in inspector
        CopyTextToPressedText();

        defaultSprite = image.sprite;
        image.color = defaultColor;

        audioSource.clip = lightsUpSound;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(key))
            OnPointerDown(null);

        if (Input.GetKeyUp(key))
            OnPointerUp(null);
    }

    void CopyTextToPressedText()
    {
        Text copyText = pressedText.GetComponent<Text>();

        if (copyText == null)
        {
            copyText = pressedText.AddComponent<Text>();
        }

        foreach (PropertyInfo property in typeof(Text).GetProperties())
        {
            if (property.CanWrite)
                property.SetValue(copyText, property.GetValue(text));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isLock == true)
            return;

        image.sprite = pressedSprite;
        text.gameObject.SetActive(false);
        pressedText.SetActive(true);

        audioSource.pitch = 1;
        audioSource.PlayOneShot(pointerDownSound);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (image.sprite != pressedSprite) // dont call this func if the OnPointerDown have not be called
            return;

        if (isLock == true)
            return;
        
        image.sprite = defaultSprite;
        text.gameObject.SetActive(true);
        pressedText.SetActive(false);

        onClik.Invoke();

        audioSource.pitch = 1;
        audioSource.PlayOneShot(pointerUpSound);
    }

    public void LightsUp(float time)
    {
        image.color = lightColor;
        Invoke(nameof(LightsDown), time);

        GameObject bloom = Instantiate(BloomPrefab, transform.parent);
        bloom.transform.position = transform.position;
        bloom.GetComponent<Image>().color = lightColor;
        Destroy(bloom, time);

        /*
        audioSource.pitch = lightsUpSoundPitch;
        audioSource.PlayOneShot(lightsUpSound, 0.3f);
        */

        audioSource.pitch = lightsUpSoundPitch;
        audioSource.Play();
    }

    void LightsDown()
    {
        image.color = defaultColor;

        audioSource.Stop();
    }

    void CalcDefaultColor()
    {
        defaultColor = lightColor;
        defaultColor.r += (1 - defaultColor.r) / 2;
        defaultColor.g += (1 - defaultColor.g) / 2;
        defaultColor.b += (1 - defaultColor.b) / 2;
    }
}
