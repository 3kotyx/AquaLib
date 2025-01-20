using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AquaLib.Items.Equipment;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AquaLib
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.snmodding.nautilus")]
    public class Plugin : BaseUnityPlugin
    {
        public new static ManualLogSource Logger { get; private set; }

        private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
        public string modDirectory { get; private set; }
        public string luaScriptsDirectory { get; private set; }

        public Script script = new Script();

        public bool IsIngame = false;

        private void Awake()
        {
            // set project-scoped logger instance
            Logger = base.Logger;

            // Create a MoonSharp script engine
            

            // Create a "Player" namespace (a table) and add functions to it
            DynValue playerNamespace = DynValue.NewTable(script);
            script.Globals["Player"] = playerNamespace;

            // Add a function to the "Player" namespace
            playerNamespace.Table["GetHealth"] = (Func<float>)(() =>
            {
                return Player.main.liveMixin.health;
            });
            playerNamespace.Table["SetHealth"] = (Action<float>)((health) =>
            {
                Player.main.liveMixin.health = health;
                Console.WriteLine($"Player health set to: {health}");
            });

            // Add a custom C# function
            script.Globals["UnityPrintMsg"] = (Action<string>)((msg) => Logger.LogInfo($"Lua: {msg}"));

            // Run Lua scripts from the "Lua" directory in the mod's folder
            modDirectory = Path.GetDirectoryName(Assembly.Location);
            luaScriptsDirectory = Path.Combine(modDirectory, "Lua");

            //// Ensure the directory exists
            //if (Directory.Exists(luaScriptsDirectory))
            //{
            //    // Get all Lua files in the directory
            //    string[] luaFiles = Directory.GetFiles(luaScriptsDirectory, "*.lua");

            //    // Loop through each Lua file and execute it
            //    foreach (string luaFile in luaFiles)
            //    {
            //        try
            //        {
            //            Logger.LogInfo($"Running Lua script: {luaFile}");
            //            string scriptContent = File.ReadAllText(luaFile);
            //            script.DoString(scriptContent);
            //        }
            //        catch (Exception ex)
            //        {
            //            Logger.LogError($"Failed to execute Lua script '{luaFile}': {ex.Message}");
            //        }
            //    }
            //}
            //else
            //{
            //    Logger.LogWarning($"Lua script directory not found: {luaScriptsDirectory}");
            //}

            // Initialize custom prefabs
            InitializePrefabs();

            // Register harmony patches, if there are any
            Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
        private void Update()
        {
            // Detect button press (e.g., "F12" key or any other button)
            if (Input.GetKeyDown(KeyCode.F9)) {
                Debug.Log("Current Scene:" + SceneManager.GetActiveScene().name);
            }
            if (SceneManager.GetActiveScene().name == "Main" && !IsIngame)
            {
                RunLua();
                IsIngame = true;
            }
        }
        private void RunLua()
        {
            // Ensure the directory exists
            if (Directory.Exists(luaScriptsDirectory))
            {
                // Get all Lua files in the directory
                string[] luaFiles = Directory.GetFiles(luaScriptsDirectory, "*.lua");

                // Loop through each Lua file and execute it
                foreach (string luaFile in luaFiles)
                {
                    try
                    {
                        Logger.LogInfo($"Running Lua script: {luaFile}");
                        string scriptContent = File.ReadAllText(luaFile);
                        script.DoString(scriptContent);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to execute Lua script '{luaFile}': {ex.Message}");
                    }
                }
            }
            else
            {
                Logger.LogWarning($"Lua script directory not found: {luaScriptsDirectory}");
            }
        }
        private void InitializePrefabs()
        {
            //YeetKnifePrefab.Register();
        }
    }
}
