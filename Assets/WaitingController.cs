using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WaitingController : MonoBehaviour
{
    public GameObject WaitingCanvas;

    private void Awake()
    {
        WaitingCanvas.SetActive(true);
    }

    private void Update()
    {
        
        if (PhotonNetwork.playerList.Length == 3)
        {
            PhotonNetwork.LoadLevel("MainGame");
        }
    }

    /*void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if (PhotonNetwork.countOfPlayers == 5)
        {
            PhotonNetwork.LoadLevel("MainGame");
        }
    }*/
}