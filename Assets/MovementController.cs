using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mediapipe;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private int bufferSize = 30;
    private float velocityThreshold = 1.5f;
    public float movementSpeed = 1f;
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
    float prevLeftHandPos = -1;
    float prevRightHandPos = -1;
    float prevLeftShoulderPos = -1;
    float prevRightShoulderPos = -1;
    private CharacterController characterController;
    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        leftVelocityBuffer = new FixedSizedQueue(bufferSize);
        rightVelocityBuffer = new FixedSizedQueue(bufferSize);
        leftShoulderBuffer = new FixedSizedQueue(bufferSize);
        rightShoulderBuffer = new FixedSizedQueue(bufferSize);
    }
    private void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        if (!leftVelocityBuffer.occluded && !rightVelocityBuffer.occluded)
        {
            bool leftOk = leftVelocityBuffer.avgValue > velocityThreshold && leftShoulderBuffer.avgValue < 0.5;
            bool rightOK = rightVelocityBuffer.avgValue > velocityThreshold && rightShoulderBuffer.avgValue < 0.5;
            if (leftOk && rightOK)
            {
                characterController.SimpleMove(forward * leftVelocityBuffer.avgValue * 2);
            }
        }
    }
    public void setState(NormalizedLandmarkList poseLandmarks)
    {
        NormalizedLandmark leftHand = poseLandmarks.Landmark[19];
        NormalizedLandmark rightHand = poseLandmarks.Landmark[20];
        NormalizedLandmark leftShoulder = poseLandmarks.Landmark[11];
        NormalizedLandmark rightShoulder = poseLandmarks.Landmark[12];

        if (leftShoulder.X < rightShoulder.X)
        {
            (leftShoulder, rightShoulder) = (rightShoulder, leftShoulder);
            (leftHand, rightHand) = (rightHand, leftHand);
        }
        updateVelocity(ref prevLeftHandPos, leftHand, leftVelocityBuffer);
        updateVelocity(ref prevLeftShoulderPos, leftShoulder, leftShoulderBuffer);
        updateVelocity(ref prevRightHandPos, rightHand, rightVelocityBuffer);
        updateVelocity(ref prevRightShoulderPos, rightShoulder, rightShoulderBuffer);
    }
    private void updateVelocity(ref float prevPos, NormalizedLandmark landmark, FixedSizedQueue buffer)
    {
        if (landmark.Visibility > 0.6)
        {
            buffer.occluded = false;
            float pos = landmark.Y;
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
