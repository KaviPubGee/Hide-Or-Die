using Unity.Netcode;
using UnityEngine;

public class ArcadeMachine : NetworkBehaviour, IInteractable
{
    [Header("Audio")]
    public AudioClip[] arcadeMachineSounds;
    public AudioSource audioSource;
    public AudioClip errorSound;

    [Header("Machine Stats")]
    [Range(0f, 1f)]
    public float breakChance = 0.5f;

    private NetworkVariable<bool> isBroken = new NetworkVariable<bool>(false);

    private MeshRenderer meshRenderer;

    public override void OnNetworkSpawn()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (IsServer)
        {
            isBroken.Value = false;
        }

        isBroken.OnValueChanged += OnBrokenChanged;

        UpdateMachineVisual();
    }

    public override void OnNetworkDespawn()
    {
        isBroken.OnValueChanged -= OnBrokenChanged;
    }

    public void Interact()
    {
        InteractServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void InteractServerRpc()
    {
        if (isBroken.Value)
        {
            BrokenMachineRpc();
            return;
        }

        if (arcadeMachineSounds.Length == 0)
        {
            Debug.LogWarning("No arcade machine sounds assigned");
            return;
        }

        int randomSoundIndex = Random.Range(0, arcadeMachineSounds.Length);

        PlaySoundRpc(randomSoundIndex);

        bool shouldBreak = Random.value <= breakChance;

        if (shouldBreak)
        {
            isBroken.Value = true;
        }
    }

    [Rpc(SendTo.Everyone)]
    private void PlaySoundRpc(int soundIndex)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource assigned");
            return;
        }

        if (arcadeMachineSounds.Length == 0)
        {
            Debug.LogWarning("No arcade machine sounds assigned");
            return;
        }

        if (soundIndex < 0 || soundIndex >= arcadeMachineSounds.Length)
        {
            Debug.LogWarning("Invalid sound index");
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.PlayOneShot(arcadeMachineSounds[soundIndex]);
    }

    [Rpc(SendTo.Everyone)]
    private void BrokenMachineRpc()
    {
        Debug.Log("This arcade machine is broken!");

        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource assigned");
            return;
        }

        if (errorSound == null)
        {
            Debug.LogWarning("No error sound assigned");
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.PlayOneShot(errorSound);
    }

    private void OnBrokenChanged(bool oldValue, bool newValue)
    {
        UpdateMachineVisual();
        if (newValue && audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void UpdateMachineVisual()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (meshRenderer == null)
        {
            return;
        }

        if (isBroken.Value)
        {
            meshRenderer.material.color = Color.red;
        }
        else
        {
            meshRenderer.material.color = Color.green;
        }
    }
}