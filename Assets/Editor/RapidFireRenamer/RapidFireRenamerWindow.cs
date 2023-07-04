using UnityEngine;
using UnityEditor;
using System.IO;

public class RapidFireRenamerWindow : EditorWindow
{
    string _name = "Name";
    

    [Header("Incrementation")]
    bool _hasIncrementation = false;
    int _startIncrementation = 1;
    int _incrementBy = 1;
    IncrementationAffixe _incrementationAffixe = IncrementationAffixe.Suffix;
    public enum IncrementationAffixe
    {
        Prefix,
        Suffix
    }




    [MenuItem("Tools/RapidFireRenamer")]
    public static void ShowWindow()
    {
        GetWindow<RapidFireRenamerWindow>("Rapid Fire Renamer");
    }

    void OnGUI()
    {

        _name = EditorGUILayout.TextField("New Name", _name);

        GUILayout.Label("", EditorStyles.boldLabel);
        IncrementationInputs();

        if (GUILayout.Button("Rename"))
        {


            if (Selection.count > 1)
            {
                _hasIncrementation = true;
            }
            else
            {
                _hasIncrementation = false;
            }

            int incrementer = _startIncrementation;

            foreach (var selectedObject in Selection.objects)
            {
                string name = _name;

                if (_hasIncrementation)
                {

                    if(IncrementationAffixe.Suffix == _incrementationAffixe)
                    {
                        name +=  "_" + incrementer.ToString("D3");
                    }
                    else if(IncrementationAffixe.Prefix == _incrementationAffixe)
                    {
                        name = incrementer.ToString("D3") + "_" + name;
                    }

                    incrementer += _incrementBy;
                }





                string assetPath = AssetDatabase.GetAssetPath(selectedObject);
                Rename(name, assetPath);
            }

            AssetDatabase.Refresh();
        }
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

    private static void Rename(string name, string assetPath)
    {
        if (AssetDatabase.IsValidFolder(assetPath))
        {
            string directory = Path.GetDirectoryName(assetPath);
            string newFolderPath = directory + "/" + name;
            AssetDatabase.MoveAsset(assetPath, newFolderPath);
            Debug.Log("valid");
        }
        else
        {
            string directory = Path.GetDirectoryName(assetPath);
            string newFileName = directory + "/" + name + Path.GetExtension(assetPath);
            AssetDatabase.MoveAsset(assetPath, newFileName);
            Debug.Log("invalid");
        }
    }
}


