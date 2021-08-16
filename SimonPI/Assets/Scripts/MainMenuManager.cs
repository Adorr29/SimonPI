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

    public void PlayRecite()
    {
        PlayerPrefs.SetString("Mode", "Recite");
        SceneManager.LoadScene("Game");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
