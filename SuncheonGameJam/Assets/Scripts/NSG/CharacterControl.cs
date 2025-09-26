using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    CharacterController controller;
    Vector3 dir; 

    [Header("이동 설정")]
    public float speed = 5.0f;
    public float jumpPower = 7.5f;

    [Header("시점 설정")]
    public float rotationSpeed = 3.0f; // 마우스 감도

    private Camera mainCamera;

    public float walkingBobbingSpeed = 14f; // 걷기 속도에 따른 흔들림 빈도
    public float bobbingAmount = 0.05f;    // 흔들림의 최대 폭 (진폭)

    private float defaultPosY = 0;
    private float timer = 0;

   void Start()
   {
        controller = GetComponent<CharacterController>();
        
        // 씬에서 메인 카메라를 찾아 저장합니다.
        mainCamera = Camera.main; 

        // 🚨 시점 조작을 위해 마우스 커서를 숨기고 잠급니다.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
   
        defaultPosY = mainCamera.transform.localPosition.y; 
   }

   
   void OnDisable()
   {
      // 🚨 게임이 끝날 때 마우스 커서를 해제합니다.
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
   }
    private void OnTriggerEnter(Collider collider)
    {
        if(LayerMask.LayerToName(collider.gameObject.layer) == "Environment")
        {
            EnvironmentMove eMove = collider.transform.parent.GetComponent<EnvironmentMove>();
            eMove.MoveStart();
        }
    }
   void Update()
   {
      // 1. 마우스 입력으로 캐릭터 회전 처리
      float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
      // 캐릭터 자체를 수평으로 회전시킵니다.
      transform.Rotate(Vector3.up * mouseX); 

      // 2. 중력 적용 (매 프레임)
      dir.y += Physics.gravity.y * Time.deltaTime; 

      // 3. 캐릭터가 지면에 있는 경우
      if (controller.isGrounded)
      {         
         var h = Input.GetAxis("Horizontal");
         var v = Input.GetAxis("Vertical");

         // 🚨 캐릭터의 방향이 아닌, 카메라의 방향을 기준으로 이동 벡터를 계산합니다.
         // 마우스로 캐릭터가 회전하므로, 카메라 회전과 캐릭터 회전을 일치시키는 것이 일반적입니다.

         // 현재 캐릭터의 앞(forward) 방향과 오른쪽(right) 방향을 사용합니다.
         Vector3 forward = transform.forward;
         Vector3 right = transform.right;

         // Y축 중력 성분을 제외한 순수 이동 방향을 계산합니다.
         Vector3 moveDirection = (forward * v) + (right * h);
         dir.x = moveDirection.x * speed;
         dir.z = moveDirection.z * speed;
         
         // 4. 점프 처리
         if (Input.GetKeyDown(KeyCode.Space))
            dir.y = jumpPower;
      }
      
      // 5. 캐릭터 이동
      controller.Move(dir * Time.deltaTime);

      // 1. 캐릭터가 움직이고 있는지 확인 (예: 키보드 입력이 있을 때)
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f)
        {
            if (!controller.isGrounded) return;
            // 2. 타이머를 증가시킵니다.
            timer += Time.deltaTime * walkingBobbingSpeed;

            // 3. 사인파를 이용하여 Y축 위치를 계산합니다.
            // Mathf.Sin() 함수는 주기적으로 -1과 1 사이의 값을 반환합니다.
            float newPosY = defaultPosY + Mathf.Sin(timer) * bobbingAmount;

            // 4. 카메라 위치를 업데이트합니다.
            mainCamera.transform.localPosition = new Vector3(
                mainCamera.transform.localPosition.x,
                newPosY,
                mainCamera.transform.localPosition.z);
        }
        else
        {
            // 멈춰있을 때는 카메라를 기본 위치로 부드럽게 복귀시킵니다.
            timer = 0;
            mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, 
                new Vector3(mainCamera.transform.localPosition.x, defaultPosY, mainCamera.transform.localPosition.z), 
                Time.deltaTime * walkingBobbingSpeed);
        }
   }
}