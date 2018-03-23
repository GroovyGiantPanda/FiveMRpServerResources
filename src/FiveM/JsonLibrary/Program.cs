using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfJsonLibrary
{
    public class Program
    {
        public class objectToSerialize
        {
            public bool banana { get; set; } = false;
            [JsonSerializable(JsonName: "cat")]
            public int mushroom { get; set; } = 132;
            public List<string> rabbit { get; set; } = new List<string>() { "asd\\AAA\"", "aq[\"\'" };
            public Dictionary<string, string> dog { get; set; } = new Dictionary<string, string>() { ["asd\\AAA\""] = "aq[\"\'" };
            public Dictionary<string, double> turtleBeachHeadset { get; set; } = new Dictionary<string, double>() { ["asd\\AAA\""] = 3.555, ["asd\"\"\\AAA\""] = 3.5556666111 };
        }

        public static void Main()
        {
            var s = JsonSerializer.SerializeObject(new objectToSerialize());
            Console.WriteLine($"Serialized: {s}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
