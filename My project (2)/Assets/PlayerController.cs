using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    [Header("Referensi Kamera")]
    // Referensi agar karakter tahu kemana kamera sedang menghadap
    public Transform cameraTransform; 

    [Header("Pergerakan")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Fisika")]
    public float gravity = 9.81f;
    private Vector3 moveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Jika kolom kamera lupa diisi di Unity, otomatis cari Main Camera
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // 1. Mengambil Input WASD
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Menghitung besaran input untuk Animasi (Batas maksimal 1)
        float inputMagnitude = Mathf.Clamp01(new Vector2(horizontal, vertical).magnitude);

        // 2. Mendapatkan arah dari Kamera
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // KUNCI SOULS-LIKE: Abaikan sumbu Y (atas/bawah) kamera 
        // Agar karakter tidak terbang ke udara saat kamera melihat ke atas
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // 3. Menghitung arah jalan relatif terhadap kamera
        Vector3 moveDir = (camForward * vertical + camRight * horizontal).normalized;

        // 4. Logika Pergerakan & Rotasi Karakter
        if (inputMagnitude >= 0.1f)
        {
            // Membuat karakter berputar dengan mulus menghadap arah gerakan
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Menerapkan kecepatan
            Vector3 move = moveDir * moveSpeed;
            moveDirection.x = move.x;
            moveDirection.z = move.z;
        }
        else
        {
            // Hentikan pergerakan jika tidak ada tombol ditekan
            moveDirection.x = 0;
            moveDirection.z = 0;
        }

        // 5. Terapkan Gravitasi
        if (!controller.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else
        {
            moveDirection.y = -0.5f; 
        }

        // Jalankan perintah gerak
        controller.Move(moveDirection * Time.deltaTime);

        // 6. Jalankan Animasi
        animator.SetFloat("Speed", inputMagnitude);

        // 7. Input Serang
        if (Input.GetMouseButtonDown(0)) 
        {
            animator.SetTrigger("Attack");
        }
    }
}