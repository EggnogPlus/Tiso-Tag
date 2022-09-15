using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{


    public GameObject playerPrefab;
    public Transform spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
       var newPlayer = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);

        // if we're host
        if (PhotonNetwork.IsMasterClient)
        {
            // Start as tagged
            newPlayer.GetComponent<PlayerMovementAdvanced>().photonView.RPC("onTagged", RpcTarget.AllBuffered); // need all buffered as to accomodate for new players
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
