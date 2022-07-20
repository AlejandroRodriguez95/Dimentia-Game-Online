using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HightlightSlot : MonoBehaviour
{

    Color32 color;
    byte maxTransparency;
    byte minTransparency;
    bool lerpingUp;
    bool lerpingDown;
    float t;
    [SerializeField] private float lerpSpeed;
    MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        color = meshRenderer.material.color;
        minTransparency = color.a;
        t = 0;
        maxTransparency = (byte)(minTransparency + 20);
        lerpingUp = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (lerpingUp == true && lerpingDown == false)
        {
            color.a = (byte)Mathf.Lerp(minTransparency, maxTransparency, t);
            t += Time.deltaTime * lerpSpeed;

            meshRenderer.material.color = color;

            if (t >= 1)
            {
                lerpingDown = true;
                lerpingUp = false;
                t = 0;
            }
        }
        if (lerpingDown == true && lerpingUp == false)
        {
            color.a = (byte)Mathf.Lerp(maxTransparency, minTransparency, t);
            t += Time.deltaTime * lerpSpeed;

            meshRenderer.material.color = color;

            if (t >= 1)
            {
                lerpingDown = false;
                lerpingUp = true;
                t = 0;
            }
        }
    }
}
