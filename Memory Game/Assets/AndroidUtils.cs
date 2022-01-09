#if UNITY_EDITOR && UNITY_ANDROID

using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Kybernetik
{
    static partial class AndroidUtils
    {
        /************************************************************************************************************************/
    
        [UnityEditor.MenuItem("File/Connect to Android Device")]
        private static void ConnectToAndroidDevice()
        {
            // Start ADB and change to tcpip mode.
    
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = GetPathToADB(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
    
            startInfo.Arguments = "tcpip 5555";
            var process = new Process { StartInfo = startInfo };
            StartProcess(process);
    
            // Wait for that process to finish then wait a bit longer for it to be properly ready.
            // If it isn't ready in time, it will give an error and you can just run this command again.
            process.WaitForExit();
            System.Threading.Thread.Sleep(1000);
    
            // Get ADB to output a list of all IP addresses.
            startInfo.Arguments = "shell ip route";
            process = new Process { StartInfo = startInfo };
    
            process.OutputDataReceived += (sender, e) =>
            {
                var data = e.Data;
                if (data == null)
                    return;
    
                // Get the last IPv4 address in the local network.
                var start = data.LastIndexOf("192.168.");
                if (start > 0)
                {
                    var end = start;
                    while (end < data.Length)
                    {
                        if (char.IsWhiteSpace(data[end]))
                            break;
    
                        end++;
                    }
    
                    var device = data.Substring(start, end - start);
                    Debug.Log("Found Device " + device);
    
                    // If we found an address, connect to it.
                    ConnectToDevice(process, device);
                }
            };
    
            StartProcess(process);
        }
    
        /************************************************************************************************************************/
    
        private static string GetPathToADB()
        {
            var path = UnityEditor.EditorPrefs.GetString("AndroidSdkRoot");
            path += "/platform-tools/adb.exe";
            return path;
        }
    
        /************************************************************************************************************************/
    
        private static void ConnectToDevice(Process process, string ip)
        {
            var startInfo = process.StartInfo;
            startInfo.Arguments = "connect " + ip;
    
            process = new Process { StartInfo = startInfo };
            StartProcess(process);
        }
    
        /************************************************************************************************************************/
    
        private static void StartProcess(Process process)
        {
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                    return;
    
                Debug.Log(e.Data);
            };
    
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                    return;
    
                Debug.LogError(e.Data);
            };
    
            Debug.Log("Start Process " + Path.GetFileName(process.StartInfo.FileName) + ": " + process.StartInfo.Arguments);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
    
        /************************************************************************************************************************/
    }
}

#endif