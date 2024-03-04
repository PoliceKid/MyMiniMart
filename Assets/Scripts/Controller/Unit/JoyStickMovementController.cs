using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyStickMovementController : MonoBehaviour, IMovement
{
    [SerializeField] FloatingJoystick floatingJoystick;
    [SerializeField] float Speed;
    private Vector3 movementDir;

    private bool hasInit;
    public void Init()
    {
        floatingJoystick = FindObjectOfType<FloatingJoystick>();
        hasInit = true;
    }

    public virtual void HandleMovement(Vector3 movementDir)
    {
        transform.position += new Vector3(movementDir.x * Speed *Time.deltaTime , 0, movementDir.z * Speed * Time.deltaTime) ;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(movementDir), Speed *Time.deltaTime); 
    }
    public bool ReachedDestination()
    {
        return movementDir == Vector3.zero;
    }
    public void UpdateMovementDir()
    {
        movementDir = new Vector3(floatingJoystick.Horizontal, 0, floatingJoystick.Vertical);
    }
    public void SetSpeed(float value)
    {
        Speed = value;
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasInit) return;
        UpdateMovementDir();
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public Vector3 GetMovementDir()
    {
        return movementDir;
    }
}
