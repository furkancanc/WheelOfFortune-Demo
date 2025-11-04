using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace WheelOfFortune.Game.Managers
{
    public interface IGameStateProvider
    {
        bool IsSafeZone { get; }
        bool IsSuperZone { get; }
        bool IsWheelSpinning { get; }
    }


    public class GameStateProvider : MonoBehaviour, IGameStateProvider
    {
        [Header("Dependencies (Auto-assigned)")]
        [SerializeField] private WheelManager wheelManager;
        [SerializeField] private ZoneManager zoneManager;

        #region UNITY LIFECYCLE
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (wheelManager == null)
                wheelManager = FindObjectOfType<WheelManager>(true);

            if (zoneManager == null)
                zoneManager = FindObjectOfType<ZoneManager>(true);

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        private void Awake()
        {
            ValidateDependencies();
        }

#endregion

        #region DEPENDENCY CHECKS

        private void ValidateDependencies()
        {
            if (wheelManager == null)
                Debug.LogWarning($"{nameof(GameStateProvider)}: WheelManager reference missing.", this);

            if (zoneManager == null)
                Debug.LogWarning($"{nameof(GameStateProvider)}: ZoneManager reference missing.", this);
        }

        #endregion

        #region PROPERTIES
        public bool IsWheelSpinning => wheelManager != null && wheelManager.IsSpinning;
        public bool IsSafeZone => zoneManager != null && (zoneManager.CurrentRound % 5 == 0);
        public bool IsSuperZone => zoneManager != null && (zoneManager.CurrentRound % 30 == 0);
        #endregion
    }
}

