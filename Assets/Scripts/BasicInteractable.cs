using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class BasicInteractable : MonoBehaviour, IInteractable, IInteractionPromptProvider
{
    [SerializeField] private string prompt = "Press E to interact";
    [SerializeField] private string message = "Nothing happens yet.";

    public string Prompt => prompt;

    public InteractionPromptData GetPromptData(PlayerInteraction player)
    {
        return InteractionPromptData.FromLegacyPrompt(prompt);
    }

    public void Configure(string newPrompt, string newMessage)
    {
        prompt = newPrompt;
        message = newMessage;
    }

    public void Interact(PlayerInteraction player)
    {
        UIController.Instance?.ShowMessage(message, 2.5f);
    }
}
