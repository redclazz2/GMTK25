using UnityEngine;

public class MenuButtonReact : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void PlayCloseAnimation()
    {
        animator.SetBool("IsClose", true);
        animator.Play("CloseAnim");
    }
}
