using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireObject : MonoBehaviour
{
    private Transform targetTransform;

    private void Update() 
    {
        if (targetTransform)
        {
            transform.position = Vector3.Lerp(transform.position,targetTransform.position,Time.deltaTime * 3);
        }    
    }
    public void FireEntities(Transform targetT)
    {
        targetTransform = targetT;
    }
}
