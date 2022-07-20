using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorLogic : MonoBehaviour
{
    float t = 0;
    Vector3 lowLimit;
    Vector3 current;
    Vector3 highLimit;
    bool scaleDown;
    bool scaleUp;
    public bool isHoldingPiece;
    bool startedHoldingPiece;

    private void Start()
    {
        scaleDown = true;
        lowLimit = transform.localScale - new Vector3(0.3f, 0.3f, 0);
        highLimit = transform.localScale;
        t = 0;
    }
    void Update()
    {
        if (!isHoldingPiece)
            LerpSelector();

        if (isHoldingPiece)
        {
            if (startedHoldingPiece)
            {
                current = transform.localScale;
                startedHoldingPiece = false;
                t = 0;
            }
            scaleUp = false;
            scaleDown = true;

            if (t < 1)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(current, lowLimit, t);
            }
        }

    }

    private void LerpSelector()
    {
        if (scaleDown == true && scaleUp == false)
        {
            t += Time.deltaTime * 1 / 2f;
            transform.localScale = Vector3.Lerp(highLimit, lowLimit, t);

            if (t>=1)
            {
                scaleDown = false;
                scaleUp = true;
                t = 0;
            }
        }
        if (scaleUp == true && scaleDown == false)
        {
            t += Time.deltaTime * 1 / 2f;
            transform.localScale = Vector3.Lerp(lowLimit, highLimit, t);


            if (t>=1)
            {
                scaleDown = true;
                scaleUp = false;
                t = 0;
            }
        }

        if (!startedHoldingPiece)
            startedHoldingPiece = true;
    }
}
