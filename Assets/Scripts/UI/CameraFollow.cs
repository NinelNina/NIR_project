using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    [SerializeField] private Vector3 _offset;

    [SerializeField] private float _size = 6f;

    private void Awake()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }
    }

    private void Start()
    {
        if (_camera != null)
        {
            _camera.orthographicSize = _size;
        }
    }

    private void Update()
    {
        _camera.transform.position = transform.position + _offset;
    }
}
