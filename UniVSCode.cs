// Created By Robin Reiter in 2015
// @robin7331 on Twitter

using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class UniVSCode : MonoBehaviour
{

    [MenuItem("Edit/Use VSCode", true)]
    static bool ValidateUncheckVSCode()
    {
        bool state = UseVSCode();
        Menu.SetChecked("Edit/Use VSCode", state);
        return IsOnAMac();
    }

    [MenuItem("Edit/Use VSCode")]
    static void UncheckVSCode()
    {
        bool state = UseVSCode();
        Menu.SetChecked("Edit/Use VSCode", !state);
        EditorPrefs.SetBool("UseVSCode", !state);
    }

    [MenuItem("File/Open Project in VSCode")]
    static void OpenProjectInVSCode()
    {
        UnityEngine.Debug.Log(ProjectPath());
        string args = " -n -b \"com.microsoft.VSCode\" --args \"" + ProjectPath() + "\" -r";
        CallVSCode(args);
    }

    static bool UseVSCode()
    {
        // if this is the first start we will enable VSCode by default
        if (!EditorPrefs.HasKey("UseVSCode"))
        {
            EditorPrefs.SetBool("UseVSCode", true);
            return true;
        }

        return EditorPrefs.GetBool("UseVSCode");
    }


    [UnityEditor.Callbacks.OnOpenAssetAttribute()]
    static bool OnOpenedAssetCallback(int instanceID, int line)
    {
        // bail out if we are not on a Mac or if we don't want to use VSCode
        if (!IsOnAMac() || !UseVSCode())
        {
            return false;
        }


        // current path without the asset folder
        string appPath = ProjectPath();

        // determine asset that has been double clicked in the project view
        UnityEngine.Object selected = EditorUtility.InstanceIDToObject(instanceID);

        // only recognize c# files
        if (selected.GetType().ToString() == "UnityEditor.MonoScript")
        {
            // determine the complete absolute path to the asset file
            string completeFilepath = appPath + "/" + AssetDatabase.GetAssetPath(selected);

            string args = null;
            if (line == -1)
            {
                args = " -n -b \"com.microsoft.VSCode\" --args \"" + completeFilepath + "\" -r";
            }
            else
            {
                args = " -n -b \"com.microsoft.VSCode\" --args -g \"" + completeFilepath + ":" + line.ToString() + "\" -r";
            }

            // call 'open'
            CallVSCode(args);

            return true;
        }

        // let unity open other assets with other apps.
        return false;
    }
    
    static string ProjectPath()
    {
        return System.IO.Path.GetDirectoryName(Application.dataPath);
    }

    static void CallVSCode(string args)
    {
        Process proc = new Process();
        proc.StartInfo.FileName = "open";
        proc.StartInfo.Arguments = args;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.Start();
    }

    static bool IsOnAMac()
    {
        return (Application.platform == RuntimePlatform.OSXEditor);
    }
}
