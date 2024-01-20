using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Scrapmire
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle Assets;
        public static ConfigFile config;
        internal ManualLogSource mls;
        static Tuple<List<AudioClip>, List<AudioClip>> audioClips;
        Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        private void Awake()
        {
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Assets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "QuagmireModAssets"));

            if (Assets == null)
            {
                Logger.LogError("Failed to load custom assets.");
                return;
            }
            mls = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_GUID);
            audioClips = Tuple.Create(new List<AudioClip>(), new List<AudioClip>());
            loadSounds();
            int iRarity = 35;
            Item quagmireFaceItem = Assets.LoadAsset<Item>("Assets/QuagmireFaceAssets/QuagmireFaceItem.asset");
            Utilities.FixMixerGroups(quagmireFaceItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(quagmireFaceItem.spawnPrefab);
            Items.RegisterScrap(quagmireFaceItem, iRarity, Levels.LevelTypes.All);

            GrabbableObject grabbableObject = FindObjectOfType<GrabbableObject>();

            mls.LogMessage("Scrapmire mod loaded!");
            harmony.PatchAll();
        }

        private void loadSounds()
        {
            string grabSfxPath = "assets/quagmirefaceassets/sounds/grabsfx";
            string dropSfxPath = "assets/quagmirefaceassets/sounds/dropsfx";

            string[] assetNames = Assets.GetAllAssetNames();

            foreach (string assetName in assetNames)
            {
                if (assetName.StartsWith(grabSfxPath))
                {
                    audioClips.Item1.Add(Assets.LoadAsset<AudioClip>(assetName));

                }
                else if (assetName.StartsWith(dropSfxPath))
                {
                    audioClips.Item2.Add(Assets.LoadAsset<AudioClip>(assetName));
                }
            }
        }
        [HarmonyPatch(typeof(GrabbableObject))]
        class GrabbableObject_PlayDropSFX_Patch
        {
            [HarmonyPatch("Start")]
            [HarmonyPrefix]
            static void Start(GrabbableObject __instance)
            {
                if (__instance.itemProperties.itemName.Equals("Scrapmire") && audioClips != null && audioClips.Item1.Count > 0)
                {
                    AudioClip randomClip = audioClips.Item1[UnityEngine.Random.Range(0, audioClips.Item1.Count)];
                    __instance.itemProperties.grabSFX = randomClip;

                }

            }
            [HarmonyPatch("PlayDropSFX")]
            [HarmonyPrefix]
            static void Prefix(GrabbableObject __instance)
            {

                if (__instance.itemProperties.itemName.Equals("Scrapmire") && audioClips != null && audioClips.Item1.Count > 0 && audioClips.Item2.Count > 0)
                {
                    AudioClip randomClip = audioClips.Item2[UnityEngine.Random.Range(0, audioClips.Item2.Count)];
                    __instance.itemProperties.dropSFX = randomClip;
                    randomClip = audioClips.Item1[UnityEngine.Random.Range(0, audioClips.Item1.Count)];

                    __instance.itemProperties.grabSFX = randomClip;

                }

            }
        }
    }
}