using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;


public class LocalPauseMenu : MonoBehaviourPunCallbacks
{

    public GameObject PauseMenuCanvas;

    public static bool GameIsPaused;
    public static bool music;
    void Start()
    {
        GameIsPaused = false;
        music = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused == false)
            {
                Time.timeScale = 0f;
                PauseMenuCanvas.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                GameIsPaused = true;
            }
            else if (GameIsPaused == true)
            {
                Time.timeScale = 1f;
                PauseMenuCanvas.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                GameIsPaused = false;
            }
        }
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        PauseMenuCanvas.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameIsPaused = false;
    }

    public void Controls()
    {
        
    }

    public void ToggleMusic()
    {

        if (music == true)
        {
            //GetComponent<BGmusic>().MusicMaker.GetComponent<AudioSource>().volume = 0f;
            //MusicMaker.GetComponent<AudioSource>().volume = 0;
            music = false;
        }
        else if (music == false)
        {
            //GetComponent<BGmusic>().MusicMaker.GetComponent<AudioSource>().volume = 0.1f;
            //MusicMaker.GetComponent<AudioSource>().volume = 1;
            music = true;
        }

    }

    public void LeaveLobby()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void QuitToDesktop()
    {
        Debug.Log("Application Quitting...");
        Application.Quit();
    }

}