using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour, IPoolable<Bullet>
{
    [SerializeField, Tooltip("When the bullet is closer than this distance to target it despawns.")]
    private float _arrivalThreshold = 0.05f;
    [SerializeField, Tooltip("Delay before the bullet despawns after reaching its target.")]
    private float _despawnDelay = 0.1f;
    private IObjectPool<Bullet> _pool;
    private Vector3 _targetPos;
    private float _speed;
    private bool _hasTarget;
    private bool _isDespawning;
    private float _despawnTimer;

    public void Initialize(Vector3 spawnPosition, Vector3 targetPosition, float speed)
    {
        transform.position = spawnPosition;
        _targetPos = targetPosition;
        _speed = speed;
        _hasTarget = true;
        _isDespawning = false;
        _despawnTimer = 0f;
    }

    public void OnReturnedToPool()
    {
        _hasTarget = false;
        _speed = 0f;
        _isDespawning = false;
        _despawnTimer = 0f;
        gameObject.SetActive(false);
    }

    public void OnTakenFromPool()
    {
        gameObject.SetActive(true);
    }
    
    public void Despawn()
    {
        if (_pool == null)
        {
            Debug.LogError($"{name} was despawned before SetPool was called.", this);
            gameObject.SetActive(false);
            return;
        }
        _pool.Release(this);
    }

    public void SetPool(IObjectPool<Bullet> pool)
    {
        _pool = pool;
    }

    void Update()
    {
        if (_isDespawning)
        {
            _despawnTimer -= Time.deltaTime;
            if (_despawnTimer <= 0f)
            {
                _isDespawning = false;
                Despawn();
            }
            return;
        }

        if (!_hasTarget)
        {
            return;
        }

        var threshold = Mathf.Max(0f, _arrivalThreshold);
        var sqrThreshold = threshold * threshold;

        var currentPosition = transform.position;
        if (Vector3.SqrMagnitude(_targetPos - currentPosition) <= sqrThreshold)
        {
            BeginDespawnCountdown();
            return;
        }

        var step = _speed * Time.deltaTime;
        if (step <= 0f)
        {
            transform.position = _targetPos;
            BeginDespawnCountdown();
            return;
        }

        var nextPosition = Vector3.MoveTowards(currentPosition, _targetPos, step);
        transform.position = nextPosition;

        if (Vector3.SqrMagnitude(_targetPos - nextPosition) <= sqrThreshold)
        {
            BeginDespawnCountdown();
        }
    }

    private void BeginDespawnCountdown()
    {
        _hasTarget = false;
        _isDespawning = true;
        _despawnTimer = Mathf.Max(0f, _despawnDelay);
    }
}
