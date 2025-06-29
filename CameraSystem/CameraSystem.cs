using System;
using System.Collections.Generic;
using System.Linq;
using AdminToys;
using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.Dummies;
using GameCore;
using InventorySystem.Items;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using Mirror;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using RemoteAdmin.Communication;
using UnityEngine;

using Logger = LabApi.Features.Console.Logger;
using Version = System.Version;

namespace CameraSystem
{
    public class CameraSystem : Plugin<Config>
    {
        public override string Name => "CameraSystem";

        public override string Description => string.Empty;

        public override string Author => "$Delta";

        public override Version Version => new Version(1, 0, 0);

        public override Version RequiredApiVersion => new Version(LabApi.Features.LabApiProperties.CompiledVersion);

        public static CameraSystem Instance;

        public ReferenceHub DummyHub;
        public ReferenceHub PlayerHub;
        public bool IsWatching;
        public List<Item> PlayerItems = new List<Item>();

        public override void Disable()
        {
            Instance = null;
            DummyHub = null;
            PlayerHub = null;

            LabApi.Events.Handlers.PlayerEvents.InteractedToy -= OnInteractedToy;
            LabApi.Events.Handlers.PlayerEvents.Hurt -= OnHurt;
            LabApi.Events.Handlers.ServerEvents.RoundStarted -= OnRoundStarted;
        }

        public override void Enable()
        {
            Instance = this;

            LabApi.Events.Handlers.PlayerEvents.InteractedToy += OnInteractedToy;
            LabApi.Events.Handlers.PlayerEvents.Hurt += OnHurt;
            LabApi.Events.Handlers.ServerEvents.RoundStarted += OnRoundStarted;
        }

        private void OnInteractedToy(PlayerInteractedToyEventArgs ev)
        {
            Logger.Debug("Interaction detected");

            if (IsWatching) return;

            DummyHub = DummyUtils.SpawnDummy(ev.Player.DisplayName);
            Player dummy = Player.Get(DummyHub);

            dummy.CustomInfo = "<color=#FFFFFF>| Смотрит камеры |</color>";
            dummy.Role = ev.Player.Role;
            dummy.Position = ev.Player.Position;
            dummy.Rotation = ev.Player.Rotation;

            foreach (Item i in ev.Player.Items)
            {
                Logger.Debug($"Adding {i} to dummy");

                PlayerItems.Add(i);
                dummy.AddItem(i.Type);
            }

            ev.Player.Role = RoleTypeId.Scp079;
            ev.Player.SendBroadcast($"<size=32><b>Чтобы перестать наблюдать введите в консоль: <color=green>\".exitcamera\"</color></b></size>", 15);

            IsWatching = true;

            PlayerHub = ev.Player.ReferenceHub;
        }

        private void OnHurt(PlayerHurtEventArgs ev)
        {
            if (ev.Player == null || ev.Attacker == null) return;
            else if (ev.Player.ReferenceHub == PlayerHub) ReplaceDummy();
        }

        private void OnRoundStarted()
        {
            GameObject interactable = UnityEngine.Object.Instantiate(NetworkManager.singleton.spawnPrefabs.First(x => x.name == "InvisibleInteractableToy"),
                Config.ToyPosition, new Quaternion(0, 0, 0, 0));

            //GameObject bench = UnityEngine.Object.Instantiate(NetworkClient.prefabs.Values.First(x => x.name == "Spawnable Work Station Structure"));

            //Room intercom = Room.List.First(x => x.Name == MapGeneration.RoomName.EzIntercom);

            //bench.transform.position = intercom.Position;
            //bench.transform.rotation = intercom.Rotation;
            //bench.transform.localScale = Vector3.one;

            //NetworkServer.Spawn(bench.gameObject);
            NetworkServer.Spawn(interactable.GetComponent<AdminToyBase>().gameObject);
        }

        public void ReplaceDummy()
        {
            Player player = Player.Get(PlayerHub);
            Player dummy = Player.Get(DummyHub);

            player.Health = dummy.Health;
            player.Role = dummy.Role;
            player.Position = dummy.Position;

            player.ClearInventory();
            foreach (Item i in PlayerItems)
            {
                player.AddItem(i.Type);
            }
            PlayerItems.Clear();

            NetworkServer.Destroy(DummyHub.gameObject);

            PlayerHub = null;
            DummyHub = null;

            IsWatching = false;
        }
    }
}
