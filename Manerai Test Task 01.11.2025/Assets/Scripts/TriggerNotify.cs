using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestTaskManerai.Utilitary
{
    public class TriggerNotify : MonoBehaviour
    {
        [SerializeField] Collider triggerUsed;
        public Action<Collider> onEnter;
        public Action<Collider> onExit;

        private void OnValidate()
        {
            if (triggerUsed == null)
                triggerUsed = GetComponent<Collider>();

            if (triggerUsed == null)
            {
                Debug.Log("Не был найден коллайдер!", transform);
                return;
            }

            triggerUsed.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            onEnter?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            onExit?.Invoke(other);
        }
    }
}