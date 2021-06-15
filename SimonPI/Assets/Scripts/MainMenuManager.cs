using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_WEBGL
        GameObject.Find("ExitButton").SetActive(false); // remove exit button for WebGL build
#endif
    }

    public void Play()
    {
        PlayerPrefs.SetString("Mode", "Classic");
        SceneManager.LoadScene("Game");
    }

    public void PlayFree()
    {
        PlayerPrefs.SetString("Mode", "Free");
        SceneManager.LoadScene("Game");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
