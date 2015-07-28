// Created By Robin Reiter in 2015
// @robin7331 on Twitter

using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class UniVSCode : MonoBehaviour
{

    [UnityEditor.Callbacks.OnOpenAssetAttribute()]
    static bool OnOpenedAssetCallback(int instanceID, int line)
    {
        // current path without the asset folder
        string appPath = System.IO.Path.GetDirectoryName(Application.dataPath);

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
            } else 
            {
                args = " -n -b \"com.microsoft.VSCode\" --args -g \"" + completeFilepath + ":" + line.ToString() + "\" -r";
            }

            // call 'open'
            Process proc = new Process();
            proc.StartInfo.FileName = "open";
            proc.StartInfo.Arguments = args;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            return true;
        }

        // let unity open other assets with other apps.
        return false;
    }
}
