using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class RapidFireRenamerWindow : EditorWindow
{
    private enum Tab
    {
        Rename,
        Replace,
        Credits
    }

    private List<string> replaceLog = new List<string>();
    private Vector2 replaceLogScrollPosition;


    private Tab currentTab = Tab.Rename;

    private string _name = "Name";
    private Vector2 scrollPosition;

    // Word replacement variables
    private string wordToReplace = "";
    private string replacementWord = "";
    private bool isCaseSensitive = false;

    [Header("Incrementation")]
    private bool _hasIncrementation = false;
    private int _startIncrementation = 1;
    private int _incrementBy = 1;
    private IncrementationAffixe _incrementationAffixe = IncrementationAffixe.Suffix;
    public enum IncrementationAffixe
    {
        Prefix,
        Suffix
    }

    private string _prefix = "";
    private string _suffix = "";

    private List<string> renameLog = new List<string>();
    private Vector2 logScrollPosition;

    [MenuItem("Tools/RapidFireRenamer")]
    public static void ShowWindow()
    {
        RapidFireRenamerWindow window = GetWindow<RapidFireRenamerWindow>("Rapid Fire Renamer");
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/RapidFireRenamer/32x32_ICON.png"); // Replace "Prefab Icon" with the desired icon name
        window.titleContent = new GUIContent("Rapid Fire Renamer", icon);
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawTabs();

        switch (currentTab)
        {
            case Tab.Rename:
                DrawRenameTab();
                break;
            case Tab.Replace:
                DrawReplaceTab();
                break;
            case Tab.Credits:
                DrawCreditsTab();
                break;
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawTabs()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUIStyle selectedTabStyle = EditorStyles.toolbarButton;
        GUIStyle unselectedTabStyle = EditorStyles.toolbarButton;

        Color selectedColor = new Color(68f / 255f, 98f / 255f, 122f / 255f);  // Set the desired color for the selected tab using RGB values

        if (currentTab == Tab.Rename)
        {
            selectedTabStyle.normal.background = MakeTexture(selectedColor);
        }
        else
        {
            unselectedTabStyle.normal.background = EditorStyles.toolbarButton.active.background;
        }

        if (GUILayout.Button("Rename", currentTab == Tab.Rename ? selectedTabStyle : unselectedTabStyle))
        {
            currentTab = Tab.Rename;
        }

        if (currentTab == Tab.Replace)
        {
            selectedTabStyle.normal.background = MakeTexture(selectedColor);
        }
        else
        {
            unselectedTabStyle.normal.background = EditorStyles.toolbarButton.active.background;
        }

        if (GUILayout.Button("Replace", currentTab == Tab.Replace ? selectedTabStyle : unselectedTabStyle))
        {
            currentTab = Tab.Replace;
        }

        //if (currentTab == Tab.Credits)
        //{
        //    selectedTabStyle.normal.background = MakeTexture(selectedColor);
        //}
        //else
        //{
        //    unselectedTabStyle.normal.background = EditorStyles.toolbarButton.active.background;
        //}

        //if (GUILayout.Button("Credits", currentTab == Tab.Credits ? selectedTabStyle : unselectedTabStyle))
        //{
        //    currentTab = Tab.Credits;
        //}

        GUILayout.EndHorizontal();
    }

    private Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private void DrawCreditsTab()
    {
        GUILayout.Label("Credits", EditorStyles.boldLabel);
        GUILayout.Label("Developer: Iñaki Van de Moortele");
        GUILayout.Label("Version: 1.0");
    }

    #region Rename Tab Functions

    private void DrawRenameTab()
    {
        _name = EditorGUILayout.TextField("New Name", _name);

        GUILayout.Space(10);
        IncrementationInputs();

        GUILayout.Space(10);
        GUILayout.Label("Affixe", EditorStyles.boldLabel);
        _prefix = EditorGUILayout.TextField("Prefix", _prefix);
        _suffix = EditorGUILayout.TextField("Suffix", _suffix);

        AffixesLogic();

        if (GUILayout.Button("Full Rename"))
        {
            FullRenameButtonLogic();
        }

        GUILayout.Space(10);
        SetPreview();

        GUILayout.Space(10);
        DrawRenameLog();
    }

    private void AffixesLogic()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Prefix", GUILayout.ExpandWidth(true)))
        {
            renameLog.Clear();
            foreach (var selectedObject in Selection.objects)
            {
                string name = selectedObject.name;

                name = _prefix + "_" + name;

                string logEntry = $"{selectedObject.name}  ->  {name}";
                renameLog.Add(logEntry);

                string assetPath = AssetDatabase.GetAssetPath(selectedObject);
                Rename(name, assetPath);

                
            }
        }

        if (GUILayout.Button("Remove First Prefix", GUILayout.ExpandWidth(true)))
        {
            renameLog.Clear();
            foreach (var selectedObject in Selection.objects)
            {
                string name = selectedObject.name;

                // Check if the name contains an underscore
                int underscoreIndex = name.IndexOf('_');
                if (underscoreIndex != -1)
                {
                    // Remove everything up to the first underscore
                    name = name.Substring(underscoreIndex + 1);
                }

                string logEntry = $"{selectedObject.name}  ->  {name}";
                renameLog.Add(logEntry);

                string assetPath = AssetDatabase.GetAssetPath(selectedObject);
                Rename(name, assetPath);

                
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Suffix"))
        {
            renameLog.Clear();
            foreach (var selectedObject in Selection.objects)
            {
                string name = selectedObject.name;

                name += "_" + _suffix;

                string logEntry = $"{selectedObject.name}  ->  {name}";
                renameLog.Add(logEntry);

                string assetPath = AssetDatabase.GetAssetPath(selectedObject);
                Rename(name, assetPath);

             
            }
        }

        if (GUILayout.Button("Remove Last Suffix"))
        {
            renameLog.Clear();
            foreach (var selectedObject in Selection.objects)
            {
                string name = selectedObject.name;

                // Check if the name contains an underscore
                int underscoreIndex = name.LastIndexOf('_');
                if (underscoreIndex != -1)
                {
                    // Remove everything after the last underscore
                    name = name.Substring(0, underscoreIndex);
                }

                string logEntry = $"{selectedObject.name}  ->  {name}";
                renameLog.Add(logEntry);

                string assetPath = AssetDatabase.GetAssetPath(selectedObject);
                Rename(name, assetPath);

            }
        }
        GUILayout.EndHorizontal();
    }

    private void IncrementationInputs()
    {
        GUILayout.Label("Incrementation", EditorStyles.boldLabel);
        _incrementationAffixe = (IncrementationAffixe)EditorGUILayout.EnumPopup("Incrementation Affixe", _incrementationAffixe);
        _startIncrementation = EditorGUILayout.IntField("Start Incrementation", _startIncrementation);
        _incrementBy = EditorGUILayout.IntField("Increment By", _incrementBy);
        if (_incrementBy == 0)
        {
            _incrementBy = 1;
        }
    }

    private void FullRenameButtonLogic()
    {
        _hasIncrementation = Selection.count > 1;

        int incrementer = _startIncrementation;

        renameLog.Clear(); // Clear the log before renaming files

        foreach (var selectedObject in Selection.objects)
        {
            string name = _name;

            if (_hasIncrementation)
            {
                if (IncrementationAffixe.Suffix == _incrementationAffixe)
                {
                    name += "_" + incrementer.ToString("D3");
                }
                else if (IncrementationAffixe.Prefix == _incrementationAffixe)
                {
                    name = incrementer.ToString("D3") + "_" + name;
                }
                incrementer += _incrementBy;
            }

            name = AddAffixes(name);

            string logEntry = $"{selectedObject.name}  ->  {name}";
            renameLog.Add(logEntry);

            string assetPath = AssetDatabase.GetAssetPath(selectedObject);
            Rename(name, assetPath);

            
        }

        AssetDatabase.Refresh();
    }

    private string AddAffixes(string name)
    {
        if (string.IsNullOrEmpty(_prefix) && string.IsNullOrEmpty(_suffix))
        {
            return name;
        }
        else if (string.IsNullOrEmpty(_prefix))
        {
            return name + "_" + _suffix;
        }
        else if (string.IsNullOrEmpty(_suffix))
        {
            return _prefix + "_" + name;
        }
        else
        {
            return _prefix + "_" + name + "_" + _suffix;
        }
    }

    private void SetPreview()
    {
        string name = _name;
        name = AddAffixes(name);


        GUILayout.Label("Preview");

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 15;
        GUILayout.Label(name, labelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        int incrementer = _startIncrementation;

        for (int i = 0; i < 2; i++)
        {
            name = _name;

            if (IncrementationAffixe.Suffix == _incrementationAffixe)
            {
                name += "_" + incrementer.ToString("D3");
            }
            else if (IncrementationAffixe.Prefix == _incrementationAffixe)
            {
                name = incrementer.ToString("D3") + "_" + name;
            }
            incrementer += _incrementBy;

            name = AddAffixes(name);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(name, labelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }

    private void DrawRenameLog()
    {
        GUILayout.Space(10);
        GUILayout.Label("Rename Log", EditorStyles.boldLabel);

        logScrollPosition = GUILayout.BeginScrollView(logScrollPosition);
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontStyle = FontStyle.Bold;
        foreach (string entry in renameLog)
        {
           
            GUILayout.Label(entry, style);
        }

        GUILayout.EndScrollView();
    }

    #endregion


    #region Replace Tab Functions
    


    private void DrawReplaceTab()
    {
        GUILayout.Label("Replace", EditorStyles.boldLabel);

        wordToReplace = EditorGUILayout.TextField("Word to Replace", wordToReplace);
        replacementWord = EditorGUILayout.TextField("Replacement Word", replacementWord);
        isCaseSensitive = EditorGUILayout.Toggle("Case Sensitive", isCaseSensitive);

        if (GUILayout.Button("Replace"))
        {
            ReplaceWord();
        }

        GUILayout.Space(10);
        DrawReplaceLog();
    }

    private void ReplaceWord()
    {
        replaceLog.Clear(); // Clear the log before replacing words

        foreach (var selectedObject in Selection.objects)
        {
            string originalName = selectedObject.name; // Store the original name

            string newName = originalName;
            if (isCaseSensitive)
            {
                newName = originalName.Replace(wordToReplace, replacementWord);
            }
            else
            {
                newName = originalName.Replace(wordToReplace, replacementWord, System.StringComparison.OrdinalIgnoreCase);
            }

            if (newName != originalName) // Check if the name has changed
            {
                string assetPath = AssetDatabase.GetAssetPath(selectedObject);
                Rename(newName, assetPath);

                string logEntry = $"{originalName}  ->  {newName}"; // Log the original and new name
                replaceLog.Add(logEntry);
            }
        }
    }


    private void DrawReplaceLog()
    {
        GUILayout.Space(10);
        GUILayout.Label("Replace Log", EditorStyles.boldLabel);

        replaceLogScrollPosition = GUILayout.BeginScrollView(replaceLogScrollPosition);
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontStyle = FontStyle.Bold;

        foreach (string entry in replaceLog)
        {
            
            GUILayout.Label(entry, style);
        }
        GUILayout.EndScrollView();
    }




    #endregion


    private void Rename(string newName, string assetPath)
    {
        string directory = Path.GetDirectoryName(assetPath);
        string newPath = Path.Combine(directory, newName);
        AssetDatabase.RenameAsset(assetPath, newName);
        AssetDatabase.ImportAsset(newPath);
    }

}
