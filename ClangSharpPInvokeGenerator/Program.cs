namespace ClangSharpPInvokeGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

#if LINUX_X86_64
    using ClangSharp_LINUX_X86_64;
#elif LINUX_X86
    using ClangSharp_LINUX_X86;
#elif WINDOWS_X86_64
    using ClangSharp_WINDOWS_X86_64;
#else
    using ClangSharp;
#endif

    public class Program
    {
        public static void Main(string[] args)
        {
            //check for valid runtime configuration
#if LINUX_X86_64 || WINDOWS_X86_64
            if (!Environment.Is64BitProcess)
                throw new Exception("Invalid runtime arch detected! This program intended to be run with 64-bit runtime only!");
#else
            if (Environment.Is64BitProcess)
                throw new Exception("Invalid runtime arch detected! This program intended to be run with 32-bit runtime only!");
#endif
#if WINDOWS_X86 || WINDOWS_X86_64
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new Exception("Invalid OS detected! This program intended to be run on Windows only!");
#else
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                throw new Exception("Invalid OS detected! This program intended to be run on Linux/Mono only!");
#endif
            Regex re = new Regex(@"(?<switch>-{1,2}\S*)(?:[=:]?|\s+)(?<value>[^-\s].*?)?(?=\s+[-]|$)");
            List<KeyValuePair<string, string>> matches = (from match in re.Matches(string.Join(" ", args)).Cast<Match>()
                                                          select new KeyValuePair<string, string>(match.Groups["switch"].Value, match.Groups["value"].Value))
                .ToList();

            var files = new List<string>();
            var includeDirs = new List<string>();
            var systemIncludeDirs = new List<string>();
            var systemAfterIncludeDirs = new List<string>();
            string outputFile = string.Empty;
            string @namespace = string.Empty;
            string libraryPath = string.Empty;
            string prefixStrip = string.Empty;
            string methodClassName = "Methods";
            string excludeFunctions = "";
            string[] excludeFunctionsArray = null;

            bool useC = false;
            bool noStdIncludes = false;

            foreach (KeyValuePair<string, string> match in matches)
            {
                if (string.Equals(match.Key, "--n") || string.Equals(match.Key, "--namespace"))
                {
                    @namespace = match.Value;
                }

                if (string.Equals(match.Key, "--l") || string.Equals(match.Key, "--libraryPath"))
                {
                    libraryPath = match.Value;
                }

                if (string.Equals(match.Key, "--i") || string.Equals(match.Key, "--include"))
                {
                    includeDirs.Add(match.Value);
                }

                if (string.Equals(match.Key, "--is") || string.Equals(match.Key, "--iSystem"))
                {
                    systemIncludeDirs.Add(match.Value);
                }

                if (string.Equals(match.Key, "--isa") || string.Equals(match.Key, "--iSystemAfter"))
                {
                    systemAfterIncludeDirs.Add(match.Value);
                }

                if (string.Equals(match.Key, "--nostd") || string.Equals(match.Key, "--noStdIncludes"))
                {
                    noStdIncludes = bool.Parse(match.Value);
                }

                if (string.Equals(match.Key, "--o") || string.Equals(match.Key, "--output"))
                {
                    outputFile = match.Value;
                }

                if (string.Equals(match.Key, "--f") || string.Equals(match.Key, "--file"))
                {
                    files.Add(match.Value);
                }

                if (string.Equals(match.Key, "--p") || string.Equals(match.Key, "--prefixStrip"))
                {
                    prefixStrip = match.Value;
                }

                if (string.Equals(match.Key, "--m") || string.Equals(match.Key, "--methodClassName"))
                {
                    methodClassName = match.Value;
                }

                if (string.Equals(match.Key, "--e") || string.Equals(match.Key, "--excludeFunctions"))
                {
                    excludeFunctions = match.Value;
                }

                if (string.Equals(match.Key, "--otr") || string.Equals(match.Key, "--outToRef"))
                {
                    Extensions.outToRef = bool.Parse(match.Value);
                }

                if (string.Equals(match.Key, "--c") || string.Equals(match.Key, "--charToByte"))
                {
                    Extensions.charToByte = bool.Parse(match.Value);
                }
#if LINUX_X86_64 || WINDOWS_X86_64
                if (string.Equals(match.Key, "--a32") || string.Equals(match.Key, "--arch32bit"))
                {
                    Extensions.arch64bit = !bool.Parse(match.Value);
                }
#endif
                if (string.Equals(match.Key, "--uc") || string.Equals(match.Key, "--useC"))
                {
                    useC = bool.Parse(match.Value);
                }

                if (string.Equals(match.Key, "--s") || string.Equals(match.Key, "--seqStructs"))
                {
                    StructVisitor.isSequential = bool.Parse(match.Value);
                }

                if (string.Equals(match.Key, "--a") || string.Equals(match.Key, "--ansiStructs"))
                {
                    StructVisitor.isAnsi = bool.Parse(match.Value);
                }

                if (string.Equals(match.Key, "--lpstr") || string.Equals(match.Key, "--useLPStrMarshaler"))
                {
                    Extensions.useLPStrMarshaler = bool.Parse(match.Value);
                }

                if (string.Equals(match.Key, "--ah") || string.Equals(match.Key, "--arrayHelpers"))
                {
                    Extensions.arrayHelpers = bool.Parse(match.Value);
                }

                if (string.Equals(match.Key, "--fixNestedStructs"))
                {
                    Extensions.fixNestedStructs = bool.Parse(match.Value);
                }

                if (string.Equals(match.Key, "--genDelegates"))
                {
                    Extensions.genDelegates = bool.Parse(match.Value);
                }
            }

            var errorList = new List<string>();
            if (!files.Any())
            {
                errorList.Add("Error: No input C/C++ files provided. Use --file or --f");
            }

            if (string.IsNullOrWhiteSpace(@namespace))
            {
                errorList.Add("Error: No namespace provided. Use --namespace or --n");
            }

            if (string.IsNullOrWhiteSpace(outputFile))
            {
                errorList.Add("Error: No output file location provided. Use --output or --o");
            }

            if (string.IsNullOrWhiteSpace(libraryPath))
            {
                errorList.Add("Error: No library path location provided. Use --libraryPath or --l");
            }

            if (errorList.Any())
            {
                Console.WriteLine("Usage: ClangPInvokeGenerator --file [fileLocation] --libraryPath [library.dll] --output [output.cs] --namespace [Namespace] --include [headerFileIncludeDirs] --excludeFunctions [func1,func2]");
                Console.WriteLine("extra options: ");
                foreach (var error in errorList)
                {
                    Console.WriteLine(error);
                }
            }

            if (!string.IsNullOrEmpty(excludeFunctions))
            {
                excludeFunctionsArray = excludeFunctions.Split(',').Select(x => x.Trim()).ToArray();
            }

            var createIndex = clang.createIndex(0, 0);
            string[] arr = { "-x", useC?"c":"c++" };
            if (Environment.Is64BitProcess && !Extensions.arch64bit)
                arr = arr.Concat(new string[] { "-m32" }).ToArray();
            if(noStdIncludes)
                arr = arr.Concat(new string[] { "-nostdinc", "-nostdinc++" }).ToArray();

            arr = arr.Concat(systemIncludeDirs.Select(x => "-isystem" + x)).ToArray();
            arr = arr.Concat(systemAfterIncludeDirs.Select(x => "-isystem-after" + x)).ToArray();
            arr = arr.Concat(includeDirs.Select(x => "-I" + x)).ToArray();

            List<CXTranslationUnit> translationUnits = new List<CXTranslationUnit>();

            foreach (var file in files)
            {
                CXTranslationUnit translationUnit;
                CXUnsavedFile[] unsavedFile = new CXUnsavedFile[0];
                var translationUnitError = clang.parseTranslationUnit2(createIndex, file, arr, 3, unsavedFile, 0, 0, out translationUnit);

                if (translationUnitError != CXErrorCode.CXError_Success)
                {
                    Console.WriteLine("Error: " + translationUnitError);
                    var numDiagnostics = clang.getNumDiagnostics(translationUnit);

                    for (uint i = 0; i < numDiagnostics; ++i)
                    {
                        var diagnostic = clang.getDiagnostic(translationUnit, i);
                        Console.WriteLine(clang.getDiagnosticSpelling(diagnostic).ToString());
                        clang.disposeDiagnostic(diagnostic);
                    }
                }

                translationUnits.Add(translationUnit);
            }

            using (var sw = new StreamWriter(outputFile))
            {
                sw.NewLine = "\n";

                sw.WriteLine("namespace " + @namespace);
                sw.WriteLine("{");

                sw.WriteLine("    using System;");
                sw.WriteLine("    using System.Runtime.InteropServices;");
                sw.WriteLine();

                var structVisitor = new StructVisitor(sw);
                foreach (var tu in translationUnits)
                {
                    clang.visitChildren(clang.getTranslationUnitCursor(tu), structVisitor.Visit, new CXClientData(IntPtr.Zero));
                }

                var typeDefVisitor = new TypeDefVisitor(sw);
                foreach (var tu in translationUnits)
                {
                    clang.visitChildren(clang.getTranslationUnitCursor(tu), typeDefVisitor.Visit, new CXClientData(IntPtr.Zero));
                }

                var enumVisitor = new EnumVisitor(sw);
                foreach (var tu in translationUnits)
                {
                    clang.visitChildren(clang.getTranslationUnitCursor(tu), enumVisitor.Visit, new CXClientData(IntPtr.Zero));
                }

                sw.WriteLine("    public static partial class " + methodClassName);
                sw.WriteLine("    {");
                {
                    var functionVisitor = new FunctionVisitor(sw, libraryPath, prefixStrip, excludeFunctionsArray);
                    foreach (var tu in translationUnits)
                    {
                        clang.visitChildren(clang.getTranslationUnitCursor(tu), functionVisitor.Visit, new CXClientData(IntPtr.Zero));
                    }
                }
                sw.WriteLine("    }");
                sw.WriteLine("}");
            }

            foreach (var tu in translationUnits)
            {
                clang.disposeTranslationUnit(tu);
            }

            clang.disposeIndex(createIndex);
        }
    }
}
