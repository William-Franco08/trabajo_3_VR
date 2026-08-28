using UnityEngine;
using UnityEngine.InputSystem;

public class Movimiento : MonoBehaviour
{
    private NIS inputActions;

    private Vector2 moveInput;

    [Header("Movimiento")]
    public float velocidadCaminar = 3f;
    public float velocidadCorrer = 6f;
    public float velocidadAgachado = 1.5f;
    public float velocidadGiro = 10f;

    [Header("Salto")]
    public float fuerzaSalto = 5f;

    [Header("Estado")]
    private bool estaCorriendo = false;
    private bool estaAgachado = false;
    private bool estaBailando = false;
    private bool estaEnSuelo = true;

    private Rigidbody rb;
    private Animator animator;

    private void Awake()
    {
        inputActions = new NIS();

        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        // Movimiento
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        // Salto
        inputActions.Player.Jump.performed += OnJump;

        // Correr
        inputActions.Player.Run.performed += OnRun;
        inputActions.Player.Run.canceled += OnRun;

        // Agacharse
        inputActions.Player.Crouch.performed += OnCrouch;

        // Bailar
        inputActions.Player.Dance.performed += OnDance;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;

        inputActions.Player.Jump.performed -= OnJump;

        inputActions.Player.Run.performed -= OnRun;
        inputActions.Player.Run.canceled -= OnRun;

        inputActions.Player.Crouch.performed -= OnCrouch;

        inputActions.Player.Dance.performed -= OnDance;

        inputActions.Player.Disable();
    }

    // =====================================================
    // MOVIMIENTO
    // =====================================================

    private void OnMove(InputAction.CallbackContext value)
    {
        moveInput = value.ReadValue<Vector2>();
    }

    private void Update()
    {
        Vector3 movimiento = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
        );

        // =================================================
        // BAILE
        // =================================================

        // Si está bailando, no se mueve
        if (!estaBailando)
        {
            // =================================================
            // GIRAR Y MOVER
            // =================================================

            if (movimiento.magnitude > 0.1f)
            {
                // Dirección hacia donde queremos mirar
                Quaternion rotacionObjetivo =
                    Quaternion.LookRotation(movimiento);

                // Girar suavemente
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    rotacionObjetivo,
                    velocidadGiro * Time.deltaTime
                );

                // =================================================
                // VELOCIDAD
                // =================================================

                float velocidadActual;

                if (estaAgachado)
                {
                    // Agachado
                    velocidadActual = velocidadAgachado;
                }
                else if (estaCorriendo)
                {
                    // Corriendo
                    velocidadActual = velocidadCorrer;
                }
                else
                {
                    // Caminando
                    velocidadActual = velocidadCaminar;
                }

                // =================================================
                // MOVIMIENTO HACIA ADELANTE
                // =================================================

                transform.Translate(
                    Vector3.forward *
                    velocidadActual *
                    Time.deltaTime
                );
            }
        }

        // =================================================
        // ANIMATOR
        // =================================================

        float velocidad = moveInput.magnitude;

        animator.SetFloat("Speed", velocidad);

        animator.SetBool(
            "IsRunning",
            estaCorriendo
        );

        animator.SetBool(
            "IsCrouching",
            estaAgachado
        );

        animator.SetBool(
            "IsGrounded",
            estaEnSuelo
        );

        animator.SetBool(
            "Dance",
            estaBailando
        );
    }

    // =====================================================
    // CORRER
    // =====================================================

    private void OnRun(InputAction.CallbackContext context)
    {
        estaCorriendo = context.ReadValueAsButton();

        // No puede correr mientras está agachado
        if (estaAgachado)
        {
            estaCorriendo = false;
        }

        // No puede correr mientras baila
        if (estaBailando)
        {
            estaCorriendo = false;
        }
    }

    // =====================================================
    // AGACHARSE
    // =====================================================

    private void OnCrouch(InputAction.CallbackContext context)
    {
        // Si está bailando, no puede agacharse
        if (estaBailando)
            return;

        // Alternar agachado / levantado
        estaAgachado = !estaAgachado;

        // Si se agacha, deja de correr
        if (estaAgachado)
        {
            estaCorriendo = false;
        }
    }

    // =====================================================
    // SALTAR
    // =====================================================

    private void OnJump(InputAction.CallbackContext context)
    {
        // No puede saltar si ya está en el aire
        if (!estaEnSuelo)
            return;

        // No puede saltar agachado
        if (estaAgachado)
            return;

        // No puede saltar bailando
        if (estaBailando)
            return;

        Debug.Log("Saltar");

        rb.AddForce(
            Vector3.up * fuerzaSalto,
            ForceMode.Impulse
        );

        estaEnSuelo = false;

        animator.SetBool(
            "IsGrounded",
            false
        );
    }

    // =====================================================
    // BAILAR
    // =====================================================

    private void OnDance(InputAction.CallbackContext context)
    {
        // Alternar baile
        estaBailando = !estaBailando;

        if (estaBailando)
        {
            // Al empezar a bailar:
            estaCorriendo = false;
            estaAgachado = false;

            // Dejar de moverse
            moveInput = Vector2.zero;
        }
    }

    // =====================================================
    // DETECTAR SUELO
    // =====================================================

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            estaEnSuelo = true;

            animator.SetBool(
                "IsGrounded",
                true
            );
        }
    }
}