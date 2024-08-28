using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
	[SerializeField] private string VersionName = "2";
	[SerializeField] private GameObject UsernameMenu;
	[SerializeField] private GameObject ConnectPanel;
	[SerializeField] private InputField UsernameInput;

	[SerializeField] private InputField JoinGameInput;
	[SerializeField] private GameObject StartButton;
	[SerializeField] private TMP_Dropdown GameModeDropdown;
	public static string roomName;
	public static string Username = "";

	private void Awake()
	{
		PhotonNetwork.ConnectUsingSettings(VersionName);
	}

	private void Start()
	{
		if (string.IsNullOrEmpty(Username))
		{
			UsernameMenu.SetActive(true);
		}
		else
		{
			PhotonNetwork.playerName = Username;
		}
	}

	private void OnConnectedToMaster()
	{
		PhotonNetwork.JoinLobby(TypedLobby.Default);
		//Debug.Log("Connected");
	}

	public void ChangeUserNameInput()
	{
		if(UsernameInput.text.Length >= 3)
		{
			StartButton.SetActive(true);
		}
		else
		{
			StartButton.SetActive(false);
		}
	}

	public void SetUserName()
	{
		UsernameMenu.SetActive(false);
		PhotonNetwork.playerName = UsernameInput.text;
	}

	public void JoinGame()
	{
		roomName = JoinGameInput.text;
		int selectedGameMode = GameModeDropdown.value;

		foreach (RoomInfo room in PhotonNetwork.GetRoomList())
        {
			Debug.Log(room);
            if (room.Name == roomName)
            {
                // Room exists, join it
                PhotonNetwork.JoinRoom(roomName);
				RunnersWin.SetPreviousRoomName(roomName);
				TaggerWin.SetPreviousRoomName(roomName);
				RunnersWin.SetPreviousGameMode(selectedGameMode);
				TaggerWin.SetPreviousGameMode(selectedGameMode);
                return;
            }
        }

		RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = 5,
            IsOpen = true,
            IsVisible = true
        };

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { "GameMode", selectedGameMode }
        };

        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
        RunnersWin.SetPreviousRoomName(roomName);
        TaggerWin.SetPreviousRoomName(roomName);
		RunnersWin.SetPreviousGameMode(selectedGameMode);
		TaggerWin.SetPreviousGameMode(selectedGameMode);
	}

	private void OnJoinedRoom()
	{
		PhotonNetwork.LoadLevel("Waiting");
	}
}