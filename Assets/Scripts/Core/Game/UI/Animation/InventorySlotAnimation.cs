using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WheelOfFortune.Game.UI;


[DisallowMultipleComponent]
public class InventorySlotAnimation :  MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField, Min(0.05f), Tooltip("Duration for the count increment tween.")]
    private float countDuration = 0.4f;

    [SerializeField, Min(0.05f), Tooltip("Duration for the pulse scale animation.")]
    private float pulseDuration = 0.15f;

    [SerializeField, Range(1f, 2f), Tooltip("Target scale factor during pulse.")]
    private float pulseScale = 1.15f;

    [SerializeField, Tooltip("Ease type for count tween.")]
    private Ease countEase = Ease.OutQuad;

    [SerializeField, Tooltip("Ease type for pulse tween.")]
    private Ease pulseEase = Ease.OutBack;

    private InventorySlot _slot;
    private Tween _pulseTween;
    private Tween _countTween;

    #region UNITY LIFECYCLE
    private void OnDisable()
    {
        KillAllTweens();
    }

    private void OnDestroy()
    {
        KillAllTweens();
    }

    #endregion

    #region DEPENDENCY INJECTION
    public void InjectDependency(InventorySlot slot)
    {
        _slot = slot;
    }
    #endregion

    #region COUNT ANIMATION
    public void PlayCountIncrement(TextMeshProUGUI text, int addAmount)
    {
        if (_slot == null || text == null)
            return;

        int startValue = _slot.Count;
        int targetValue = startValue + addAmount;
        int current = startValue;

        _countTween?.Kill();

        _countTween = DOTween
            .To(() => current, x =>
            {
                current = x;
                text.SetText(x.ToString());
            },
            targetValue,
            countDuration)
            .SetEase(countEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _pulseTween?.Kill();
                Pulse();
                _countTween = null;
            });
    }
    #endregion

    #region PULSE ANIMATION
    public void Pulse()
    {
        if (_slot == null)
            return;

        _pulseTween?.Kill();

        Transform slotTransform = _slot.transform;

        _pulseTween = slotTransform
            .DOScale(pulseScale, pulseDuration)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(pulseEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (slotTransform != null)
                    slotTransform.localScale = Vector3.one;

                _pulseTween = null;
            });
    }
    #endregion

    #region CLEANUP
    public void KillAllTweens()
    {
        _pulseTween?.Kill();
        _countTween?.Kill();
        _pulseTween = null;
        _countTween = null;
    }
    #endregion
}
