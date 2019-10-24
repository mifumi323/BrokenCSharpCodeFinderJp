using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BrokenCSharpCodeFinderJp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: CSharpFileChecker <dir>");
                return;
            }

            var dir = args[0];
            CheckDir(dir);
        }

        private static void CheckDir(string dir)
        {
            foreach (var file in Directory.EnumerateFiles(dir, "*.cs"))
            {
                CheckFile(file);
            }
            foreach (var subDir in Directory.EnumerateDirectories(dir))
            {
                var dirName = Path.GetFileName(subDir);
                if (dirName == "obj" || dirName == "bin" || dirName == ".vs")
                {
                    continue;
                }
                CheckDir(subDir);
            }
        }

        private static void CheckFile(string file)
        {
            try
            {
                var text = ReadAllText(file);
                if (text == "")
                {
                    ShowError(file, "ファイル内容がありません。");
                    return;
                }
                if (text == null)
                {
                    ShowError(file, "ファイル内容を読み取れません。");
                    return;
                }
                var syntaxTree = CSharpSyntaxTree.ParseText(text);
                var root = syntaxTree.GetCompilationUnitRoot();
                var diag = syntaxTree.GetDiagnostics();
                if (diag.Any())
                {
                    ShowError(file, diag.First().GetMessage());
                }
            }
            catch (Exception ex)
            {
                ShowError(file, ex.Message);
            }
        }

        private static string ReadAllText(string file)
        {
            var bytes = File.ReadAllBytes(file);
            if (bytes.Length == 0)
            {
                return "";
            }
            if (TryGetString(bytes, Encoding.UTF8, out var utf8Text))
            {
                return utf8Text;
            }
            if (TryGetString(bytes, Encoding.GetEncoding(932), out var sjisText))
            {
                return sjisText;
            }
            return null;
        }

        private static bool TryGetString(byte[] bytes, Encoding encoding, out string text)
        {
            var encodedText = encoding.GetString(bytes);
            var encodedBytes = encoding.GetBytes(encodedText);
            if (encodedBytes.SequenceEqual(bytes))
            {
                text = encodedText;
                return true;
            }
            else
            {
                text = "";
                return false;
            }
        }

        private static void ShowError(string file, string message)
        {
            Console.WriteLine($"{file} : {message}");
        }
    }
}
