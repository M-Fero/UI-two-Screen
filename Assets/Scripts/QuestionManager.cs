﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuestionManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public List<Button> answerButtons;

    [Header("Question Settings")]
    public List<Question> allQuestions = new List<Question>();
    public float questionDelay = 1.5f;

    [Header("Lock Display Settings")]
    public Image lockDisplay;
    public Sprite[] lockPhases;

    [Header("Background Display Settings")]
    public Image secondaryDisplay;
    public Sprite[] secondaryPhases;

    [Header("Warning Text Display Settings")]
    public Image thirdDisplay;
    public Sprite[] thirdPhases;
    [SerializeField] float gameRestartDelay = 3f;

    [Header("Button Colors")]
    public Color normalButtonColor = Color.white;
    public Color wrongButtonColor = Color.red;
    public Color correctButtonColor = Color.green;

    [Header("Game Over UI")]
    public GameObject gameOverPanel1;
    public GameObject gameOverPanel2;

    private List<Question> questionsToAsk = new List<Question>();
    private int currentQuestionIndex = 0;
    private int correctAnswersCount = 0;
    private bool canAnswer = true;
    private bool isGameOver = false;

    private const int correctAnswersToWin = 3; // ✨ Changed from 5 to 3
    private const int maxQuestionsPerGame = 5;  // ✨ Limit to 5 questions

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

        if (gameOverPanel1 != null)
            gameOverPanel1.SetActive(false); // ✨ Hide Game Over Panel at start
        if (gameOverPanel2 != null)
            gameOverPanel2.SetActive(false); // ✨ Hide Game Over Panel at start
    }

    void InitializeQuestions()
    {
        questionsToAsk = new List<Question>(allQuestions);
        ShuffleList(questionsToAsk);

        // ✨ Limit to only 5 questions
        if (questionsToAsk.Count > maxQuestionsPerGame)
        {
            questionsToAsk = questionsToAsk.GetRange(0, maxQuestionsPerGame);
        }
    }

    void DisplayCurrentQuestion()
    {
        if (currentQuestionIndex >= questionsToAsk.Count)
        {
            isGameOver = true;

            if (correctAnswersCount >= correctAnswersToWin)
            {
                Debug.Log("Game Over! You won by answering 3 questions correctly!");
            }
            else
            {
                Debug.Log("Game Over! You failed to answer enough questions correctly!");
                if (gameOverPanel1 != null)
                    gameOverPanel1.SetActive(true);// ✨ Show Game Over text on screen
                if (gameOverPanel2 != null)
                    gameOverPanel2.SetActive(true);// ✨ Show Game Over text on screen
            }

            StartCoroutine(RestartSceneAfterDelay(5f));
            return;
        }


        if (correctAnswersCount >= correctAnswersToWin)
        {
            Debug.Log("Game Over! You won by answering 3 questions correctly!");
            isGameOver = true;
            StartCoroutine(RestartSceneAfterDelay(gameRestartDelay)); // ✨ Restart after 5 sec
            return;
        }

        ResetButtonColors();

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
            UpdateDisplays();
            HighlightButton(selectedIndex, correctButtonColor); // ✨ Highlight correct in Green
        }
        else
        {
            Debug.Log($"Wrong! You selected: {selectedAnswer}. Correct was: {correctAnswer}");
            HighlightButton(selectedIndex, wrongButtonColor); // ✨ Highlight wrong in Red
        }

        // Disable all buttons after answering
        //SetButtonsInteractable(false);

        currentQuestionIndex++;

        StartCoroutine(DelayNextQuestion());
    }

    void HighlightButton(int buttonIndex, Color color)
    {
        if (buttonIndex >= 0 && buttonIndex < answerButtons.Count)
        {
            ColorBlock cb = answerButtons[buttonIndex].colors;
            cb.normalColor = color;
            cb.selectedColor = color;
            cb.highlightedColor = color;
            answerButtons[buttonIndex].colors = cb;
        }
    }

    //void SetButtonsInteractable(bool interactable)
    //{
    //    foreach (var button in answerButtons)
    //    {
    //        button.interactable = interactable;
    //    }
    //}


    void UpdateDisplays()
    {
        int phaseIndex = Mathf.Clamp(correctAnswersCount - 1, 0, lockPhases.Length - 1);

        // Update Lock Display
        if (lockPhases.Length > 0 && lockDisplay != null)
        {
            if (!lockDisplay.enabled && correctAnswersCount > 0)
            {
                lockDisplay.enabled = true;
            }
            lockDisplay.sprite = lockPhases[phaseIndex];
        }

        // Update Secondary Display
        if (secondaryPhases.Length > 0 && secondaryDisplay != null)
        {
            if (!secondaryDisplay.enabled && correctAnswersCount > 0)
            {
                secondaryDisplay.enabled = true;
            }
            secondaryDisplay.sprite = secondaryPhases[phaseIndex];
        }

        // Update Third Display
        if (thirdPhases.Length > 0 && thirdDisplay != null)
        {
            if (!thirdDisplay.enabled && correctAnswersCount > 0)
            {
                thirdDisplay.enabled = true;
            }
            thirdDisplay.sprite = thirdPhases[phaseIndex];
        }
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
    void ResetButtonColors()
    {
        foreach (var button in answerButtons)
        {
            ColorBlock cb = button.colors;
            cb.normalColor = normalButtonColor;
            cb.selectedColor = normalButtonColor;
            cb.highlightedColor = normalButtonColor;
            button.colors = cb;
        }
    }


    IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGame()
    {
        correctAnswersCount = 0;
        currentQuestionIndex = 0;
        isGameOver = false;
        InitializeQuestions();
        DisplayCurrentQuestion();
        lockDisplay.enabled = false;
    }
}
