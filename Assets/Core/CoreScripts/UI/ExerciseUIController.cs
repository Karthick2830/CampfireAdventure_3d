using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ExerciseGame.Core.Exercise;

namespace ExerciseGame.Core.UI
{
    public class ExerciseUIController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField]
        private ExerciseUIDataProvider dataProvider;

        [SerializeField]
        private ExerciseManager exerciseManager;

        [Header("Exercise Information")]
        [SerializeField] private TMP_Text exerciseNameText;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private TMP_Text instructionDetailText;
        [SerializeField] private TMP_Text feedbackText;

        [Header("Progress")]
        [SerializeField] private TMP_Text repText;
        [SerializeField] private TMP_Text setText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TMP_Text progressText;

        [Header("Pause")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject pausePanel;

        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button endWorkoutButton;

        [Header("Completion")]
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private TMP_Text completionText;


        private void Awake()
        {
            if (dataProvider == null)
                dataProvider =
                    FindFirstObjectByType<ExerciseUIDataProvider>();

            if (exerciseManager == null)
                exerciseManager =
                    FindFirstObjectByType<ExerciseManager>();
        }


        private void OnEnable()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);

            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (endWorkoutButton != null)
                endWorkoutButton.onClick.AddListener(OnEndWorkoutClicked);
        }


        private void OnDisable()
        {
            if (pauseButton != null)
                pauseButton.onClick.RemoveListener(OnPauseClicked);

            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(OnResumeClicked);

            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);

            if (endWorkoutButton != null)
                endWorkoutButton.onClick.RemoveListener(OnEndWorkoutClicked);
        }


        private void Update()
        {
            if (dataProvider == null)
                return;

            ExerciseUIData data = dataProvider.CurrentData;

            if (data == null)
                return;

            UpdateUI(data);
        }


        public void UpdateUI(ExerciseUIData data)
        {
            if (data == null)
                return;

            UpdateExerciseInformation(data);
            UpdateProgress(data);
            UpdatePauseState(data);
            UpdateCompletionState(data);
        }


        private void UpdateExerciseInformation(ExerciseUIData data)
        {
            if (exerciseNameText != null)
                exerciseNameText.text = data.ExerciseName;

            if (instructionText != null)
                instructionText.text = data.Instruction;

            if (instructionDetailText != null)
                instructionDetailText.text = data.InstructionDetail;

            if (feedbackText != null)
                feedbackText.text = data.Feedback;
        }


        private void UpdateProgress(ExerciseUIData data)
        {
            if (repText != null)
            {
                repText.text =
                    $"{data.CurrentRep} / {data.TargetReps}";
            }

            if (setText != null)
            {
                setText.text =
                    $"{data.CurrentSet} / {data.TargetSets}";
            }

            if (progressSlider != null)
            {
                progressSlider.value =
                    Mathf.Clamp01(data.Progress);
            }

            if (progressText != null)
            {
                progressText.text =
                    $"{Mathf.RoundToInt(data.Progress * 100f)}%";
            }
        }


        private void UpdatePauseState(ExerciseUIData data)
        {
            if (pausePanel != null)
                pausePanel.SetActive(data.IsPaused);

            if (pauseButton != null)
            {
                pauseButton.gameObject.SetActive(!data.IsPaused);
            }
        }


        private void UpdateCompletionState(ExerciseUIData data)
        {
            if (completionPanel != null)
            {
                completionPanel.SetActive(
                    data.IsExerciseCompleted);
            }

            if (completionText != null &&
                data.IsExerciseCompleted)
            {
                completionText.text = "Exercise Completed!";
            }
        }


        // --------------------------------------------------
        // Pause Controls
        // --------------------------------------------------

        private void OnPauseClicked()
        {
            if (exerciseManager == null)
                return;

            exerciseManager.PauseExercise();
        }


        private void OnResumeClicked()
        {
            if (exerciseManager == null)
                return;

            exerciseManager.ResumeExercise();
        }


        private void OnRestartClicked()
        {
            if (exerciseManager == null)
                return;

            exerciseManager.ResetExercise();
            exerciseManager.StartExercise();
        }


        private void OnEndWorkoutClicked()
        {
            if (exerciseManager == null)
                return;

            exerciseManager.EndExercise();
        }
    }
}