using UnityEngine;

public class Door : MonoBehaviour
{
    public bool abierta = false;
    public Animator animator;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void AbrirPuerta()
    {
        abierta = true;
        if (animator != null)
            animator.SetTrigger("Abrir");
    }

    public void CerrarPuerta()
    {
        abierta = false;
        if (animator != null)
            animator.SetTrigger("Cerrar");
    }
}
