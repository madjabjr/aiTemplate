using System.Text;
using System.Text.RegularExpressions;

namespace aiTemplate
{
    public class Program
    {
        public enum MainMenuActions : int
        {
            invalid = 0,
            createTemplate = 1,
            fillTemplate = 2,
            deleteTemplate = 3,
            modifyTemplate = 4,
            displayTemplate = 5,
            help = 6,
            exit = 7
        }

        public enum ModifyMenuActions : int
        {
            invalid = 0,
            SystemInfo = 1,
            SecondaryInfo = 2,
            Prompt = 3,
            TemplateName = 4,
            Exit = 5
        }

        public Dictionary<int, template> tempChoices = new Dictionary<int, template> { };

        public bool changedTemplates = false;

        public static void Main(string[] args)
        {

            //File.Copy(@"aipromptsTemplateResc.txt", Path.Combine(Environment.CurrentDirectory, "aipromptsTemplate.txt"), true);
            TemplateProcessor processor = new TemplateProcessor();
            if (processor.templates.Count != 0)
            {
                processor.templates.Clear();
            }
            Program pInst = new Program();
            List<menusRes> menus = jsonHandler.LoadMenus();
            menusRes mainMenu = new menusRes();
            foreach (menusRes menu in menus)
            {
                if (menu.Menuname == "mainMenu") { mainMenu = menu; }
            }
            List<template> templates = jsonHandler.LoadTemplates();
            do
            {
                if (pInst.changedTemplates)
                {
                    templates = jsonHandler.LoadTemplates();
                    pInst.changedTemplates = false;
                }
                foreach (string instruction in mainMenu.Instructions)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(instruction);
                    Console.ResetColor();
                }
                int userChoice;
                if (pInst.TryGetInput(out userChoice))
                {
                    if (userChoice == (int)MainMenuActions.exit)
                    {
                        break;
                    }
                    pInst.handleMainMenuAction((MainMenuActions)userChoice, processor, templates, menus);
                }
                else
                {
                    Console.Clear();
                }
            } while (true);

        }
        public bool TryGetInput(out int input)
        {
            return int.TryParse(Console.ReadLine(), out input);
        }


        public void handleMainMenuAction(MainMenuActions action, TemplateProcessor processor, List<template> templates, List<menusRes> menus)
        {
            // Ensure that the enum is defined
            if (!Enum.IsDefined(typeof(MainMenuActions), action))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Not Valid Action! Try Again!");
                Console.ResetColor();
                return;
            }
            menusRes createMenu = new menusRes();
            menusRes modifyMenu = new menusRes();
            foreach (menusRes menu in menus)
            {
                if (menu.Menuname == "createInstructions") { createMenu = menu; }
                if (menu.Menuname == "modifySelect") { modifyMenu = menu; }
            }
            switch (action)
            {
                case MainMenuActions.createTemplate:
                    do
                    {
                        processor.createNewTemplate(createMenu);
                        changedTemplates = true;
                    } while (askUserForRepeat("Continue"));
                    break;

                case MainMenuActions.fillTemplate:
                    do
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Available Templates:\n");
                        Console.ResetColor();
                        int i = 1;
                        tempChoices.Clear();
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (template template in templates)
                        {
                            stringBuilder.Append($"{i}. {template.name}\n");
                            if (!tempChoices.ContainsValue(template))
                            {
                                if (tempChoices.ContainsKey(i))
                                {
                                    i = tempChoices.Count;
                                }
                                tempChoices.Add(i, template);
                            }
                            i++;
                        }
                        tempChoices.Add(i, null);
                        stringBuilder.Append($"{i}. Exit");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(stringBuilder.ToString());
                        Console.ResetColor();
                        bool Input = int.TryParse(Console.ReadLine(), out int templateInput);
                        if (Input)
                        {
                            if (templateInput == tempChoices.Count)
                            {
                                Console.Clear();
                                break;
                            }
                            string processedTemplate = processor.fillTemplate(tempChoices[templateInput]);
                            //string processedTemplate = processor.ProcessTemplate(tempChoices[templateInput]);
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.WriteLine(processedTemplate);
                            Console.ResetColor();
                            if (Utilities.getYesNo("Do you want to copy to Clipboard?", "Y/N"))
                            {
                                try
                                {
                                    Clipboard.SetText(processedTemplate);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Copied to Clipboard!\n");
                                    Console.ResetColor();
                                }
                                catch (Exception e) { Console.WriteLine(e.ToString()); }
                            }
                            string outputFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"filledTemplate_{Guid.NewGuid()}.txt"); // Generate a unique file name
                            File.WriteAllText(outputFilePath, processedTemplate);
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.BackgroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"Template has been filled and saved as {outputFilePath}");
                            Console.ResetColor();
                        }
                    } while (askUserForRepeat("Continue"));
                    break;
                case MainMenuActions.deleteTemplate:
                    do
                    {
                        if (changedTemplates)
                        {
                            templates = jsonHandler.LoadTemplates();
                            changedTemplates = false;
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Available Templates:\n");
                        Console.ResetColor();
                        int i = 1;
                        tempChoices.Clear();
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (template template in templates)
                        {
                            stringBuilder.Append($"{i}. {template.name}\n");
                            if (!tempChoices.ContainsValue(template))
                            {
                                if (tempChoices.ContainsKey(i))
                                {
                                    i = tempChoices.Count;
                                }
                                tempChoices.Add(i, template);
                            }
                            i++;
                        }
                        tempChoices.Add(i, null);
                        stringBuilder.Append($"{i}. Exit");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(stringBuilder.ToString());
                        Console.ResetColor();
                        bool Input = int.TryParse(Console.ReadLine(), out int templateInput);
                        if (Input)
                        {
                            if (templateInput == tempChoices.Count)
                            {
                                Console.Clear();
                                break;
                            }
                            if (jsonHandler.removeTemplate(tempChoices[templateInput]))
                            {
                                changedTemplates = true;
                            }
                        }


                    } while (askUserForRepeat("Continue"));
                    changedTemplates = true;
                    break;
                case MainMenuActions.modifyTemplate:
                    do
                    {
                        if (changedTemplates)
                        {
                            templates = jsonHandler.LoadTemplates();
                            changedTemplates = false;
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Available Templates:\n");
                        Console.ResetColor();
                        int i = 1;
                        tempChoices.Clear();
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (template template in templates)
                        {
                            stringBuilder.Append($"{i}. {template.name}\n");
                            if (!tempChoices.ContainsValue(template))
                            {
                                if (tempChoices.ContainsKey(i))
                                {
                                    i = tempChoices.Count;
                                }
                                tempChoices.Add(i, template);
                            }
                            i++;
                        }
                        tempChoices.Add(i, null);
                        stringBuilder.Append($"{i}. Exit");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(stringBuilder.ToString());
                        Console.ResetColor();
                        template chosenTemplate = null;
                        bool Input = int.TryParse(Console.ReadLine(), out int templateInput);
                        if (Input)
                        {
                            if (templateInput == tempChoices.Count)
                            {
                                Console.Clear();
                                break;
                            }
                            chosenTemplate = tempChoices[templateInput];
                            do
                            {
                                Console.Clear();
                                foreach (string instruction in modifyMenu.Instructions)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine(instruction);
                                    Console.ResetColor();
                                }
                                int userChoice;
                                if (TryGetInput(out userChoice))
                                {
                                    if (userChoice == (int)ModifyMenuActions.Exit)
                                    {
                                        break;
                                    }
                                    handleModifyMenuAction((ModifyMenuActions)userChoice, chosenTemplate);
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Please Enter only one number from options and Try Again!");
                                    Console.ResetColor();
                                }
                            } while (true);
                        }
                        changedTemplates = true;

                    } while (askUserForRepeat("Continue"));

                    break;
                case MainMenuActions.displayTemplate:
                    do
                    {
                        if (changedTemplates)
                        {
                            templates = jsonHandler.LoadTemplates();
                            changedTemplates = false;
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Available Templates:\n");
                        Console.ResetColor();
                        int i = 1;
                        tempChoices.Clear();
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (template template in templates)
                        {
                            stringBuilder.Append($"{i}. {template.name}\n");
                            if (!tempChoices.ContainsValue(template))
                            {
                                if (tempChoices.ContainsKey(i))
                                {
                                    i = tempChoices.Count;
                                }
                                tempChoices.Add(i, template);
                            }
                            i++;
                        }
                        tempChoices.Add(i, null);
                        stringBuilder.Append($"{i}. Exit");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(stringBuilder.ToString());
                        Console.ResetColor();
                        bool Input = int.TryParse(Console.ReadLine(), out int templateInput);
                        if (Input)
                        {
                            if (templateInput == tempChoices.Count)
                            {
                                Console.Clear();
                                break;
                            }
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine(processor.displayTempalte(tempChoices[templateInput]));
                            Console.ResetColor();
                        }
                    } while (askUserForRepeat("Continue"));
                    break;
                case MainMenuActions.help:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Enter Help Stuff Here");
                    Console.ResetColor();
                    break;

                case MainMenuActions.invalid:
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Something went Wrong! Try Again!");
                    Console.ResetColor();
                    break;
            }
        }

        private bool askUserForRepeat(string repeatText)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n1. " + repeatText);
            Console.WriteLine("2. Back to Main Menu\n");
            Console.ResetColor();
            bool Input = int.TryParse(Console.ReadLine(), out int userInput);
            if (Input)
            {
                if (userInput == 1)
                {
                    return true;
                }
                else
                {
                    // Go back to main menu -- user ended repeat.
                    Console.Clear();
                    return false;
                }
            }
            else
            {
                // Go back to main menu -- invalid user input.
                Console.Clear();
                return false;
            }
        }
        public void handleModifyMenuAction(ModifyMenuActions action, template template)
        {
            // Ensure that the enum is defined
            if (!Enum.IsDefined(typeof(ModifyMenuActions), action))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Not Valid Action! Try Again!");
                Console.ResetColor();
                return;
            }

            switch (action)
            {
                case ModifyMenuActions.SystemInfo:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Current System Info: \n");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(template.systemInfo + "\n\n");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Enter New System Info: \n");
                    Console.ResetColor();
                    template.systemInfo = Console.ReadLine();
                    if (template.systemInfo.Contains("{{") && template.systemInfo.Contains("}}"))
                    {
                        if (template.otherVariables.Count == 0)
                        {
                            template.otherVariables = Utilities.ExtractOtherVariables(template.systemInfo);
                        }
                        else
                        {
                            List<string> templList = Utilities.ExtractOtherVariables(template.systemInfo);
                            if (templList.Count > 0)
                            {
                                foreach (string vari in templList)
                                {
                                    if (template.otherVariables.Contains(vari))
                                    {
                                        continue;
                                    }
                                    template.otherVariables.Add(vari);
                                }
                            }
                        }
                    }
                    jsonHandler.modifyTemplate(template);
                    changedTemplates = true;
                    break;
                case ModifyMenuActions.SecondaryInfo:
                    if (template.hasSecondaryInfo)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Current Secondary Info: \n");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(template.secondaryInfo + "\n\n");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Enter New Secondary Info: \n");
                        Console.ResetColor();
                        template.secondaryInfo = Console.ReadLine();
                        if (template.secondaryInfo.Contains("{{") && template.secondaryInfo.Contains("}}"))
                        {
                            if (template.otherVariables.Count == 0)
                            {
                                template.otherVariables = Utilities.ExtractOtherVariables(template.secondaryInfo);
                            }
                            else
                            {
                                List<string> templList = Utilities.ExtractOtherVariables(template.secondaryInfo);
                                if (templList.Count > 0)
                                {
                                    foreach (string vari in templList)
                                    {
                                        if (template.otherVariables.Contains(vari))
                                        {
                                            continue;
                                        }
                                        template.otherVariables.Add(vari);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Selected does not have Secondary Information!");
                        Console.ForegroundColor = ConsoleColor.Green;
                        bool useSecondInfo = Utilities.getYesNo("\nWould you like to Use Secondary Info?", "Y/N");
                        template.hasSecondaryInfo = useSecondInfo;
                        if (useSecondInfo)
                        {
                            Console.WriteLine("\nEnter Secondary Info: ");
                            Console.ResetColor();
                            string secondinfo = Utilities.ReadLineOrEscape();
                            if (secondinfo == null) { return; }
                            template.secondaryInfo = secondinfo;
                        }
                        else { template.secondaryInfo = null; }
                    }

                    jsonHandler.modifyTemplate(template);
                    changedTemplates = true;
                    break;
                case ModifyMenuActions.Prompt:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Current Prompt: \n");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(template.prompt + "\n\n");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Enter New Prompt: \n");
                    Console.ResetColor();
                    template.prompt = Console.ReadLine();
                    if (template.prompt.Contains("{{") && template.prompt.Contains("}}"))
                    {
                        if (template.otherVariables.Count == 0)
                        {
                            template.otherVariables = Utilities.ExtractOtherVariables(template.prompt);
                        }
                        else
                        {
                            List<string> templList = Utilities.ExtractOtherVariables(template.prompt);
                            if (templList.Count > 0)
                            {
                                foreach (string vari in templList)
                                {
                                    if (template.otherVariables.Contains(vari))
                                    {
                                        continue;
                                    }
                                    template.otherVariables.Add(vari);
                                }
                            }
                        }
                    }
                    jsonHandler.modifyTemplate(template);
                    changedTemplates = true;
                    break;
                case ModifyMenuActions.TemplateName:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Current Template Name: \n");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(template.name + "\n\n");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Enter New Template Name\n");
                    Console.ResetColor();
                    template.name = Console.ReadLine();
                    jsonHandler.modifyTemplate(template);
                    changedTemplates = true;
                    break;


            }
        }
    }



    public class TemplateProcessor
    {
        public readonly Dictionary<string, string> templates = new Dictionary<string, string>();

        private enum templateSections : int
        {
            invalid = 0,
            systemInfo = 1,
            prompt = 2,
            secondaryInfo = 3,
            templateName = 4,
            exit = 5
        }



        public static string GetUserInput(string placeholder)
        {
            return Utilities.getMultiLineInput($"Enter value for '{placeholder}':");

        }

        public string displayTempalte(template template)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("---\n");
            sb.Append("Content Goes Here\n");
            sb.Append("---\n\n");
            if (template.hasSecondaryInfo)
            {
                sb.Append(template.secondaryInfo + "\n");
            }
            sb.Append('\n');
            sb.Append(template.systemInfo + "\n");
            sb.Append("\n" + template.prompt + "\n");

            return sb.ToString();
        }

        public string fillTemplate(template template)
        {
            string processedTemplate = "";
            StringBuilder sb = new StringBuilder();
            sb.Append("---\n");
            string content;
            if (Utilities.getYesNo($"Use value From Clipboard for the Content?", "Y/N"))
            {
                content = Clipboard.GetText();
            }
            else
            {
                content = Utilities.getMultiLineInput("Enter Content: \n");
            }
            sb.Append(content + "\n");
            sb.Append("---\n\n");
            if (template.hasSecondaryInfo)
            {
                sb.Append(template.secondaryInfo + "\n");
            }
            sb.Append('\n');
            sb.Append(template.systemInfo + "\n");
            sb.Append("\n" + template.prompt + "\n");
            if (template.otherVariables.Count > 0)
            {
                var regex = new Regex(@"{{\s*(?!\\})([^}]+)\s*}}");
                var matches = regex.Matches(sb.ToString());

                processedTemplate = sb.ToString();
                foreach (Match match in matches)
                {
                    string placeholder = match.Groups[1].Value;
                    var clipBool = Utilities.getYesNo($"Use value From Clipboard for '{placeholder}'?", "Y/N");
                    string userInput = GetUserInput(placeholder);
                    processedTemplate = processedTemplate.Replace("{{" + placeholder + "}}", userInput);
                }
            }


            return processedTemplate;
        }

        public void createNewTemplate(menusRes menu)
        {
            template newTemp = new template();
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (string instruction in menu.Instructions)
            {

                Console.WriteLine(instruction);
                Console.WriteLine();
            }
            Console.WriteLine("What is the name of the new Template?\n");
            Console.ResetColor();
            string newTempname = Utilities.ReadLineOrEscape();
            if (newTempname == null) { return; }
            newTemp.name = newTempname;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nWhat is the Prompt?\n");
            Console.ResetColor();
            string prompt = Utilities.ReadLineOrEscape();
            if (prompt == null) { return; }
            newTemp.prompt = prompt;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nEnter System Info: \n");
            Console.ResetColor();
            string systemInfo = Utilities.ReadLineOrEscape();
            if (systemInfo == null) { return; }
            newTemp.systemInfo = systemInfo;
            bool useSecondInfo = Utilities.getYesNo("\nWould you like to Use Secondary Info?", "Y/N");
            newTemp.hasSecondaryInfo = useSecondInfo;
            if (useSecondInfo)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nEnter Secondary Info: ");
                Console.ResetColor();
                string secondinfo = secondinfo = Utilities.ReadLineOrEscape();
                if (secondinfo == null) { return; }
                newTemp.secondaryInfo = secondinfo;
            }
            else { newTemp.secondaryInfo = null; }
            newTemp.otherVariables = GetTemplateVariables(newTemp);
            jsonHandler.addTemplate(newTemp);


        }
        public List<string> GetTemplateVariables(template template)
        {
            List<string> builtList = new List<string>();
            if (template.prompt.Contains("{{") && template.prompt.Contains("}}"))
            {
                builtList.AddRange(Utilities.ExtractOtherVariables(template.prompt));
            }
            if (template.systemInfo.Contains("{{") && template.systemInfo.Contains("}}"))
            {
                builtList.AddRange(Utilities.ExtractOtherVariables(template.systemInfo));
            }
            if (template.hasSecondaryInfo)
            {
                if (template.secondaryInfo.Contains("{{") && template.secondaryInfo.Contains("}}"))
                {
                    builtList.AddRange(Utilities.ExtractOtherVariables(template.secondaryInfo));
                }
            }
            return builtList;
        }
    }

}
