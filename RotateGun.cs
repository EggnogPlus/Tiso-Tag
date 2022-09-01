using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateGun : MonoBehaviour
{
    public SwingGrapple grapple;

    private Quaternion desiredRotation;
    private float rotationSpeed = 5f;


    //i like a fat coooock - not ollie

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
        
        //transform.LookAt(grapple.GetGrapplePoint());
    }
}
