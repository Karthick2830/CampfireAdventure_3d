using System.Text;
using CampfireAdventure.Objectives;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CampfireAdventure.UI
{
    /// <summary>
    /// Top-left objective panel: the current objective and its one-line instruction, then a checklist of
    /// every step (done steps are ticked and struck through, the current step is highlighted).
    /// Refreshes only when the objective manager reports a change.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ObjectiveTrackerView : MonoBehaviour
    {
        [SerializeField] private ObjectiveManager objectives;
        [SerializeField] private TMP_Text currentTitle;
        [SerializeField] private TMP_Text currentInstruction;
        [SerializeField, Tooltip("One entry per objective, in order.")] private Image[] checkBoxes = new Image[0];
        [SerializeField] private TMP_Text[] rowLabels = new TMP_Text[0];
        [SerializeField] private Sprite uncheckedSprite;
        [SerializeField] private Sprite checkedSprite;

        [Header("Colours")]
        [SerializeField] private Color doneColor = new Color(0.62f, 0.62f, 0.58f, 1f);
        [SerializeField] private Color currentColor = new Color(1f, 0.82f, 0.4f, 1f);
        [SerializeField] private Color pendingColor = new Color(0.9f, 0.9f, 0.86f, 0.75f);

        private readonly StringBuilder sb = new StringBuilder(96);

        private void OnEnable() => objectives.Changed += Refresh;
        private void OnDisable() => objectives.Changed -= Refresh;
        private void Start() => Refresh();

        private void Refresh()
        {
            if (objectives.Context == null) return;

            if (objectives.IsFinished)
            {
                currentTitle.text = "Adventure complete!";
                currentInstruction.text = "Press F5 to play again.";
            }
            else
            {
                var c = objectives.Current; var ctx = objectives.Context;
                sb.Clear().Append(c.GetTitle(ctx));
                string progress = c.GetProgress(ctx);
                if (progress.Length > 0) sb.Append("  <color=#FFD27A>").Append(progress).Append("</color>");
                currentTitle.text = sb.ToString();
                currentInstruction.text = c.GetInstruction(ctx);
            }

            for (int i = 0; i < rowLabels.Length && i < objectives.Count; i++)
            {
                bool done = objectives.IsDone(i), current = !objectives.IsFinished && objectives.Index == i;
                checkBoxes[i].sprite = done ? checkedSprite : uncheckedSprite;
                checkBoxes[i].color = done ? Color.white : (current ? currentColor : pendingColor);
                sb.Clear();
                if (done) sb.Append("<s>").Append(objectives.TitleOf(i)).Append("</s>");
                else sb.Append(objectives.TitleOf(i));
                rowLabels[i].text = sb.ToString();
                rowLabels[i].color = done ? doneColor : (current ? currentColor : pendingColor);
                rowLabels[i].fontStyle = current ? FontStyles.Bold : FontStyles.Normal;
            }
        }
    }
}
