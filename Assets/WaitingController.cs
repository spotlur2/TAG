using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class WaitingController : MonoBehaviour
{

    [SerializeField] private Button quitButton;
    public GameObject WaitingCanvas;

    private void Start()
    {
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }
    private void Awake()
    {
        WaitingCanvas.SetActive(true);
    }

    private void Update()
    {
        if (PhotonNetwork.playerList.Length == 3) // Check if all players are present
        {
            LoadScene();
        }
    }

    /*void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if (PhotonNetwork.countOfPlayers == 5)
        {
            PhotonNetwork.LoadLevel("MainGame");
        }
    }*/

     private void LoadScene()
    {
        // Get the selected game mode from room properties
        if (PhotonNetwork.room.CustomProperties.TryGetValue("GameMode", out object gameModeObj))
        {
            int gameMode = (int)gameModeObj;
            string sceneName = GetSceneNameForGameMode(gameMode);
            PhotonNetwork.LoadLevel(sceneName); // Load the scene for the game mode
        }
    }

    private string GetSceneNameForGameMode(int gameMode)
    {
        // Define the scene names for different game modes
        switch (gameMode)
        {
            case 0:
                return "MainGame";
            case 1:
                return "InfectionGame";
            case 2:
                return "FreezeGame";
            default:
                PhotonNetwork.Disconnect();
                return "MainMenu"; //Scene doesnt exist
        }
    }

    private void OnQuitClicked()
    {
        MenuController.Username = PhotonNetwork.playerName;
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("MainMenu");
    }
}