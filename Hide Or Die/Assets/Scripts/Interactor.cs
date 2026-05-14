using UnityEngine;

public class Interactor : MonoBehaviour
{
    [Header("Interaction")]
    public Transform InteractorSource;
    public float InteractRange;
    public LayerMask interactableLayer;
    public float interactTime = 8f;

    private IInteractable currentInteractable;
    private IInteractable activeInteractable;

    private float interactTimer = 0f;
    private bool isHoldingInteraction = false;


    // Update is called once per frame
    void Update()
    {
        CheckForInteractable();
        HandleInteractionInput();
    }

    private void CheckForInteractable()
    {
        Ray r = new Ray(InteractorSource.position, InteractorSource.forward);
        if (Physics.Raycast(r, out RaycastHit hitInfo, InteractRange, interactableLayer))
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactable))
            {
                currentInteractable = interactable;
                Debug.Log("Hold E to Deplete Boredom");
                return;
            }
        }

        currentInteractable = null;
    }

    void HandleInteractionInput()
    {
        // If we are holding E while looking at a usable object
        if (currentInteractable != null && currentInteractable.CanInteract() && Input.GetKey(KeyCode.E))
        {
            // Start interaction only once
            if (!isHoldingInteraction)
            {
                isHoldingInteraction = true;
                activeInteractable = currentInteractable;
                activeInteractable.StartInteract();
            }

            interactTimer += Time.deltaTime;

            Debug.Log("Using machine: " + interactTimer.ToString("F1") + "/" + interactTime);

            if (interactTimer >= interactTime)
            {
                activeInteractable.CompleteInteract();

                Debug.Log("Boredom filled!");

                ResetInteraction();
            }
        }
        else
        {
            // If player released E or looked away
            if (isHoldingInteraction)
            {
                activeInteractable.StopInteract();
                ResetInteraction();
            }
        }
    }

    void ResetInteraction()
    {
        interactTimer = 0f;
        isHoldingInteraction = false;
        activeInteractable = null;
    }
}
