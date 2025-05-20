using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurretBehaivor : MonoBehaviour
{
    [Header("Set-up")]
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private GameObject projectile;
    [Header("Turret Settings")]
    [SerializeField] private float Projectilelifetime = 5f;
    [SerializeField] private float ShootStrenght = 50f;
    [SerializeField] private float TurnSpeed = 1f;
    [SerializeField] private float MaxRayDistance = 100f;
    [Header("Debug Settings")]
    [SerializeField] private Color MouseTargetColor;
    [SerializeField] private float MouseTargetSphereSize = 1f;
    [SerializeField] private Color TurretTargetColor;
    [SerializeField] private float TurretTargetSizeAddition = 1f;
    [SerializeField] private Color TurretShootDirectionColor;
    [SerializeField] private LayerMask LayerMaskToDisplayOnLook;
    private InputSystem_Actions _inputSystem;
    private GameObject _debugt;
    private Mouse _mouse;
    private Camera _camera;
    private Vector3 _mouseHitPosition = Vector3.zero;
    void OnMouse1Down(InputAction.CallbackContext callback)
    {
        GameObject cloned = Instantiate(projectile, shootOrigin.position, shootOrigin.rotation);
        if (cloned.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForce(cloned.transform.forward * ShootStrenght, ForceMode.VelocityChange);
            Destroy(cloned, Projectilelifetime);
        }
    }
    void Start()
    {
        _camera = Camera.main;
        _mouse = Mouse.current;
        _inputSystem = new();
        _inputSystem.Player.Attack.performed += OnMouse1Down;
        _inputSystem.Player.Attack.Enable();
    }
    void RotateTowardsMouse()
    {
        Vector3 mousepos = _mouse.position.ReadValue();
        Ray targetRay = _camera.ScreenPointToRay(mousepos);
        if (Physics.Raycast(targetRay, out RaycastHit hit, MaxRayDistance))
        {
            _mouseHitPosition = hit.point;
        }
        else
        {
            _mouseHitPosition = targetRay.origin + targetRay.direction * MaxRayDistance;
        }
        if (_mouseHitPosition != Vector3.zero)
        {
            Vector3 direction = Vector3.Normalize(_mouseHitPosition-transform.position);
            Quaternion targetQ = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetQ, Time.deltaTime * TurnSpeed);
        }
    }
    void GetNewTarget()
    {
        if (Physics.Raycast(shootOrigin.position, shootOrigin.forward, out RaycastHit hit, MaxRayDistance, LayerMaskToDisplayOnLook.value))
        {
            if (hit.collider)
            {
                _debugt = hit.collider.gameObject;
            }
        }
        else
        {
            _debugt = null;
        }
    }
    void Update()
    {
        RotateTowardsMouse();
        GetNewTarget();
    }
    void OnDrawGizmos()
    {
        Gizmos.color = TurretShootDirectionColor;
        Gizmos.DrawRay(new Ray(shootOrigin.position, shootOrigin.forward));
        Gizmos.color = MouseTargetColor;
        Gizmos.DrawSphere(_mouseHitPosition, MouseTargetSphereSize);
        if (_debugt != null && _debugt.TryGetComponent<MeshFilter>(out MeshFilter renderer))
        {
            Gizmos.color = TurretTargetColor;
            Gizmos.DrawMesh(renderer.mesh, _debugt.transform.position, _debugt.transform.rotation, _debugt.transform.lossyScale + new Vector3(TurretTargetSizeAddition, TurretTargetSizeAddition, TurretTargetSizeAddition));
        }
        Gizmos.color = Color.clear;
    }
    void OnDestroy()
    {
        _inputSystem.Player.Attack.performed -= OnMouse1Down;
        _inputSystem.Player.Attack.Disable();
        
    }
}
