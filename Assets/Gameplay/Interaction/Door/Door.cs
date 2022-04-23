using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    [SerializeField] private GameObject toggleObject;

    private Animator animator;
    private bool isOpen = false;
    private bool animating = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override bool Interact(Unit interactingUnit)
    {
        if (animating) return false;

        if (isOpen)
        {
            toggleObject.SetActive(true);
            animator.Play("Door_Close");
        }
        else
        {
            animator.Play("Door_Open");
        }
        animating = true;
        return true;
    }

    public void OnDoorOpened()
    {
        toggleObject.SetActive(false);
        isOpen = true;
        animating = false;
    }

    public void OnDoorClosed()
    {
        isOpen = false;
        animating = false;
    }
}
