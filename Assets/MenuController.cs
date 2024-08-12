using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
	[SerializeField] private string VersionName = "1";
	[SerializeField] private GameObject UsernameMenu;
	[SerializeField] private GameObject ConnectPanel;

	[SerializeField] private InputField UsernameInput;
	[SerializeField] private InputField CreateGameInput;
	[SerializeField] private InputField JoinGameInput;

	[SerializeField] private GameObject StartButton;
	public static string roomName;

	private void Awake()
	{
		PhotonNetwork.ConnectUsingSettings(VersionName);
	}

	private void Start()
	{
		UsernameMenu.SetActive(true);
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

	public void CreateGame()
	{
		roomName = CreateGameInput.text;
		PhotonNetwork.CreateRoom(roomName, new RoomOptions() {MaxPlayers = 5, IsOpen = true, IsVisible = true}, TypedLobby.Default);
		RunnersWin.SetPreviousRoomName(roomName);
		TaggerWin.SetPreviousRoomName(roomName);
	}

	public void JoinGame()
	{
		roomName = JoinGameInput.text;
		PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions() {MaxPlayers = 5, IsOpen = true, IsVisible = true}, TypedLobby.Default);
		RunnersWin.SetPreviousRoomName(roomName);
		TaggerWin.SetPreviousRoomName(roomName);
	}

	private void OnJoinedRoom()
	{
		PhotonNetwork.LoadLevel("Waiting");
	}
}