using System.Collections;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera _mainCam = null;

    [Header("Movement")]
    [SerializeField] private float _movementSpeed = 10f;
    [SerializeField] private float _sprintMultiplier = 1.5f;
    private float _nextFootstepTimer = 0f;
    private Rigidbody _rigidbody = null;
    private Vector2 _moveDirection = Vector2.zero;

    [Header("Audio")]
    [SerializeField] private AudioClip[] _footstepClips = null;
    [SerializeField] private AudioClip[] _runningClips = null;
    [SerializeField] private AudioClip[] _weaponClips = null;

    [Header("Weapon")]
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private GameObject _weapon = null;
    [SerializeField] private float _fireDistance = 120f;
    [SerializeField] private float _weaponDamage = 10f;
    [SerializeField] private float _weaponCooldown = 3f;
    private bool _canFire = true;
    private Vector3 _weaponRotationOffset = Vector3.zero;    

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
        _weaponRotationOffset = _weapon.transform.localEulerAngles;
    }

    private void Update() {
        _moveDirection = InputManager.MoveDirection;        

        if (InputManager.IsFireHeld && _canFire) {
            Fire();
        }
    }

    private IEnumerator WeaponCooldown(float cooldown) {
        _canFire = false;
        yield return new WaitForSeconds(cooldown);
        _canFire = true;
    }

    private void FixedUpdate() {
        if (_moveDirection != Vector2.zero) {
            MovePlayer();
        }
    }

    private void LateUpdate() {
        RotatePlayer();
    }

    private void MovePlayer() {
        float currentSpeed = InputManager.IsSprintHeld ? _movementSpeed * _sprintMultiplier : _movementSpeed;
        Vector3 forceDirection = _moveDirection.x * transform.right + _moveDirection.y * transform.forward;
        
        _rigidbody.AddForce(currentSpeed * forceDirection, ForceMode.Force);

        if (_nextFootstepTimer <= Time.time) {
            AudioClip footstepClip = null;

            if (InputManager.IsSprintHeld) {
                footstepClip = _runningClips[Random.Range(0, _runningClips.Length)];
                //footstep clips are too long and I want them to play quicker
                _nextFootstepTimer = Time.time + (footstepClip.length / 4f);
            }
            else {
                footstepClip = _footstepClips[Random.Range(0, _footstepClips.Length)];
                _nextFootstepTimer = Time.time + footstepClip.length;
            }            

            AudioManager.Instance.PlayPlayerEffect(footstepClip, AudioType.Footsteps);
        }
    }

    private void RotatePlayer() {
        transform.rotation = Quaternion.Euler(0f, _mainCam.transform.eulerAngles.y, 0f);
        _weapon.transform.rotation = Quaternion.Euler(_mainCam.transform.eulerAngles.x + _weaponRotationOffset.x, _mainCam.transform.eulerAngles.y, _mainCam.transform.eulerAngles.z);
    }

    private void Fire() {
        StartCoroutine(WeaponCooldown(_weaponCooldown));

        AudioClip weaponClip = _weaponClips[Random.Range(0,_weaponClips.Length)];
        if (weaponClip != null) {
            AudioManager.Instance.PlayPlayerEffect(weaponClip, AudioType.Weapon);
        }

        RaycastHit hitInfo;
        if (Physics.Raycast(_mainCam.transform.position, _mainCam.transform.forward, out hitInfo, _fireDistance, _enemyLayer)) {
            IDamageable target = hitInfo.collider.GetComponent<IDamageable>();
            target?.TakeDamage(_weaponDamage);
        }
    }
}
