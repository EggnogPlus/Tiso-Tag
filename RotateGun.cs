using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using DG.Tweening;

public class RotateGun : MonoBehaviourPunCallbacks
{
    public SwingGrapple grapple;
    public GameObject gunToTilt;

    private Quaternion desiredRotation;
    private float rotationSpeed = 5f;

    private PlayerMovementAdvanced pm;

    PlayerMovementAdvanced.MovementState state;

    // Update is called once per frame
    void Update()
    {
        if (!grapple.IsGrappling())
        {
            desiredRotation = transform.parent.rotation;
        }
        else
        {
            desiredRotation = Quaternion.LookRotation(grapple.GetGrapplePoint() -transform.position);
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);

        if (photonView.IsMine)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {


                transform.Rotate(0, 0, 10);



                /*
                if(((transform.rotation.eulerAngles.z < 50) && (transform.rotation.eulerAngles.z > 0)))
                {
                    transform.Rotate(0, 0, 1, Space.Self);
                }
                else if (((transform.rotation.eulerAngles.z < 100) && (transform.rotation.eulerAngles.z > 49)))
                {
                    transform.Rotate(0, 0, 0.25f, Space.Self);
                }
                */

                //GetComponent<RotateGun>().transform.rotation = Mathf.Clamp(0, 0, 50);

            }
        }
       

                
        //transform.LookAt(grapple.GetGrapplePoint());
    }
}
