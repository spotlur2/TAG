using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InfectionManager : Photon.MonoBehaviour
{
    public new PhotonView photonView;
    public GameObject playerPrefab;
    public GameObject GameCanvas;
    public GameObject StartCanvas;
    public GameObject SceneCamera;
    public Text CountdownAndTimerText;
    public Text PingText;
    [SerializeField] private UIController uiController;

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
        player.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.player); // Ensure correct ownership

        // Set the scene camera reference for the player
        Player playerScript = player.GetComponent<Player>();
        playerScript.SceneCamera = SceneCamera;

        // Set the player reference in the UIController
        uiController.SetPlayer(playerScript);

        if (PhotonNetwork.isMasterClient)
        {
            StartCoroutine(StartGameCountdown());
        }
        else
        {
            StartCanvas.SetActive(true);
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
            PhotonNetwork.room.IsOpen = false;

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

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        bool allPlayersIt = true;
        Player itPlayer = null;

        foreach (GameObject playerObj in players)
        {
            Player player = playerObj.GetComponent<Player>();
            if (player.isIt)
            {
                itPlayer = player;
            }
            else
            {
                allPlayersIt = false;
            }
        }

        if (allPlayersIt)
        {
            if (itPlayer != null)
            {
                TaggerWin.SetTaggerName(itPlayer.photonView.owner.NickName);
            }
            StartCoroutine(ShowGameOverAndLoadScene("TaggerWin"));
        }
        else
        {
            StartCoroutine(ShowGameOverAndLoadScene("RunnersWin"));
        }

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
        if (randomIndex < 0)
        {
            randomIndex = 0;
        }
        return playerIDs[randomIndex];
    }

    private void CheckGameEndConditions()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        bool allPlayersIt = true;

        foreach (GameObject playerObj in players)
        {
            Player player = playerObj.GetComponent<Player>();
            if (!player.isIt)
            {
                allPlayersIt = false;
                break;
            }
        }

        if (allPlayersIt && !gameEnded)
        {
            EndGame();
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
                player.isIt = true;
                player.SetAsIt();
            }
        }
        UpdateItPlayerSpeeds();
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
                player.Tag();
            }
        }
        UpdateItPlayerSpeeds();
    }

    private void UpdateItPlayerSpeeds()
    {
        int itCount = 0;
        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            Player player = playerObj.GetComponent<Player>();
            if (player.isIt)
            {
                itCount++;
            }
        }

        float newSpeed = 30f;
        if (itCount == 2)
        {
            newSpeed = 25f;
        }
        else if (itCount == 3)
        {
            newSpeed = 20f;
        }
        else if (itCount >= 4)
        {
            newSpeed = 15f;
        }

        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            Player player = playerObj.GetComponent<Player>();
            if (player.isIt)
            {
                player.MoveSpeed = newSpeed;
            }
        }
    }

    public void HandleTagging(int taggedPlayerID)
    {
        photonView.RPC("TagPlayer", PhotonTargets.All, taggedPlayerID);
    }
}
