using CitizenFX.Core;
using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Server.Classes
{
    class BlipModel
    {
        public float[] coords;
        public string name;
        public int sprite;
        public int color;

        public BlipModel() { }
    }

#if SERVER
    // Temporary thing for 
    class Temp_BlipSaveLoad : BaseScript
    {
        static string[] fileContents = null;
        const string fileName = "blips.txt";

        public Temp_BlipSaveLoad()
        {
            Server.GetInstance().RegisterEventHandler("TempBlipModule.requestBlips", new Action<Player>(RequestBlips));
            Server.GetInstance().RegisterEventHandler("TempBlipModule.addBlip", new Action<Player, string>(AddBlip));
            fileContents = File.ReadAllLines(fileName);
            Log.Debug($"Read saved blip file");
        }

        internal void RequestBlips([FromSource] CitizenFX.Core.Player source)
        {
            source.TriggerEvent("TempBlipModule.receiveInitialBlips", Helpers.MsgPack.Serialize(fileContents));
        }

        public void AddBlip([FromSource] Player source, string serializedBlip)
        {
            BlipModel deserializedBlip = Helpers.MsgPack.Deserialize<BlipModel>(serializedBlip);
            Vector3 coords = deserializedBlip.coords.ToVector3();
            Log.Debug($"Saved blip by {source.Name} at ({coords.X}, {coords.Y}, {coords.Z}) with color {deserializedBlip.color}, type {deserializedBlip.sprite} and name {deserializedBlip.name}.");
            new PlayerList().ToList().ForEach(p => p.TriggerEvent("TempBlipModule.addBlip", serializedBlip));

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(serializedBlip);
            }
        }
    }
#endif
}