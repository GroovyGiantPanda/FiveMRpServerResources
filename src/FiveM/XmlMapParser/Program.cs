using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace XmlMapParser
{
    class Program
    {
        class MapModel
        {
            public double[] Position { get; set; }
            public List<PropModel> Props { get; set; }

            public override string ToString()
            {
                return $"Map Position: (x: {Position[0]}, y: {Position[1]}, z: {Position[2]})\nProps:\n{string.Join("\n", Props)}";
                //return "X";
            }
        }

        public class PropModel
        {
            public string ModelHash { get; set; }
            public double[] Position { get; set; }
            public double[] Rotation { get; set; }
            public string ModelName { get; set; }
            public bool IsVisible { get; set; }
            public bool IsFrozen { get; set; }
            public int InitialHandle { get; set; }
            public bool IsDynamic { get; set; }
            public Dictionary<string, object> Attachment { get; set; }

            public override string ToString()
            {
                return $"Prop: {ModelName}, Position: (x: {Position[0]}, y: {Position[1]}, z: {Position[2]}), Rotation: (x: {Rotation[0]}, y: {Rotation[1]}, z: {Rotation[2]})";
            }
        }

        static void Main(string[] args)
        {
            XDocument doc = XDocument.Load(args[0]);
            Console.WriteLine(doc.Element("SpoonerPlacements").Elements("Placement").ToList().Count);
            IEnumerable<PropModel> childList = doc.Element("SpoonerPlacements")
                .Elements("Placement")
                .Select(i => new PropModel
                {
                    ModelName = i.Element("HashName").Value,
                    IsDynamic = i.Element("Dynamic") != null && bool.Parse(i.Element("Dynamic").Value),
                    InitialHandle = int.Parse(i.Element("InitialHandle").Value),
                    IsFrozen = i.Element("Frozen") != null && bool.Parse(i.Element("Frozen").Value),
                    IsVisible = i.Element("IsVisible") != null && bool.Parse(i.Element("IsVisible").Value),
                    Position = ExtractPosition(i.Element("PositionRotation").Elements()),
                    Rotation = ExtractRotation(i.Element("PositionRotation").Elements()),
                    Attachment = bool.Parse(i.Element("Attachment").Attribute("isAttached").Value) ? ExtractAttachment(i.Element("Attachment")) : null
                });

            if (!childList.Any()) throw new InvalidDataException();
            MapModel map = new MapModel {Position = childList.First().Position, Props = childList.ToList()};
            Console.WriteLine($"{map}");
            MessagePackSerializer<MapModel> serializer = MessagePackSerializer.Get<MapModel>();
            using (MemoryStream buffer = new MemoryStream())
            {
                serializer.Pack(buffer, map);
                File.WriteAllBytes(Path.ChangeExtension(args[0], "mp"), buffer.ToArray());
            }

        }

        private static Dictionary<string, object> ExtractAttachment(XElement element)
        {
            return new Dictionary<string, object>()
            {
                ["AttachedTo"] = int.Parse(element.Element("AttachedTo").Value),
                ["BoneIndex"] = int.Parse(element.Element("BoneIndex").Value),
                ["X"] = double.Parse(element.Element("X").Value, NumberStyles.Any, CultureInfo.InvariantCulture),
                ["Y"] = double.Parse(element.Element("Y").Value, NumberStyles.Any, CultureInfo.InvariantCulture),
                ["Z"] = double.Parse(element.Element("Z").Value, NumberStyles.Any, CultureInfo.InvariantCulture),
                ["Pitch"] = double.Parse(element.Element("AttachedTo").Value, NumberStyles.Any, CultureInfo.InvariantCulture),
                ["Roll"] = double.Parse(element.Element("AttachedTo").Value, NumberStyles.Any, CultureInfo.InvariantCulture),
                ["Yaw"] = double.Parse(element.Element("AttachedTo").Value, NumberStyles.Any, CultureInfo.InvariantCulture)
            };
        }

        private static double[] ExtractPosition(IEnumerable<XElement> elements)
        {
            var PositionRotation = elements.ToDictionary(e => e.Name, a =>
            {
                bool b = double.TryParse(a.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double d);
                if (!b)
                {
                    Console.WriteLine($"Couldn't parse value '{a.Value}'");
                    throw new InvalidDataException();
                }
                else
                {
                    return d;
                }
            });
            return new double[] { PositionRotation["X"], PositionRotation["Y"], PositionRotation["Z"] };
        }

        private static double[] ExtractRotation(IEnumerable<XElement> elements)
        {
            var PositionRotation = elements.ToDictionary(e => e.Name.LocalName, a =>
            {
                bool b = double.TryParse(a.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double d);
                if (!b)
                {
                    Console.WriteLine($"Couldn't parse value '{a.Value}'");
                    throw new InvalidDataException();
                }
                else
                {
                    return d;
                }
            });
            return new double[] { PositionRotation["Pitch"], PositionRotation["Roll"], PositionRotation["Yaw"] };
        }
    }
}
