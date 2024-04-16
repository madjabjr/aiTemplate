using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace aiTemplate
{
    public class Clipboard
    {
        public static void SetText(string p_Text)
        {
            Thread STAThread = new Thread(
                delegate ()
                {
                    System.Windows.Clipboard.SetText(p_Text);
                });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }

        public static string GetText()
        {
            string clipString = "";

            Thread staThread = new Thread(
            delegate ()
            {
                {
                    clipString = System.Windows.Clipboard.GetText();
                }

            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return clipString;
        }

    }

    public class Utilities
    {
        public static string getMultiLineInput()
        {
            Console.WriteLine("Enter Content Text: ");
            ConsoleKeyInfo keyInfo = new ConsoleKeyInfo();
            StringBuilder sb = new StringBuilder();
            int index = 0;
            while (keyInfo.Key != ConsoleKey.Escape)
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    return null;
                }

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (index > 0)
                    {
                        Console.CursorLeft = index - 1;

                        sb.Remove(index - 1, 1);

                        Console.Write(" \b");

                        index--;
                    }

                }
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    index++;
                    Console.Write("\r\n");
                    sb.Append("\r\n");
                }
                if (keyInfo.KeyChar > 31 && keyInfo.KeyChar < 127)
                {
                    index++;
                    Console.Write(keyInfo.KeyChar);
                    sb.Append(keyInfo.KeyChar);

                }


            }
            return sb.ToString(); ;
        }

        public static bool getYesNo(string question, string choices)
        {
            string checkString = "";
            while (true)
            {
                Console.WriteLine(question + ": (" + choices + "): \n");
                checkString = Console.ReadLine();
                if ((checkString).ToUpper() == choices[0].ToString().ToUpper() || checkString.ToUpper() == choices[2].ToString().ToUpper())
                {
                    break;
                }
                else
                {
                    Console.WriteLine($"Please enter only {choices[0]} or {choices[2]} and Try Again!\n");
                }

            }
            if (checkString.ToUpper() == choices[2].ToString().ToUpper())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string ReadLineOrEscape()
        {
            ConsoleKeyInfo keyInfo = new ConsoleKeyInfo();
            StringBuilder sb = new StringBuilder();
            int index = 0;

            while (keyInfo.Key != ConsoleKey.Enter)
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    return null;
                }

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (index > 0)
                    {
                        Console.CursorLeft = index - 1;

                        sb.Remove(index - 1, 1);

                        Console.Write(" \b");

                        index--;
                    }

                }

                if (keyInfo.KeyChar > 31 && keyInfo.KeyChar < 127)
                {
                    index++;
                    Console.Write(keyInfo.KeyChar);
                    sb.Append(keyInfo.KeyChar);

                }


            }
            return sb.ToString(); ;
        }
        public static bool FileExists(string rootpath, string filename)
        {
            if (File.Exists(Path.Combine(rootpath, filename)))
                return true;

            foreach (string subDir in Directory.GetDirectories(rootpath, "*", SearchOption.AllDirectories))
            {
                if (File.Exists(Path.Combine(subDir, filename)))
                    return true;
            }

            return false;
        }

    }

    public class menusRes
    {
        public string Menuname { get; set; }
        public List<string> Instructions { get; set; }
    }

    public class template
    {
        public string name { get; set; }
        public bool hasSecondaryInfo { get; set; }
        public string secondaryInfo { get; set; }
        public string systemInfo { get; set; }
        public string prompt { get; set; }
        public List<string> otherVariables { get; set; }
    }

    public class jsonHandler
    {
        public static List<menusRes> LoadMenus()
        {
            using (StreamReader file = File.OpenText(@"menus.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                List<menusRes> menus = (List<menusRes>)serializer.Deserialize(file, typeof(List<menusRes>));
                return menus;
            }

        }

        public static List<template> LoadTemplates()
        {
            using (StreamReader file = File.OpenText("aipromptTemplate.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                List<template> templates = (List<template>)serializer.Deserialize(file, typeof(List<template>));
                return templates;
            }

        }

        public static bool addTemplate(template newtemplate)
        {
            if (newtemplate == null)
            {
                return false;
            }
            List<template> currTemplates = LoadTemplates();
            List<template> newTemplates = new List<template>();
            foreach (template template in currTemplates)
            {
                if (template == newtemplate)
                {
                    return false;
                }
                newTemplates.Add(template);
            }
            newTemplates.Add(newtemplate);
            string tempFilename = Path.Combine(Environment.SpecialFolder.MyDocuments + "aipromptsTemplate.json");
            if (newTemplates.Count > currTemplates.Count)
            {
                using (StreamWriter file = File.CreateText(tempFilename))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, newTemplates);
                }
                // Delete original file
                File.Delete("aipromptsTemplate.json");

                File.Move(tempFilename, "aipromptsTemplate.json");
                return true;
            }
            return false;

        }

        public static bool removeTemplate(template deltemplate)
        {
            if (deltemplate == null)
            {
                return false;
            }
            List<template> currTemplates = LoadTemplates();
            List<template> newTemplates = new List<template>();
            foreach (template template in currTemplates)
            {
                if (template == deltemplate)
                {
                    continue;
                }
                newTemplates.Add(template);
            }
            string tempFilename = Path.Combine(Environment.SpecialFolder.MyDocuments + "aipromptsTemplate.json");
            if (newTemplates.Count < currTemplates.Count) {
                using (StreamWriter file = File.CreateText(tempFilename))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, newTemplates);
                }
                // Delete original file
                File.Delete("aipromptsTemplate.json");

                File.Move(tempFilename, "aipromptsTemplate.json");
                return true;
            }
           return false;

        }
        public static bool modifyTemplate(template modtemplate)
        {
            if (modtemplate == null)
            {
                return false;
            }
            List<template> currTemplates = LoadTemplates();
            List<template> newTemplates = new List<template>();
            foreach (template template in currTemplates)
            {
                if (template == modtemplate)
                {
                    newTemplates.Add(modtemplate);
                    continue;
                }
                newTemplates.Add(template);
            }
            string tempFilename = Path.Combine(Environment.SpecialFolder.MyDocuments + "aipromptsTemplate.json");
            if (newTemplates.Count < currTemplates.Count)
            {
                using (StreamWriter file = File.CreateText(tempFilename))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, newTemplates);
                }
                // Delete original file
                File.Delete("aipromptsTemplate.json");

                File.Move(tempFilename, "aipromptsTemplate.json");
                return true;
            }
            return false;

        }
    }
}
