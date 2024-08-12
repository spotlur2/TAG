using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : Photon.MonoBehaviour
{
    public new PhotonView photonView;
    public GameObject playerPrefab;
    public GameObject GameCanvas;
    public GameObject StartCanvas;
    public GameObject SceneCamera;
    public Text CountdownAndTimerText;
    public Text PingText;

    private float countdownTime = 5f;
    private float gameTime = 5f;
    private bool gameStarted = false;
    private bool gameEnded = false; // Track if the game has ended

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (GameCanvas == null || StartCanvas == null || SceneCamera == null || CountdownAndTimerText == null || PingText == null || photonView == null)
        {
            Debug.LogError("One or more GameObjects or components are not assigned in the Inspector.");
            return;
        }

        StartCoroutine(WaitAndSpawnPlayer()); // Start coroutine to delay player spawning

        GameCanvas.SetActive(true);
        StartCanvas.SetActive(true);
        SceneCamera.SetActive(true);

    }

    private IEnumerator WaitAndSpawnPlayer()
    {
        yield return new WaitForSeconds(1f); // Adjust the delay time as needed
        SpawnPlayer(); // Spawn players after the delay
    }

    private void Update()
    {
        if (PingText != null)
        {
            PingText.text = "Ping: " + PhotonNetwork.GetPing();
        }

        if (gameStarted && CountdownAndTimerText != null)
        {
            CountdownAndTimerText.text = FormatTime(gameTime);
        }
    }

    private void SpawnPlayer()
    {
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity, 0);
        player.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.player); // Ensure correct ownership

        // Set the scene camera reference for the player
        Player playerScript = player.GetComponent<Player>();
        playerScript.SceneCamera = SceneCamera;

        if (PhotonNetwork.isMasterClient)
        {
            StartCoroutine(StartGameCountdown());
        }
        else
        {
            StartCanvas.SetActive(true); // Ensure StartCanvas is active for non-master clients as well
        }

        SceneCamera.SetActive(false); // Disable the scene camera when the player spawns
    }

    private IEnumerator StartGameCountdown()
    {
        CountdownAndTimerText.gameObject.SetActive(true);
        while (countdownTime > 0)
        {
            photonView.RPC("UpdateCountdownTextRPC", PhotonTargets.All, countdownTime);
            yield return new WaitForSeconds(1f);
            countdownTime--;
        }

        photonView.RPC("UpdateCountdownTextRPC", PhotonTargets.All, "Go!");
        yield return new WaitForSeconds(1f);

        if (PhotonNetwork.isMasterClient)
        {
            List<int> playerIDs = GetAllPlayerIDs();
            int selectedPlayerID = SelectRandomPlayer(playerIDs);
            photonView.RPC("SetPlayerAsIt", PhotonTargets.All, selectedPlayerID);
        }

        photonView.RPC("StartGameRPC", PhotonTargets.All);
    }

    [PunRPC]
    private void StartGameRPC()
    {
        gameStarted = true;
        StartCanvas.SetActive(false); // Hide the start canvas after the countdown
        StartCoroutine(GameTimer());
    }

    private IEnumerator GameTimer()
    {
        while (gameTime > 0)
        {
            yield return new WaitForSeconds(1f);
            gameTime--;
            photonView.RPC("UpdateGameTimeRPC", PhotonTargets.All, gameTime);
            CheckGameEndConditions(); // Check end conditions during the timer
        }

        if (!gameEnded) // Only call EndGame if game hasn't ended yet
        {
            photonView.RPC("UpdateCountdownTextRPC", PhotonTargets.All, "Time's up!");
            yield return new WaitForSeconds(2f);
            EndGame();
        }
    }

    [PunRPC]
    private void UpdateCountdownTextRPC(float time)
    {
        if (CountdownAndTimerText != null)
        {
            CountdownAndTimerText.text = "Game starts in: " + time.ToString("F0");
        }
    }

    [PunRPC]
    private void UpdateCountdownTextRPC(string message)
    {
        if (CountdownAndTimerText != null)
        {
            CountdownAndTimerText.text = message;
        }
    }

    [PunRPC]
    private void UpdateGameTimeRPC(float time)
    {
        gameTime = time;
        if (CountdownAndTimerText != null)
        {
            CountdownAndTimerText.text = FormatTime(gameTime);
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

    private void EndGame()
    {
        gameEnded = true;
        Debug.Log("Game Over.");

        // Find all player objects
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // Check if there is only one player left and they are "it"
        bool isItWinner = players.Length == 1 && players[0].GetComponent<Player>().isIt;
        Player itPlayer = null;

        if (isItWinner)
        {
            // Player who is "it" wins
            foreach (GameObject playerObj in players)
            {
                Player player = playerObj.GetComponent<Player>();
                if (player.isIt)
                {
                    itPlayer = player;
                    break;
                }
            }

            if (itPlayer != null)
            {
                // Set the tagger's name in TaggerWin before loading the scene
                TaggerWin.SetTaggerName(itPlayer.photonView.owner.NickName);
            }

            Debug.Log("Player who is 'it' wins!");
            StartCoroutine(ShowGameOverAndLoadScene("TaggerWin"));
        }
        else
        {
            // Timer ran out or not all players are tagged
            Debug.Log("Runners win!");
            StartCoroutine(ShowGameOverAndLoadScene("RunnersWin"));
        }

        // Disable player controls by setting speed and jump force to 0
        foreach (GameObject playerObj in players)
        {
            Player player = playerObj.GetComponent<Player>();
            player.MoveSpeed = 0;
            player.JumpForce = 0;
        }
    }

    private IEnumerator ShowGameOverAndLoadScene(string sceneName)
    {
        // Show the GameOver text
        if (CountdownAndTimerText != null)
        {
            CountdownAndTimerText.gameObject.SetActive(false);
        }
        if (GameCanvas != null)
        {
            GameObject gameOverText = GameCanvas.transform.Find("GameOver")?.gameObject;
            if (gameOverText != null)
            {
                gameOverText.SetActive(true);
            }
        }

        // Wait for 5 seconds
        yield return new WaitForSeconds(5f);

        // Load the appropriate scene
        SceneManager.LoadScene(sceneName);
    }

    private List<int> GetAllPlayerIDs()
    {
        List<int> playerIDs = new List<int>();
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            playerIDs.Add(player.ID);
        }
        return playerIDs;
    }

    private int SelectRandomPlayer(List<int> playerIDs)
    {
        int randomIndex = Random.Range(0, playerIDs.Count);
        return playerIDs[randomIndex];
    }

    private void CheckGameEndConditions()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // Check if all players are tagged except "it" player
        int taggedCount = 0;
        Player itPlayer = null;

        foreach (GameObject playerObj in players)
        {
            Player player = playerObj.GetComponent<Player>();
            if (player.isIt)
            {
                itPlayer = player;
            }
            else if (player.tagged)
            {
                taggedCount++;
            }
        }

        if (itPlayer != null && taggedCount == players.Length - 1)
        {
            // All players except "it" are tagged
            Debug.Log("Player who is 'it' wins!");
            if (!gameEnded)
            {
                EndGame();
            }
        }
    }

    [PunRPC]
    private void SetPlayerAsIt(int playerID)
    {
        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            Player player = playerObj.GetComponent<Player>();
            if (player.photonView.owner.ID == playerID)
            {
                Debug.Log("RAN");
                player.isIt = true;
                player.PlayerNameText.color = Color.red; // Turn the name tag red
                player.SetAsIt();
            }
        }
    }

    [PunRPC]
    private void TagPlayer(int playerID)
    {
        Debug.Log("Player with ID " + playerID + " has been tagged.");

        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            Player player = playerObj.GetComponent<Player>();
            if (player.photonView.owner.ID == playerID)
            {
                player.tagged = true;
                Debug.Log("Tagging player with ID " + playerID);
                player.Tag();
            }
        }
    }

    public void HandleTagging(int taggedPlayerID)
    {
        photonView.RPC("TagPlayer", PhotonTargets.All, taggedPlayerID);
    }
}
