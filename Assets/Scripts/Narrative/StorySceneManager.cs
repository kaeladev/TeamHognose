using Ink.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum CharacterFlags: byte
{
    None = 0,
    Inky = 1 << 0,
    Squill = 1 << 1,
    Soup = 1 << 2,
    Tort = 1 << 3,
    Yuzu = 1 << 4,
    All = 1 << 5,
}

/*
This manager class should:
- Store persistent playthrough data, aka context for what the player has done
- Load the correct story based on playthrough data/player choices
- Progress through the story while updating visuals appropriately
*/
public class StorySceneManager : MonoBehaviour
{
    // StorySceneManager Singleton
    public static StorySceneManager PersistentInstance;

    // Public Data to set up pre-known start-of-day scenes
    public int              YuzuPetsForSecretEnding = 10;
    public TextAsset[]      WorkDayScenes;
    public string           BakerySceneName;

    // Persistent Data between scenes, for calculating ending
    private byte            PursuedCharacters = 0;
    private int             CurrentDay = 1;
    private int             TimesYuzuPetted = 0;
    private int             TimesYuzuFedTreat = 0;
    private int             ScoreAffectingOptionsDiscovered = 0;
    private int             GoodScoreOptionsSelected = 0;

    // Current Scene Data, to be loaded at runtime per day
    private bool            HasFirstChoiceOccurred = false;
    private bool            WaitingForChoiceInput = false;
    private byte            CharactersInCurrentScene = 0;
    private TextAsset       CurrentInkScript;
    private Story           CurrentStory;
    private string          CurrentStoryText;
    private List<string>    CurrentStoryTags;

    private void Awake()
    {
        RemoveChildren();

        Debug.Log("StorySceneManager Awake");
        if (PersistentInstance)
        {
            // A second StorySceneManager has attempted to create itself, so destroy
            Destroy(gameObject);
            ResetForNewDay(); // TODO: Is this the best place for this logic?
            return;
        }

        // The first time a StorySceneManager attempts to create itself, store as static instance
        PersistentInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()    // Does this happen only once or every time level is loaded?
    {
        RemoveChildren();

        Debug.Log("StorySceneManager Start");

        CurrentInkScript = WorkDayScenes[CurrentDay - 1];
        CurrentStoryTags = new List<string>();
        CurrentStory = new Story(CurrentInkScript.text);
        ContinueStory();
    }

    void OnLevelWasLoaded(int level)
    {
        Debug.Log("StorySceneManager OnLevelWasLoaded");
    }

    void Update()
    {
        if (CurrentStory.currentChoices.Count > 0)
        {
            if (!WaitingForChoiceInput)
            {
                // Display all the choices, if there are any!
                for (int i = 0; i < CurrentStory.currentChoices.Count; i++)
                {
                    Choice choice = CurrentStory.currentChoices[i];
                    Button button = CreateChoiceView(choice.text.Trim());
                    // Tell the button what to do when we press it
                    button.onClick.AddListener(delegate {
                        OnClickChoiceButton(choice);
                    });
                }
            }

            WaitingForChoiceInput = true;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (CurrentStory.canContinue)
            {
                ContinueStory();
            }
            else if (IsFinalWorkDay())
            {
                CalculateAndLoadFinalScene();
            }
            else
            {
                // TODO: Async load?
                SceneManager.LoadScene(BakerySceneName);
            }
        }
    }

    void ContinueStory()
    {
        CurrentStoryText = CurrentStory.Continue();
        CurrentStoryTags = CurrentStory.currentTags;
        IncreaseScoresForTags();

        // Update visuals
        RemoveChildren();
        CurrentStoryText = CurrentStoryText.Trim();
        CreateContentView(CurrentStoryText); // Display the text on screen!
    }

    void UpdateCharactersInScene() // This happens after the first choice in each scene, besides end
    {
        byte CharactersFromPreviousScene = CharactersInCurrentScene;
        CharactersInCurrentScene = BuildCharacterListFromCurrentStory();

        if (CurrentDay == WorkDayScenes.Length)
        {
            // On the final day, skip all of this
            return;
        }
        else if (CurrentDay == 1)
        {
            // On the first day, player has not and cannot choose; mark all characters pursued
            PursuedCharacters = (byte)CharacterFlags.All;
            return;
        }

        PursuedCharacters = (byte)(CharactersInCurrentScene & CharactersFromPreviousScene);
    }

    byte BuildCharacterListFromCurrentStory()
    {
        byte BuiltByte = (byte)CharacterFlags.None;

        foreach (string Tag in CurrentStoryTags)
        {
            switch (Tag.ToLower())
            {
                case "inky":
                    BuiltByte = (byte)(BuiltByte | (byte)CharacterFlags.Inky);
                    break;
                case "squill":
                    BuiltByte = (byte)(BuiltByte | (byte)CharacterFlags.Squill);
                    break;
                case "soup":
                    BuiltByte = (byte)(BuiltByte | (byte)CharacterFlags.Soup);
                    break;
                case "tort":
                    BuiltByte = (byte)(BuiltByte | (byte)CharacterFlags.Tort);
                    break;
                case "yuzu":
                    BuiltByte = (byte)(BuiltByte | (byte)CharacterFlags.Yuzu);
                    break;
                case "all":
                    BuiltByte = (byte)CharacterFlags.All;
                    break;
                default:
                    // For any other tag, do not add on to the byte
                    break;
            }
        }
        return BuiltByte;
    }

    void IncreaseScoresForTags()
    {
        foreach (string Tag in CurrentStoryTags)
        {
            switch (Tag.ToLower())
            {
                case "score":
                    ScoreAffectingOptionsDiscovered++;
                    break;
                case "good":
                    GoodScoreOptionsSelected++;
                    break;
                case "treat":
                    TimesYuzuFedTreat++;
                    break;
                default:
                    // For any other tag, do not add to any scores
                    break;
            }
        }
    }

    void CalculateAndLoadFinalScene()
    {
        if (TimesYuzuPetted >= YuzuPetsForSecretEnding && TimesYuzuFedTreat == GetAmountOfBranchingStoryDays())
        {
            // Secret Yuzu Ending always takes highest prio
            Debug.Log("ENDING: SECRET");
            // CurrentInkScript = ;
        }
        else if (PursuedCharacters != 0)
        {
            string PursuedCharacterName = GetNameForCharacterFlag((CharacterFlags)PursuedCharacters).ToUpper();
            if (GoodScoreOptionsSelected == ScoreAffectingOptionsDiscovered)
            {
                // Max score reached for pursued character == Good Ending! Yay!
                Debug.Log("ENDING: GOOD " + PursuedCharacterName);
                // CurrentInkScript = ;
            }
            else
            {
                // Average Ending for pursued character
                Debug.Log("ENDING: AVERAGE " + PursuedCharacterName);
                // CurrentInkScript = ;
            }
        }
        else
        {
            // No specific character was pursued; default to Average Ending for Inky
            Debug.Log("ENDING: DEFAULT");
            // CurrentInkScript = ;
        }
    }

    void ResetForNewDay()
    {
        CurrentDay++;
        HasFirstChoiceOccurred = false;

    }

    public void PetYuzu()
    {
        TimesYuzuPetted++;
    }

    string GetNameForCharacterFlag(CharacterFlags Flag)
    {
        switch (Flag)
        {
            case CharacterFlags.Inky:
                return "Inky";
            case CharacterFlags.Squill:
                return "Squilliam";
            case CharacterFlags.Soup:
                return "Lil' Soup";
            case CharacterFlags.Tort:
                return "Tortilla";
            case CharacterFlags.Yuzu:
                return "Yuzu";
            default:
                return "None";
        }
    }

    int GetAmountOfBranchingStoryDays()
    {
        return WorkDayScenes.Length - 2;
    }

    bool IsFinalWorkDay()
    {
        return CurrentDay == WorkDayScenes.Length;
    }

    // TEMPORARY BASIC INK EXAMPLE FUNCTIONS FOR UI DISPLAY

    // When we click the choice button, tell the story to choose that choice!
    void OnClickChoiceButton(Choice choice)
    {
        CurrentStory.ChooseChoiceIndex(choice.index);
        ContinueStory();
        if (!HasFirstChoiceOccurred)
        {
            HasFirstChoiceOccurred = true;
            UpdateCharactersInScene();
        }
        WaitingForChoiceInput = false;
    }

    // Creates a textbox showing the the line of text
    void CreateContentView(string text)
    {
        Text storyText = Instantiate(textPrefab) as Text;
        storyText.text = text;
        storyText.transform.SetParent(canvas.transform, false);
    }

    // Creates a button showing the choice text
    Button CreateChoiceView(string text)
    {
        // Creates the button from a prefab
        Button choice = Instantiate(buttonPrefab) as Button;
        choice.transform.SetParent(canvas.transform, false);

        // Gets the text from the button prefab
        Text choiceText = choice.GetComponentInChildren<Text>();
        choiceText.text = text;

        // Make the button expand to fit the text
        HorizontalLayoutGroup layoutGroup = choice.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.childForceExpandHeight = false;

        return choice;
    }

    // Destroys all the children of this gameobject (all the UI)
    void RemoveChildren()
    {
        int childCount = canvas.transform.childCount;
        for (int i = childCount - 1; i >= 0; --i)
        {
            Destroy(canvas.transform.GetChild(i).gameObject);
        }
    }

    [SerializeField]
    private Canvas canvas = null;

    // UI Prefabs
    [SerializeField]
    private Text textPrefab = null;
    [SerializeField]
    private Button buttonPrefab = null;
}
