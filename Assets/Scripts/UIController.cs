using System;
using UnityEngine;
using UnityEngine.UI;


public class UIController : MonoBehaviour
{
    private Player player;

    public void MoveLeft()
    {
        if (player != null && player.photonView.isMine)
        {
            player.FlipTrue();
            player.Move(Vector3.left);
        }
    }

    public void MoveRight()
    {
        if (player != null && player.photonView.isMine)
        {
            player.FlipFalse();
            player.Move(Vector3.right);
        }
    }

    public void Jump()
    {
        Debug.Log("PRESSED");
        if (player != null && player.photonView.isMine && player.isGrounded)
        {
            player.Jump();
        }
    }

    public void SetPlayer(Player currentPlayer)
    {
        if (currentPlayer.photonView.isMine)
        {
            player = currentPlayer;
        }
    }
}
