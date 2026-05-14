using Unity.Netcode;
using UnityEngine;

public class ArcadeMachine : NetworkBehaviour, IInteractable
{
    [Header("Audio")]
    public AudioClip[] arcadeMachineSounds;
    public AudioSource audioSource;

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

    public bool CanInteract()
    {
        return !isBroken.Value;
    }

    public void StartInteract()
    {
        StartInteractRpc();
    }

    public void StopInteract()
    {
        StopInteractRpc();
    }

    public void CompleteInteract()
    {
        CompleteInteractRpc();
    }

    [Rpc(SendTo.Server)]
    private void StartInteractRpc()
    {
        if (isBroken.Value)
        {
            return;
        }

        if (arcadeMachineSounds.Length == 0)
        {
            Debug.LogWarning("No arcade machine sounds assigned");
            return;
        }

        int randomSoundIndex = Random.Range(0, arcadeMachineSounds.Length);

        StartSoundRpc(randomSoundIndex);
    }

    [Rpc(SendTo.Server)]
    private void StopInteractRpc()
    {
        StopSoundRpc();
    }

    [Rpc(SendTo.Server)]
    private void CompleteInteractRpc()
    {
        if (isBroken.Value)
        {
            return;
        }

        StopSoundRpc();

        bool shouldBreak = Random.value <= breakChance;

        if (shouldBreak)
        {
            isBroken.Value = true;
        }
    }

    [Rpc(SendTo.Everyone)]
    private void StartSoundRpc(int soundIndex)
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

        audioSource.Stop();
        audioSource.clip = arcadeMachineSounds[soundIndex];
        audioSource.loop = true;
        audioSource.Play();
    }

    [Rpc(SendTo.Everyone)]
    private void StopSoundRpc()
    {
        if (audioSource == null) return;

        audioSource.Stop();
        audioSource.loop = false;
    }

    private void OnBrokenChanged(bool oldValue, bool newValue)
    {
        UpdateMachineVisual();
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