using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe;
public class TargetStateController : MonoBehaviour
{
  private enum State
  {
    REST = 0,
    GUARD = 1,
    EXTENDED = 2
  }
  private State currentLeftState = State.REST;
  private State currentRightState = State.REST;
  [SerializeField] private GameObject leftHandObj;
  [SerializeField] private GameObject rightHandObj;
  private Vector3 leftTarget;
  private Vector3 rightTarget;
  private float centerOffset = .31f;
  private Vector3 guardPosition = new Vector3(-0.16f, 0, 0.2f);
  private Vector3 extendPosition = new Vector3(-0.16f, 0, 0.75f);
  private Vector3 restPosition = new Vector3(0.50f, 0.13f);
  private const float moveSpeed = 3f;
  //private Vector3 restPosition = 
  private void Start()
  {
    leftTarget = restPosition;
    leftTarget.y = -centerOffset;
    rightTarget = restPosition;
    rightTarget.y = centerOffset;
  }
  private void Update()
  {
    leftHandObj.transform.localPosition = Vector3.MoveTowards(
      leftHandObj.transform.localPosition,
      leftTarget,
      moveSpeed * Time.deltaTime
      );

    rightHandObj.transform.localPosition = Vector3.MoveTowards(
      rightHandObj.transform.localPosition,
      rightTarget,
      moveSpeed * Time.deltaTime
      );
  }

  public void setState(NormalizedLandmarkList poseLandmarks)
  {
    NormalizedLandmark[] rightArm = { poseLandmarks.Landmark[12], 
                                     poseLandmarks.Landmark[14], 
                                     poseLandmarks.Landmark[20] };
    NormalizedLandmark[] leftArm = { poseLandmarks.Landmark[11],
                                     poseLandmarks.Landmark[13],
                                     poseLandmarks.Landmark[19] };

    if(leftArm[0].X > rightArm[0].X)
    {
      (leftArm, rightArm) = (rightArm, leftArm);
    }
    currentLeftState = determineArmState(leftArm);
    leftTarget = getTarget(currentLeftState);
    leftTarget.y = -centerOffset;
    currentRightState = determineArmState(rightArm);
    rightTarget = getTarget(currentRightState);
    rightTarget.y = centerOffset;

  }
  private State determineArmState(NormalizedLandmark[] arm)
  {
    if (arm[2].Y >= arm[0].Y)
    {
      return State.REST;
    } else
    {
      Vector3 elbowShoulder = vectorBetween(arm[1], arm[0]);
      Vector3 elbowFinger = vectorBetween(arm[1], arm[2]);
      float angle = Vector3.Angle(elbowShoulder, elbowFinger);
      if(angle > 100)
      {
        return State.EXTENDED;
      }
      return State.GUARD;
    }
  }
  private Vector3 vectorBetween(NormalizedLandmark from, NormalizedLandmark to)
  {
    return new Vector3(from.X - to.X, from.Y - to.Y, 0);
  }
  private Vector3 getTarget(State state)
  {
    switch (state)
    {
      case State.REST:
        //Debug.Log("State rest");
        return restPosition;
      case State.GUARD:
        //Debug.Log("State guard");
        return guardPosition;
      case State.EXTENDED:
        //Debug.Log("State extended");
        return extendPosition;
    }
    return restPosition;
  }
}
