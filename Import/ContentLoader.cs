﻿using System.IO;
using System.Text;
using IrregularZ.Graphics;
using static System.IO.Path;

namespace IrregularZ.Import
{
    public static class ContentLoader
    {
        public static byte[] ReadAllBytes(string filename) => File.ReadAllBytes(Combine("Content", filename));

        public static Model ReadModel(string filename) =>
            WavefrontLoader.Read(Encoding.ASCII.GetString(ReadAllBytes(filename)));
    }
}