using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestTaskManerai.Effects
{
    public class PunchEffectController : MonoBehaviour
    {
        [SerializeField] SpriteRenderer effectSprite;
        [SerializeField] private float lifetime = 2;
        [SerializeField] private float TargetScaleOverLifetime = 1;

        private void OnValidate()
        {
            if(effectSprite == null)
                effectSprite = GetComponent<SpriteRenderer>();

            if (effectSprite == null)
                Debug.LogError("Не был найден SpriteRenderer!", transform);
        }

        public void InitializeEffect(Color? basicColor = null, float targetLifetime = 2, float targetScale = 1)
        {
            if(basicColor == null)
                basicColor = Color.white;

            lifetime = targetLifetime;
            effectSprite.color = basicColor.Value;
            TargetScaleOverLifetime = targetScale;
        }

        private void Start()
        {
            StartCoroutine(EffectProcess());
        }

        private IEnumerator EffectProcess() 
        {
            float remainingLifetime = lifetime;
            float startScaleValue = transform.localScale.x;

            while (remainingLifetime > 0)
            {
                var time = 1 - remainingLifetime / lifetime;

                Color baseColor = effectSprite.color;
                baseColor.a = Mathf.Lerp(1, 0, time);
                effectSprite.color = baseColor;

                float scaleValue = Mathf.Lerp(startScaleValue,
                    TargetScaleOverLifetime,
                    time);
                transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);

                yield return new WaitForEndOfFrame();
                remainingLifetime -= Time.deltaTime;
            }

            Destroy(gameObject);
        }
    }
}