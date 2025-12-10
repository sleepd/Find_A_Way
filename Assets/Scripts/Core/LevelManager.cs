using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    public PlayerController Player { get; private set; }
    private Coroutine _bindRoutine;

    void OnEnable()
    {
        TryBindPlayer();
    }

    void OnDisable()
    {
        UnbindPlayer();
    }

    public override void Awake()
    {
        base.Awake();
        TryBindPlayer();
    }

    private void TryBindPlayer()
    {
        if (Player != null)
        {
            return;
        }

        // Try immediate bind.
        Player = FindAnyObjectByType<PlayerController>();
        if (Player != null && Player.Health != null)
        {
            BindPlayerEvents();
            return;
        }

        // Fallback to coroutine in case player is spawned later.
        if (_bindRoutine == null)
        {
            _bindRoutine = StartCoroutine(BindPlayerWhenReady());
        }
    }

    private void UnbindPlayer()
    {
        if (Player != null && Player.Health != null)
        {
            Player.Health.Died -= HandlePlayerDied;
        }
        if (_bindRoutine != null)
        {
            StopCoroutine(_bindRoutine);
            _bindRoutine = null;
        }
        Player = null;
    }

    private void HandlePlayerDied()
    {
        Debug.Log("[LevelManager] Player died, reloading scene.");
        var scene = SceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            SceneManager.LoadScene(scene.buildIndex);
        }
    }

    private System.Collections.IEnumerator BindPlayerWhenReady()
    {
        while (Player == null || Player.Health == null)
        {
            Player = FindAnyObjectByType<PlayerController>();
            if (Player != null && Player.Health != null)
            {
                BindPlayerEvents();
                _bindRoutine = null;
                yield break;
            }
            yield return null;
        }
        _bindRoutine = null;
    }

    private void BindPlayerEvents()
    {
        if (Player == null || Player.Health == null)
        {
            return;
        }

        Player.Health.Died += HandlePlayerDied;
    }

    /// <summary>
    /// Called by PlayerController once it is initialized to guarantee binding.
    /// </summary>
    public void RegisterPlayer(PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        UnbindPlayer();
        Player = player;
        BindPlayerEvents();
    }
}
