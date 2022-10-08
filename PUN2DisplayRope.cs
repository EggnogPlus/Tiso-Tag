using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PUN2DisplayRope : MonoBehaviourPunCallbacks
{
    [PunRPC]
    private void Update()
    {
        if(GetComponent<SwingGrapple>().amIGrappling == true)
        {
            GetComponent<SwingGrapple>().photonView.RPC("DrawRope", RpcTarget.AllBuffered);
        }



    }
}
