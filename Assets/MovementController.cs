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
    public class FixedSizedQueue
    {
        public float avgValue { get; set; }
        ConcurrentQueue<float> q = new ConcurrentQueue<float>();
        public bool occluded = false;
        public int Limit { get; set; }
        public int Count() {
            return q.Count;
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
    float previousLeftShoulderPos = -1;
    float previousRightShoulderPos = -1;
    private void Start()
    {

        leftVelocityBuffer = new FixedSizedQueue();
        rightVelocityBuffer = new FixedSizedQueue();
        leftVelocityBuffer.Limit = bufferSize;
        rightVelocityBuffer.Limit = bufferSize;
    }
    private void Update()
    {
        if(leftVelocityBuffer.avgValue > velocityThreshold && rightVelocityBuffer.avgValue > velocityThreshold)
        {
            
        }
    }
    public void setState(NormalizedLandmarkList poseLandmarks)
    {
        if(poseLandmarks.Landmark[19].Visibility > 0.6 && poseLandmarks.Landmark[20].Visibility > 0.6)
        {
            float leftPos = poseLandmarks.Landmark[11].Y - poseLandmarks.Landmark[19].Y;
            float rightPos = poseLandmarks.Landmark[12].Y - poseLandmarks.Landmark[20].Y;
            if (prevLeftHandPos == -1)
            {
                prevLeftHandPos = leftPos;
                prevRightHandPos = rightPos;
            }
            float leftVelocity = Math.Abs((leftPos - prevLeftHandPos)) / Time.deltaTime;
            float rightVelocity = Math.Abs((rightPos - prevRightHandPos)) / Time.deltaTime;
            leftVelocityBuffer.Enqueue(leftVelocity);
            rightVelocityBuffer.Enqueue(rightVelocity);
            prevLeftHandPos = leftPos;
            prevRightHandPos = rightPos;
        }

    }

}
