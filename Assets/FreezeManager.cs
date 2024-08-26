using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FreezeManager : Photon.MonoBehaviour
{
    public new PhotonView photonView;
    public GameObject playerPrefab;
    public GameObject GameCanvas;
    public GameObject StartCanvas;
    public GameObject SceneCamera;
    public Text CountdownAndTimerText;
    public Text PingText;

    private float countdownTime = 5f;
    private float gameTime = 200f;
    private bool gameStarted = false;
    private bool gameEnded = false;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (GameCanvas == null || StartCanvas == null || SceneCamera == null || CountdownAndTimerText == null || PingText == null || photonView == null)
        {
            return;
        }

        StartCoroutine(WaitAndSpawnPlayer());

        GameCanvas.SetActive(true);
        StartCanvas.SetActive(true);
        SceneCamera.SetActive(true);
    }

    private IEnumerator WaitAndSpawnPlayer()
    {
        yield return new WaitForSeconds(1f);
        SpawnPlayer();
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
        player.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.player);

        Player playerScript = player.GetComponent<Player>();
        playerScript.SceneCamera = SceneCamera;

        if (PhotonNetwork.isMasterClient)
        {
            StartCoroutine(StartGameCountdown());
        }
        else
        {
            StartCanvas.SetActive(true);
        }

        SceneCamera.SetActive(false);
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
            PhotonNetwork.room.IsOpen = false;

            List<int> playerIDs = GetAllPlayerIDs();
            int itPlayerID = SelectRandomPlayer(playerIDs);
            int medicPlayerID;
            do
            {
                medicPlayerID = SelectRandomPlayer(playerIDs);
            } while (medicPlayerID == itPlayerID);

            photonView.RPC("SetPlayerRoles", PhotonTargets.All, itPlayerID, medicPlayerID);
        }

        photonView.RPC("StartGameRPC", PhotonTargets.All);
    }

    [PunRPC]
    private void StartGameRPC()
    {
        gameStarted = true;
        StartCanvas.SetActive(false);
        StartCoroutine(GameTimer());
    }

    private IEnumerator GameTimer()
    {
        while (gameTime > 0)
        {
            yield return new WaitForSeconds(1f);
            gameTime--;
            photonView.RPC("UpdateGameTimeRPC", PhotonTargets.All, gameTime);
            CheckGameEndConditions();
        }

        if (!gameEnded)
        {
            photonView.RPC("UpdateCountdownTextRPC", PhotonTargets.All, "Time's up!");
            yield return new WaitForSeconds(2f);
            EndGame(false);
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

    private void EndGame(bool isItWinner)
    {
        gameEnded = true;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (isItWinner)
        {
            foreach (GameObject playerObj in players)
            {
                Player player = playerObj.GetComponent<Player>();
                if (player.isIt)
                {
                    TaggerWin.SetTaggerName(player.photonView.owner.NickName);
                    break;
                }
            }

            StartCoroutine(ShowGameOverAndLoadScene("TaggerWin"));
        }
        else
        {
            StartCoroutine(ShowGameOverAndLoadScene("RunnersWin"));
        }

        // Disable player controls
        foreach (GameObject playerObj in players)
        {
            Player player = playerObj.GetComponent<Player>();
            player.MoveSpeed = 0;
            player.JumpForce = 0;
        }
    }

    private IEnumerator ShowGameOverAndLoadScene(string sceneName)
    {
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

        yield return new WaitForSeconds(5f);

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

        int frozenCount = 0;
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
                frozenCount++;
            }
        }

        // Check if the "it" player wins (all other players are tagged)
        if (itPlayer != null && frozenCount == players.Length - 1)
        {
            if (!gameEnded)
            {
                EndGame(true); // Pass true to indicate that "it" player wins
            }
        }
        // If time runs out, runners win
        else if (gameTime <= 0)
        {
            if (!gameEnded)
            {
                EndGame(false); // Pass false to indicate that runners win
            }
        }
}


    [PunRPC]
    private void SetPlayerRoles(int itPlayerID, int medicPlayerID)
    {
        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            Player player = playerObj.GetComponent<Player>();
            if (player.photonView.owner.ID == itPlayerID)
            {
                player.isIt = true;
                player.SetAsIt();
            }
            else if (player.photonView.owner.ID == medicPlayerID)
            {
                player.isMedic = true;
                player.SetAsMedic(); // Ensure you have this method for the medic
            }
        }
    }

    [PunRPC]
    private void UnfreezePlayer(int playerID)
    {
        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            Player player = playerObj.GetComponent<Player>();
            if (player.photonView.owner.ID == playerID)
            {
                player.tagged = false;
                player.MoveSpeed = player.originalMoveSpeed; // Restore original speed
                player.JumpForce = player.originalJumpForce; // Restore original jump force
            }
        }
    }

    [PunRPC]
    private void TagPlayer(int playerID)
    {
        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            Player player = playerObj.GetComponent<Player>();
            if (player.photonView.owner.ID == playerID)
            {
                player.tagged = true;
                player.MoveSpeed = 0;
                player.JumpForce = 0;
            }
        }
    }

    public void HandleTagging(int taggedPlayerID)
    {
        photonView.RPC("TagPlayer", PhotonTargets.All, taggedPlayerID);
    }

    public void HandleUnfreeze(int unfreezePlayerID)
    {
        photonView.RPC("UnfreezePlayer", PhotonTargets.All, unfreezePlayerID);
    }
}
