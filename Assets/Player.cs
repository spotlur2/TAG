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
    public bool isIt = false;
    public bool tagged = false;

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
    }

    private void Update()
    {
        if (photonView.isMine && !tagged)
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

        if (Input.GetKeyDown(KeyCode.W) && isGrounded == true)
        {
            rb.velocity = Vector2.up * JumpForce;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded == true)
        {
            rb.velocity = Vector2.up * JumpForce;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
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
        sr.color = Color.red; // Change the sprite color to red
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
    }

    [PunRPC]
    public void TagPlayer(int playerID)
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.HandleTagging(playerID);
        }
    }

    public void Tag()
    {
        tagged = true;
        StartSpectating();
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
