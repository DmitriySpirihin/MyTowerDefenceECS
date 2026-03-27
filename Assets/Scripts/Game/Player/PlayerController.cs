using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Entities;
using Unity.Transforms;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _tiltSpeed = 2f;
    [SerializeField] private float _tiltThreshold = 15f;
    [SerializeField] private float _minMagnitude = 0.1f;

    private bool _canMove;

    private IServiceLocator _serviceLocator;
    private IInputService _inputService;
    private EntityManager _entityManager;
    private EntityQuery _playerQuery;
    private EntityQuery _stateQuery;

    void Start()
    {
        _serviceLocator = SceneManager.GetActiveScene().GetRootGameObjects()[0].GetComponent<IServiceLocator>();
        _inputService = _serviceLocator?.GetService<IInputService>();
        // Getting the entity to sync fire
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _playerQuery = _entityManager.CreateEntityQuery(typeof(CPlayerTag));
        _stateQuery = _entityManager.CreateEntityQuery(typeof(CGlobalPlayerState));
  
        // subscriptions
        if(_inputService == null) return;
        _inputService.OnButtonPressed += Shoot;
    }

    private void Update()
    {
        if(_inputService == null) return;
        Move(_inputService.GetPosition().x);

        // Change ecs componets data
        if (_playerQuery.HasSingleton<CPlayerTag>())
        {
            Entity playerEntity = _playerQuery.GetSingletonEntity();
            _entityManager.SetComponentData(playerEntity, LocalTransform.FromPositionRotation(transform.position, transform.rotation));
        }

        if (_stateQuery.HasSingleton<CGlobalPlayerState>())
        {
            var state = _stateQuery.GetSingleton<CGlobalPlayerState>();
            state.IsFiring = _canMove;// && _energyService.HasEnergy;
            _stateQuery.SetSingleton(state);
        }
    }

    private void Move(float posX)
    {
        // Calculating the desired position
        Vector3 desiredPosition = new Vector3(posX, transform.position.y, transform.position.z);
        // Handling moving
        transform.position = Vector3.Lerp(transform.position, desiredPosition, _canMove ? _speed * Time.deltaTime : 0f);
        
        // Checking the distance between actual and desired positions
        float magnitude = Vector3.SqrMagnitude(desiredPosition - transform.position);
        // Calculating needed tilt
        float currentTilt = magnitude < _minMagnitude ? 0f : posX < transform.position.x ? _tiltThreshold : -_tiltThreshold;
        // Calculating new rotation
        Quaternion newRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, _canMove ? currentTilt : 0f);
        // Handling rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, _tiltSpeed * Time.deltaTime);
    }

    private void Shoot(bool needShoot)
    {
        _canMove = needShoot;
    }

    void OnDestroy()
    {
        if (_inputService != null)
        {
            _inputService.OnButtonPressed -= Shoot;
        }
    }
}
