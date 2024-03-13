using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ResourceUI : MonoBehaviour
{
    [SerializeField] public ResourceType type;
    [SerializeField] private TextMeshProUGUI valueText;

    public void Init()
    {
        valueText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateValue(float value)
    {
        if (string.IsNullOrEmpty(value.ToString())) return;

        valueText.text = value.ToString();
    }
}
