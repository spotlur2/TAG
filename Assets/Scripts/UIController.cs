using System;
using UnityEngine;
using UnityEngine.UI;


public class UIController : MonoBehaviour
{
    private Player player;
    private bool isMovingLeft = false;
    private bool isMovingRight = false;

    // public void MoveLeft()
    // {
    //     if (player != null && player.photonView.isMine)
    //     {
    //         player.FlipTrue();
    //         player.Move(Vector3.left);
    //     }
    // }

    // public void MoveRight()
    // {
    //     if (player != null && player.photonView.isMine)
    //     {
    //         player.FlipFalse();
    //         player.Move(Vector3.right);
    //     }
    // }

     public void StartMovingLeft()
    {
        isMovingLeft = true;
    }

    public void StartMovingRight()
    {
        isMovingRight = true;
    }

    public void StopMoving()
    {
        isMovingLeft = false;
        isMovingRight = false;
    }

    private void Update()
    {
        if (isMovingLeft && player != null && player.photonView.isMine)
        {
            player.Move(Vector3.left);
        }
        else if (isMovingRight && player != null && player.photonView.isMine)
        {
            player.Move(Vector3.right);
        }
    }

    public void Jump()
    {
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

    public void TestButtonClick()
{
    Debug.Log("Button Clicked!");
}

}
