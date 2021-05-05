using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Il2CppDumper
{
    public class FilterHeaderGenerator
    {
        private Il2CppExecutor executor;
        private Metadata metadata;
        private Il2Cpp il2Cpp;
        private List<Filter> json;
        private Dictionary<Il2CppTypeDefinition, string> typeDefImageNames = new Dictionary<Il2CppTypeDefinition, string>();
        private HashSet<string> structNameHashSet = new HashSet<string>(StringComparer.Ordinal);
        private static HashSet<string> keyword = new HashSet<string>(StringComparer.Ordinal)
        { "klass", "monitor", "register", "_cs", "auto", "friend", "template", "near", "far", "flat", "default", "_ds", "interrupt", "inline",
            "unsigned", "signed", "asm", "if", "case", "break", "continue", "do", "new", "_", "short", "union"};

        public FilterHeaderGenerator(Il2CppExecutor il2CppExecutor, FilterJson filterJson)
        {
            executor = il2CppExecutor;
            metadata = il2CppExecutor.metadata;
            il2Cpp = il2CppExecutor.il2Cpp;
            json = filterJson.Filters;
        }

        public void WriteHeader(string outputDir)
        {
            var sb = new StringBuilder();

            for (var imageIndex = 0; imageIndex < metadata.imageDefs.Length; imageIndex++)
            {
                var imageDef = metadata.imageDefs[imageIndex];
                var imageName = metadata.GetStringFromIndex(imageDef.nameIndex);
                var typeEnd = imageDef.typeStart + imageDef.typeCount;
                for (int typeIndex = imageDef.typeStart; typeIndex < typeEnd; typeIndex++)
                {
                    var typeDef = metadata.typeDefs[typeIndex];
                    typeDefImageNames.Add(typeDef, imageName);
                }
            }

            foreach (var imageDef in metadata.imageDefs)
            {
                var imageName = metadata.GetStringFromIndex(imageDef.nameIndex);
                var typeEnd = imageDef.typeStart + imageDef.typeCount;
                for (int typeIndex = imageDef.typeStart; typeIndex < typeEnd; typeIndex++)
                {
                    var typeDef = metadata.typeDefs[typeIndex];
                    var typeName = executor.GetTypeDefName(typeDef, true, true);
                    foreach (var filter in json)
                    {
                        if (typeName.Equals($"{filter.Namespace}.{filter.Class}", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var methodEnd = typeDef.methodStart + typeDef.method_count;
                            for (var i = typeDef.methodStart; i < methodEnd; ++i)
                            {
                                var methodDef = metadata.methodDefs[i];
                                var methodName = metadata.GetStringFromIndex(methodDef.nameIndex);
                                foreach (var method in filter.Methods)
                                {
                                    if (methodName.Equals(method, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        var methodPointer = il2Cpp.GetMethodPointer(imageName, methodDef);
                                        if (methodPointer > 0)
                                        {
                                            //Console.WriteLine($"typeName: {FixName(typeName)}_{methodName} pointer: 0x{methodPointer,2:X}");
                                            sb.Append($"#define _{FixName(typeName)}_{methodName}_ ({filter.Type})0x{methodPointer:X}\n");
                                        }
                                    }
                                }
                            }
                            sb.Append("\n");
                        }
                    }

                }
            }
            File.WriteAllText(outputDir + "offset.h", sb.ToString());
        }

        private static string FixName(string str)
        {
            if (keyword.Contains(str))
            {
                str = "_" + str;
            }
            if (Regex.IsMatch(str, "^[0-9]"))
            {
                return "_" + str;
            }
            else
            {
                return Regex.Replace(str, "[^a-zA-Z0-9_]", "_");
            }
        }

    }
}
