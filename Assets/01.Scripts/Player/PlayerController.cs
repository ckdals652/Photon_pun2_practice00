using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerController : MonoBehaviourPun
{
    private Rigidbody playerRigidBody;
    private PlayerCameraAim playerCameraAim;
    private PlayerAction input;

    private float activeMoveSpeed;
    private float activeVerticalSpeed;
    private float activeMaxSpeed;
    [SerializeField] private float baseMoveSpeed = 10f;
    [SerializeField] private float baseVerticalSpeed = 10f;
    [SerializeField] private float baseMaxSpeed = 10f;

    [SerializeField] private float runCoefficient = 3f;

    [SerializeField] private float dashForce = 45f;
    private bool dashPressed = false;
    private Vector3 lastMoveDirection = Vector3.forward;

    [SerializeField] private CinemachineBrain mainCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    

    [SerializeField] private Transform firePoint;
    [SerializeField] private string firePointText = "FirePoint";
    [SerializeField] private string bulletText = "Bullet";
    Quaternion fireOffset = Quaternion.Euler(-20f, 0f, 0f);


    private void Awake()
    {
        if (IsPhotonViewIsMine())
        {
            playerRigidBody = GetComponent<Rigidbody>();
            input = new PlayerAction();
        }
    }

    private void OnEnable()
    {
        if (IsPhotonViewIsMine())
        {
            //이동 활성화
            input.Enable();

            input.PlayerActionMap.Run.started += OnRunStarted;
            input.PlayerActionMap.Run.canceled += OnRunCanceled;

            input.PlayerActionMap.Dash.started += OnDashStarted;

            input.PlayerActionMap.Attack.started += OnAttackStarted;
        }
    }

    private void OnDisable()
    {
        if (IsPhotonViewIsMine())
        {
            //이동 비활성화
            input.Disable();

            input.PlayerActionMap.Run.started -= OnRunStarted;
            input.PlayerActionMap.Run.canceled -= OnRunCanceled;

            input.PlayerActionMap.Dash.started -= OnDashStarted;

            input.PlayerActionMap.Attack.started -= OnAttackStarted;
        }
    }

    private void Start()
    {
        if (IsPhotonViewIsMine())
        {
            //카메라 찾아주기
            mainCamera = FindObjectOfType<CinemachineBrain>();
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            playerCameraAim = FindObjectOfType<PlayerCameraAim>();
            //virtualCamera.Follow = transform;
            //virtualCamera.LookAt = transform;
            playerCameraAim.SetInput(input);
            playerCameraAim.target = transform;
            playerCameraAim.virtualCamera = virtualCamera;

            firePoint = transform.Find(firePointText);

            activeMoveSpeed = baseMoveSpeed;
            activeVerticalSpeed = baseVerticalSpeed;
            activeMaxSpeed = baseMaxSpeed;
        }
    }


    private void FixedUpdate()
    {
        if (!IsPhotonViewIsMine()) return;

        Move();

        if (dashPressed)
        {
            Dash();
            dashPressed = false;
        }
    }

    private void Move()
    {
        // 1. 입력 받기
        Vector2 moveInput = input.PlayerActionMap.Move.ReadValue<Vector2>();

        // 2. 카메라 기준 방향 설정
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;

        // y축 방향 제거 (지면 기준 방향으로)
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 3. 카메라 기준 이동 방향 계산
        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;

        // 4. 수직 이동 처리 (Space / Ctrl)
        float y = 0f;
        if (Keyboard.current.spaceKey.isPressed) y += 1f;
        if (Keyboard.current.leftCtrlKey.isPressed) y -= 1f;

        // 5. 최종 이동 벡터
        Vector3 force = moveDirection.normalized * activeMoveSpeed + Vector3.up * (y * activeVerticalSpeed);

        // 6. 이동 적용
        playerRigidBody.AddForce(force, ForceMode.Force);

        // 7. 마지막 방향 저장
        if (force != Vector3.zero)
        {
            lastMoveDirection = force;
        }

        // 8. 최대 속도 제한
        if (playerRigidBody.velocity.magnitude > activeMaxSpeed)
        {
            playerRigidBody.velocity = playerRigidBody.velocity.normalized * activeMaxSpeed;
        }

        // 9. 이동 방향이 있을 때 회전
        if (force != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(force);
            transform.rotation = Quaternion.Slerp
                (transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
            //transform.rotation = targetRotation;
        }
    }

    private void OnRunStarted(InputAction.CallbackContext context)
    {
        if (Mathf.Approximately(activeMoveSpeed, baseMoveSpeed) &&
            Mathf.Approximately(activeVerticalSpeed, baseVerticalSpeed))
        {
            activeMoveSpeed *= runCoefficient;
            activeVerticalSpeed *= runCoefficient;
            activeMaxSpeed *= runCoefficient;
        }
    }

    private void OnRunCanceled(InputAction.CallbackContext context)
    {
        activeMoveSpeed = baseMoveSpeed;
        activeVerticalSpeed = baseVerticalSpeed;
        activeMaxSpeed = baseMaxSpeed;
    }

    private void Dash()
    {
        playerRigidBody.AddForce(lastMoveDirection.normalized
                                 * dashForce, ForceMode.Impulse);
    }

    private void OnDashStarted(InputAction.CallbackContext context)
    {
        dashPressed = true;
    }

    private void OnAttackStarted(InputAction.CallbackContext context)
    {
        GameObject bullet = PhotonNetwork.Instantiate(bulletText, firePoint.position,
            mainCamera.transform.rotation * fireOffset, 0);
        
        bullet.GetComponent<Bullet>().SetShooter(photonView.Owner.ActorNumber);
    }

    private bool IsPhotonViewIsMine()
    {
        return photonView.IsMine;
    }
}