using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public void Button_Start()
    {
        SceneManager.LoadSceneAsync("DownloadScene");
    }

    private void OnDestroy()
    {
        Debug.Log("I'm Destroyed");
    }
}
