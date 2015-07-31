// Created By Robin Reiter in 2015
// @robin7331 on Twitter

using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using SimpleJSON;

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

    [MenuItem("File/Set Debugging Port in VSCode")]
    static void SetDebuggingPortInVSCode()
    {
        if (!IsOnAMac())
        {
            UnityEngine.Debug.LogError("Debugging port can only be determined if you are on a Mac!");
            return;
        }

        int port = GetDebugPort();
        if (port == -1)
        {
            UnityEngine.Debug.LogWarning("Debugging port can only be determined if you are in Play Mode");
        }
        else if (port == 0)
        {
            UnityEngine.Debug.LogWarning("Debugging Port could not be determined.");
        }
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

    static int GetDebugPort()
    {
        if (!Application.isPlaying)
        {
            return -1;
        }


        string args = "-c /^Unity$/ -i 4tcp -a";

        //* Create your Process
        Process process = new Process();
        process.StartInfo.FileName = "lsof";
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();

        // since Unity is not thread safe, we need to call lsof in the mainthread and wait for its output
        string output = process.StandardOutput.ReadToEnd();
        ParseOutput(output);

        process.WaitForExit();

        return 1;
    }

    static void ParseOutput(string output)
    {
        string[] lines = output.Split('\n');

        foreach (string line in lines)
        {
            int port = ParseLine(line);

            if (port > -1)
            {
                // Port found so put into the build config of VSCode
                WritePortToVSCodeConfig(port);
                return;
            }
        }
    }

    static int ParseLine(string line)
    {
        int port = -1;

        if (line.StartsWith("Unity"))
        {
            string[] portions = line.Split(new string[] { "TCP *:" }, System.StringSplitOptions.None);

            if (portions.Length >= 2)
            {

                Regex digitsOnly = new Regex(@"[^\d]");
                string cleanPort = digitsOnly.Replace(portions[1], "");

                if (int.TryParse(cleanPort, out port))
                {
                    return port;
                }

            }

        }

        return port;
    }

    static void WritePortToVSCodeConfig(int port)
    {
        UnityEngine.Debug.Log("Current mono debugging port is " + port);
        //  UnityEngine.Debug.Log("Writing Port to VSCode config...");

        string filepath = ProjectPath() + "/.settings/launch.json";

        if (!File.Exists(filepath))
        {
            UnityEngine.Debug.Log("launch.json File does not yet exist. Will be created");
            CreateLaunchFile(filepath, port);
        }
        else
        {
            UpdateLaunchFile(filepath, port);
        }
    }

    // if there is a launch file, we can edit the port of the unity configuration
    static void UpdateLaunchFile(string filename, int port)
    {
        string rawContent = File.ReadAllText(filename);
        JSONNode N = JSON.Parse(rawContent);


        // try to update the launch file 
        JSONNode updated = SetUnityConfiguration(N, GenerateUnityConfiguration(port));

        // if unsuccessfull create a new launch file
        if (updated == null)
        {
            CreateLaunchFile(filename, port);
            return;
        }
        else
        {
            File.WriteAllText(filename, N.ToString());
        }

        //  UnityEngine.Debug.Log("Port is written to launch.json!");

    }

    // if no launch file exists, we create one
    static void CreateLaunchFile(string filename, int port)
    {
        JSONNode N = GenerateLaunchConfiguration(port);
        File.WriteAllText(filename, N.ToString());
    }

    static JSONNode GenerateLaunchConfiguration(int port)
    {
        JSONNode N = new JSONClass();
        N["version"] = "0.1.0";
        N["configurations"][-1] = GenerateUnityConfiguration(port);

        return N;
    }

    static JSONClass GenerateUnityConfiguration(int port)
    {
        JSONClass conf = new JSONClass();

        conf["name"] = "Unity";
        conf["type"] = "mono";
        conf["address"] = "localhost";
        conf["port"].AsInt = port;
        conf["sourceMaps"].AsBool = false;

        return conf;
    }

    static JSONNode SetUnityConfiguration(JSONNode N, JSONClass unityConfiguration)
    {
        //  UnityEngine.Debug.Log("Updateing conf..." + N.ToString());

        if (N != null && N["configurations"] != null)
        {
            int index = 0;
            bool found = false;
            foreach (JSONNode conf in N["configurations"].AsArray)
            {
                //  UnityEngine.Debug.Log(index + ": " + conf.ToString());
                if (conf["name"].Value == "Unity")
                {
                    //  UnityEngine.Debug.Log("Found at index " + index);
                    found = true;
                    break;
                }

                index++;
            }

            if (found)
            {
                N["configurations"][index] = unityConfiguration;
                return N;
            }
            else
            {
                N["configurations"][-1] = unityConfiguration;
                return N;
            }
        }

        return null;
    }

    static string RemoveCommentsFromFileContent(string content)
    {
        string[] lines = content.Split('\n');
        string newContent = "";
        foreach (string line in lines)
        {
            if (!line.Contains("// "))
                newContent += line;
        }

        return newContent;
    }

    static bool IsOnAMac()
    {
        return (Application.platform == RuntimePlatform.OSXEditor);
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
}
