using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WheelOfFortune.Infrastructure.Events;
using static WheelOfFortune.Infrastructure.Events.EventBus;

namespace WheelOfFortune.Game.Managers
{
    public class GameManager : MonoBehaviour
    {
        private bool _isReloading;

        #region UNITY LIFECYCLE

        private void OnEnable()
        {
            Subscribe<CollectRewardsButtonClickedEvent>(OnCollectRewardsButtonClicked);
            Subscribe<GiveUpButtonClicked>(OnGiveUpClicked);
        }

        private void OnDisable()
        {
            Unsubscribe<CollectRewardsButtonClickedEvent>(OnCollectRewardsButtonClicked);
            Unsubscribe<GiveUpButtonClicked>(OnGiveUpClicked);
        }

        #endregion

        #region EVENT HANDLERS

        private void OnCollectRewardsButtonClicked(CollectRewardsButtonClickedEvent e)
        {
            ResetGame();
        }

        private void OnGiveUpClicked(GiveUpButtonClicked e)
        {
            ResetGame();
        }

        #endregion

        #region GAME RESET LOGIC


        private void ResetGame()
        {
            if (_isReloading)
                return;

            _isReloading = true;

            SceneManager.sceneLoaded += OnSceneReloaded;

            Scene activeScene = SceneManager.GetActiveScene();
            string sceneName = activeScene.name;

#if UNITY_EDITOR
            Debug.Log($"[GameManager] Reloading scene: {sceneName}");
#endif

            DOTween.KillAll();
            DOTween.Clear();

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        private void OnSceneReloaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneReloaded;

            DOTween.KillAll();
            DOTween.Clear();

            _isReloading = false;
        }

        #endregion
    }
}
