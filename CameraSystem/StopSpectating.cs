using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using LabApi.Features.Wrappers;
using Mirror;
using NetworkManagerUtils.Dummies;

namespace CameraSystem
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class StopSpectating : ICommand
    {
        public string Command { get; set; } = "exitcamera";

        public string[] Aliases { get; set; } = { "ecamera" };

        public string Description { get; set; } = "Если вы наблюдаете через камеры, то введя эту команду вы выйдите из этого режима";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player.ReferenceHub != CameraSystem.Instance.PlayerHub)
            {
                response = "Вы не смотрите камеры";
                return false;
            }

            CameraSystem.Instance.ReplaceDummy();

            response = "Успех";
            return true;
        }
    }
}
