using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestionManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public List<Button> answerButtons;
    

    [Header("Question Settings")]
    public List<Question> allQuestions = new List<Question>();
    public float questionDelay = 1.5f;

    [Header("Lock Display Settings")]
    public Image lockDisplay; // Add this reference in inspector
    public Sprite[] lockPhases; // Assign 5 sprites in order: [0]=1/5, [1]=2/5... [4]=5/5

    private List<Question> questionsToAsk = new List<Question>();
    private int currentQuestionIndex = 0;
    private int correctAnswersCount = 0;
    private bool canAnswer = true;
    private bool isGameOver = false;

    private void Start()
    {
        if (allQuestions == null || allQuestions.Count == 0)
        {
            Debug.LogError("No questions assigned to QuestionManager!");
            return;
        }

        for (int i = 0; i < allQuestions.Count; i++)
        {
            var question = allQuestions[i];

            if (question.answers == null || question.answers.Count != 4)
            {
                Debug.LogError($"Question #{i} ('{question.questionText}') is missing answers or does not have exactly 4 answers.");
            }

            if (question.correctAnswerIndex < 0 || question.correctAnswerIndex >= 4)
            {
                Debug.LogError($"Question #{i} ('{question.questionText}') has an invalid correctAnswerIndex: {question.correctAnswerIndex}");
            }
        }

        InitializeQuestions();
        DisplayCurrentQuestion();
        //lockDisplay.enabled = false; // Hide lock at start
    }

    void InitializeQuestions()
    {
        questionsToAsk = new List<Question>(allQuestions);
        ShuffleList(questionsToAsk);
    }

    void DisplayCurrentQuestion()
    {
        if (currentQuestionIndex >= questionsToAsk.Count)
        {
            Debug.Log("Game Over!");
            Debug.Log($"Correct Answers: {correctAnswersCount}/{allQuestions.Count}");
            isGameOver = true;
            return;
        }

        if (correctAnswersCount >= 5)
        {
            Debug.Log("Game Over! You won by answering 5 questions correctly!");
            isGameOver = true;
            return;
        }

        Question currentQuestion = questionsToAsk[currentQuestionIndex];

        if (currentQuestion.answers == null || currentQuestion.answers.Count != answerButtons.Count)
        {
            Debug.LogError($"Question {currentQuestionIndex} does not have exactly {answerButtons.Count} answers!");
            isGameOver = true;
            return;
        }

        if (currentQuestion.correctAnswerIndex < 0 || currentQuestion.correctAnswerIndex >= answerButtons.Count)
        {
            Debug.LogError($"Question {currentQuestionIndex} has invalid correctAnswerIndex: {currentQuestion.correctAnswerIndex}");
            isGameOver = true;
            return;
        }

        questionText.text = currentQuestion.questionText;

        for (int i = 0; i < answerButtons.Count; i++)
        {
            TextMeshProUGUI answerText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            answerText.text = currentQuestion.answers[i];

            int buttonIndex = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(buttonIndex));
        }

        canAnswer = true;
    }

    void OnAnswerSelected(int selectedIndex)
    {
        if (!canAnswer || isGameOver)
            return;

        canAnswer = false;

        Question currentQuestion = questionsToAsk[currentQuestionIndex];
        string selectedAnswer = currentQuestion.answers[selectedIndex];
        string correctAnswer = currentQuestion.answers[currentQuestion.correctAnswerIndex];

        if (selectedIndex == currentQuestion.correctAnswerIndex)
        {
            Debug.Log($"Correct! You selected: {selectedAnswer}");
            correctAnswersCount++;
            UpdateLockDisplay(); // Update lock only on correct answers
        }
        else
        {
            Debug.Log($"Wrong! You selected: {selectedAnswer}. Correct was: {correctAnswer}");
        }

        currentQuestionIndex++;

        StartCoroutine(DelayNextQuestion());
    }

    void UpdateLockDisplay()
    {
        if (lockPhases.Length == 0 || lockDisplay == null) return;

        // Show lock on first correct answer
        if (!lockDisplay.enabled && correctAnswersCount > 0)
        {
            lockDisplay.enabled = true;
        }

        // Phase 0 = 1st correct, Phase 4 = 5th correct
        int phaseIndex = Mathf.Clamp(correctAnswersCount - 1, 0, lockPhases.Length - 1);
        lockDisplay.sprite = lockPhases[phaseIndex];
    }

    IEnumerator DelayNextQuestion()
    {
        yield return new WaitForSeconds(questionDelay);
        DisplayCurrentQuestion();
    }

    void ShuffleList(List<Question> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Question temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void RestartGame()
    {
        correctAnswersCount = 0;
        currentQuestionIndex = 0;
        isGameOver = false;
        InitializeQuestions();
        DisplayCurrentQuestion();
        lockDisplay.enabled = false; // Reset lock visibility
    }
}