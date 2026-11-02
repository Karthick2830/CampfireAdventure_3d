using UnityEngine;

namespace ExerciseGame.Core.UI
{
    public class ExerciseUIDataDebug : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private ExerciseUIDataProvider dataProvider;


        private void Awake()
        {
            if (dataProvider == null)
            {
                dataProvider =
                    FindFirstObjectByType<ExerciseUIDataProvider>();
            }
        }


        private void OnGUI()
        {
            if (dataProvider == null)
                return;

            ExerciseUIData data =
                dataProvider.CurrentData;

            if (data == null)
                return;


            GUIStyle style =
                new GUIStyle(GUI.skin.label);

            style.fontSize = 20;


            float y = 20f;


            GUI.Label(
                new Rect(20, y, 600, 30),
                "Exercise: " + data.ExerciseName,
                style);

            y += 35f;


            GUI.Label(
                new Rect(20, y, 600, 30),
                "Set: " +
                data.CurrentSet +
                " / " +
                data.TargetSets,
                style);

            y += 35f;


            GUI.Label(
                new Rect(20, y, 600, 30),
                "Rep: " +
                data.CurrentRep +
                " / " +
                data.TargetReps,
                style);

            y += 35f;


            GUI.Label(
                new Rect(20, y, 600, 30),
                "Progress: " +
                (data.Progress * 100f).ToString("F0") +
                "%",
                style);

            y += 35f;


            GUI.Label(
                new Rect(20, y, 700, 30),
                "Instruction: " +
                data.Instruction,
                style);

            y += 35f;


            GUI.Label(
                new Rect(20, y, 700, 30),
                "Detail: " +
                data.InstructionDetail,
                style);

            y += 35f;


            GUI.Label(
                new Rect(20, y, 600, 30),
                "Pose Ready: " +
                data.IsPoseReady,
                style);

            y += 35f;


            GUI.Label(
                new Rect(20, y, 600, 30),
                "Paused: " +
                data.IsPaused,
                style);

            y += 35f;


            GUI.Label(
                new Rect(20, y, 600, 30),
                "Exercise Completed: " +
                data.IsExerciseCompleted,
                style);
        }
    }
}