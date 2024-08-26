using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Photon.MonoBehaviour
{
    public new PhotonView photonView;
    public Rigidbody2D rb;
    public GameObject PlayerCamera;
    public SpriteRenderer sr;
    public Text PlayerNameText;
    public Transform groundCheck;
    public LayerMask groundlayer;
    public GameObject SceneCamera; // Reference to the scene camera

    public bool isGrounded;
    public float MoveSpeed;
    public float JumpForce;
    public float originalMoveSpeed; // Store the original move speed
    public float originalJumpForce; // Store the original jump force
    public bool isIt = false;
    public bool tagged = false;
    public bool isMedic = false;
    public int gameMode;

    private void Awake()
    {
        if (photonView.isMine)
        {
            PlayerCamera.SetActive(true);
            PlayerNameText.text = PhotonNetwork.playerName;
        }
        else
        {
            PlayerNameText.text = photonView.owner.NickName;
            PlayerNameText.color = Color.white;
        }

        if (PhotonNetwork.room.CustomProperties.TryGetValue("GameMode", out object gameModeObj))
        {
            gameMode = (int)gameModeObj;
        }

        // Store the original movement values
        originalMoveSpeed = MoveSpeed;
        originalJumpForce = JumpForce;
    }

    private void Update()
    {
        if (photonView.isMine)
        {
            CheckInput();
        }
    }

    private void CheckInput()
    {

        var move = new Vector3(Input.GetAxisRaw("Horizontal"), 0);
        transform.position += move * MoveSpeed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.A))
        {
            photonView.RPC("FlipTrue", PhotonTargets.AllBuffered);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            photonView.RPC("FlipFalse", PhotonTargets.AllBuffered);
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 1, groundlayer);

        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.velocity = Vector2.up * JumpForce;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            rb.velocity = Vector2.up * JumpForce;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = Vector2.up * JumpForce;
        }
    }

    [PunRPC]
    private void FlipTrue()
    {
        sr.flipX = true;
    }

    [PunRPC]
    private void FlipFalse()
    {
        sr.flipX = false;
    }

    public void SetAsIt()
    {
        if (gameMode != 1){
            sr.color = Color.red; // Change the sprite color to red
        }
        else {
            sr.color = Color.green;
        }
        PlayerNameText.color = Color.red;
    }

    public void SetAsMedic()
    {
        sr.color = Color.blue; // Change the sprite color to blue for medic
        PlayerNameText.color = Color.blue;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isIt && collision.gameObject.CompareTag("Player"))
        {
            Player otherPlayer = collision.gameObject.GetComponent<Player>();
            if (otherPlayer != null && !otherPlayer.isIt)
            {
                photonView.RPC("TagPlayer", PhotonTargets.MasterClient, otherPlayer.photonView.owner.ID);
            }
        }
        else if (isMedic && collision.gameObject.CompareTag("Player"))
        {
            Player otherPlayer = collision.gameObject.GetComponent<Player>();
            if (otherPlayer != null && otherPlayer.tagged)
            {
                photonView.RPC("UnfreezePlayer", PhotonTargets.MasterClient, otherPlayer.photonView.owner.ID);
            }
        }
    }

    [PunRPC]
    public void TagPlayer(int playerID)
    {
        if (gameMode == 0)
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.HandleTagging(playerID);
            }
        }
        else if (gameMode == 1)
        {
            InfectionManager infectionManager = FindObjectOfType<InfectionManager>();
            if (infectionManager != null)
            {
                infectionManager.HandleTagging(playerID);
            }
        }
        else if (gameMode == 2)
        {
            FreezeManager freezeManager = FindObjectOfType<FreezeManager>();
            if (freezeManager != null)
            {
                freezeManager.HandleTagging(playerID);
            }
        }
    }

    [PunRPC]
    public void UnfreezePlayer(int playerID)
    {
        FreezeManager freezeManager = FindObjectOfType<FreezeManager>();
        if (freezeManager != null)
        {
            freezeManager.HandleUnfreeze(playerID);
        }
    }

    public void Tag()
    {
        tagged = true;
        if (gameMode == 0)
        {
            StartSpectating();
        }
        else if (gameMode == 1)
        {
            isIt = true;
            SetAsIt();
        }
        else if (gameMode == 2)
        {
            // Handle tagging logic for freeze tag
        }
    }

    private void StartSpectating()
    {
        if (photonView.isMine)
        {
            SceneCamera.SetActive(true); // Activate the scene camera
            PhotonNetwork.Destroy(gameObject); // Destroy the player object
        }
    }
}
