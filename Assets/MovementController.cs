using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mediapipe;
using RootMotion.FinalIK;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private int bufferSize = 10;
    private float velocityThreshold = 1.5f;
    public float movementSpeed = 1f;
    [SerializeField] private Animator anim;
    [SerializeField] private VRIK ik;

    public class FixedSizedQueue
    {
        public float avgValue { get; set; }
        ConcurrentQueue<float> q = new ConcurrentQueue<float>();
        public bool occluded {get; set;}
        public int Limit { get; set; }
        public int Count() {
            return q.Count;
        }

        public FixedSizedQueue(int limit)
        {
            Limit = limit;
        }

        public void Enqueue(float obj)
        {
            q.Enqueue(obj);
            avgValue += (obj / Limit);
            if (q.Count > Limit)
            {
                float overflow;
                q.TryDequeue(out overflow);
                avgValue -= overflow / Limit;
            }
            
        }
    }
    FixedSizedQueue leftVelocityBuffer;
    FixedSizedQueue leftShoulderBuffer;
    FixedSizedQueue rightShoulderBuffer;
    FixedSizedQueue rightVelocityBuffer;
    FixedSizedQueue headVelocityBuffer;
    

    float prevLeftHandPos = -1;
    float prevRightHandPos = -1;
    float prevLeftShoulderPos = -1;
    float prevRightShoulderPos = -1;
    float prevHeadPos = -1;

    float ikTargetWeight = 1f;
    float ikGradual = 1f;
    private CharacterController characterController;
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        leftVelocityBuffer = new FixedSizedQueue(bufferSize);
        rightVelocityBuffer = new FixedSizedQueue(bufferSize);
        leftShoulderBuffer = new FixedSizedQueue(bufferSize);
        rightShoulderBuffer = new FixedSizedQueue(bufferSize);
        headVelocityBuffer = new FixedSizedQueue(bufferSize);
    }
    private void Update()
    {
        ikTargetWeight = 1;
        anim.SetFloat("Speed", 0);
        Vector3 moveVelocity = new Vector3(0, 0, 0);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        if (!leftVelocityBuffer.occluded && !rightVelocityBuffer.occluded)
        {
            bool leftOk = leftVelocityBuffer.avgValue > velocityThreshold && leftShoulderBuffer.avgValue < 0.7;
            bool rightOK = rightVelocityBuffer.avgValue > velocityThreshold && rightShoulderBuffer.avgValue < 0.7;
            if (leftOk && rightOK)
            {
                anim.SetFloat("Speed", leftVelocityBuffer.avgValue*movementSpeed);
                moveVelocity = forward * leftVelocityBuffer.avgValue * movementSpeed;
                ikTargetWeight = 0;
            }
        }
        if (ikGradual != ikTargetWeight)
        {
            ikGradual += (ikTargetWeight - ikGradual)*Time.deltaTime;
        }
        ik.solver.SetIKPositionWeight(ikGradual);
        if(headVelocityBuffer.avgValue < 0.5)
        {
            if (prevHeadPos >= 0.75)
            {
                Vector3 left = -transform.TransformDirection(transform.right)*movementSpeed;
                moveVelocity += left;
            }
            else if(prevHeadPos <= 0.25)
            {
                Vector3 right = transform.TransformDirection(transform.right)*movementSpeed;
                moveVelocity += right;
            }
        } 
        characterController.SimpleMove(moveVelocity);

    }
    public void setState(NormalizedLandmarkList poseLandmarks)
    {
        NormalizedLandmark leftHand = poseLandmarks.Landmark[19];
        NormalizedLandmark rightHand = poseLandmarks.Landmark[20];
        NormalizedLandmark leftShoulder = poseLandmarks.Landmark[11];
        NormalizedLandmark rightShoulder = poseLandmarks.Landmark[12];
        NormalizedLandmark nose = poseLandmarks.Landmark[0];

        if (leftShoulder.X < rightShoulder.X)
        {
            (leftShoulder, rightShoulder) = (rightShoulder, leftShoulder);
            (leftHand, rightHand) = (rightHand, leftHand);
        }
        updateVelocity(ref prevLeftHandPos, leftHand, leftVelocityBuffer);
        updateVelocity(ref prevLeftShoulderPos, leftShoulder, leftShoulderBuffer);
        updateVelocity(ref prevRightHandPos, rightHand, rightVelocityBuffer);
        updateVelocity(ref prevRightShoulderPos, rightShoulder, rightShoulderBuffer);
        updateVelocity(ref prevHeadPos, nose, headVelocityBuffer, "X");

    }
    private void updateVelocity(ref float prevPos, NormalizedLandmark landmark, FixedSizedQueue buffer, String dir="Y")
    {
        if (landmark.Visibility > 0.6)
        {
            buffer.occluded = false;
            float pos;
            if(dir == "Y")
            {
                pos = landmark.Y;
            }
            else
            {
                pos = landmark.X;
            }
            if (prevPos == -1)
            {
                prevPos = pos;
            }
            else
            {
                float velocity = Math.Abs((pos - prevPos)) / Time.deltaTime;
                buffer.Enqueue(velocity);
                prevPos = pos;
            }
        }
        else
        {
            buffer.occluded = true;
        }
    }
}
