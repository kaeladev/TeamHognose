using FMOD.Studio;
using Ink.Runtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum CharacterFlags: byte
{
    None    = 0,
    Inky    = 1 << 0,
    Squill  = 1 << 1,
    Soup    = 1 << 2,
    Tort    = 1 << 3,
    Yuzu    = 1 << 4,
    All     = Inky | Squill | Soup | Tort | Yuzu,
}

enum EmotionFlags : int
{
    happy = 0,
    sad = 1,
    question = 2,
    sassy = 3, 
    hello = 4,
    bye = 5,
    alerted = 6,
    answer = 7,
    tired = 8,
    angry = 9,
    relief = 10,
    surprise = 11,
    sleeping = 12,
    dialogue = 13
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
    public TextAsset        GameIntroInkScene;
    public TextAsset[]      WorkDayInkScenes;
    public string           BakerySceneName;
    public string           FMODDialogueEventPaths = "event:/DLG/DLG_";

    // Persistent Data between scenes, for calculating ending
    private byte            PursuedCharacters = 0;
    private int             CurrentDay = 0;
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

    void Awake()
    {
        bool CreateStorySceneManagerSingleton = !PersistentInstance;

        if (!CreateStorySceneManagerSingleton)
        {
            // A second StorySceneManager has attempted to create itself, so destroy
            Destroy(gameObject);
            PersistentInstance.ProgressToNewDay();
        }
        else
        {
            // The first time a StorySceneManager attempts to create itself, store as static instance
            PersistentInstance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start() // First time startup of singleton instance
    {
        ResetForNewDay();
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
                    Button button = DisplayChoices(choice.text.Trim());
                    // Tell the button what to do when we press it
                    button.onClick.AddListener(delegate {
                        OnClickChoiceButton(choice);
                    });
                }
            }

            WaitingForChoiceInput = true;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (CurrentStory.canContinue)
            {
                ContinueStory();
            }
            else if (IsFinalWorkDay())
            {
                CalculateAndLoadFinalScene();
            }
            else if (SceneManager.GetActiveScene().name != BakerySceneName)
            {
                RemoveExistingUI();
                GoToBakery();
            }
        }
    }

    void ContinueStory(bool FromBranch = false)
    {
        CurrentStoryText = CurrentStory.Continue();
        CurrentStoryTags = CurrentStory.currentTags;
        IncreaseScoresForTags();

        if (FromBranch && !HasFirstChoiceOccurred)
        {
            HasFirstChoiceOccurred = true;
            UpdateCharactersInScene();
        }

        RemoveExistingUI();

        CurrentStoryText = CurrentStoryText.Trim();
        MarkEndOfSceneIfRelevant();

        DisplayStoryText(); // Display the text on screen!
    }

    void MarkEndOfSceneIfRelevant()
    {
        if (CurrentStory.canContinue || CurrentStory.currentChoices.Count > 0)
        {
            return;
        }

        CurrentStoryText += "\nScene Over; Press Anywhere to Return To Bakery";
        Debug.Log("Story Stats after Day " + CurrentDay.ToString()
                    + "\n\t Potential Pursued Characters: " + GetNamesForPotentíalPursuedCharacters()
                    + "\n\t Total Score Options Discovered: " + ScoreAffectingOptionsDiscovered.ToString()
                    + "\n\t Total Good Score Options Chosen: " + GoodScoreOptionsSelected.ToString()
                    + "\n\t Yuzu Fed Treats: " + TimesYuzuFedTreat.ToString() + " Times"
                    + "\n\t Yuzu Petted: " + TimesYuzuPetted.ToString() + "/" + YuzuPetsForSecretEnding.ToString() + " Times");
    }

    void UpdateCharactersInScene() // This happens after the first choice in each scene, besides end
    {
        byte CharactersFromPreviousScene = CharactersInCurrentScene;
        CharactersInCurrentScene = BuildCharacterListFromCurrentStory();

        if (CurrentDay == WorkDayInkScenes.Length)
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

    int FindEmotionValueInTags()
    {
        EmotionFlags EmotionFound = EmotionFlags.dialogue;

        foreach (string Tag in CurrentStoryTags)
        {
            switch (Tag.ToLower())
            {
                case "happy":
                    EmotionFound = EmotionFlags.dialogue;
                    break;
                case "sad":
                    EmotionFound = EmotionFlags.sad;
                    break;
                case "question":
                    EmotionFound = EmotionFlags.question;
                    break;
                case "sassy":
                    EmotionFound = EmotionFlags.sassy;
                    break;
                case "bye":
                    EmotionFound = EmotionFlags.bye;
                    break;
                case "alerted":
                    EmotionFound = EmotionFlags.alerted;
                    break;
                case "answer":
                    EmotionFound = EmotionFlags.answer;
                    break;
                case "tired":
                    EmotionFound = EmotionFlags.tired;
                    break;
                case "angry":
                    EmotionFound = EmotionFlags.angry;
                    break;
                case "relief":
                    EmotionFound = EmotionFlags.relief;
                    break;
                case "surprise":
                    EmotionFound = EmotionFlags.surprise;
                    break;
                case "sleeping":
                    EmotionFound = EmotionFlags.sleeping;
                    break;
                default:
                    // If any other tags found, stick with existing tag
                    break;
            }
        }

        return (int)EmotionFound;
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
            if (GoodScoreOptionsSelected > 0 && GoodScoreOptionsSelected == ScoreAffectingOptionsDiscovered)
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

    string GetNamesForCharacterFlags(byte CharacterFlagsByte)
    {
        string BuiltString = "/";

        if ((CharacterFlagsByte & (byte)CharacterFlags.Inky) != 0)
        {
            BuiltString += "Inky/";
        }
        if ((CharacterFlagsByte & (byte)CharacterFlags.Squill) != 0)
        {
            BuiltString += "Squilliam/";
        }
        if ((CharacterFlagsByte & (byte)CharacterFlags.Soup) != 0)
        {
            BuiltString += "Lil' Soup/";
        }
        if ((CharacterFlagsByte & (byte)CharacterFlags.Tort) != 0)
        {
            BuiltString += "Tortilla/";
        }
        if ((CharacterFlagsByte & (byte)CharacterFlags.Yuzu) != 0)
        {
            BuiltString += "Yuzu/";
        }

        if (BuiltString.Length < 2)
        {
            BuiltString = "None";
        }

        return BuiltString;
    }

    string GetNamesForPotentíalPursuedCharacters()
    {
        return GetNamesForCharacterFlags(PursuedCharacters);
    }

    int GetAmountOfBranchingStoryDays()
    {
        return WorkDayInkScenes.Length - 2;
    }

    bool IsFinalWorkDay()
    {
        return CurrentDay == WorkDayInkScenes.Length;
    }

    public void ProgressToNewDay()
    {
        CurrentDay++;
        ResetForNewDay();
        Debug.Log("StorySceneManager: Starting Day " + PersistentInstance.CurrentDay.ToString());
    }

    public void ResetForNewDay()
    {
        Debug.Log("StorySceneManager Resetting for New Day");

        HasFirstChoiceOccurred = false;

        UICanvas = Camera.main.GetComponentInChildren<Canvas>();

        PortraitCanvas = GetComponentInChildren<Canvas>();

        CurrentInkScript = CurrentDay == 0 ? GameIntroInkScene : WorkDayInkScenes[CurrentDay - 1];
        CurrentStoryTags = new List<string>();
        CurrentStory = new Story(CurrentInkScript.text);

        RemoveExistingUI();
        ContinueStory();
    }

    public void PetYuzu()
    {
        TimesYuzuPetted++;
    }

    void GoToBakery()
    {
        // TODO: Async load? Or fake loading screen for fun?
        SceneManager.LoadScene(BakerySceneName);
    }

    // TEMPORARY BASIC INK EXAMPLE FUNCTIONS FOR UI DISPLAY
    // TODO: MAKE PRETTIER

    // When we click the choice button, tell the story to choose that choice!
    void OnClickChoiceButton(Choice choice)
    {
        CurrentStory.ChooseChoiceIndex(choice.index);
        ContinueStory(true);
        WaitingForChoiceInput = false;
    }

    // Creates a textbox showing the the line of text
    void DisplayStoryText()
    {
        Text storyText = Instantiate(textPrefab) as Text;
        storyText.text = CurrentStoryText;
        storyText.transform.SetParent(UICanvas.transform, false);

        UpdatePortraitCanvas(false);
        PlayDialogueSounds();
    }

    // Creates a button showing the choice text
    Button DisplayChoices(string text)
    {
        // Creates the button from a prefab
        Button choice = Instantiate(buttonPrefab) as Button;
        choice.transform.SetParent(UICanvas.transform, false);

        // Gets the text from the button prefab
        Text choiceText = choice.GetComponentInChildren<Text>();
        choiceText.text = text;

        // Make the button expand to fit the text
        HorizontalLayoutGroup layoutGroup = choice.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.childForceExpandHeight = false;

        return choice;
    }

    void UpdatePortraitCanvas(bool Deactivate)
    {
        Color Visible = Color.white;
        Color Invisible = Color.white;
        Invisible.a = 0;

        if (PortraitCanvas)
        {
            byte CharactersSpeaking = BuildCharacterListFromCurrentStory();
            if (CharactersSpeaking == 0)
            {
                return;
            }

            string CharacterNamesInScene = GetNamesForCharacterFlags(CharactersSpeaking);
            
            Image[] Images = PortraitCanvas.GetComponentsInChildren<Image>();
            foreach (Image i in Images)
            {
                if (Deactivate || !HasFirstChoiceOccurred)
                {
                    i.color = Invisible;
                }
                else if (i.tag == "All")
                {
                    i.color = Visible;
                }
                else if (CharacterNamesInScene.Contains(i.tag))
                {
                    i.color = Visible;
                }
                else
                {
                    i.color = Invisible;
                }
            }
        }
    }

    void PlayDialogueSounds()
    {
        if (PortraitCanvas)
        {
            byte CharactersSpeaking = BuildCharacterListFromCurrentStory();
            if (CharactersSpeaking == 0 || CharactersSpeaking == 31)
            {
                return;
            }

            string CharacterNamesInScene = GetNamesForCharacterFlags(CharactersSpeaking);

            Image[] Images = PortraitCanvas.GetComponentsInChildren<Image>();
            foreach (Image i in Images)
            {
                if (CharacterNamesInScene.Contains(i.tag))
                {
                    string CharacterDialogueEventPath = FMODDialogueEventPaths + i.tag;
                    EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(CharacterDialogueEventPath);
                    FMOD.RESULT Result = instance.setParameterByName("Emotion", FindEmotionValueInTags());

                    instance.start();
                    instance.release();
                }
            }
        }
    }

    // Destroys all the children of this canvas gameobject (all the UI)
    void RemoveExistingUI()
    {
        if (UICanvas)
        {
            int childCount = UICanvas.transform.childCount;
            for (int i = childCount - 1; i >= 0; --i)
            {
                Destroy(UICanvas.transform.GetChild(i).gameObject);
            }
        }

        UpdatePortraitCanvas(true);
    }

    private Canvas UICanvas = null;
    private Canvas PortraitCanvas = null;

    // UI Prefabs
    [SerializeField]
    private Text textPrefab = null;
    [SerializeField]
    private Button buttonPrefab = null;
}
