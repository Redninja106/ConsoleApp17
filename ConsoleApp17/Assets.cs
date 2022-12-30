using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ConsoleApp17;
internal static class Assets
{
    private static readonly Dictionary<string, ITexture> sprites = new();

    public static ITexture GetSpriteTexture(string path)
    {
        if (!sprites.TryGetValue(path, out ITexture? result))
        {
            result = Graphics.LoadTexture(path);
            sprites.Add(path, result);
        }

        return result;
    }
}

//public class SpriteSheetInfo
//{
//    public TextureAtlas Atlas;

//    public class TextureAtlas
//    {
//        [XmlAttribute]
//        publicstring imagePath;
//        [XmlArray]
//        public SubTexture[] subTextures;
//    }

//    public class SubTexture
//    {
//        [XmlAttribute]
//        public string name;
//        [XmlAttribute]
//        public int x, y, w, h;
//    }
//}

