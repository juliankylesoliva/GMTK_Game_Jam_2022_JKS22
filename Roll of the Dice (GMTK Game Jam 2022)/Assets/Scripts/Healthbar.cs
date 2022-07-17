using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    [SerializeField] Transform meterFill;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SetFillScale(float ratio)
    {
        meterFill.localScale = new Vector3(1f, ratio, 1f);
    }
}
