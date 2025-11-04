using System;
using DG.Tweening;
using UnityEngine;

namespace WheelOfFortune.Game.UI
{
    [Serializable]
    public class FlyingIconAnimationConfig
    {
        [Header("Scatter Settings")]
        [SerializeField, Min(1)] private int iconCount = 10;
        [SerializeField, Range(10f, 200f)] private float scatterRadius = 90f;
        [SerializeField, Range(0.1f, 1f)] private float scatterDuration = 0.25f;

        [Header("Fly Settings")]
        [SerializeField, Range(0f, 0.5f)] private float gatherDelay = 0.15f;
        [SerializeField, Range(0.2f, 1f)] private float flyDuration = 0.45f;

        public int IconCount => iconCount;
        public float ScatterRadius => scatterRadius;
        public float ScatterDuration => scatterDuration;
        public float GatherDelay => gatherDelay;
        public float FlyDuration => flyDuration;
    }

    public class FlyingIconAnimation : MonoBehaviour
    {
        [Header("Config (Auto-assigned if not set)")]
        [SerializeField] private FlyingIconAnimationConfig config;

        private FlyingIconView _view;
        private Sequence _sequence;

        private static FlyingIconAnimation _instance;
        public static FlyingIconAnimationConfig Config => _instance?.config;

        #region UNITY LIFECYCLE

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
        }

        private void OnDisable()
        {
            Kill();
        }

        private void OnDestroy()
        {
            Kill();
        }

        #endregion

        #region DEPENDENCY INJECTION
        public void InjectDependency(FlyingIconView view)
        {
            _view = view;
        }
        #endregion

        #region SCATTER PHASE
        public void Scatter(Vector3 fromScreenPos)
        {
            if (_view == null || config == null)
            {
                Debug.LogWarning($"{nameof(FlyingIconAnimation)}: Missing view or config reference.", this);
                return;
            }

            Kill();

            Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized *
                             UnityEngine.Random.Range(config.ScatterRadius * 0.6f, config.ScatterRadius);

            Vector3 scatterPos = fromScreenPos + (Vector3)offset;

            _sequence = DOTween.Sequence()
                .Append(_view.RectTransform.DOMove(scatterPos, config.ScatterDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true));
        }
        #endregion

        #region FLY PHASE
        public void FlyTo(Vector3 targetPos, Action onComplete)
        {
            if (_view == null || config == null)
            {
                Debug.LogWarning($"{nameof(FlyingIconAnimation)}: Missing view or config reference.", this);
                return;
            }

            Kill();

            float randomDelay = UnityEngine.Random.Range(0f, config.GatherDelay);

            _sequence = DOTween.Sequence()
                .AppendInterval(randomDelay)
                .Append(_view.RectTransform.DOMove(targetPos, config.FlyDuration)
                    .SetEase(Ease.InOutQuad)
                    .SetUpdate(true))
                .Join(_view.RectTransform.DOScale(0.5f, 0.25f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetUpdate(true))
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                    _sequence = null;
                });
        }

        #endregion

        #region CLEANUP
        public void Kill()
        {
            _sequence?.Kill();
            _sequence = null;
        }
        #endregion
    }
}
