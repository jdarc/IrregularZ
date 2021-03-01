using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using IrregularZ.Graphics;

namespace IrregularZ.Import
{
    public static class WavefrontLoader
    {
        private static readonly Regex VertexRegex = new Regex("v (.*) (.*) (.*)", RegexOptions.IgnoreCase);
        private static readonly Regex FaceRegEx = new Regex("f (.*) (.*) (.*)", RegexOptions.IgnoreCase);
        private static readonly Regex UseMaterialRegex = new Regex("usemtl (.*)", RegexOptions.IgnoreCase);

        public static Model Read(string directives)
        {
            var materialLoader = new MaterialsLoader(directives);
            using var br = new StringReader(directives);
            var assembler = new Assembler();
            var line = br.ReadLine();
            while (line != null)
            {
                if (line.Length > 0)
                {
                    switch (line.Substring(0, line.IndexOf(' ')).Trim().ToLower())
                    {
                        case "usemtl":
                            assembler.Color = UseMaterial(materialLoader, line);
                            break;
                        case "v":
                            var match = VertexRegex.Match(line);
                            var x = float.Parse(match.Groups[1].Value);
                            var y = float.Parse(match.Groups[2].Value);
                            var z = float.Parse(match.Groups[3].Value);
                            assembler.AddVertex(x, y, z);
                            break;
                        case "f":
                            LoadFace(assembler, line);
                            break;
                    }
                }

                line = br.ReadLine();
            }

            return assembler.Compile();
        }

        private static void LoadFace(Assembler model, string line)
        {
            var v = new int[16];
            var vi = 0;
            var match = FaceRegEx.Match(line);
            for (var i = 1; i < match.Groups.Count; i++)
            {
                v[vi++] = int.Parse(ExtractVertexIndex(match.Groups[i].Value)) - 1;
            }

            model.CreateTriangle(v[0], v[1], v[2]);
            for (var i = 1; i < vi - 2; i++)
            {
                model.CreateTriangle(v[0], v[i + 1], v[i + 2]);
            }
        }

        private static string ExtractVertexIndex(string token)
        {
            var idx = token.IndexOf("/", StringComparison.Ordinal);
            return idx == -1 ? token : token.Substring(0, idx);
        }

        private static Color UseMaterial(MaterialsLoader materialLoader, string line) =>
            materialLoader.FindByName(UseMaterialRegex.Match(line).Groups[1].Value) ?? Color.White;
    }
}