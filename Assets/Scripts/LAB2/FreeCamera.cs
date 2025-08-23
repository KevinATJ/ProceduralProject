using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 10f;
    public float sprintMultiplier = 2f;
    public float climbSpeed = 5f;

    [Header("Mouse")]
    public float lookSpeed = 2f;
    public bool invertY = false;

    private float rotX = 0f;
    private float rotY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 rot = transform.localRotation.eulerAngles;
        rotX = rot.x;
        rotY = rot.y;
    }

    void Update()
    {
        HandleMouse();
        HandleMovement();
    }

    void HandleMouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        rotY += mouseX;
        rotX += invertY ? mouseY : -mouseY;
        rotX = Mathf.Clamp(rotX, -90f, 90f);

        transform.localRotation = Quaternion.Euler(rotX, rotY, 0f);
    }

    void HandleMovement()
    {
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            speed *= sprintMultiplier;

        Vector3 move = new Vector3();

        move += transform.forward * Input.GetAxis("Vertical");
        move += transform.right * Input.GetAxis("Horizontal");

        if (Input.GetKey(KeyCode.E))
            move += Vector3.up;
        if (Input.GetKey(KeyCode.Q))
            move += Vector3.down;

        transform.position += move * speed * Time.deltaTime;
    }
}

