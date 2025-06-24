using UnityEngine;

namespace CameraSystem
{
    public class Config
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = true;
        public Vector3 ToyPosition { get; set; } = new Vector3(0, 0, 0);
    }
}
