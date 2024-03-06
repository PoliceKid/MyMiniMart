using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitModelView : MonoBehaviour
{
    [SerializeField] public Transform hatSlotTransform;

    public void InitModel(GameObject hatModel)
    {
        hatModel.transform.parent = hatSlotTransform;
        hatModel.transform.localPosition = Vector3.zero;
        hatModel.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }
}
