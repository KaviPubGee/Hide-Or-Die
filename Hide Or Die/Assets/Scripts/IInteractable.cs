public interface IInteractable
{
    void StartInteract();
    void StopInteract();
    void CompleteInteract();
    bool CanInteract();
}