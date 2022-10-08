using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGmusic : MonoBehaviour
{
    public static BGmusic instance;
    public AudioSource Musicmaker;

    // music not working at initial lobby

    void Awake()
    {
        Musicmaker.volume = 0.1f;

        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Update()
    {
        if(LocalPauseMenu.music == false)
        {
            Musicmaker.volume = 0f;
        }
        
        if(LocalPauseMenu.music == true)
        {
            Musicmaker.volume = 0.1f;
        }
    }
}