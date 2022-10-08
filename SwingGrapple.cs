using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SwingGrapple : MonoBehaviourPunCallbacks
{
    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappable;
    public Transform gunTip, camera, player;
    private float maxDistance = 100f;
    private SpringJoint joint;
    public bool amIGrappling;


    public GameObject swingGun;

    private void Awake()
    {
        lr = GetComponentInChildren<LineRenderer>();
 
    }

   // [PunRPC]
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && LocalPauseMenu.GameIsPaused == false)
        {
            //swingGun.SetActive(true);
            StartGrapple();
            //GetComponent<SwingGrapple>().photonView.RPC("StartGrapple", RpcTarget.AllBuffered); - if i need to send an rpc to display it online
        }
        else if(Input.GetMouseButtonUp(0) && LocalPauseMenu.GameIsPaused == false)
        {
            //swingGun.SetActive(false);
            StopGrapple();
            //GetComponent<SwingGrapple>().photonView.RPC("StopGrapple", RpcTarget.AllBuffered); - if i need to send an rpc to display it online
        }

        //GetComponent<PlayerMovementAdvanced>().photonView.RPC("onUntagged", RpcTarget.AllBuffered); - RPC example


    }

    //called after update
    private void LateUpdate()
    {
        /* if (photonView.IsMine && amIGrappling == true)
            GetComponent<SwingGrapple>().photonView.RPC("DrawRope", RpcTarget.AllBuffered); */
        DrawRope();
    }

    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappable))
        {
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;


            float distanceFromPoint = Vector3.Distance(a: player.position, b: grapplePoint);
            
            //distance grapple will try to keep you from grapple point
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            //ajdust these to liking - current is ok but could be improved
            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;

            amIGrappling = true;

        }
    }

    void DrawRope()
    {
        if (!joint) return;
        lr.SetPosition(index: 0, gunTip.position);
        lr.SetPosition(index: 1, grapplePoint);

    } 

    void StopGrapple()
    {
        lr.positionCount = 0;
        Destroy(joint);
        amIGrappling=false;
    }

    public bool IsGrappling()
    {
        return joint != null;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}
