using UnityEngine;

namespace CampfireAdventure.Gathering
{
    /// <summary>
    /// Describes one kind of gatherable resource (wood, stone). Create more under
    /// Assets/_Project/ScriptableObjects/Items. Pure data: no behaviour.
    /// </summary>
    [CreateAssetMenu(fileName = "Resource_", menuName = "Campfire Adventure/Resource Definition")]
    public sealed class ResourceDefinition : ScriptableObject
    {
        [SerializeField] private ResourceType type = ResourceType.Wood;
        [SerializeField] private string displayName = "Wood";
        [SerializeField, Tooltip("Shown in the prompt, e.g. \"Chop wood\".")] private string collectVerb = "Chop wood";
        [SerializeField, Tooltip("Shown in the HUD next to the count.")] private Sprite icon;
        [SerializeField] private Color color = Color.white;
        [SerializeField, Min(1), Tooltip("Units gained from one successful collection.")] private int amountPerCollection = 1;
        [SerializeField, Min(1), Tooltip("Units the campfire needs before it can be prepared.")] private int requiredForCampfire = 2;

        public ResourceType Type => type;
        public string DisplayName => displayName;
        public string CollectVerb => collectVerb;
        public Sprite Icon => icon;
        public Color Color => color;
        public int AmountPerCollection => amountPerCollection;
        public int RequiredForCampfire => requiredForCampfire;
    }
}
