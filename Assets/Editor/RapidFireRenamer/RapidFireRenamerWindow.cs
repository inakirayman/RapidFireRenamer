using UnityEngine;
using UnityEditor;
using System.IO;

public class RapidFireRenamerWindow : EditorWindow
{
    string _name = "Object Name";
    bool _hasNumberSuffix = false;

    [MenuItem("Tools/RapidFireRenamer")]
    public static void ShowWindow()
    {
        GetWindow<RapidFireRenamerWindow>("Rapid Fire Renamer");
    }

    void OnGUI()
    {
        _name = EditorGUILayout.TextField("Name", _name);

        if (Selection.count > 1)
        {
            _hasNumberSuffix = true;
        }
        else
        {
            _hasNumberSuffix = false;
        }



        if (GUILayout.Button("Rename"))
        {
            int incrementer = 1;

            foreach (var selectedObject in Selection.objects)
            {
                string name = _name;    
                
                if (_hasNumberSuffix)
                {

                   name += "_" + incrementer.ToString("D3");
                    
                }

                string assetPath = AssetDatabase.GetAssetPath(selectedObject);

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

                incrementer++;
            }

            AssetDatabase.Refresh();
        }
    }
}
