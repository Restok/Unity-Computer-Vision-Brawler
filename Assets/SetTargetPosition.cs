using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTargetPosition : MonoBehaviour
{
  public GameObject target;
    void Update()
    {
      target.transform.position = transform.position;
    }
}
