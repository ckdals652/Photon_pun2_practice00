using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class PlayerCameraAim : MonoBehaviourPun
{
    private PlayerAction input;

    public Transform target;
    public CinemachineVirtualCamera virtualCamera;
    public Vector3 offset = new Vector3(0, 1, -6);
    public float rotationSpeed = 30f;

    [SerializeField] private float yaw = 0f;
    [SerializeField] private float pitch = 15f;

    public float minPitch = -80f;
    public float maxPitch = 80f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void LateUpdate()
    {
        //if (target == null) return;
        if (!photonView.IsMine) return;

        MouseAim();
    }

    private void MouseAim()
    {
        // Input System 기반 마우스 입력 받기
        Vector2 mouseDelta = input.PlayerActionMap.MouseAim.ReadValue<Vector2>();

        yaw += mouseDelta.x * rotationSpeed * Time.deltaTime;
        pitch -= mouseDelta.y * rotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        virtualCamera.transform.position = desiredPosition;
        virtualCamera.transform.LookAt(target);
    }

    public void SetInput(PlayerAction a)
    {
        input = a;
    }
}