using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Classes
{
    // TODO: Test and debug
    // Might make this class more general later to actually inform other classes that it's in range or getting close to a marker
    class Marker
    {
        public Vector3 Position { get; private set; }
        public Vector3 Rotation { get; private set; }
        public Vector3 Direction { get; private set; }
        public MarkerType Type { get; private set; }
        public Vector3 Scale { get; private set; }
        public System.Drawing.Color Color { get; private set; }

        public Marker(Vector3 position, MarkerType type = MarkerType.VerticleCircle)
        {
            this.Position = position;
            this.Rotation = new Vector3(0, 0, 0);
            this.Direction = new Vector3(0, 0, 0);
            this.Color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            this.Type = type;
            this.Scale = 1.0f * new Vector3(1f, 1f, 1f);
        }

        public Marker(Vector3 position, MarkerType type, System.Drawing.Color color, float scale = 0.3f)
        {
            this.Position = position;
            this.Rotation = new Vector3(0, 0, 0);
            this.Direction = new Vector3(0, 0, 0);
            this.Color = color;
            this.Type = type;
            this.Scale = scale * new Vector3(1f, 1f, 1f);
        }

        public Marker(Vector3 position, MarkerType type, System.Drawing.Color color, Vector3 scale, Vector3 rotation, Vector3 direction)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.Direction = direction;
            this.Color = color;
            this.Type = type;
            this.Scale = scale;
        }
    }

    internal static class MarkerHandler
    {
        // For e.g. cinematic mode
        static public bool HideAllMarkers = false;

        const float drawThreshold = 250f;

        // Any other class can add or remove markers from this dictionary (by ID preferrably)
        static internal Dictionary<int, Marker> All = new Dictionary<int, Marker>();
        static internal List<Marker> Close = new List<Marker>();

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
            PeriodicUpdate();
        }

        static public Task OnTick()
        {
            if (!HideAllMarkers)
            {
                Close.ForEach(m => CitizenFX.Core.World.DrawMarker(m.Type, m.Position, m.Direction, m.Rotation, m.Scale, m.Color));
            }
            return Task.FromResult(0);
        }

        static public async Task PeriodicUpdate()
        {
            while (true)
            {
                await RefreshClose();
                await BaseScript.Delay(1000);
            }
        }

        static public Task RefreshClose()
        {
            Close = All.ToList().Select(m => m.Value).Where(m => m.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(drawThreshold, 2)).ToList();
            return Task.FromResult(0);
        }
    }
}
