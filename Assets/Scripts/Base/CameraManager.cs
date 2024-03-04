using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static CameraManager Instance { get; private set; }

    [SerializeField] Camera MainCam;
    [SerializeField] Vector3 CammOffset;
    [Range(0f, 1f)]
    public float SmoothTimePos = .3f;
    [HideInInspector] public Vector3 VelocityCamPos;

    //Vector3 PosFollowing = Vector3.zero;
    private bool IsFollowing;

    private bool hasInit;
    GameObject targetObject;
    public void Init()
    {
        if (Instance == null)
            Instance = this;
        hasInit = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasInit) return;
        CamMovementFollowing();
    }
    public void FollowTarget(GameObject targetPoint)
    {
        targetObject = targetPoint;
       
        IsFollowing = true;
    }
    public void CamMovementFollowing()
    {
        if (!IsFollowing) return;
        Vector3 target = new Vector3(targetObject.transform.position.x, MainCam.transform.position.y, targetObject.transform.position.z -25);
        MainCam.transform.position = Vector3.SmoothDamp(MainCam.transform.position, target, ref VelocityCamPos, SmoothTimePos);
    }

 
}
