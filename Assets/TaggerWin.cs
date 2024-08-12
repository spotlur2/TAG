using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using UnityEngine.UI;

public class TaggerWin : MonoBehaviour
{
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private string VersionName = "1";
    public Text TitleText;

    private static string previousRoomName; // Store the previous room name
    private static string taggerName; // Store the name of the "it" player

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

        TitleText.text = taggerName + " Wins!";

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
            Debug.LogWarning("Room name is not set.");
            return;
        }

        // Connect to Photon and join the room
        PhotonNetwork.ConnectUsingSettings(VersionName);
    }

    private void OnQuitClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void TryJoinRoom()
    {
        if (!string.IsNullOrEmpty(previousRoomName))
        {
            PhotonNetwork.JoinOrCreateRoom(previousRoomName, new RoomOptions() { MaxPlayers = 5 }, TypedLobby.Default);
        }
        else
        {
            Debug.LogWarning("Room name is not set.");
        }
    }

    public static void SetPreviousRoomName(string roomName)
    {
        previousRoomName = roomName;
    }

    public static void SetTaggerName(string name)
    {
        taggerName = name;
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

    private void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from Photon: " + cause.ToString());
        // Optionally handle reconnection or additional logic
    }
}
