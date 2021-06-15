using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenu;

    public PauseMenuManager instance { get; private set; }

    public bool isPause => pauseMenu.activeSelf;

    private void Awake()
    {
        if (instance != null)
            Debug.LogWarning("You should have only one instance of PauseMenuManager");

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Resume();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel")) // Escape button on PC and Back button on Android
        { 
            if (isPause == true)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);

        Time.timeScale = 0;

        AudioListener.pause = true;
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);

        Time.timeScale = 1;

        AudioListener.pause = false;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
