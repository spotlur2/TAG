using UnityEngine;

public class UIController : MonoBehaviour
{
    public Player player; // Reference to the Player script

    public void MoveLeft()
    {
        if (player != null)
        {
            player.FlipTrue();
            player.Move(Vector3.left);
        }
    }

    public void MoveRight()
    {
        if (player != null)
        {
            player.FlipFalse();
            player.Move(Vector3.right);
        }
    }

    public void Jump()
    {
        if (player != null && player.isGrounded)
        {
            player.Jump();
        }
    }

    public void SetPlayer(Player currentPlayer)
    {
        player = currentPlayer;
    }
}
