using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using MsgPack.Serialization;

namespace XmlMapLoader
{
    internal class Loader : BaseScript
    {
        private readonly Dictionary<MapModel, HashSet<int>> loadedProps = new Dictionary<MapModel, HashSet<int>>();

        private readonly Dictionary<MapModel, bool> maps = new Dictionary<MapModel, bool>();
        private readonly float visibilityDistance = (float) Math.Pow(500f, 2);


        public Loader()
        {
            string currentResourceName = API.GetCurrentResourceName();
            Debug.WriteLine($"currentResourceName: {currentResourceName}");
            int numMaps = API.GetNumResourceMetadata(currentResourceName, "load_msgpacked_map_object");
            Debug.WriteLine($"numMaps: {numMaps}");
            for (int i = 0; i < numMaps; i++)
            {
                string mapFilePath = API.GetResourceMetadata(currentResourceName, "load_msgpacked_map_object", i);
                Debug.WriteLine($"mapFilePath: {mapFilePath}");
                string mapFileContents = API.LoadResourceFile(currentResourceName, mapFilePath);
                Debug.WriteLine($"mapFileContents: {mapFileContents}");

                MessagePackSerializer<MapModel> serializer = MessagePackSerializer.Get<MapModel>();

                using (MemoryStream buffer = new MemoryStream(StringToBytes(mapFileContents)))
                {
                    MapModel map = serializer.Unpack(buffer);
                    maps.Add(map, false);
                }
            }
            Tick += OnTick;
        }

        private async Task OnTick()
        {
            try
            {
                IEnumerable<MapModel> disabledMapsToEnable = maps.Where(m =>
                    !m.Value && m.Key.Position.DoublesToVector3().DistanceToSquared(Game.PlayerPed.Position) <=
                    visibilityDistance).Select(m => m.Key);
                IEnumerable<MapModel> enabledMapsToDisable = maps.Where(m =>
                    m.Value && m.Key.Position.DoublesToVector3().DistanceToSquared(Game.PlayerPed.Position) >
                    visibilityDistance).Select(m => m.Key);
                disabledMapsToEnable.ToList().ForEach(EnableMap);
                enabledMapsToDisable.ToList().ForEach(DisableMap);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e}");
            }
        }

        private void DisableMap(MapModel map)
        {
            maps[map] = false;
            if (loadedProps.ContainsKey(map))
            {
                var propsToUnload = loadedProps[map];
                propsToUnload.ToList().ForEach(p => Entity.FromHandle(p).Delete());
                loadedProps[map] = new HashSet<int>();
            }
        }

        private void EnableMap(MapModel map)
        {
            maps[map] = true;
            if (!loadedProps.ContainsKey(map)) loadedProps.Add(map, new HashSet<int>());

            map.Props.ForEach(async p =>
            {
                Model m = new Model(p.ObjectType);
                await m.Request(20);
                Entity prop = Entity.FromHandle(API.CreateObjectNoOffset((uint) m.Hash, (float) p.Position[0],
                    (float) p.Position[1], (float) p.Position[2], false, false, false));
                prop.Quaternion = p.Quaternion.DoublesToQuaternion();
                loadedProps[map].Add(prop.Handle);
            });
        }

        /// <summary>
        ///     Converts string to byte[], where every character is represented as one <see cref="byte" />.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] StringToBytes(string data)
        {
            byte[] buffer = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
                buffer[i] = (byte) data[i];

            return buffer;
        }

        private class MapModel
        {
            public double[] Position { get; set; }
            public List<PropModel> Props { get; set; }

            public override string ToString()
            {
                return
                    $"Map Position: (x: {Position[0]}, y: {Position[1]}, z: {Position[2]})\nProps:\n{string.Join("\n", Props)}";
            }
        }

        public class PropModel
        {
            public string ObjectType { get; set; }
            public double[] Position { get; set; }
            public double[] Quaternion { get; set; }

            public override string ToString()
            {
                return
                    $"Prop: {ObjectType}, Position: (x: {Position[0]}, y: {Position[1]}, z: {Position[2]}), Quaternion: (x: {Quaternion[0]}, y: {Quaternion[1]}, z: {Quaternion[2]}, w: {Quaternion[3]})";
            }
        }
    }

    internal static class Extensions
    {
        public static Vector3 DoublesToVector3(this double[] d)
        {
            return new Vector3((float) d[0], (float) d[1], (float) d[2]);
        }

        public static Quaternion DoublesToQuaternion(this double[] d)
        {
            return new Quaternion((float) d[0], (float) d[1], (float) d[2], (float) d[3]);
        }
    }
}