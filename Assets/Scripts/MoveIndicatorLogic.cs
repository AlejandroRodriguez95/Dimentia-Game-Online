using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveIndicatorLogic : MonoBehaviour
{
    private Material mat;
    void Start()
    {
        mat = this.GetComponent<MeshRenderer>().material;
        mat.SetFloat("_Size", transform.lossyScale.y);
    }

}
