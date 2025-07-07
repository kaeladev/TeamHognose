using FMOD.Studio;
using Ink.Runtime;
using System.Collections.Generic;
using TMPro;
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
    Big     = 1 << 5,
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
    public static StorySceneManager PersistentStoryInstance;

    // Public Data to set up pre-known start-of-day scenes
    public int              YuzuPetsForSecretEnding = 10;
    public TextAsset        GameIntroInkScene;
    public TextAsset[]      WorkDayInkScenes;
    public string           BakerySceneName;
    public string           FMODDialogueEventPaths = "event:/DLG/DLG_";

    // UI Stuff.....
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DialogueText;

    public TextMeshProUGUI      OptionText_2;
    public TextMeshProUGUI      OptionText_2_ChoiceA;
    public TextMeshProUGUI      OptionText_2_ChoiceB;

    public TextMeshProUGUI      OptionText_3;
    public TextMeshProUGUI      OptionText_3_ChoiceA;
    public TextMeshProUGUI      OptionText_3_ChoiceB;
    public TextMeshProUGUI      OptionText_3_ChoiceC;

    public TMP_FontAsset        FontForInky;
    public TMP_FontAsset        FontForSquilliam;
    public TMP_FontAsset        FontForSoup;
    public TMP_FontAsset        FontForTortilla;
    public TMP_FontAsset        FontForYuzu;

    private TMP_FontAsset       DefaultFont;
    private Canvas              UICanvas = null;
    private Canvas              PortraitCanvas = null;
    private Canvas              BGCanvas = null;

    // Persistent Data between scenes, for calculating ending
    private byte            PursuedCharacters = 0;
    private int             CurrentDay = 0;
    private int             TimesYuzuPetted = 0;
    private int             TimesYuzuFedTreat = 0;
    private int             ScoreAffectingOptionsDiscovered = 0;
    private int             GoodScoreOptionsSelected = 0;
    private Color           Visible = Color.white;
    private Color           GreyedOut = Color.gray;
    private Color           Invisible = new Color(255, 255, 255, 0);

    // Current Scene Data, to be loaded at runtime per day
    private bool            HasFirstChoiceOccurred = false;
    private bool            WaitingForChoiceInput = false;
    private byte            CharactersInCurrentScene = 0;
    private byte            CharactersSpeaking = 0;
    private TextAsset       CurrentInkScript;
    private Story           CurrentStory;
    private string          CurrentStoryText;
    private List<string>    CurrentStoryTags;

    void Awake()
    {
        bool CreateStorySceneManagerSingleton = !PersistentStoryInstance;

        if (!CreateStorySceneManagerSingleton)
        {
            // A second StorySceneManager has attempted to create itself, so destroy
            Destroy(gameObject);
            PersistentStoryInstance.ProgressToNewDay();
        }
        else
        {
            // The first time a StorySceneManager attempts to create itself, store as static instance
            PersistentStoryInstance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start() // First time startup of singleton instance
    {
        DefaultFont = DialogueText.font;
        ResetForNewDay();
    }

    void Update()
    {
        if (CurrentStory.currentChoices.Count > 0)
        {
            if (!WaitingForChoiceInput)
            {
                DisplayChoices();
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
                DeactivateExistingUI();
                GoToBakery();
            }
        }
    }

    void ContinueStory(bool FromBranch = false)
    {
        CurrentStoryText = CurrentStory.Continue();
        CurrentStoryTags = CurrentStory.currentTags;
        IncreaseScoresForTags();

        CharactersSpeaking = BuildCharacterListFromCurrentStory();

        if (FromBranch && !HasFirstChoiceOccurred)
        {
            HasFirstChoiceOccurred = true;
            UpdateCharactersInScene();
        }

        DeactivateExistingUI();

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

        CurrentStoryText += "\n Press Anywhere to Return To Bakery";
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
            case CharacterFlags.Big:
                return "Big Soup";
            default:
                return "None";
        }
    }

    string GetNamesForCharacterFlags(byte CharacterFlagsByte)
    {
        string BuiltString = "";

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
        else
        {
            BuiltString = BuiltString.Remove(BuiltString.Length - 1);
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
        Debug.Log("StorySceneManager: Starting Day " + PersistentStoryInstance.CurrentDay.ToString());
    }

    public void ResetForNewDay()
    {
        Debug.Log("StorySceneManager Resetting for New Day");

        HasFirstChoiceOccurred = false;

        Canvas[] Canvases = GetComponentsInChildren<Canvas>(true);
        foreach (Canvas i in Canvases)
        {
            if (i.gameObject.tag == "UI")
            {
                UICanvas = i;
            }
            else if (i.gameObject.tag == "Portraits")
            {
                PortraitCanvas = i;
            }
            else
            {
                BGCanvas = i;
            }
        }

        CurrentInkScript = CurrentDay == 0 ? GameIntroInkScene : WorkDayInkScenes[CurrentDay - 1];
        CurrentStoryTags = new List<string>();
        CurrentStory = new Story(CurrentInkScript.text);

        DeactivateExistingUI();
        ContinueStory();
        SetAllCanvasActive(true);
    }

    public void PetYuzu()
    {
        TimesYuzuPetted++;
    }

    void GoToBakery()
    {
        // TODO: Async load? Or fake loading screen for fun?
        SetAllCanvasActive(false);
        SceneManager.LoadScene(BakerySceneName);
    }

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
        TextMeshProUGUI DialogueBoxToUse;

        if (!IsStoryAtBranch())
        {
            DialogueBoxToUse = DialogueText;
        }
        else
        {
            if (CurrentStory.currentChoices.Count == 2)
            {
                DialogueBoxToUse = OptionText_2;
            }
            else
            {
                DialogueBoxToUse = OptionText_3;
            }
        }

        TMP_FontAsset FontToUse = GetFontForCurrentSpeaker();

        if (DialogueBoxToUse)
        {
            DialogueBoxToUse.font = FontToUse;
            DialogueBoxToUse.SetText(CurrentStoryText);
        }

        if (NameText && !IsStoryAtBranch())
        {
            if (CharactersSpeaking == 0)
            {
                NameText.SetText("You");
            }
            else
            {
                string[] CharacterNamesInScene = GetNamesForCharacterFlags(CharactersSpeaking).Split('/');
                if (CharacterNamesInScene.Length == 1)
                {
                    NameText.SetText(CharacterNamesInScene[0]);
                }
                else
                {
                    NameText.SetText("Group");
                }
            }

            NameText.font = FontToUse;
            ActivateExistingUIForTag("Dialogue");
        }

        UpdatePortraitCanvas(false);
        PlayDialogueSounds();
    }

    void DisplayChoices()
    {
        if (CurrentStory.currentChoices.Count == 2)
        {
            ActivateExistingUIForTag("Question_2");

            Choice ChoiceA = CurrentStory.currentChoices[0];
            Choice ChoiceB = CurrentStory.currentChoices[1];

            Button ButtonA = OptionText_2_ChoiceA.gameObject.GetComponentInParent<Button>();
            Button ButtonB = OptionText_2_ChoiceB.gameObject.GetComponentInParent<Button>();

            ButtonA.GetComponentInChildren<TextMeshProUGUI>().SetText(ChoiceA.text.Trim());
            ButtonB.GetComponentInChildren<TextMeshProUGUI>().SetText(ChoiceB.text.Trim());

            // Tell the buttons what to do when we press it
            ButtonA.onClick.AddListener(delegate {
                OnClickChoiceButton(ChoiceA);
            });

            ButtonB.onClick.AddListener(delegate {
                OnClickChoiceButton(ChoiceB);
            });

            return;
        }

        if (CurrentStory.currentChoices.Count == 3)
        {
            ActivateExistingUIForTag("Question_3");

            Choice ChoiceA = CurrentStory.currentChoices[0];
            Choice ChoiceB = CurrentStory.currentChoices[1];
            Choice ChoiceC = CurrentStory.currentChoices[2];

            Button ButtonA = OptionText_3_ChoiceA.gameObject.GetComponentInParent<Button>();
            Button ButtonB = OptionText_3_ChoiceB.gameObject.GetComponentInParent<Button>();
            Button ButtonC = OptionText_3_ChoiceC.gameObject.GetComponentInParent<Button>();

            ButtonA.GetComponentInChildren<TextMeshProUGUI>(true).SetText(ChoiceA.text.Trim());
            ButtonB.GetComponentInChildren<TextMeshProUGUI>(true).SetText(ChoiceB.text.Trim());
            ButtonC.GetComponentInChildren<TextMeshProUGUI>(true).SetText(ChoiceC.text.Trim());

            // Tell the buttons what to do when we press it
            ButtonA.onClick.AddListener(delegate {
                OnClickChoiceButton(ChoiceA);
            });

            ButtonB.onClick.AddListener(delegate {
                OnClickChoiceButton(ChoiceB);
            });

            ButtonC.onClick.AddListener(delegate {
                OnClickChoiceButton(ChoiceC);
            });

            return;
        }

        Debug.Log("Unsupported number of choices in StorySceneManager::DisplayChoices() : " + CurrentStory.currentChoices.Count);
    }

    void UpdatePortraitCanvas(bool Deactivate)
    {
        if (PortraitCanvas)
        {
            string CharacterNamesSpeaking = GetNamesForCharacterFlags(CharactersSpeaking);
            string CharacterNamesInScene = GetNamesForCharacterFlags(CharactersInCurrentScene);

            if (CharacterNamesSpeaking == "None")
            {
                BGCanvas.GetComponentInChildren<Image>(true).color = GreyedOut;
            }
            else
            {
                BGCanvas.GetComponentInChildren<Image>(true).color = Visible;
            }

            Image[] Images = PortraitCanvas.GetComponentsInChildren<Image>(true);
            foreach (Image i in Images)
            {
                if (Deactivate || !HasFirstChoiceOccurred || IsStoryAtBranch())
                {
                    i.color = Invisible;
                }
                else if (i.tag == "All")
                {
                    i.color = Visible;
                }
                else if (CharacterNamesSpeaking.Contains(i.tag))
                {
                    i.color = Visible; // Actively speaking character should be highlighted
                }
                else if (CharacterNamesInScene.Contains(i.tag))
                {
                    i.color = GreyedOut; // Characters involved but not speaking are greyed out
                }
                else if (CurrentDay >= 2 && CurrentDay < 5)
                {
                    i.color = Invisible;    // Characters not involved leave the room these days
                }
                else // On Day 1 and 5, whole group hangs around for the whole scene
                {
                    i.color = GreyedOut;
                }
            }
        }
    }

    void PlayDialogueSounds()
    {
        if (PortraitCanvas)
        {
            if (CharactersSpeaking == 0 || CharactersSpeaking == (byte)CharacterFlags.All)
            {
                return;
            }

            string CharacterNamesInScene = GetNamesForCharacterFlags(CharactersSpeaking);

            Image[] Images = PortraitCanvas.GetComponentsInChildren<Image>(true);
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

    void SetAllCanvasActive(bool ShouldBeActive)
    {
        BGCanvas.gameObject.SetActive(ShouldBeActive);
        PortraitCanvas.gameObject.SetActive(ShouldBeActive);
        UICanvas.gameObject.SetActive(ShouldBeActive);
    }

    // Deactivates all the children of this canvas gameobject (all the UI)
    void DeactivateExistingUI()
    {
        OptionText_2_ChoiceA.gameObject.GetComponentInParent<Button>(true).onClick.RemoveAllListeners();
        OptionText_2_ChoiceB.gameObject.GetComponentInParent<Button>(true).onClick.RemoveAllListeners();
        OptionText_3_ChoiceB.gameObject.GetComponentInParent<Button>(true).onClick.RemoveAllListeners();
        OptionText_3_ChoiceB.gameObject.GetComponentInParent<Button>(true).onClick.RemoveAllListeners();
        OptionText_3_ChoiceC.gameObject.GetComponentInParent<Button>(true).onClick.RemoveAllListeners();
        
        if (UICanvas)
        {
            Image[] Images = UICanvas.GetComponentsInChildren<Image>(true);
            foreach (Image i in Images)
            {
                i.gameObject.SetActive(false);
            }
        }

        UpdatePortraitCanvas(true);
    }

    void ActivateExistingUIForTag(string UITag)
    {
        Image[] Images = UICanvas.GetComponentsInChildren<Image>(true);
        foreach (Image i in Images)
        {
            if (i.gameObject.tag == UITag)
            {
                i.gameObject.SetActive(true);
            }
        }
    }

    bool IsStoryAtBranch()
    {
        return CurrentStory.currentChoices.Count > 0;
    }

    TMP_FontAsset GetFontForCurrentSpeaker()
    {
        if (CharactersSpeaking > 0)
        {
            string[] CharacterNamesInScene = GetNamesForCharacterFlags(CharactersSpeaking).Split('/');
            if (CharacterNamesInScene.Length == 1)
            {
                switch ((CharacterFlags)CharactersSpeaking)
                {
                    case CharacterFlags.Inky:
                        return FontForInky;
                    case CharacterFlags.Squill:
                        return FontForSquilliam;
                    case CharacterFlags.Soup:
                    case CharacterFlags.Big:
                        return FontForSoup;
                    case CharacterFlags.Tort:
                        return FontForTortilla;
                    case CharacterFlags.Yuzu:
                        return FontForYuzu;
                    default:
                        break;
                }
            }
        }
        return DefaultFont;
    }
}
