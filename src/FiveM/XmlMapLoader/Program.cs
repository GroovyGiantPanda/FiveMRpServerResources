using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace XmlMapLoader
{
    class Program
    {
        static void Main( string[] args )
        {
            string xmlMapString = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<CMapData>
  <name/>
  <parent/>
  <flags value=""0""/>
  <contentFlags value=""65""/>
  <streamingExtentsMin x=""-9074.18500000"" y=""-8753.31900000"" z=""-633.60140000""/>
  <streamingExtentsMax x=""10925.82000000"" y=""11246.68000000"" z=""5366.39800000""/>
  <entitiesExtentsMin x=""-9074.18500000"" y=""-8753.31900000"" z=""-633.60140000""/>
  <entitiesExtentsMax x=""10925.82000000"" y=""11246.68000000"" z=""5366.39800000""/>
  <entities>
    <Item type=""CEntityDef"">
      <archetypeName>Prop_Huge_Display_01</archetypeName>
      <flags value=""32""/>
      <guid value=""0""/>
      <position x=""857.90960000"" y=""1271.13300000"" z=""359.43000000""/>
      <rotation x=""-0.67661800"" y=""0.20525170"" z=""0.03854875"" w=""0.70609630""/>
      <scaleXY value=""1.00000000""/>
      <scaleZ value=""1.00000000""/>
      <parentIndex value=""-1""/>
      <lodDist value=""500.00000000""/>
      <childLodDist value=""500.00000000""/>
      <lodLevel>LODTYPES_DEPTH_HD</lodLevel>
      <numChildren value=""0""/>
      <priorityLevel>PRI_REQUIRED</priorityLevel>
      <extensions/>
      <ambientOcclusionMultiplier value=""255""/>
      <artificialAmbientOcclusion value=""255""/>
      <tintValue value=""0""/>
    </Item>
</entities>
</CMapData>";
            XDocument doc = XDocument.Parse(xmlMapString);
            HashSet<MapModel> maps = new HashSet<MapModel>();
            var childList = from i in doc.Element("CMapData").Element("entities").Elements("Item")
                            where i.Attribute("type").Value == "CEntityDef"
                            select new PropModel
                            {
                                ObjectType = i.Element("archetypeName").Value,
                                Position = i.Element("position").Attributes().OrderBy(a => a.Name.ToString()).Select(a => double.Parse(a.Value)).ToArray(),
                                Quaternion = i.Element("rotation").Attributes().OrderBy(a => a.Name.ToString()).Select(a => double.Parse(a.Value)).ToArray()
                            };

            if(childList.Any())
            {
                maps.Add(new MapModel { Position = childList.First().Position, Props = childList.ToList() });
            }

            foreach ( var e in maps.First().Props )
                Console.WriteLine( e.ObjectType );
            Console.ReadKey();
        }
    }
}
