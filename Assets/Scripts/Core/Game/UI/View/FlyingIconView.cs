using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Infrastructure.Pooling;

namespace WheelOfFortune.Game.UI
{
    [DisallowMultipleComponent]
    public class FlyingIconView : MonoBehaviour
    {
        [Header("References (Auto-assigned)")]
        [SerializeField] private Image iconImage;
        [SerializeField] private FlyingIconAnimation iconAnimation;

        private RectTransform _rectTransform;
        private PoolManager<FlyingIconView> _pool;
        

        public RectTransform RectTransform => _rectTransform ??= GetComponent<RectTransform>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!iconImage)
                iconImage = GetComponentInChildren<Image>(true);
            if (!iconAnimation)
                iconAnimation = GetComponentInChildren<FlyingIconAnimation>();
        }
#endif

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (!iconImage)
                iconImage = GetComponent<Image>();
        }

        private void OnDisable()
        {
            iconAnimation?.Kill();
        }

        public void Init(PoolManager<FlyingIconView> pool) => _pool = pool;

        public void Setup(Sprite icon, Vector3 startPos)
        {
            iconImage.sprite = icon;
            RectTransform.position = startPos;
            RectTransform.localScale = Vector3.one;
            gameObject.SetActive(true);
        }

        public void PlayScatter(Vector3 scatterPos)
        {
            iconAnimation.InjectDependency(this);
            iconAnimation.Scatter(scatterPos);
        }

        public void PlayFlyTo(Vector3 targetPos, Action onComplete = null)
        {
            iconAnimation.InjectDependency(this);
            iconAnimation.FlyTo(targetPos, () =>
            {
                onComplete?.Invoke();
                ReturnToPool();
            });
        }

        public void ReturnToPool()
        {
            iconAnimation.Kill();
            _pool?.Return(this);
        }
    }
}
