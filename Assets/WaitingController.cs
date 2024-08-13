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
        
        if (PhotonNetwork.playerList.Length == 2)
        {
            PhotonNetwork.LoadLevel("MainGame");
        }
    }

    /*void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if (PhotonNetwork.countOfPlayers == 2)
        {
            PhotonNetwork.LoadLevel("MainGame");
        }
    }*/
}