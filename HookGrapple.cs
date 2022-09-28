using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HookGrapple : MonoBehaviour
{
    [Header("Refrences")]
    private PlayerMovementAdvanced pm;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatisGrappleable;
    public LineRenderer Hooklr;
    public Camera camera;
    public GameObject HookGun;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    [Header("Input")]
    [SerializeField]public KeyCode grapplingKey;

    private bool grappling;

    private void Start()
    {
        HookGun.SetActive(false);
        pm = GetComponent<PlayerMovementAdvanced>();
       // pm.freeze = false;
    }

    private void Update()
    {

        if (Input.GetKeyDown(grapplingKey))
        {
            HookGun.SetActive(true);
            StartGrapple();
        }

        if(grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (grappling)
        {
            Hooklr.SetPosition(index: 0, gunTip.position);
        }
    }

    private void StartGrapple()
    {
        pm.readyToSlide = false;


        //Debug.Log("start grapple run");

        if (grapplingCdTimer > 0) return;

        grappling = true;

        //pm.freeze = true; //maybe don't use

        RaycastHit hit;
        if(Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatisGrappleable))
        {
            grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        Hooklr.enabled = true;
        Hooklr.SetPosition(1, grapplePoint);
    }

    private void ExecuteGrapple()
    {
        //Debug.Log("execute grapple run");
        //pm.freeze = false;
        camera.fieldOfView = 100f;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        pm.JumpToPosition(grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }
    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }
    public void StopGrapple()
    {
        //pm.freeze = false;

        grappling = false;

        HookGun.SetActive(false);

        camera.fieldOfView = 90f;

        grapplingCdTimer = grapplingCd;

        Hooklr.enabled = false;

        pm.readyToSlide = true;
    }
}
