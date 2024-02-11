using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    SpriteRenderer BoardSpaceHighlight;

    void Awake()
    {
        BoardSpaceHighlight = GetComponent<SpriteRenderer>();
    }

    public void ToggleHighlight(bool isEnabled)
    {
        BoardSpaceHighlight.enabled = isEnabled;
    }
}
