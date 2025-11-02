using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TestTaskManerai.Effects;
using TestTaskManerai.Utilitary;
using UnityEngine;

namespace TestTaskManerai.Head
{
    /// <summary>
    /// Основной контроллер.
    /// Да, God-object. Осознаю это. Но действую по принципу "Время - деньги".
    /// В данном случае, думаю, нет смысла сильно усложнять системы, тут объективно одного скрипта хватит.
    /// Тем не менее, тут три составляющих системы, их вполне возможно (И даже нужно) разграничить:
    /// - Реакция на базовый удар
    /// - Апперкот
    /// - Румянец
    /// Они небольшие, потому пока можно в один скрипт.
    /// </summary>
    public class HeadController : MonoBehaviour
    {
        [Header("Relations")]
        [SerializeField] SpringJoint spring;
        [SerializeField] Collider torso;
        [SerializeField] Rigidbody rb;

        #region Punch reaction
        [Header("Regular punch")]
        [SerializeField] PunchEffectController punchControllerEffectPrefab;
        [Tooltip("Значение <b>скорости<\b> руки, при которой эффект случается")]
        [SerializeField] private float punchValue = 2;
        [SerializeField] private GameObject regularPunchSound;
        [Tooltip("Значение <b>скорости<\b> руки, при которой эффект случается")]
        [SerializeField] private float strongPunchValue = 10;
        [SerializeField] private float strongPunchVelocityMultiplier = 1.5f;
        [SerializeField] private GameObject strongPunchSound;
        #endregion

        #region Uppercut
        [Header("Uppercut")]
        [SerializeField] float uppercutAllowedAngle = 30;
        [SerializeField] float uppercutPowerMultiply = 2;
        [Range(0f, 1f)]
        [SerializeField] float uppercutSpringSoftenValue = 0.7f;
        private bool hasBeenUppercut = false;
        #endregion

        #region blush
        [Header("Blush")]
        [SerializeField] float blushAppearTimer = 1;
        [SerializeField] TriggerNotify blushAppearCollider;
        [SerializeField] List<SpriteRenderer> blushes = new List<SpriteRenderer>();
        private Coroutine blushChecking;
        private Coroutine blushControl;
        private bool isBlushed = false;
        #endregion

        #region Unity
        private void OnEnable()
        {
            blushAppearCollider.onEnter += BlushColliderEnter;
            blushAppearCollider.onExit += BlushColliderExit;
        }
        private void OnDisable()
        {
            blushAppearCollider.onEnter -= BlushColliderEnter;
            blushAppearCollider.onExit -= BlushColliderExit;
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider == torso)
                return;

            HandlePunch(collision);

            RestoreAfterUppercut();
            HandleUppercut(collision);
        }
        #endregion

        #region Punches

        private void HandlePunch(Collision c)
        {
            float otherSpeed = c.transform.GetComponent<VelocityCounter>().FrameVelocity.magnitude;

            Debug.Log("Punch speed : " + otherSpeed);

            if (otherSpeed > punchValue && otherSpeed <= strongPunchValue)
            {
                PunchEffectController pec = CreateEffect(c);
                pec.InitializeEffect(Color.white, 2, 0.1f);
                Instantiate(regularPunchSound, pec.transform);                
            }
            else if(otherSpeed > strongPunchValue)
            {
                PunchEffectController pec = CreateEffect(c);
                pec.InitializeEffect(Color.red, 1, 0.5f);
                rb.velocity *= strongPunchVelocityMultiplier;
                Instantiate(strongPunchSound, pec.transform);
                Debug.Log("Strong Punch!");
            }
        }

        private PunchEffectController CreateEffect(Collision from)
        {
            GameObject punchEffectGO = Instantiate(punchControllerEffectPrefab.gameObject);
            ContactPoint contact = from.GetContact(0);
            punchEffectGO.transform.position = contact.point;
            punchEffectGO.transform.LookAt(contact.point + contact.normal);
            return punchEffectGO.GetComponent<PunchEffectController>();
        }
        #endregion

        #region Uppercut Control
        private void HandleUppercut(Collision c)
        {
            float angleUp = Vector3.Angle(c.GetContact(0).normal, Vector3.up);

            if (angleUp < uppercutAllowedAngle)
            {
                //TODO - Звук сильного удара, наложение эхо
                rb.velocity *= uppercutPowerMultiply;
                spring.spring *= uppercutSpringSoftenValue;
                hasBeenUppercut = true;
            }
        }

        private void RestoreAfterUppercut()
        {
            if (!hasBeenUppercut)
                return;

            spring.spring /= uppercutSpringSoftenValue;
            hasBeenUppercut = false;
        }
        #endregion

        #region Blush
        private void BlushColliderEnter(Collider c)
        {
            blushChecking = StartCoroutine(CheckBlush());
        }

        private IEnumerator CheckBlush()
        {
            yield return new WaitForSeconds(blushAppearTimer);
            ActivateBlush();
        }
        private void BlushColliderExit(Collider c)
        {
            StopCoroutine(blushChecking);
            DeactivateBlush();
        }
        private void ActivateBlush()
        {
            isBlushed = true;
            if (blushControl != null) //Перезапуск нужен, чтобы восстановить с начального состояния проверки
                StopCoroutine(blushControl);

            blushControl = StartCoroutine(ControlBlushes());
        }
        private IEnumerator ControlBlushes()
        {
            //Не буду заморачиваться со скалированием и скоростями, чтобы не усложнять

            while (isBlushed)
            {
                foreach (SpriteRenderer sr in blushes)
                {
                    Color initial = sr.color;
                    if (initial.a <= 0.95f)
                        initial.a += Time.deltaTime;
                    else
                        initial.a = 1;
                    sr.color = initial;
                }

                yield return new WaitForEndOfFrame();
            }

            float initialAlphaFromSample = blushes.Max(sr => sr.color.a);
            while (initialAlphaFromSample > 0.02f)
            {
                foreach (SpriteRenderer sr in blushes)
                {
                    Color initial = sr.color;
                    if (initial.a >= 0.05f) //Могут быть неравномерными (Потенциально)
                        initial.a -= Time.deltaTime;
                    else
                        initial.a = 0;

                    sr.color = initial;
                }

                initialAlphaFromSample = blushes[0].color.a;
                yield return new WaitForEndOfFrame();
            }
        }
        private void DeactivateBlush()
        {
            isBlushed = false;
        }
        #endregion
    }
}