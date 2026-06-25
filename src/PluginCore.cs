using BepInEx;

using BepInEx.Configuration;

using Comfort.Common;

using EFT;

using HarmonyLib;

using UnityEngine;



namespace RaidIntelOverlay

{

    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]

    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]

    public sealed class PluginCore : BaseUnityPlugin

    {

        internal static PluginCore Instance { get; private set; }



        internal ConfigEntry<KeyCode> ToggleKey;

        internal TrackedItemsConfig TrackedItems;



        private RaidIntelOverlayBehaviour _overlay;

        private bool _wasInRaid;



        private void Awake()

        {

            Instance = this;

            BindConfig();

            _overlay = gameObject.AddComponent<RaidIntelOverlayBehaviour>();

            RaidIntelFikaSync.Instance.Initialize(Logger);



            var harmony = new Harmony(PluginInfo.GUID);

            harmony.PatchAll(typeof(PluginCore).Assembly);

            Logger.LogInfo($"{PluginInfo.NAME} v{PluginInfo.VERSION} loaded");

        }



        private void OnDestroy()

        {

            RaidIntelFikaSync.Instance.Shutdown();

        }



        private void BindConfig()

        {

            ToggleKey = Config.Bind(

                "General",

                "Toggle Overlay Key",

                KeyCode.F9,

                "Показать / скрыть overlay во время рейда");



            TrackedItems = new TrackedItemsConfig(Config);

        }



        private void Update()

        {

            if (!Singleton<GameWorld>.Instantiated)

            {

                if (_wasInRaid)

                {

                    _overlay?.SetVisible(false);

                    _wasInRaid = false;

                }



                return;

            }



            var world = Singleton<GameWorld>.Instance;

            var inRaid = world?.MainPlayer != null;
            var syncInRaid = RaidIntelFikaSync.ShouldRunFikaSync(world);

            if (syncInRaid)

            {

                _wasInRaid = true;

                RaidIntelFikaSync.Instance.TryRefreshAvailability();

                RaidIntelFikaSync.Instance.Tick(Time.deltaTime, true);

            }
            else if (inRaid)

            {

                _wasInRaid = true;

            }



            if (UnityEngine.Input.GetKeyDown(ToggleKey.Value))

            {

                if (!inRaid)

                {

                    Logger.LogInfo("[RAID_INTEL] Overlay доступен только после спавна в рейде.");

                    return;

                }



                _overlay?.Toggle();

                Logger.LogInfo($"[RAID_INTEL] Overlay {(_overlay != null && _overlay.IsVisible ? "shown" : "hidden")}");

            }

        }



        [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.OnGameStarted))]

        private static class GameWorldStartedPatch

        {

            [HarmonyPostfix]

            private static void Postfix()

            {

                Instance?.TrackedItems.Reload();

                ItemNameResolver.ClearCache();

                RaidIntelFikaSync.Instance.TryRefreshAvailability();

            }

        }

    }

}


