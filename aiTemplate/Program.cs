using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;


public class Program
{
    private enum MainMenuActions : int
    {
        invalid = 0,
        createTemplate = 1,
        fillTemplate = 2,
        exit = 3,
        help = 4
    }

    private static Dictionary<int, string> tempChoices;

    private static string tempMenu = "";

    private static bool changedTemplates = false;

    private static readonly string mainMenu = "1. Create Template\n" +
        "2. Fill Templates\n" +
        "3. Exit\n" +
        "4. Help or More Info\n";


    public static void Main(string[] args)
    {
        TemplateProcessor processor = new TemplateProcessor();
        if (processor.templates.Count != 0 )
        {
            processor.templates.Clear();
        }
        
        processor.ReadTemplates(@"aipromptsTemplate.txt");
        do
        {
            Console.WriteLine(mainMenu);
            int userChoice;
            if (TryGetInput(out userChoice))
            {
                if (userChoice == (int)MainMenuActions.exit)
                {
                    break;
                }
                handleMainMenuAction((MainMenuActions)userChoice, processor);
            }
            else
            {
                Console.WriteLine("Please Enter only one number from options and Try Again!");
                Console.Clear();
            }
        } while (true);    
        
    }
    public static bool TryGetInput(out int input)
    {
        return int.TryParse(Console.ReadLine(), out input);
    }

   
    private static void handleMainMenuAction(MainMenuActions action, TemplateProcessor processor)
    {
        // Ensure that the enum is defined
        if (!Enum.IsDefined(typeof(MainMenuActions), action))
        {
            Console.WriteLine("Not Valid Action! Try Again!");
            return;
        }

        switch (action)
        {
            case MainMenuActions.createTemplate:
                do
                {
                    processor.createNewTemplate(@"aipromptsTemplate.txt");
                } while (askUserForRepeat("Continue"));
              
                break;

            case MainMenuActions.fillTemplate:
                do {
                    Console.WriteLine("Available Templates:\n");
                    int i = 1;
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var templateType in processor.templates.Keys)
                    {
                        stringBuilder.Append($"{i}. {templateType}\n");
                        tempChoices.Add(i,templateType);
                        i++;
                    }
                    tempChoices.Add(i, "Exit");
                    Console.WriteLine(stringBuilder.ToString());
                    bool Input = int.TryParse(Console.ReadLine(), out int templateInput);
                    if (Input)
                    {
                        
                        string processedTemplate = processor.ProcessTemplate(tempChoices[templateInput]);
                        Console.WriteLine(processedTemplate);
                        if (processor.getYesNo("Do you want to copy to Clipboard?\n", "Y/N"))
                        {
                            try
                            {
                                Clipboard.SetText(processedTemplate);
                                Console.WriteLine("Copied to Clipboard!\n");
                            }
                            catch (Exception e) { Console.WriteLine(e.ToString()); }
                        }
                        string outputFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"output_{Guid.NewGuid()}.txt"); // Generate a unique file name
                        File.WriteAllText(outputFilePath, processedTemplate);
                        Console.WriteLine($"Template has been filled and saved as {outputFilePath}");
                    }   
                } while (askUserForRepeat("Continue"));
                break;
            case MainMenuActions.help:
                Console.WriteLine("Enter Help Stuff Here");
                break;

            case MainMenuActions.invalid:
            default:
                Console.WriteLine("Something went Wrong! Try Again!");
                break;
        }
    }

    private static bool askUserForRepeat(string repeatText)
    {
        Console.WriteLine("1. " + repeatText);
        Console.WriteLine("2. Back to Main Menu\n");
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
}


public class TemplateProcessor
{
    public readonly Dictionary<string, string> templates = new Dictionary<string, string>();

    public static readonly string createInstructions = "You can add additional variables to templates such as speaker by using {{speaker}} within any of the template blocks: \n" +
        "Name of Template should be something simple and small.\n" +
        "System Info refers to the how AI should act. For example \"Write like a College Student\".\n" +
        "Secondary Info refers to any additional information the AI needs, like the speaker or the person who wrote it.\n" +
        "Prompt is where you write the main quesiton or instructions for the AI\n";


    public void ReadTemplates(string filePath)
    {
        string content = File.ReadAllText(filePath);

        var regex = new Regex(@"{{Template Start}}(.*?){{Template End}}", RegexOptions.Singleline);
        var matches = regex.Matches(content);

        foreach (Match match in matches)
        {
            //string templateType = match.Groups[3].Value;
            string templateContent = match.Groups[1].Value;
            Regex regex1 = new Regex(@"{{type=([^}]+)}}");
            string templateType = regex1.Match(templateContent).Value;
            templateContent = templateContent.Replace("\r\n" + templateType + "\r\n", "");
            templateType = templateType.Substring(7, templateType.Length - 9);
            templates.Add(templateType, templateContent);
        }
    }

    public string GetUserInput(string templateType, string placeholder, bool clipBool)
    {
        if (clipBool)
        {
            return Clipboard.GetText();
        }
        else
        {
            if (placeholder == "content")
            {
                return getMultiLineInput();
            }
            else
            {
                Console.WriteLine($"Enter value for '{placeholder}':");
                return Console.ReadLine();
            }
            
        }
        
    }

    public string getMultiLineInput()
    {
        Console.WriteLine("Enter Text: ");
        StringBuilder sb = new StringBuilder();
        while (true)
        {
            string line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
            { break; }
            sb.Append(line);
        }
        return sb.ToString();
    }

    public string ProcessTemplate(string templateType)
    {
        if (!templates.TryGetValue(templateType, out string template))
        {
            throw new ArgumentException($"Template type '{templateType}' not found");
        }

        var regex = new Regex(@"{{\s*(?!\\})([^}]+)\s*}}");
        var matches = regex.Matches(template);

        string processedTemplate = template;
        foreach (Match match in matches)
        {
            string placeholder = match.Groups[1].Value; 
            var clipBool = getYesNo("Use value From Clipboard?", "Y/N");
            string userInput = GetUserInput(templateType, placeholder, clipBool);
            processedTemplate = processedTemplate.Replace("{{" + placeholder + "}}", userInput);

        }

        return processedTemplate;
    }
    public void createNewTemplate(string document)
    {
        Console.WriteLine(createInstructions);
        Console.WriteLine("What is the name of the new Template?");
        string newTempname = Console.ReadLine();
        Console.WriteLine("What is the Prompt?");
        string prompt = Console.ReadLine();
        Console.WriteLine("Enter System Info: ");
        string systemInfo = Console.ReadLine();
        bool useSecondInfo = getYesNo("Would you like to Use Secondary Info?", "Y/N");
        string secondinfo = "";
        if (useSecondInfo)
        {
            Console.WriteLine("Enter Secondary Info: ");
            secondinfo = Console.ReadLine();
        }
        using (StreamWriter output = new StreamWriter(document))
        {
            output.NewLine = "{{Template Start}}";
            output.NewLine = "{{type=" + newTempname + "}}" ;
            output.NewLine = "";
            output.NewLine = "---";
            output.NewLine = "{{content}}";
            output.NewLine = "---";
            if (string.IsNullOrEmpty(secondinfo)) { output.NewLine = secondinfo; }
            output.NewLine = "";
            output.NewLine = systemInfo;
            output.NewLine = "";
            output.NewLine = prompt;
            output.NewLine = "";
            output.NewLine =
            output.NewLine = "";
            output.NewLine = "{{Template End}}";

        }
            
           
    }
    public bool getYesNo(string question, string choices)
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
}

public static class Clipboard
{
    public static void SetText(string p_Text)
    {
        Thread STAThread = new Thread(
            delegate ()
            {
                // Use a fully qualified name for Clipboard otherwise it
                // will end up calling itself.
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

