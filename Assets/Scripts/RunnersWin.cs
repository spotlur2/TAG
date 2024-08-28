using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using UnityEngine.UI;

public class RunnersWin : MonoBehaviour
{
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private string VersionName = "2";

    private static string previousRoomName; // Store the previous room name
    private static string Username;
    private static int previousGameMode;

    private void Start()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        Username = PhotonNetwork.playerName;

        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.room.IsOpen = true;
        }

        // Disconnect immediately when the scene loads
        StartCoroutine(DisconnectAndWait());
    }

    private IEnumerator DisconnectAndWait()
    {
        PhotonNetwork.Disconnect();
        yield return new WaitUntil(() => !PhotonNetwork.connected); // Wait until disconnected
    }

    private void OnPlayAgainClicked()
    {
        if (string.IsNullOrEmpty(previousRoomName))
        {
            return;
        }

        // Connect to Photon and join the room
        PhotonNetwork.ConnectUsingSettings(VersionName);
    }

    private void OnQuitClicked()
    {
        MenuController.Username = Username;
        SceneManager.LoadScene("MainMenu");
    }

    private void TryJoinRoom()
    {
		RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = 5,
            IsOpen = true,
            IsVisible = true
        };

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { "GameMode", previousGameMode }
        };

        PhotonNetwork.JoinOrCreateRoom(previousRoomName, roomOptions, TypedLobby.Default);
    }

    public static void SetPreviousRoomName(string roomName)
    {
        previousRoomName = roomName;
    }

    public static void SetPreviousGameMode(int gameMode)
    {
        previousGameMode = gameMode;
    }

    private void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    private void OnJoinedLobby()
    {
        TryJoinRoom(); // Attempt to join the room when connected to the lobby
    }

    private void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Waiting"); // Load the Waiting scene when joined a room
    }
}
