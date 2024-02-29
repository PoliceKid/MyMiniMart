using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimationController : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator anim;
    private string currentState;
    public void Init(Transform visualCotainer,string initState)
    {
        anim = visualCotainer.GetComponentInChildren<Animator>();
        InitState(initState);
    }
    private void InitState(string initState)
    {
        currentState = initState;
        anim.SetBool(initState, true);
    }
    public void ChangeState(string newState)
    {
        anim.SetBool(currentState,false);
        currentState = newState;
        anim.SetBool(newState, true);
    }

}
