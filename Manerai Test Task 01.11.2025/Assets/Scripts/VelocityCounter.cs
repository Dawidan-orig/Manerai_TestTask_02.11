using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestTaskManerai.Utilitary
{
    public class VelocityCounter : MonoBehaviour
    {
        Vector3 frameVelocity = Vector3.zero;
        Vector3 lastPos = Vector3.zero;

        public Vector3 FrameVelocity { get => frameVelocity; }

        private void Update()
        {
            frameVelocity = (transform.position - lastPos) / Time.deltaTime;

            lastPos = transform.position;
        }
    }
}
