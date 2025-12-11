using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace FinalProject
{


    // -------------------------------------------
    // CONSOLE HELPER CLASS
    // -------------------------------------------
    class ConsoleHelper
    {
        //-------------------------
        //Print C.M.S
        //-------------------------
        public void printClinicName()
        {
            CenterText(" ██████╗   ███╗   ███╗   ███████╗\r\n██╔════╝   ████╗ ████║   ██╔════╝\r\n██║        ██╔████╔██║   ███████╗\r\n██║        ██║╚██╔╝██║   ╚════██║\r\n╚██████╗██╗██║ ╚═╝ ██║██╗███████║\r\n ╚═════╝╚═╝╚═╝     ╚═╝╚═╝╚══════╝\r\n                                 ", ConsoleColor.DarkYellow);
            CenterText("   ___ _ _      _      __  __                                       _     ___         _             \r\n  / __| (_)_ _ (_)__  |  \\/  |__ _ _ _  __ _ __ _ ___ _ __  ___ _ _| |_  / __|_  _ __| |_ ___ _ __  \r\n | (__| | | ' \\| / _| | |\\/| / _` | ' \\/ _` / _` / -_) '  \\/ -_) ' \\  _| \\__ \\ || (_-<  _/ -_) '  \\ \r\n  \\___|_|_|_||_|_\\__| |_|  |_\\__,_|_||_\\__,_\\__, \\___|_|_|_\\___|_||_\\__| |___/\\_, /__/\\__\\___|_|_|_|\r\n                                            |___/                             |__/                  ", ConsoleColor.DarkYellow);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }




        //-------------------------
        //Center Text
        //-------------------------
        public void CenterText(string text, ConsoleColor color = ConsoleColor.White) //Center Texts
        {
            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            int windowWidth = Console.WindowWidth;

            // Find the longest line
            int maxLength = lines.Max(l => l.Length);

            Console.ForegroundColor = color;

            foreach (string line in lines)
            {
                // Center based on the longest line
                int leftPadding = (windowWidth - maxLength) / 2;
                if (leftPadding < 0) leftPadding = 0;

                Console.SetCursorPosition(leftPadding, Console.CursorTop);
                Console.WriteLine(line.PadRight(maxLength)); // pad the shorter lines
            }

            Console.ResetColor();
        }
       





        private int AppointmentBlockWidth = 0;

        //-------------------------
        //Center Text With Different Colors
        //-------------------------
        public void PrintCenteredColoredLine(string label, string value) //Center Text with Different Color
        {
            int windowWidth = Console.WindowWidth;

            // Known appointment labels for a stable label column
            string[] labels =
            {
        "Patient:", "Doctor:", "Reason:", "Medical History:",
        "Start:", "End:", "Status Doctor:", "Status Secretary:", "Diagnosis:"
    };

            // Determine a stable label width (space after label included)
            int maxLabelWidth = labels.Max(l => l.Length) + 1;

            // Pad label to stable width
            string paddedLabel = label.PadRight(maxLabelWidth);

            // Proposed combined text
            string combined = paddedLabel + value;

            // Update block width to be at least as wide as this combined text
            AppointmentBlockWidth = Math.Max(AppointmentBlockWidth, combined.Length);

            // But never let block width exceed console width (avoid negative leftPadding)
            if (AppointmentBlockWidth > windowWidth)
                AppointmentBlockWidth = windowWidth;

            // If this particular combined text is longer than available width, trim the value
            if (combined.Length > windowWidth)
            {
                // space available for value after label within window
                int availForValue = windowWidth - paddedLabel.Length;
                if (availForValue <= 0)
                {
                    // nothing fits after label — fall back to printing trimmed whole text
                    string whole = (paddedLabel + value).Substring(0, Math.Max(0, windowWidth - 3)) + "...";
                    int leftPadding = (windowWidth - whole.Length) / 2;
                    if (leftPadding < 0) leftPadding = 0;

                    Console.SetCursorPosition(leftPadding, Console.CursorTop);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(whole);
                    Console.ResetColor();
                    Console.WriteLine();
                    return;
                }
                // trim value to fit, leaving room for "..."
                if (value.Length > availForValue)
                {
                    int take = Math.Max(0, availForValue - 3);
                    value = value.Substring(0, take) + "...";
                    combined = paddedLabel + value;
                }
            }

            // Now compute left padding using the (capped) AppointmentBlockWidth
            int leftPad = (windowWidth - AppointmentBlockWidth) / 2;
            if (leftPad < 0) leftPad = 0;

            // If AppointmentBlockWidth is less than this combined line (rare after trimming), re-center for this line
            if (combined.Length > AppointmentBlockWidth)
            {
                // center this line specifically
                leftPad = (windowWidth - combined.Length) / 2;
                if (leftPad < 0) leftPad = 0;
            }

            // Print centered with colors
            Console.SetCursorPosition(leftPad, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(paddedLabel);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(value);

            Console.ResetColor();
            Console.WriteLine();
        }







        //-------------------------
        //Center Appointment Number
        //-------------------------
        public void CenterAppointment(string text, ConsoleColor color = ConsoleColor.White) //Center Appointment Number
        {
            int windowWidth = Console.WindowWidth;

            
            string trimmed = text.TrimStart();
            int textWidth = trimmed.Length;

            int leftPadding = (windowWidth - textWidth) / 2;
            if (leftPadding < 0) leftPadding = 0;

            Console.ForegroundColor = color;

            // Position cursor for centering
            Console.SetCursorPosition(leftPadding, Console.CursorTop);
            Console.WriteLine(trimmed);

            Console.ResetColor();
        }





        //-------------------------
        //Arrow UI
        //-------------------------
        public int ShowMenuWithUICenter(List<string> options, string headerText = "") // Arrow Key UI
        {
            int selected = 0; //selected num

            while (true)
            {
                Console.Clear();
                printClinicName();

                // Print header info (like doctor details)
                if (!string.IsNullOrEmpty(headerText))
                {
                    CenterText(headerText, ConsoleColor.Yellow);
                    CenterText("--------------------------------------------------------------------------------------\n");
                }

                // Print menu options
                for (int i = 0; i < options.Count; i++)
                {
                    if (i == selected)
                    {
                        
                        CenterText($"{options[i]}", ConsoleColor.Green);
                        
                    }
                    else
                    {
                        CenterText($"{options[i]}");
                    }
                }

                CenterText("[↑↓Arrow keys to Navigate] [Enter to Select] [ESC to Go Back]", ConsoleColor.Yellow);

                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.UpArrow) //Arrow Up
                    selected = (selected - 1 + options.Count) % options.Count;
                else if (key.Key == ConsoleKey.DownArrow) //Arrow Down
                    selected = (selected + 1) % options.Count;
                else if (key.Key == ConsoleKey.Enter)
                    return selected;
                else if (key.Key == ConsoleKey.Escape) //Esc
                    return -1; // Return last option (Back) if Esc pressed
            }
        }





        //-------------------------
        //Arrow UI with Pages
        //-------------------------
        public int ShowMenuWithPagesCenter(List<string> options, string headerText = "")
        {
            int selected = 0;
            int page = 0;
            int itemsPerPage = 10;
            int totalPages = (int)Math.Ceiling((double)options.Count / itemsPerPage);

            while (true)
            {
                Console.Clear();
                printClinicName();

                // Print header info
                if (!string.IsNullOrEmpty(headerText))
                {
                    CenterText(headerText);
                    CenterText("-------------------------------------------------------------\n");
                }

                int start = page * itemsPerPage;
                int end = Math.Min(start + itemsPerPage, options.Count);

                // Display only items for the current page
                for (int i = start; i < end; i++)
                {
                    if (i == selected)
                    {
                        CenterText($"{options[i]}", ConsoleColor.Green);
                    }
                    else
                    {
                        CenterText($"{options[i]}");
                    }
                }

                // Page info
                CenterText($"\n\tPage {page + 1} of {totalPages}", ConsoleColor.Yellow);
                CenterText("\t[↑↓ Navigate] [Previous Page] [→ Next Page] [Enter Select] [ESC Go Back]", ConsoleColor.Yellow);

                // Read key input
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.UpArrow) // Arrow Up Key
                {
                    selected--;
                    if (selected < start) selected = end - 1;
                }
                else if (key.Key == ConsoleKey.DownArrow) // Arrow Down Key
                {
                    selected++;
                    if (selected >= end) selected = start;
                }
                else if (key.Key == ConsoleKey.RightArrow) // Arrow Right Key
                {
                    if (page < totalPages - 1)
                    {
                        page++;
                        selected = page * itemsPerPage;
                    }
                }
                else if (key.Key == ConsoleKey.LeftArrow) //Arrow Left Key
                {
                    if (page > 0)
                    {
                        page--;
                        selected = page * itemsPerPage;
                    }
                }
                else if (key.Key == ConsoleKey.Enter) //Enter
                {
                    return selected;
                }
                else if (key.Key == ConsoleKey.Escape) //Esc
                {
                    return -1; // Back option
                }
                else if(key.Key == ConsoleKey.Q) // Q
                {
                    return -10; //Search
                }
            }
        }
    }








    //-------------------------
    //MAIN PROGRAM
    //-------------------------

    class Program
    {

        static void Main(string[] args)
        {
            ConsoleHelper helper = new ConsoleHelper();
            Doctor doc = new Doctor();
            Patient pat = new Patient();
            Secretary sec = new Secretary();




            while (true)
            {
                Console.Clear();

                // ASCII for the Menu Options
                List<string> mainOptions = new List<string> { "██╗      ██████╗  ██████╗ ██╗███╗   ██╗\r\n██║     ██╔═══██╗██╔════╝ ██║████╗  ██║\r\n██║     ██║   ██║██║  ███╗██║██╔██╗ ██║\r\n██║     ██║   ██║██║   ██║██║██║╚██╗██║\r\n███████╗╚██████╔╝╚██████╔╝██║██║ ╚████║\r\n╚══════╝ ╚═════╝  ╚═════╝ ╚═╝╚═╝  ╚═══╝", "███████╗██╗ ██████╗ ███╗   ██╗██╗   ██╗██████╗ \r\n██╔════╝██║██╔════╝ ████╗  ██║██║   ██║██╔══██╗\r\n███████╗██║██║  ███╗██╔██╗ ██║██║   ██║██████╔╝\r\n╚════██║██║██║   ██║██║╚██╗██║██║   ██║██╔═══╝ \r\n███████║██║╚██████╔╝██║ ╚████║╚██████╔╝██║     \r\n╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝ ╚═════╝ ╚═╝     \r\n", "███████╗██╗  ██╗██╗████████╗\r\n██╔════╝╚██╗██╔╝██║╚══██╔══╝\r\n█████╗   ╚███╔╝ ██║   ██║   \r\n██╔══╝   ██╔██╗ ██║   ██║   \r\n███████╗██╔╝ ██╗██║   ██║   \r\n╚══════╝╚═╝  ╚═╝╚═╝   ╚═╝   \r\n" };
                List<string> SignUp = new List<string> { "██████╗  ██████╗  ██████╗████████╗ ██████╗ ██████╗ \r\n██╔══██╗██╔═══██╗██╔════╝╚══██╔══╝██╔═══██╗██╔══██╗\r\n██║  ██║██║   ██║██║        ██║   ██║   ██║██████╔╝\r\n██║  ██║██║   ██║██║        ██║   ██║   ██║██╔══██╗\r\n██████╔╝╚██████╔╝╚██████╗   ██║   ╚██████╔╝██║  ██║\r\n╚═════╝  ╚═════╝  ╚═════╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝\r\n", "██████╗  █████╗ ████████╗██╗███████╗███╗   ██╗████████╗\r\n██╔══██╗██╔══██╗╚══██╔══╝██║██╔════╝████╗  ██║╚══██╔══╝\r\n██████╔╝███████║   ██║   ██║█████╗  ██╔██╗ ██║   ██║   \r\n██╔═══╝ ██╔══██║   ██║   ██║██╔══╝  ██║╚██╗██║   ██║   \r\n██║     ██║  ██║   ██║   ██║███████╗██║ ╚████║   ██║   \r\n╚═╝     ╚═╝  ╚═╝   ╚═╝   ╚═╝╚══════╝╚═╝  ╚═══╝   ╚═╝   \r\n" };

               
                int choice = helper.ShowMenuWithUICenter(mainOptions); //Arrow UI


                switch (choice)
                {
                    //-------------------------
                    //LOG IN
                    //-------------------------
                    case 0: 
                        Console.Clear();
                        helper.printClinicName();

                        // --- Input Handling ---
                        // The following section prompts for username and password, 
                        // attempting to center the input prompts manually.

                        //Enter username
                        string prompt = "Enter username: ";
                        int padding = (Console.WindowWidth - prompt.Length - 20) / 2;
                        Console.SetCursorPosition(Math.Max(0, padding), Console.CursorTop);
                        Console.Write(prompt);
                        string user = Console.ReadLine()!;

                        // Enter password
                        string passwordPrompt = "Enter password: ";
                        int passwordPadding = (Console.WindowWidth - passwordPrompt.Length - 20) / 2;
                        Console.SetCursorPosition(Math.Max(0, padding), Console.CursorTop);
                        Console.Write(prompt); ;
                        string pass = Console.ReadLine()!;


                        // --- Account Type Check ---

                        if (user.StartsWith("dr_", StringComparison.OrdinalIgnoreCase)) //Check if the username starts with dr_
                        {
                            try
                            {
                                var doctors = DoctorDataBase.LoadDoctors();

                                // Checks the doctors database for an account match
                                var doctor = doctors.FirstOrDefault(d =>
                                    d.UserName?.Equals(user, StringComparison.OrdinalIgnoreCase) == true &&
                                    d.Password == pass);

                                if (doctor != null)
                                {
                                    // Account Found
                                    helper.CenterText($"\n\nWelcome, {doctor.UserName}!", ConsoleColor.Green);
                                    Console.ReadKey(true);

                                    // Create and stores the doctors data
                                    Doctor docAcc = new Doctor
                                    {
                                        User = doctor.UserName,
                                        Pass = doctor.Password,
                                        conNum = doctor.Contact,
                                        Hosp = doctor.Hospital ?? "",
                                        Spec = doctor.Specialization,
                                        Code = doctor.Code
                                    };

                                    try { docAcc.Menu(); } catch { } // Displays the Doctors Menu
                                }
                                else
                                {
                                    // If no account found or wrong credentials
                                    helper.CenterText("No Doctor account found or wrong password!", ConsoleColor.Red);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Error in loading/opening doctors file
                                helper.CenterText($"ERROR loading doctor accounts:\n{ex.Message}", ConsoleColor.Red);
                            }
                        }
                        
                        // Check if it's patient acc
                        else if (user.StartsWith("pat_", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var patients = PatientDataBase.LoadPatients();

                                // Find the details if it matches in the patients database
                                var patient = patients.FirstOrDefault(p =>
                                    p.UserName?.Equals(user, StringComparison.OrdinalIgnoreCase) == true &&
                                    p.Password == pass);

                                if (patient != null)
                                {
                                    // Account Found
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    helper.CenterText($"\n\nWelcome, {patient.UserName}!", ConsoleColor.Green);
                                    Console.ResetColor();
                                    Console.ReadKey(true);

                                    // Create and stores patient details
                                    Patient patAcc = new Patient
                                    {
                                        User = patient.UserName,
                                        Pass = patient.Password,
                                        ConNum = patient.ConNum,
                                        medHis = string.Join("|", patient.MedHistory),
                                        Age = patient.AgePatient,
                                        Code = patient.Code
                                    };

                                    try { patAcc.Menu(); } catch { } //Displays Patient Menu
                                }
                                else
                                {
                                    // No account found or wrong credentials
                                    helper.CenterText("No Patient account found or wrong password!", ConsoleColor.Red);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Error in loading patient file
                                helper.CenterText($"ERROR loading patient accounts:\n{ex.Message}", ConsoleColor.Red);
                            }
                        }

                        // Check if it's secretary account
                        else if (user.StartsWith("sec_", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var secretaryList = SecretaryDataBase.LoadSecretary();

                                // Check if the credentials match anything in the secretary database
                                var secretaryAcc = secretaryList.FirstOrDefault(s =>
                                    s.userName?.Equals(user, StringComparison.OrdinalIgnoreCase) == true &&
                                    s.password == pass);

                                if (secretaryAcc != null)
                                {
                                    // Account Found
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    helper.CenterText($"\n\nWelcome, {secretaryAcc.userName}!",ConsoleColor.Green);
                                    Console.ResetColor();
                                    Console.ReadKey(true);

                                    // Create and stores secretary's details
                                    Secretary secAcc = new Secretary
                                    {
                                        Username = secretaryAcc.userName,
                                        Docname = secretaryAcc.doctorName,
                                        Doccode = secretaryAcc.doctorCode,
                                        Docnum = secretaryAcc.DocNum,
                                        Seccode = secretaryAcc.SecCode,
                                        TimeIn = secretaryAcc.timeIn,
                                        TimeOut = secretaryAcc.timeOut
                                    };

                                    try { secAcc.Menu(); } catch { } //Display Secretary Menu
                                }
                                else
                                {
                                    // Account not found or wrong credentials
                                    helper.CenterText("No Secretary account found or wrong password!", ConsoleColor.Red);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Error loading secretary file
                                helper.CenterText($"ERROR loading secretary accounts:\n{ex.Message}", ConsoleColor.Red);
                            }
                        }
                        else
                        {
                            // Invalid username format
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("\nInvalid username format! Use 'dr_' for doctors or 'pat_' for patients.");
                            Console.ResetColor();
                        }

                        helper.CenterText("\nPress any key to continue...");
                        Console.ReadKey(true);



                        break;

                    //-------------------------
                    //SIGN UP
                    //-------------------------
                    case 1:
                        int ch = helper.ShowMenuWithUICenter(SignUp); // Arrow UI

                        switch (ch)
                        {
                            case -1: // ESC back to menu
                                break;

                            case 0: // Doctor Sign Up
                                doc.SignUp();
                                break;

                            case 1: // Patient Sign Up
                                pat.SignUp();
                                break;
                        }
                        break;

                    //-------------------------
                    //EXIT
                    //-------------------------
                    case 2:
                        return; // Exit program
                }
            }
        }
    }


    // -------------------------------------------
    // MAIN ACCOUNT CLASS
    // -------------------------------------------
    public abstract class Accounts
    {
        private string? username;
        private string? password;
        private string? contactNum;
        private string? code;
        private string[] medHis = [];
        private int? agePat;

        public int? AgePat
        {
            get => agePat;
            set => agePat = value;
        }

        public string? User
        {
            get => username;
            set => username = value;
        }

        public string[] MedHis
        {
            get => medHis;
            set => medHis = value;
        }

        public string? Pass
        {
            get => password;
            set => password = value;
        }

        public string? conNum
        {
            get => contactNum;
            set => contactNum = value;
        }

        public string? Code
        {
            get => code;
            set => code = value;
        }

        public abstract void Menu();
        public abstract void SignUp();

    }







    // -------------------------------------------
    // FOR DOCTOR AND SECRETARY'S APPOINTMENT CLASS
    // -------------------------------------------
    public interface ViewPatientDetails
    {
        void ViewAppointments();
    }









    // -------------------------------------------
    // DOCTOR CLASS
    // -------------------------------------------
    public class Doctor : Accounts, ViewPatientDetails
    {
        ConsoleHelper helper = new ConsoleHelper();
        Schedule sched = new Schedule();

        //For Doctor SignUp
        private string authKey = "doctor12345";
        private string userName = string.Empty;
        private string password = string.Empty;
        private string contactNum = string.Empty;
        private string hospitalName = string.Empty;
        private string? code = string.Empty;


        //-------------------------
        //LIST FOR DOCTOR SPECIALIZATION
        //-------------------------
        List<string> DoctorSpecializations = new List<string>
{
    "General Practitioner (GP)",
    "Family Medicine",
    "Internal Medicine",
    "Hospitalist",
    "Emergency Medicine",
    "Pediatrician",
    "Neonatologist",
    "Pediatric Surgeon",
    "Pediatric Cardiologist",
    "Pediatric Oncologist",
    "Neurologist",
    "Neurosurgeon",
    "Psychiatrist",
    "Psychologist",
    "Neuropsychiatrist",
    "Cardiologist",
    "Cardiothoracic Surgeon",
    "Vascular Surgeon",
    "Pulmonologist",
    "Thoracic Surgeon",
    "Hematologist",
    "Oncologist",
    "Immunologist",
    "Allergist",
    "Orthopedic Surgeon",
    "Rheumatologist",
    "Sports Medicine Specialist",
    "Dermatologist",
    "Plastic Surgeon",
    "Cosmetic Surgeon",
    "Endocrinologist",
    "Gastroenterologist",
    "Colorectal Surgeon",
    "Nephrologist",
    "Urologist",
    "Infectious Disease Specialist",
    "Obstetrician",
    "Gynecologist",
    "OB-GYN",
    "Ophthalmologist",
    "Optometrist",
    "Otolaryngologist (ENT)",
    "Audiologist",
    "Dentist",
    "Oral Surgeon",
    "Orthodontist",
    "Periodontist",
    "Physiatrist",
    "Physical Therapist",
    "Pathologist",
    "Radiologist",
    "Nuclear Medicine Specialist",
    "Anesthesiologist",
    "Pain Management Specialist",
    "Sleep Medicine Specialist",
    "Occupational Medicine Specialist",
    "Public Health Physician",
    "Preventive Medicine Specialist"
};

        //For Doctor Account
        private string? fieldSpec;
        private string? hospital;
        private bool hasSched = false;
        List<string> DocMenu = new List<string> { "Check Appointment", "Add Schedule", "Add Secretary", "Remove Secretary", "Log Out" };

        //For Sign Up
        public string UserName
        {
            get => userName;
            set => userName = value;
        }

        public string Password
        {
            get => password;
            set => password = value;
        }

        public string Contact
        {
            get => contactNum;
            set => contactNum = value;
        }

        public string Hosp
        {
            get => hospitalName;
            set => hospitalName = value;
        }

        //For Menu
        public string? Spec
        {
            get => fieldSpec;
            set => fieldSpec = value;
        }

        public string? Hospital
        {
            get => hospital;
            set => hospital = value;
        }

        public string? CodeDoc
        {
            get => code;
            set => code = value;
        }

        //Time for Schedule
        public DateTime Start => sched.StartSched;
        public DateTime End => sched.EndSched;

        //-------------------------
        //Manage Appoitments (CONFIRM/CANCEL)
        //-------------------------
        private void Manage(ConsoleHelper helper, List<AppointmentData> appointments, List<AppointmentData> displayedAppointments, string filePath)
        {
            // Display header for the management interface
            helper.CenterText("Manage Appointment", ConsoleColor.Yellow);
            helper.CenterText("───────────────────────────────────────────────────────────", ConsoleColor.Yellow);

            // Get the user's selected appointment number and validate the input.
            if (int.TryParse(Console.ReadLine(), out int selected) && selected > 0 && selected <= displayedAppointments.Count)
            {
                // Get the selected appointment from the temporary list shown to the user.
                var selectedApp = displayedAppointments[selected - 1];

                // Find the corresponding object in the master 'appointments' list to modify its status permanently.
                var originalApp = appointments.First(a => a.TimeStart == selectedApp.TimeStart && a.DoctorUser == selectedApp.DoctorUser);

                Console.Write("\nEnter action [CONFIRM/CANCEL]: ");
                string action = Console.ReadLine()!.Trim().ToUpper();

                if (action == "CONFIRM")
                {
                    // Update doctor's status field.
                    originalApp.StatusDoc = "CONFIRMED";
                    helper.CenterText("Appointment confirmed successfully!", ConsoleColor.Green);
                }
                else if (action == "CANCEL")
                {
                    // Require a reason for cancellation.
                    Console.Write("Enter reason for cancellation: ");
                    string reason = Console.ReadLine()!.Trim();

                    if (string.IsNullOrEmpty(reason))
                    {
                        helper.CenterText("Cancellation reason cannot be empty!", ConsoleColor.Red);
                        Console.ReadKey(true);
                        return;
                    }

                    // Set status to cancelled for both doctor and secretary views.
                    originalApp.StatusDoc = "CANCELLED";
                    originalApp.StatusSec = "CANCELLED";

                    // Store the cancellation reason in the diagnosis field for the patient.
                    originalApp.Diagnosis = $"CANCELLED: {reason} [DOCTOR]";
                    helper.CenterText("Appointment cancelled successfully!", ConsoleColor.Red);
                }
                else
                {
                    // Handle invalid action input.
                    helper.CenterText("Invalid action!", ConsoleColor.Red);
                    Console.ReadKey(true);
                    return;
                }

                // --- Save to JSON ---

                // Save the updated master list back to the Doctor's/Secretary's file.
                // This is necessary because 'originalApp' was modified.
                File.WriteAllText(filePath, JsonSerializer.Serialize(appointments, new JsonSerializerOptions { WriteIndented = true }));

                // Update the corresponding Patient's file for synchronization.
                string patientFile = $"{selectedApp.PatCode}.json"; // Get the patient's unique data file.
                if (File.Exists(patientFile))
                {
                    string patData = File.ReadAllText(patientFile);
                    if (!string.IsNullOrWhiteSpace(patData))
                    {
                        var patAppointments = JsonSerializer.Deserialize<List<AppointmentData>>(patData);

                        // Find the matching appointment in the patient's file to update its status.
                        var matching = patAppointments?.FirstOrDefault(x => x.TimeStart == selectedApp.TimeStart && x.DoctorUser == selectedApp.DoctorUser);
                        if (matching != null)
                        {
                            // Copy the updated status and diagnosis to the patient's record.
                            matching.StatusDoc = originalApp.StatusDoc;
                            matching.StatusSec = originalApp.StatusSec;
                            matching.Diagnosis = originalApp.Diagnosis;

                            // Save the patient's updated file.
                            File.WriteAllText(patientFile, JsonSerializer.Serialize(patAppointments, new JsonSerializerOptions { WriteIndented = true }));
                        }
                    }
                }

                helper.CenterText("Changes saved successfully!", ConsoleColor.Yellow);
                Console.ReadKey(true);
            }
            else
            {
                // Handle invalid appointment number input.
                helper.CenterText("Invalid appointment number!", ConsoleColor.Red);
                Console.ReadKey(true);
            }
        }


        //-------------------------
        //Doctor's Diagnosis
        //-------------------------
        private void Diagnosis(ConsoleHelper helper, List<AppointmentData> appointments, List<AppointmentData> displayedAppointments, string filePath)
        {
            // Ask the doctor which appointment they want to record a diagnosis for.
            Console.Write("\nEnter appointment number to diagnose: ");

            // Check if the input is a valid number corresponding to an appointment in the list.
            if (int.TryParse(Console.ReadLine(), out int selected) && selected > 0 && selected <= displayedAppointments.Count)
            {
                // Get the chosen appointment from the list currently being shown on screen.
                var selectedApp = displayedAppointments[selected - 1];

                // Find the exact same appointment in the master list ('appointments') to make the permanent change.
                var originalApp = appointments.First(a => a.TimeStart == selectedApp.TimeStart && a.DoctorUser == selectedApp.DoctorUser);

                // --- Validation Check ---
                // Block the diagnosis if the appointment was cancelled by anyone (Doctor, Secretary, or Patient).
                if (originalApp.StatusDoc == "CANCELLED" || originalApp.StatusSec == "CANCELLED")
                {
                    helper.CenterText("The PATIENT or YOU cancelled the appointment...\nUnable to give Diagnosis", ConsoleColor.Red);
                    Console.ReadKey(true);
                    return; // Return back.
                }

                // --- Record Diagnosis ---
                Console.Write("Enter Diagnosis: ");
                string diagnosis = Console.ReadLine()!;

                // Store the entered diagnosis in the appointment record.
                originalApp.Diagnosis = diagnosis;

                helper.CenterText("Patient Diagnosed Successfully!", ConsoleColor.DarkGreen);


                // --- Save to JSON ---

                // Save the updated list (with the new diagnosis) back to the doctor's appointment file (JSON).
                File.WriteAllText(filePath, JsonSerializer.Serialize(appointments, new JsonSerializerOptions { WriteIndented = true }));

                // Also update the matching appointment in the patient's personal file.
                string patientFile = $"{selectedApp.PatCode}.json"; // Get the patient's unique data file name.
                if (File.Exists(patientFile))
                {
                    string patData = File.ReadAllText(patientFile);
                    if (!string.IsNullOrWhiteSpace(patData))
                    {
                        var patAppointments = JsonSerializer.Deserialize<List<AppointmentData>>(patData);

                        // Find the exact appointment in the patient's file.
                        var matching = patAppointments?.FirstOrDefault(x => x.TimeStart == selectedApp.TimeStart && x.DoctorUser == selectedApp.DoctorUser);
                        if (matching != null)
                        {
                            // Update the diagnosis in the patient's record.
                            matching.Diagnosis = originalApp.Diagnosis;

                            // Save the patient's updated file so they can see the diagnosis.
                            File.WriteAllText(patientFile, JsonSerializer.Serialize(patAppointments, new JsonSerializerOptions { WriteIndented = true }));
                        }
                    }
                }

                helper.CenterText("Changes saved successfully!", ConsoleColor.Yellow);
            }
        }


        //-------------------------
        //Search Appoitment
        //-------------------------
        private List<AppointmentData> SearchAppointments(ConsoleHelper helper, List<AppointmentData> allAppointments)
        {
            Console.Clear();
            helper.printClinicName();

            // Display header for the search function.
            helper.CenterText("Search Appointments", ConsoleColor.Yellow);
            helper.CenterText("────────────────────────────────────────────", ConsoleColor.Yellow);

            // Get the word or phrase the user wants to search for.
            Console.Write("\nEnter search term: ");
            string searchTerm = Console.ReadLine()!.Trim().ToUpper();

            // --- Search Logic ---
            // Filter the main list of appointments based on the search term.
            var matches = allAppointments.Where(a =>
                // Check if the search term matches any part of the Patient's Username.
                (a.PatientUser?.ToUpper() ?? "").Contains(searchTerm) ||
                // Search the Secretary's Status field (e.g., PENDING).
                (a.StatusSec?.ToUpper() ?? "").Contains(searchTerm) ||
                // Search the Doctor's Status field (e.g., CONFIRMED, CANCELLED).
                (a.StatusDoc?.ToUpper() ?? "").Contains(searchTerm) ||
                // Search the Patient's Medical History.
                (a.MedHis?.ToUpper() ?? "").Contains(searchTerm) ||
                // Search the Doctor's Username.
                (a.DoctorUser?.ToUpper() ?? "").Contains(searchTerm) ||
                // Search the Diagnosis field (where the medical notes are stored).
                (a.Diagnosis?.ToUpper() ?? "").Contains(searchTerm) ||
                // Search the Reason for the appointment.
                (a.Reason?.ToUpper() ?? "").Contains(searchTerm)
            ).ToList();

            // If no appointments matched the search term, inform the user and return the full list.
            if (matches.Count == 0)
            {
                helper.CenterText("No matching appointments found.", ConsoleColor.Red);
                Console.ReadKey(true);
                return allAppointments;
            }

            // Return the list of matching appointments to be displayed.
            return matches; // displayedAppointments will show filtered list
        }



        //-------------------------
        //For Viewing Patient who Booked
        //-------------------------
        public virtual void ViewAppointments()
        {
            ConsoleHelper helper = new ConsoleHelper();
            string filePath = $"{Code}.json"; // Name the file path for the Doctor's/Secretary's appointments using their unique code.

            // Load appointments once
            List<AppointmentData> appointments = new List<AppointmentData>();

            // Check if the appointment file exists for this user.
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(jsonData))
                    // Load the appointments from the JSON file.
                    appointments = JsonSerializer.Deserialize<List<AppointmentData>>(jsonData) ?? new List<AppointmentData>();
            }
            else
            {
                // If file is missing.
                Console.Clear();
                helper.printClinicName();
                helper.CenterText("File not found!", ConsoleColor.Red);
                Console.ReadKey(true);
                return;
            }

            // If no appointments found.
            if (appointments.Count == 0)
            {
                Console.Clear();
                helper.printClinicName();
                helper.CenterText("No Appointments Scheduled.", ConsoleColor.Yellow);
                Console.ReadKey(true);
                return;
            }

            // Main interactive loop: allows the user to stay on this screen to view, manage, or search.
            bool exit = false;
            // 'displayedAppointments' starts as the full list, but will change if the user performs a search.
            List<AppointmentData> displayedAppointments = appointments;

            while (!exit)
            {
                Console.Clear();
                helper.printClinicName();
                // Display the user's name (Doctor or Secretary) in the header.
                helper.CenterText($"{User} Appointments", ConsoleColor.Yellow);
                helper.CenterText("────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────", ConsoleColor.Yellow);


                // Loop through and display the current list of appointments (either all or the search results).
                int count = 1;
                foreach (var a in displayedAppointments)
                {
                    // Print appointment details, using the helper for centering and color formatting.
                    helper.CenterAppointment($"[{count}]", ConsoleColor.Yellow);
                    helper.PrintCenteredColoredLine("Patient: ", a.PatientUser ?? "");
                    helper.PrintCenteredColoredLine("Doctor: ", a.DoctorUser ?? "");
                    // ... (Other details are printed) ...
                    helper.CenterAppointment("────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────", ConsoleColor.Green);
                    count++;
                }

                // Display the list of available commands to the user.
                helper.CenterText("[Enter to Select] [TAB to Manage Appointment] " +
                    "[V to Give Diagnosis] [Q to Search] [ESC to Go Back]", ConsoleColor.Yellow);

                // Wait for a key press to decide the next action.
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Tab:  // TAB key
                                          // Call the Manage function (Confirm/Cancel appointments).
                        Manage(helper, appointments, displayedAppointments, filePath);
                        break;

                    case ConsoleKey.V:    // V key
                                          // Call the Diagnosis function (record medical notes).
                        Diagnosis(helper, appointments, displayedAppointments, filePath);
                        break;

                    case ConsoleKey.Q:    // Q key
                                          // Call the Search function and update the list of appointments being displayed.
                        displayedAppointments = SearchAppointments(helper, appointments);
                        break;

                    case ConsoleKey.Escape: // ESC key
                                            // Exits and return to the main menu.
                        exit = true;
                        break;
                }
            }
        }



        //-------------------------
        //Remove Secretary's Account
        //-------------------------
        private void SearchAndRemoveSecretary()
        {
            ConsoleHelper helper = new ConsoleHelper();

            // --- Load and Filter Data ---
            // Load the master list of ALL secretaries in the system.
            var allSecs = SecretaryDataBase.LoadSecretary();

            // Filter the master list to get only the secretaries assigned to *this* doctor (using the Doctor's unique code).
            var mySecs = allSecs.Where(s => s.doctorCode == Code).ToList();

            // Check if the doctor has any secretaries to manage.
            if (mySecs.Count == 0)
            {
                helper.CenterText("You have no secretaries to remove.", ConsoleColor.Yellow);
                Console.ReadKey(true);
                return; // Stop the process.
            }

            // --- Search Input ---
            // Clear screen and ask the doctor to enter a search term (like a partial name).
            Console.Clear();
            helper.printClinicName();
            helper.CenterText("Enter part of the secretary name to search:");
            helper.CenterText("(Example: 'james')");

            Console.Write("\nSearch: ");
            string keyword = Console.ReadLine()?.Trim().ToLower() ?? "";

            if (string.IsNullOrEmpty(keyword))
                return;

            // Filter the doctor's secretaries list based on the search keyword.
            var filtered = mySecs
                .Where(s => s.userName?.ToLower().Contains(keyword) == true)
                .ToList();

            // Check if the search found any matches.
            if (filtered.Count == 0)
            {
                helper.CenterText("No secretary found with that name.", ConsoleColor.Yellow);
                Console.ReadKey(true);
                return; // Stop the process.
            }

            // --- Selection Menu ---
            // Prepare the list of matching secretaries for the menu display.
            List<string> options = filtered
                .Select(s => $"{s.userName} | Code: {s.SecCode}")
                .ToList();

            options.Add("Back");

            // Display a menu with page navigation for the doctor to select a secretary to remove.
            int choice = helper.ShowMenuWithPagesCenter(options, "Select secretary to remove:");

            // Check if the user chose "Back" or pressed ESC.
            if (choice == -1 || choice == options.Count - 1)
                return;

            // Get the selected secretary object from the filtered list.
            var selected = filtered[choice];

            // --- Confirmation ---
            // Ask for final confirmation before deleting.
            Console.Clear();
            helper.printClinicName();
            helper.CenterText("Are you sure you want to delete:", ConsoleColor.Yellow);
            helper.CenterText($"{selected.userName}", ConsoleColor.Red);
            helper.CenterText("[Y] Yes    [N] No", ConsoleColor.Yellow);

            var key = Console.ReadKey(true).Key;

            if (key != ConsoleKey.Y)
                return;

            // --- Deletion ---
            // Call the database function to permanently remove the secretary record from the system file.
            SecretaryDataBase.DeleteSecretary(selected.SecCode ?? "");

            helper.CenterText("Secretary removed successfully!", ConsoleColor.Green);
            Console.ReadKey(true);
        }



        //-------------------------
        //Doctor's Menu
        //-------------------------
        public override void Menu()
        {
            // loops until they log out.
            while (true)
            {

                string timeIn = "NOT SET";
                string timeOut = "NOT SET";

                string filePath = "doctors.json";

                // --- Load Current Schedule ---
                // Check the main doctor file to retrieve the user's current Time In and Time Out,
                // which are displayed in the menu header.
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    List<DoctorData> doctors = JsonSerializer.Deserialize<List<DoctorData>>(json) ?? new List<DoctorData>();

                    // Find the data for the currently logged-in doctor.
                    var currentDoctor = doctors.FirstOrDefault(d =>
                        d.UserName?.Equals(User, StringComparison.OrdinalIgnoreCase) == true);

                    if (currentDoctor != null)
                    {

                        if (!string.IsNullOrEmpty(currentDoctor.timeIn))
                            timeIn = currentDoctor.timeIn;
                        if (!string.IsNullOrEmpty(currentDoctor.timeOut))
                            timeOut = currentDoctor.timeOut;
                    }
                }

                // Header displaying the doctor's details and current schedule.
                string header =
                    $"Doctor : {User}\n" +
                    $"Specialization: {fieldSpec}\n" +
                    $"Hospital: {Hosp}\n" +
                    $"Contact Number: {conNum}\n" +
                    $"Time In: {timeIn}\n" +
                    $"Time Out: {timeOut}\n";


                // Display the main menu options to the doctor.
                int choice = helper.ShowMenuWithUICenter(DocMenu, header);

                Console.Clear();
                helper.printClinicName();


                switch (choice)
                {
                    //-------------------------
                    //VIEW APPOINTMENTS
                    //-------------------------
                    case 0: // Viewing of Patients who appointed
                            // Navigate to the appointment viewing and management screen.
                        ViewAppointments();
                        Console.ReadKey(true);
                        break;

                    //-------------------------
                    //SET SCHEDULE
                    //-------------------------
                    case 1: //Doctor choose their own sched

                        Console.Write("Enter start time (yyyy MM dd HH:mm): ");
                        string startInput = Console.ReadLine()!;

                        Console.Write("Enter end time (yyyy MM dd HH:mm): ");
                        string endInput = Console.ReadLine()!;

                        // Attempt to parse the user's input into date/time objects.
                        if (DateTime.TryParse(startInput, out DateTime start) &&
                            DateTime.TryParse(endInput, out DateTime end))
                        {
                            DateTime tomorrow = DateTime.Now.Date.AddDays(1);

                            // Validation: Check if the schedule dates are for tomorrow.
                            if (start.Date != tomorrow || end.Date != tomorrow)
                            {
                                helper.CenterText("You can only schedule appointments for tomorrow!!", ConsoleColor.Red);
                            }
                            // Validation: Check if the end time is after the start time.
                            else if (end <= start)
                            {

                                helper.CenterText("End time must be after start time.\n", ConsoleColor.Red);

                            }
                            else
                            {
                                // If valid, save the new schedule to the doctor's data object.
                                sched.StartSched = start;
                                sched.EndSched = end;
                                hasSched = true;

                                // Update the schedule times in the main doctor database file.
                                if (!string.IsNullOrWhiteSpace(User))
                                {
                                    DoctorDataBase.UpdateDoctorTime(User, start.ToString("yyyy-MM-dd HH:mm"), end.ToString("yyyy-MM-dd HH:mm"));
                                }
                                else
                                {
                                    helper.CenterText("Error: Doctor username is null! Cannot update schedule.", ConsoleColor.Red);
                                }

                                helper.CenterText($"Schedule added: {start:yyyy-MM-dd HH:mm} - {end:HH:mm}", ConsoleColor.DarkGreen);
                            }
                        }
                        else
                        {
                            // Handle invalid date/time format error.
                            helper.CenterText("Invalid format! Please use yyyy MM dd HH:mm format!!", ConsoleColor.Yellow);
                        }

                        Console.ReadKey(true);
                        break;

                    //-------------------------
                    //ADD SECRETARY
                    //-------------------------
                    case 2: //Doctor add their secretary

                        Console.Clear();
                        helper.printClinicName();
                        Console.WriteLine();
                        helper.CenterText("==== Secretary Registration ====", ConsoleColor.Yellow);
                        Console.WriteLine();

                        // Load the list of all secretaries to check for unique username.
                        var secretary = SecretaryDataBase.LoadSecretary();

                        Console.Write("Enter username: ");
                        string username = Console.ReadLine()!;

                        // Prepend "sec_" to the username for identification.
                        username = $"sec_{username}";

                        // Check if the secretary username already exists.
                        bool usernameExist = secretary.Any(s => s.userName?.Equals(username, StringComparison.OrdinalIgnoreCase) == true);

                        if (usernameExist)
                        {
                            helper.CenterText($"{username} already exists!! Please choose another one!!", ConsoleColor.Red);
                            helper.CenterText("\nPress any key to return...");
                            Console.ReadKey(true);
                            return;
                        }

                        // Get password and confirmation.
                        Console.Write("Enter password: ");
                        string password = Console.ReadLine() ?? "";

                        Console.Write("Confirm password: ");
                        string confirm = Console.ReadLine() ?? "";

                        // Validate passwords match.
                        if (confirm == password)
                        {

                            string file = "doctors.json";
                            DateTime docStart = DateTime.MinValue;
                            DateTime docEnd = DateTime.MinValue;

                            // Load the doctor's schedule times again to assign to the new secretary.
                            if (File.Exists(filePath))
                            {
                                string json = File.ReadAllText(file);
                                List<DoctorData> doctors = JsonSerializer.Deserialize<List<DoctorData>>(json) ?? new List<DoctorData>();

                                var currentDoctor = doctors.FirstOrDefault(d =>
                                    d.UserName?.Equals(User, StringComparison.OrdinalIgnoreCase) == true);

                                if (currentDoctor != null)
                                {
                                    DateTime.TryParse(currentDoctor.timeIn, out docStart);
                                    DateTime.TryParse(currentDoctor.timeOut, out docEnd);
                                }
                            }

                            // Create the new secretary record.
                            SecretaryData newSec = new SecretaryData
                            {
                                userName = username,
                                password = password,
                                doctorCode = Code ?? "", // Link the secretary to this doctor.
                                doctorName = User ?? "",
                                DocNum = conNum ?? "",
                                SecCode = SecretaryDataBase.GenerateUniqueSecCode(), // Generate a unique ID.
                                timeIn = docStart,
                                timeOut = docEnd
                            };

                            secretary.Add(newSec);

                            // Save the updated secretary list (with the new record) to the file.
                            SecretaryDataBase.SaveSecretary(secretary);

                            Console.Clear();
                            helper.printClinicName();

                            Console.WriteLine("\n");
                            helper.CenterText($"Successfully Registered {username}!!", ConsoleColor.DarkGreen);
                            Console.WriteLine("\n");

                        }
                        else
                        {
                            // Handle password mismatch error.
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Password doesn't match!! Enter again");
                            Console.ResetColor();
                            return;
                        }

                        Console.ReadKey(true);
                        return;

                    //-------------------------
                    //REMOVE SECRETARY
                    //-------------------------
                    case 3: //Search and Renmove Secretary
                            // Call the function to search for and remove a linked secretary.
                        SearchAndRemoveSecretary();
                        Console.ReadKey(true);
                        break;

                    //-------------------------
                    //LOG OUT
                    //-------------------------
                    case 4: //Log Out
                        helper.CenterText("\nLogging out...", ConsoleColor.DarkGreen);
                        Console.ReadKey(true);
                        return; // Exit the loop and return from the Menu method.
                }
            }
        }



        //-------------------------
        //Doctor Sign Up
        //-------------------------
        public override void SignUp()
        {
            Console.Clear();
            helper.printClinicName();
            Console.WriteLine();
            helper.CenterText("==== Doctor Registration ====", ConsoleColor.Yellow);
            Console.WriteLine();
            Console.Write("Enter authorization key: ");
            string key = Console.ReadLine() ?? "";

            // Check if the provided key matches the required authorization key.
            if (key == authKey)
            {
                // Load all existing doctor accounts to check for username uniqueness and for saving new data.
                var doctors = DoctorDataBase.LoadDoctors();

                try
                {
                    Console.Write("Enter username: ");
                    string username = Console.ReadLine() ?? "";

                    if (string.IsNullOrWhiteSpace(username)) // Handles null or empty username
                    {
                        helper.CenterText("Password cannot be empty!", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        return;
                    }

                    // Prefix the username with "dr_" for system identification.
                    username = $"dr_{username}";
                    bool usernameExist = doctors.Any(d => d.UserName?.Equals(username, StringComparison.OrdinalIgnoreCase) == true);

                    if (usernameExist)
                    {
                        // Handle duplicate username error.
                        helper.CenterText($"{username} already exists!! Please choose another one!!", ConsoleColor.Red);
                        helper.CenterText("\nPress any key to return...");
                        Console.ReadKey(true);
                        return;
                    }

                    UserName = username;

                    Console.Write("Enter password: ");
                    Password = Console.ReadLine() ?? "";

                    if (string.IsNullOrWhiteSpace(Password)) // Handles null or empty password
                    {
                        helper.CenterText("Password cannot be empty!", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        return;
                    }

                    Console.Write("Confirm password: ");
                    string confirm = Console.ReadLine() ?? "";

                    // Proceed with registration if passwords match.
                    if (confirm == Password)
                    {
                        Console.Write("Enter contact number: ");
                        Contact = Console.ReadLine() ?? "";

                        if (string.IsNullOrWhiteSpace(Contact)) 
                        {
                            helper.CenterText("Contact number cannot be empty!", ConsoleColor.Red);
                            Thread.Sleep(1000);
                            return;
                        }

                        Console.Write("Enter Hospital/Clinic Name: ");
                        Hosp = Console.ReadLine() ?? "";

                        if (string.IsNullOrWhiteSpace(Hosp))
                        {
                            helper.CenterText("Hospital/Clinic cannot be empty!", ConsoleColor.Red);
                            Thread.Sleep(1000);
                            return;
                        }

                        // Display menu to select specialization.
                        Console.Clear();
                        helper.printClinicName();
                        string head = "Choose Specialization";
                        int specilization = helper.ShowMenuWithPagesCenter(DoctorSpecializations, head);
                        string spec = DoctorSpecializations[specilization];

                        // Create a new DoctorData object.
                        DoctorData newDoc = new DoctorData
                        {
                            UserName = UserName,
                            Password = Password,
                            Contact = Contact,
                            Hospital = Hosp,
                            Specialization = spec,
                            Code = DoctorDataBase.GenerateUniqueDoctorCode() // Generate unique doctor ID.
                        };


                        // Add the new doctor to the list and save the updated list to the file.
                        doctors.Add(newDoc);
                        DoctorDataBase.SaveDoctors(doctors);

                        // Create a dedicated JSON file (e.g., DR-XXXXX.json) for the doctor's appointments.
                        DoctorDataBase.CreateDoctorFile(newDoc.Code);


                        // Display success account registration message.
                        Console.Clear();
                        helper.printClinicName();

                        Console.WriteLine("\n");
                        helper.CenterText($"Successfully Registered {UserName}!!", ConsoleColor.DarkGreen);
                        Console.WriteLine("\n");

                        helper.CenterText($"Username: {UserName}\nContact Number: {Contact}\nHospital/Clinic Name: {Hosp}\nSpecialization: {spec}", ConsoleColor.Yellow);

                        helper.CenterText("\bPress any key to return to menu...", ConsoleColor.Yellow);
                        Console.ReadKey(true);

                    }
                    else
                    {
                        // Handle password mismatch error.
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Password doesn't match!! Enter again");
                        Console.ResetColor();
                        return;
                    }
                }

                catch (Exception ex)
                {
                    // General error handler for unexpected issues.
                    Console.Clear();
                    helper.printClinicName();
                    helper.CenterText("An unexpected error occurred!", ConsoleColor.Red);
                    helper.CenterText(ex.Message, ConsoleColor.DarkRed);
                    Console.ReadKey(true);
                }



            }
            else
            {
                // Handle incorrect authorization key error.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Wrong authentication code!! TRY AGAIN!!");
                Console.ResetColor();
                return;
            }
        }
    }






        // -------------------------------------------
        // PATIENT CLASS
        // -------------------------------------------
        public class Patient : Accounts
    {

        ConsoleHelper helper = new ConsoleHelper();
        Schedule sched = new Schedule();

        //For Sign Up
        private string userName = string.Empty;
        private string password = string.Empty;
        private int? agePatient;
        private string[] medHistory = [];
        


        //For Menu
        private int? age;
        private string? medicalHistory;
        private string? reason;

        List<string> PatientMenu = new List<string> { "View Appointments", "Book Appointment", "Log Out" };

        //For Sign Up
        public string UserName
        {
            get => userName;
            set => userName = value;
        }

        public string Password
        {
            get => password;
            set => password = value;
        }

        public string? ConNum
        {
            get => conNum;
            set => conNum = value;
        }

        public int? AgePatient
        {
            get => agePatient;
            set => agePatient = value;
        }

        public string[] MedHistory
        {
            get => medHistory;
            set => medHistory = value;
        }

      
        
        //For Menu
        public int? Age
        {
            get => age;
            set => age = value;
        }

        public string? medHis
        {
            get => medicalHistory;
            set => medicalHistory = value;
        }

        public string? Reason
        {
            get => reason;
            set => reason = value;
        }

        //---------------
        //Time Slot Availability Checker
        //---------------
        public bool IsTimeTaken(List<AppointmentData> appointments, DateTime start, DateTime end)
        {
            // Iterate through every existing appointment to check for conflicts.
            foreach (var app in appointments)
            {
                // Calculate if the new time range (start/end) overlaps with the existing appointment's time range.
                bool overlap =
                    (start < app.TimeEnd && end > app.TimeStart);

                if (overlap)
                    // If any overlap is detected, the time slot is taken.
                    return true;
            }

            // If no conflicts are found after checking all existing appointments, the time slot is available.
            return false;
        }



        //---------------
        //Search Appointment
        //---------------
        private List<AppointmentData> Search(ConsoleHelper helper, List<AppointmentData> allAppointments)
        {
            Console.Clear();
            helper.printClinicName();
            helper.CenterText("Search Appointments", ConsoleColor.Yellow);
            helper.CenterText("────────────────────────────────────────────", ConsoleColor.Yellow);

            Console.Write("\nEnter search term: ");
            // Get user input and convert it to uppercase for case-insensitive searching across fields.
            string searchTerm = Console.ReadLine()!.Trim().ToUpper();

            // appointment is a match if the search term is contained within any of the
            // specified string fields (Patient, Statuses, History, Diagnosis, Reason).
            var matches = allAppointments.Where(a =>
                (a.PatientUser?.ToUpper() ?? "").Contains(searchTerm) ||
                (a.StatusSec?.ToUpper() ?? "").Contains(searchTerm) ||
                (a.StatusDoc?.ToUpper() ?? "").Contains(searchTerm) ||
                (a.MedHis?.ToUpper() ?? "").Contains(searchTerm) ||
                (a.DoctorUser?.ToUpper() ?? "").Contains(searchTerm) ||
                (a.Diagnosis?.ToUpper() ?? "").Contains(searchTerm) ||
                (a.Reason?.ToUpper() ?? "").Contains(searchTerm)
            ).ToList();

            if (matches.Count == 0)
            {
                // If no matches are found, notify the user.
                helper.CenterText("No matching appointments found.", ConsoleColor.Red);
                Console.ReadKey(true);
                // Return the original, unfiltered list.
                return allAppointments;
            }

            // Return the filtered list of matching appointments.
            return matches;
        }



        //-----------------
        //Patient Menu
        //-----------------
        public override void Menu() 
        {
            // Main menu loop.
            while (true)
            {
                // Display patient information as the menu header.
                string header =
                    $"Patient : {User}\n" +
                    $"Age: {age}\n" +
                    $"Contact Number: {ConNum}\n" +
                    $"Patiend Code: {Code}\n" +
                    $"Medical History: {medicalHistory}\n";

                // Show the main patient menu and get user choice.
                int choice = helper.ShowMenuWithUICenter(PatientMenu, header); //UI

                Console.Clear();
                helper.printClinicName();

                switch (choice)
                {

                    case 0: // Viewing of booked appointments
                        Console.WriteLine("\n");
                        helper.CenterText("---- Booked Appointment ----", ConsoleColor.Yellow);
                        Console.WriteLine("\n");

                        string filePath = $"{Code}.json"; // Path to the patient's appointment file (e.g., P-XXXXX.json).

                        if (File.Exists(filePath))
                        {
                            string jsonData = File.ReadAllText(filePath);

                            if (string.IsNullOrWhiteSpace(jsonData))
                            {
                                helper.CenterText("No Appointments Scheduled.", ConsoleColor.Yellow);
                                Console.ReadKey(true);
                                break;
                            }

                            // Deserialize all appointments for the patient.
                            List<AppointmentData> appointments = JsonSerializer.Deserialize<List<AppointmentData>>(jsonData) ?? new List<AppointmentData>();

                            // Filter the appointments based on a search term entered by the user.
                            appointments = Search(helper, appointments);

                            int count = 1;
                            // Display the resulting list of appointments (filtered or full).
                            foreach (var a in appointments)
                            {

                                // Number
                                helper.CenterAppointment($"[{count}]", ConsoleColor.Yellow);

                                // Patient
                                helper.PrintCenteredColoredLine("Patient: ", a.PatientUser ?? "");

                                helper.PrintCenteredColoredLine("Doctor: ", a.DoctorUser ?? "");

                                // Reason
                                helper.PrintCenteredColoredLine("Reason: ", a.Reason ?? "");

                                // Medical History
                                helper.PrintCenteredColoredLine("Medical History: ", a.MedHis ?? "");

                                // Start & End
                                helper.PrintCenteredColoredLine("Start: ", $"{a.TimeStart:yyyy-MM-dd HH:mm}");
                                helper.PrintCenteredColoredLine("End: ", $"{a.TimeEnd:yyyy-MM-dd HH:mm}");

                                // Status Doctor
                                helper.PrintCenteredColoredLine("Status Doctor: ", a.StatusDoc ?? "");

                                // Status Secretary
                                helper.PrintCenteredColoredLine("Status Secretary: ", a.StatusSec ?? "");

                                // Diagnosis
                                helper.PrintCenteredColoredLine("Diagnosis: ", a.Diagnosis ?? "");

                                // Divider
                                helper.CenterAppointment("────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────", ConsoleColor.Green);

                                count++;
                            }

                            helper.CenterText("[↑↓Arrow keys to Navigate] [Enter to Select] [ESC to Go Back]", ConsoleColor.Yellow);
                        }
                        else
                        {
                            Console.WriteLine("File not found!"); // In case file not found
                        }

                        Console.ReadKey(true);

                        break;

                    case 1: // Choose doctor and book appointment

                        // Load all doctors from the database.
                        List<DoctorData> doctors = DoctorDataBase.LoadDoctors();
                        List<DoctorData> availableDoctors = new List<DoctorData>();
                        DoctorData chosenDoctor;
                        List<string> doctorOptions = new List<string>();

                        // Filter for doctors who have set their availability (timeIn/timeOut).
                        foreach (var doctor in doctors)
                        {
                            if (!string.IsNullOrEmpty(doctor.timeIn) && !string.IsNullOrEmpty(doctor.timeOut))
                            {
                                availableDoctors.Add(doctor);
                                doctorOptions.Add($"{doctor.UserName} | {doctor.Hospital} | {doctor.Specialization}" +
                                    $" | Time: {doctor.timeIn} - {doctor.timeOut}");

                            }
                        }

                        if (availableDoctors.Count == 0) // If no doctors available
                        {
                            helper.CenterText("No doctors available. Check again later.", ConsoleColor.Yellow);
                            Console.ReadKey(true);
                            break;
                        }

                        string doctorHeader =
                            "Available Doctors\n";

                        // Show menu of available doctors.
                        int selectedIndex = helper.ShowMenuWithPagesCenter(doctorOptions, doctorHeader);

                        List<DoctorData> searchedOptions = new List<DoctorData>();

                        if (selectedIndex == -1)
                        {
                            // User pressed ESC to return.
                            break;
                        }
                        else if (selectedIndex == -10)
                        {
                            // User chose the search option.
                            Console.Clear();
                            helper.printClinicName();
                            Console.Write("Search doctors (name, hospital, specialization, time): ");
                            string query = Console.ReadLine()?.ToLower() ?? "";

                            // Filter available doctors based on the search query.
                            searchedOptions = availableDoctors
                                .Where(d =>
                                    (d.UserName?.ToLower().Contains(query) ?? false) ||
                                    (d.Hospital?.ToLower().Contains(query) ?? false) ||
                                    (d.Specialization?.ToLower().Contains(query) ?? false) ||
                                    (d.timeIn?.ToLower().Contains(query) ?? false) ||
                                    (d.timeOut?.ToLower().Contains(query) ?? false))
                                .ToList();

                            if (searchedOptions.Count == 0) // If search terms doesn't match any of the doctors
                            {
                                helper.CenterText("No doctors matched your search.", ConsoleColor.Red);
                                Console.ReadKey(true);
                                // Fall back to showing all available doctors if search yields no results.
                                searchedOptions = new List<DoctorData>(availableDoctors);
                            }

                            // Convert filtered list to strings for display in a new menu.
                            List<string> searchedStrings = searchedOptions
                                .Select(d => $"{d.UserName} | {d.Hospital} | {d.Specialization} | Time: {d.timeIn} - {d.timeOut}")
                                .ToList();

                            // Show menu with searched options.
                            int searchIndex = helper.ShowMenuWithPagesCenter(searchedStrings, doctorHeader);


                            if (searchIndex == -1) continue; // ESC pressed after search.

                            chosenDoctor = searchedOptions[searchIndex];

                        }
                        else
                        {
                            // A doctor was selected directly from the initial menu.
                            chosenDoctor = availableDoctors[selectedIndex];
                        }

                        Console.Clear();
                        helper.printClinicName();
                        // Shows selected doctor
                        Console.WriteLine($"\nYou selected: {chosenDoctor.UserName} ({chosenDoctor.Specialization}) ({chosenDoctor.timeIn} - {chosenDoctor.timeOut})\n");


                        // Prompt for appointment reason and validate.
                        while (true)
                        {
                            Console.Write("Enter reason for appointment: ");
                            reason = Console.ReadLine() ?? "";

                            if (string.IsNullOrWhiteSpace(reason))
                            {
                                helper.CenterText("Reason cannot be empty!", ConsoleColor.Red);
                                continue;
                            }

                            break;
                        }

                        // Prompt for start time and validate format.
                        Console.Write("Enter start time (yyyy MM dd HH:mm): ");
                        if (!DateTime.TryParse(Console.ReadLine(), out DateTime start))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Invalid start time format!");
                            Console.ResetColor();
                            Console.ReadKey(true);
                            break;
                        }

                        // Prompt for end time and validate format.
                        Console.Write("Enter end time (yyyy MM dd HH:mm): ");
                        if (!DateTime.TryParse(Console.ReadLine(), out DateTime end))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Invalid end time format!");
                            Console.ResetColor();
                            Console.ReadKey(true);
                            break;
                        }

                        // Parse the doctor's duty times for validation.
                        DateTime dutyStart = DateTime.Parse(chosenDoctor.timeIn ?? "");
                        DateTime dutyEnd = DateTime.Parse(chosenDoctor.timeOut ?? "");

                        // Validate appointment time is not before the doctor's duty starts.
                        if (start < dutyStart)
                        {
                            helper.CenterText($"Doctor starts at {dutyStart:yyyy MMMM dd HH:mm}", ConsoleColor.Red);
                            Console.ReadKey(true);
                            break;
                        }

                        // Validate appointment time is not after the doctor's duty ends.
                        if (end > dutyEnd)
                        {
                            helper.CenterText($"Doctor is only available until {dutyEnd:yyyy MMMM dd HH:mm}", ConsoleColor.Red);
                            Console.ReadKey(true);
                            break;
                        }

                        // Validate end time is after start time.
                        if (end <= start)
                        {
                            helper.CenterText("End time must be after start time!", ConsoleColor.Red);
                            Console.ReadKey(true);
                            break;
                        }

                        string doctorFile = $"{chosenDoctor.Code}.json";
                        List<AppointmentData> doctorAppointments = new List<AppointmentData>();

                        // Load the chosen doctor's existing appointments.
                        if (File.Exists(doctorFile))
                        {
                            string docJson = File.ReadAllText(doctorFile);

                            if (!string.IsNullOrWhiteSpace(docJson))
                            {
                                doctorAppointments = JsonSerializer.Deserialize<List<AppointmentData>>(docJson)
                                                         ?? new List<AppointmentData>();
                            }
                        }

                        // Check if the proposed time slot overlaps with any existing appointment.
                        if (IsTimeTaken(doctorAppointments, start, end))
                        {
                            helper.CenterText("Time already taken. Please choose another timeslot", ConsoleColor.Red);
                            Console.ReadKey(true);
                            break;
                        }

                        // Create the new appointment record.
                        AppointmentData record = new AppointmentData
                        {
                            DoctorUser = chosenDoctor.UserName,
                            PatCode = Code,
                            DocCode = chosenDoctor.Code,
                            PatientUser = User,
                            Reason = reason,
                            MedHis = medicalHistory,
                            TimeStart = start,
                            TimeEnd = end,
                            StatusSec = "NOT SET",
                            StatusDoc = "NOT SET",
                            Diagnosis = "NO DIAGNOSIS YET"

                        };

                        // Add and save the new record to the DOCTOR's file.
                        doctorAppointments.Add(record);

                        File.WriteAllText(
                            doctorFile,
                            JsonSerializer.Serialize(doctorAppointments, new JsonSerializerOptions { WriteIndented = true })
                        );


                        // === SAVE TO PATIENT FILE ===
                        string patientFile = $"{Code}.json";
                        List<AppointmentData> patientAppointments = new List<AppointmentData>();

                        // Load the patient's existing appointments.
                        if (File.Exists(patientFile))
                        {
                            string patJson = File.ReadAllText(patientFile);

                            if (!string.IsNullOrWhiteSpace(patJson))
                            {
                                patientAppointments = JsonSerializer.Deserialize<List<AppointmentData>>(patJson)
                                                         ?? new List<AppointmentData>();
                            }
                        }

                        // Add new appointment and save the updated list to the PATIENT's file.
                        patientAppointments.Add(record);

                        File.WriteAllText(
                            patientFile,
                            JsonSerializer.Serialize(patientAppointments, new JsonSerializerOptions { WriteIndented = true })
                        );


                        // Display final confirmation.
                        helper.CenterText("Appointment successfully booked!!", ConsoleColor.DarkGreen);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Doctor: {chosenDoctor.UserName}");
                        Console.WriteLine($"Schedule: {start:MMM dd, yyyy hh:mm tt} - {end:hh:mm tt}");
                        Console.WriteLine($"Reason: {reason}");
                        Console.ResetColor();

                        Console.ReadKey(true);
                        break;

                    case 2: // Exit Account
                        helper.CenterText("\nLogging out...", ConsoleColor.DarkGreen);
                        Console.ReadKey(true);
                        // Return from the menu, breaking the loop.
                        return;
                }
            }
        }

        //-----------
        // Patient Sign Up
        //----------
        public override void SignUp()
        {
            // Load all existing patient data from the database.
            var patients = PatientDataBase.LoadPatients();

            Console.Clear();
            helper.printClinicName();
            Console.WriteLine();

            string username;

            try
            {
                Console.Write("Enter username: ");
                username = Console.ReadLine() ?? "";

                if (string.IsNullOrWhiteSpace(username))
                {
                    helper.CenterText("Username cannot be empty!", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    return;
                }

                // Prefix the username with "pat_" for system identification.
                username = $"pat_{username}";

                // Check if the prefixed username already exists.
                bool usernameExist = patients.Any(p => p.UserName?.Equals(username, StringComparison.OrdinalIgnoreCase) == true);

                if (usernameExist)
                {
                    helper.CenterText($"{username} already exists!! Please choose another one", ConsoleColor.Red);
                    helper.CenterText("\nPress any key to return...");
                    Console.ReadKey(true);
                    return;
                }

                UserName = username;

                Console.Write("Enter password: ");
                Password = Console.ReadLine() ?? "";

                if (string.IsNullOrWhiteSpace(password))
                {
                    helper.CenterText("Password cannot be empty!", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    return;
                }

                Console.Write("Confirm password: ");
                string confirm = Console.ReadLine() ?? "";

                // Proceed with data collection only if passwords match.
                if (confirm == Password)
                {
                    Console.Write("Enter age: ");
                    // Note: Using int.Parse may cause an exception if non-numeric input is provided.
                    AgePatient = int.Parse(Console.ReadLine() ?? "");

                    if (AgePatient <= 0)
                    {
                        helper.CenterText("Invalid age! Enter a positive number.", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        return;
                    }

                    Console.Write("Enter Medical History (Separate with spaces): ");
                    // Read medical history and split the string into an array of strings.
                    MedHistory = Console.ReadLine()!.Split(' ');

                    // A quick check to see if the MedHistory array is null (though Split() on a non-null string won't return null).
                    if (MedHistory == null || MedHistory.Length == 0)
                    {
                        helper.CenterText("Medical history cannot be empty!", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        return;
                    }


                    Console.Write("Enter contact number: ");
                    ConNum = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(ConNum))
                    {
                        helper.CenterText("Contact number cannot be empty!", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        return;
                    }

                    // Create the new PatientData object.
                    PatientData newPat = new PatientData
                    {
                        UserName = UserName,
                        Password = Password,
                        ConNum = ConNum,
                        MedHistory = MedHistory,
                        AgePatient = AgePatient,
                        // Generate a unique patient code (e.g., P-XXXXXX).
                        Code = PatientDataBase.GenerateUniquePatientCode()
                    };

                    // Update class properties with new user data.
                    User = UserName;
                    MedHis = MedHistory;
                    AgePat = AgePatient;
                    Code = newPat.Code;

                    this.Code = newPat.Code;

                    // Add the new patient to the list and save the complete list back to the patient database file.
                    patients.Add(newPat);
                    PatientDataBase.SavePatient(patients);

                    // Create a dedicated JSON file for the patient's appointments (e.g., P-XXXXXX.json).
                    PatientDataBase.CreatePatientFile(newPat.Code);

                    Console.Clear();
                    helper.printClinicName();

                    // Display success message and registered details.
                    Console.WriteLine("\n");
                    helper.CenterText($"Successfully registered {UserName}", ConsoleColor.DarkGreen);
                    Console.WriteLine("\n");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"Username: {UserName}\nAge: {AgePatient}\n");
                    Console.Write("Medical History: ");
                    for (int i = 0; i < MedHistory.Length; i++)
                    {
                        Console.Write($" {MedHistory[i]}|");
                    }
                    Console.WriteLine($"\nContact Number: {ConNum}");
                    Console.ResetColor();

                    helper.CenterText("\nPress any key to return to menu...", ConsoleColor.Yellow);
                    Console.ReadKey(true);

                }
                else
                {
                    // Handle password mismatch error.
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Password do not match!!Enter again!!");
                    Console.ResetColor();
                    Console.ReadKey(true);
                    return;
                }
            }
            // Catch any unexpected errors, such as a failed file write or invalid int.Parse attempt.
            catch (Exception ex)
            {
                Console.Clear();
                helper.printClinicName();
                helper.CenterText("An unexpected error occurred!", ConsoleColor.Red);
                helper.CenterText(ex.Message, ConsoleColor.DarkRed);
                Console.ReadKey(true);
            }
        }

    }





    // -------------------------------------------
    // SECRETARY CLASS
    // -------------------------------------------
    public class Secretary : Accounts, ViewPatientDetails
    {

      
        private string? username;
        private string? doccode;
        private string? seccode;
        private string? docname;
        private string? docnum;
        private DateTime timeIn;
        private DateTime timeOut;
         

        public string? Username
        {
            get => username;
            set => username = value;
        }

        public string? Doccode
        {
            get => doccode;
            set => doccode = value;
        }

        public string? Seccode
        {
            get => seccode;
            set => seccode = value;
        }

        public string? Docname
        {
            get => docname;
            set => docname = value;
        }

        public string? Docnum
        {
            get => docnum;
            set => docnum = value;
        }

        public DateTime TimeIn
        {
            get => timeIn;
            set => timeIn = value;
        }

        public DateTime TimeOut
        {
            get => timeOut;
            set => timeOut = value;
        }

        
        ConsoleHelper helper = new ConsoleHelper();

        // Secretary Menu Options
        List<string> SecMenu = new List<string> { "Check doctor's Appointment", "Register Patient", "Log Out" };

        //----------
        // Secretary's Menu
        //---------
        public override void Menu()
        {
            while (true) // Keep showing menu until Log Out
            {
                //Secretary's details
                string header =
                    $"Secretary: {Username}\n" +
                    $"Doctor In Charge: {Docname}\n" +
                    $"Doctor's Contact Number: {Docnum}\n" +
                    $"Doctor's Time In: {TimeIn}\n" +
                    $"Doctor's Time Out: {TimeOut}";

                int choice = helper.ShowMenuWithUICenter(SecMenu, header);

                Console.Clear();
                helper.printClinicName();

                switch (choice)
                {
                    case 0: // Viewing the doctor's appointment
                        ViewAppointments();
                        Console.ReadKey(true);  // Wait for key
                        break; // go back to the menu

                    case 1: // Register a patient
                        SignUp();
                        break; // return to menu after patient menu exits

                    case 2: // Log Out
                        helper.CenterText("\nLogging Out...", ConsoleColor.DarkGreen);
                        Console.ReadKey(true);
                        return; // exit the menu loop
                }
            }
        }

        //----------
        // Manage Appointments
        //---------
        private void Manage(ConsoleHelper helper, List<AppointmentData> appointments, List<AppointmentData> displayedAppointments, string filePath)
        {
            helper.CenterText("Manage Appointment", ConsoleColor.Yellow);
            helper.CenterText("───────────────────────────────────────────────────────────", ConsoleColor.Yellow);

            Console.Write("\nEnter appointment number to manage: ");

            // Attempt to parse the selected number and validate it is within the displayed list's range.
            if (int.TryParse(Console.ReadLine(), out int selected) && selected > 0 && selected <= displayedAppointments.Count)
            {
                // Get the specific appointment from the currently displayed (potentially filtered) list.
                var selectedApp = displayedAppointments[selected - 1];

                // Find the matching appointment in the full, original list using unique identifiers (Time and Doctor).
                var originalApp = appointments.First(a => a.TimeStart == selectedApp.TimeStart && a.DoctorUser == selectedApp.DoctorUser);

                Console.Write("\nEnter action [CONFIRM/CANCEL]: ");
                string action = Console.ReadLine()!.Trim().ToUpper();

                if (action == "CONFIRM")
                {
                    // Update the secretary status to CONFIRMED.
                    originalApp.StatusSec = "CONFIRMED";
                    helper.CenterText("Appointment confirmed successfully!", ConsoleColor.Green);
                }
                else if (action == "CANCEL")
                {
                    Console.Write("Enter reason for cancellation: ");
                    string reason = Console.ReadLine()!.Trim();

                    // Update both secretary and doctor statuses to CANCELLED.
                    originalApp.StatusSec = "CANCELLED";
                    originalApp.StatusDoc = "CANCELLED";

                    // Record the cancellation reason in the Diagnosis field.
                    originalApp.Diagnosis = $"CANCELLED: {reason}";
                    helper.CenterText("Appointment cancelled successfully!", ConsoleColor.Red);
                }
                else
                {
                    helper.CenterText("Invalid action!", ConsoleColor.Red);
                    Console.ReadKey(true);
                    return;
                }

                // --- Synchronization Logic ---

                // Save the updated list back to the DOCTOR's file (using the provided filePath).
                File.WriteAllText(filePath, JsonSerializer.Serialize(appointments, new JsonSerializerOptions { WriteIndented = true }));

                // Get the path to the patient's corresponding file (e.g., P-XXXXX.json).
                string patientFile = $"{selectedApp.PatCode}.json";

                // Load, update, and save the patient's file to keep records synchronized.
                if (File.Exists(patientFile))
                {
                    string patData = File.ReadAllText(patientFile);
                    if (!string.IsNullOrWhiteSpace(patData))
                    {
                        var patAppointments = JsonSerializer.Deserialize<List<AppointmentData>>(patData);

                        // Find the exact matching appointment in the patient's file.
                        var matching = patAppointments?.FirstOrDefault(x => x.TimeStart == selectedApp.TimeStart && x.DoctorUser == selectedApp.DoctorUser);

                        if (matching != null)
                        {
                            // Copy the updated statuses and diagnosis from the doctor's record to the patient's record.
                            matching.StatusSec = originalApp.StatusSec;
                            matching.StatusDoc = originalApp.StatusDoc;
                            matching.Diagnosis = originalApp.Diagnosis;

                            // Save the synchronized patient data back to their file.
                            File.WriteAllText(patientFile, JsonSerializer.Serialize(patAppointments, new JsonSerializerOptions { WriteIndented = true }));
                        }
                    }
                }

                helper.CenterText("Changes saved successfully!", ConsoleColor.Yellow);
                Console.ReadKey(true);
            }
            else
            {
                // Handle invalid input error.
                helper.CenterText("Invalid appointment number!", ConsoleColor.Red);
                Console.ReadKey(true);
            }
        }

        //----------
        // Search Appointment
        //---------
        private List<AppointmentData> SearchAppointments(ConsoleHelper helper, List<AppointmentData> allAppointments)
        {
            Console.Clear();
            helper.printClinicName();
            helper.CenterText("Search Appointments", ConsoleColor.Yellow);
            helper.CenterText("────────────────────────────────────────────", ConsoleColor.Yellow);

            Console.Write("\nEnter search term: ");
            // Get search term and convert to uppercase for case-insensitive filtering.
            string searchTerm = Console.ReadLine()!.Trim().ToUpper();

            // Use LINQ to filter appointments. Checks if the search term is contained
            // in the patient name, statuses, history, diagnosis, doctor, or reason.
            var matches = allAppointments.Where(a =>
                (a.PatientUser?.ToUpper() ?? "").Contains(searchTerm) ||
                (a.StatusSec?.ToUpper() ?? "").Contains(searchTerm) ||
                (a.StatusDoc?.ToUpper() ?? "").Contains(searchTerm) ||
                (a.MedHis?.ToUpper() ?? "").Contains(searchTerm) ||
                (a.DoctorUser?.ToUpper() ?? "").Contains(searchTerm) ||
                (a.Diagnosis?.ToUpper() ?? "").Contains(searchTerm) ||
                (a.Reason?.ToUpper() ?? "").Contains(searchTerm)
            ).ToList();


            if (matches.Count == 0)
            {
                helper.CenterText("No matching appointments found.", ConsoleColor.Red);
                Console.ReadKey(true);
                // Return the original full list if no matches were found.
                return allAppointments;
            }

            // Return the list of filtered matches.
            return matches;
        }


        //----------
        // View the Managed Doctor's Appointment
        //---------
        public virtual void ViewAppointments()
        {
            Console.Clear();
            ConsoleHelper helper = new ConsoleHelper();
            helper.printClinicName();

            helper.CenterText($"{Docname} Appointments", ConsoleColor.Yellow);
            helper.CenterText("────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────", ConsoleColor.Yellow);

            string filePath = $"{Doccode}.json"; // Doctor's/Secretary's appointment file path.

            List<AppointmentData> appointments = new List<AppointmentData>();
            // Check if the appointment file exists and load data.
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(jsonData))
                    // Deserialize JSON data into the list of appointments.
                    appointments = JsonSerializer.Deserialize<List<AppointmentData>>(jsonData) ?? new List<AppointmentData>();
            }
            else
            {
                // Handle missing file error.
                Console.Clear();
                helper.printClinicName();
                helper.CenterText("File not found!", ConsoleColor.Red);
                Console.ReadKey(true);
                return;
            }

            if (appointments.Count == 0)
            {
                // Handle case where file exists but contains no appointments.
                Console.Clear();
                helper.printClinicName();
                helper.CenterText("No Appointments Scheduled.", ConsoleColor.Yellow);
                Console.ReadKey(true);
                return;
            }

            // Main interactive loop for viewing, searching, and managing.
            bool exit = false;
            // 'displayedAppointments' starts as the full list and holds the search results when filtered.
            List<AppointmentData> displayedAppointments = appointments;

            while (!exit)
            {
                Console.Clear();
                helper.printClinicName();
                helper.CenterText($"{User} Appointments", ConsoleColor.Yellow);
                helper.CenterText("────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────", ConsoleColor.Yellow);

                // Display all appointments (which may be the full or filtered list).
                int count = 1;
                foreach (var a in displayedAppointments)
                {
                    // Code block for centering and printing appointment details...
                    helper.CenterAppointment($"[{count}]", ConsoleColor.Yellow);
                    helper.PrintCenteredColoredLine("Patient: ", a.PatientUser ?? "");
                    helper.PrintCenteredColoredLine("Doctor: ", a.DoctorUser ?? "");
                    helper.PrintCenteredColoredLine("Reason: ", a.Reason ?? "");
                    helper.PrintCenteredColoredLine("Medical History: ", a.MedHis ?? "");
                    helper.PrintCenteredColoredLine("Start: ", $"{a.TimeStart:yyyy-MM-dd HH:mm}");
                    helper.PrintCenteredColoredLine("End: ", $"{a.TimeEnd:yyyy-MM-dd HH:mm}");
                    helper.PrintCenteredColoredLine("Status Doctor: ", a.StatusDoc ?? "");
                    helper.PrintCenteredColoredLine("Status Secretary: ", a.StatusSec ?? "");
                    helper.PrintCenteredColoredLine("Diagnosis: ", a.Diagnosis ?? "");
                    helper.CenterAppointment("────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────", ConsoleColor.Green);
                    count++;
                }

                helper.CenterText("[Enter to Select] [TAB to Manage Appointment] [Q to Search] [ESC to Go Back]", ConsoleColor.Yellow);

                // Wait for key press to determine next action.
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Tab:  // Manage appointment
                                          // Calls the Manage method to confirm or cancel an appointment.
                        Manage(helper, appointments, displayedAppointments, filePath);
                        break;

                    case ConsoleKey.Q:    // Search appointments
                                          // Replaces the displayed list with the search results.
                        displayedAppointments = SearchAppointments(helper, appointments);
                        break;

                    case ConsoleKey.Escape:
                        exit = true; // Exits the view loop.
                        break;
                }
            }
        }


        //----------
        // Secretary Sign Up for Patient
        //---------
        public override void SignUp()
        {
            Patient forPat = new Patient();

            
            forPat.SignUp(); // Redirect the secretary to the patient SIGN UP area

            
            if (forPat.MedHistory == null || forPat.MedHistory.Length == 0)
            {
                //Default if medical history is empty or there is no medical history
                forPat.MedHistory = new string[] { "None" }; // or leave as empty array
                return;
            }

            // Map collected data from the temporary patient object to class properties 
            // needed for the patient menu (although some are already set in forPat.SignUp).
            forPat.Age = forPat.AgePatient;
            forPat.medHis = string.Join(" | ", forPat.MedHistory);
            forPat.User = forPat.UserName;

            // Immediately open the main menu for the newly created patient account.
            forPat.Menu();
        }

        
    }











    // -------------------------------------------
    // FOR SCHEDULE CLASS
    // -------------------------------------------
    public class Schedule
    {
        private DateTime start;
        private DateTime end;

        public DateTime StartSched
        {
            get => start;
            set => start = value;
        }

        public DateTime EndSched
        {
            get => end;
            set => end = value;
        }
    }
















    // -------------------------------------------
    // FOR APPOINTING APPOINTMENT CLASS
    // -------------------------------------------
    

    public class AppointmentData
    {

        private string? doctorUser;
        private string? patCode;
        private string? docCode;
        private string? patientUser;
        private string? reason;
        private DateTime timeIn;
        private DateTime timeEnd;
        private string? medHis;
        private string? statusSec;
        private string? statusDoc;
        private string? diagnosis; 

        public string? DoctorUser
        {
            get => doctorUser;
            set => doctorUser = value;
        }

        public string? PatCode
        {
            get => patCode;
            set => patCode = value;
        }

        public string? DocCode
        {
            get => docCode;
            set => docCode = value;
        }

        public string? PatientUser
        {
            get => patientUser;
            set => patientUser = value;
        }

        public string? Reason
        {
            get => reason;
            set => reason = value;
        }

        public DateTime TimeStart
        {
            get => timeIn;
            set => timeIn = value;
        }

        public DateTime TimeEnd
        {
            get => timeEnd;
            set => timeEnd = value;
        }

        public string? MedHis
        {
            get => medHis;
            set => medHis = value;
        }

        public string? StatusSec
        {
            get => statusSec;
            set => statusSec = value;
        }

        public string? StatusDoc
        {
            get => statusDoc;
            set => statusDoc = value;
        }

        public string? Diagnosis
        {
            get => diagnosis;
            set => diagnosis = value;
        }
    }








    // -------------------------------------------
    // DOCTOR'S DATA CLASS
    // -------------------------------------------
    public class DoctorData
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Contact { get; set; }
        public string? Hospital { get; set; }
        public string? Specialization { get; set; }
        public string? Code { get; set; }

        public string? timeIn { get; set; }
        public string? timeOut { get; set; }
    }







    // -------------------------------------------
    // DOCTOR'S DATABASE CLASS
    // -------------------------------------------
    public class DoctorDataBase
    {


        private static string filePath = "doctors.json";

        //----------
        // Load Doctor's Details
        //---------
        public static List<DoctorData> LoadDoctors()
        {
            // If the data file doesn't exist, return an empty list to prevent errors.
            if (!File.Exists(filePath))
                return new List<DoctorData>();

            // Read all JSON content from the file.
            string json = File.ReadAllText(filePath);
            // Deserialize the JSON string into a list of DoctorData objects. Returns a new list if deserialization fails (e.g., file is empty).
            return JsonSerializer.Deserialize<List<DoctorData>>(json) ?? new List<DoctorData>();
        }


        //----------
        //Save Doctor's Details
        //---------
        public static void SaveDoctors(List<DoctorData> doctors)
        {
            // Serialize the list of DoctorData objects to a formatted JSON string.
            string json = JsonSerializer.Serialize(doctors, new JsonSerializerOptions { WriteIndented = true });
            // Write the JSON string back to the doctor data file, overwriting the old content.
            File.WriteAllText(filePath, json);
        }


        //----------
        //Create Doctor's Own File
        //---------
        public static void CreateDoctorFile(string code)
        {
            string filePath = $"{code}.json";

            // Create a unique appointment file for the doctor using their code (e.g., dr_XXXXXX.json).
            if (!File.Exists(filePath))
            {
                // Write an empty string to create an empty JSON file.
                File.WriteAllText(filePath, string.Empty);
            }
            else
            {
                Console.WriteLine($"{code}json already exists.");
            }
        }

        //----------
        //Generate Doctor's Code
        //---------
        public static string GenerateRandomCode(int length = 6)
        {
            // Define the characters allowed in the random code.
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            // Generate a string of specified length using random characters.
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        //----------
        //Unique Doctor's Code
        //---------
        public static string GenerateUniqueDoctorCode()
        {
            List<DoctorData> doctors = DoctorDataBase.LoadDoctors();
            string newCode;
            bool codeExists;

            do
            {
                // Generate a new code prefixed with "dr_".
                newCode = "dr_" + GenerateRandomCode(6);
                // Check if this generated code is already in use by another doctor.
                codeExists = doctors.Any(d => d.Code == newCode);
            }
            while (codeExists); // Repeat until a unique code is found.

            return newCode;
        }


        //----------
        //Update Doctor's Available Time
        //---------
        public static void UpdateDoctorTime(string username, string timeIn, string timeOut) //Update the file to add timein timeout
        {

            ConsoleHelper helper = new ConsoleHelper();

            string filePath = "doctors.json"; // The file containing the list of all doctors.

            if (!File.Exists(filePath))
            {
                Console.WriteLine("doctors.json not found!");
                return;
            }

            // Load the entire list of doctors.
            string json = File.ReadAllText(filePath);
            List<DoctorData> doctors = JsonSerializer.Deserialize<List<DoctorData>>(json) ?? new List<DoctorData>();

            // Find the specific doctor by their username (case-insensitive search).
            var doctor = doctors.FirstOrDefault(d =>
                d.UserName?.Equals(username, StringComparison.OrdinalIgnoreCase) == true);

            if (doctor != null)
            {
                // Update the doctor's availability fields.
                doctor.timeIn = timeIn;
                doctor.timeOut = timeOut;

                // Save the entire updated list back to the data file.
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(filePath, JsonSerializer.Serialize(doctors, options));

                helper.CenterText($"Updated {username}'s TimeIn and TimeOut successfully!", ConsoleColor.DarkGreen);
            }
            else
            {
                helper.CenterText("Doctor not found!!", ConsoleColor.Red);
            }
        }

    }







    // -------------------------------------------
    // PATIENT'S DATA CLASS
    // -------------------------------------------
    public class PatientData
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? ConNum { get; set; }
        public string[] MedHistory { get; set; } = Array.Empty<string>();
        public int? AgePatient { get; set; }
        public string? Code { get; set; }
       
    }









    // -------------------------------------------
    // PATIENT'S DATABASE CLASS
    // -------------------------------------------
    public class PatientDataBase
    {
        private static string filePath = "patients.json";


        //----------
        //Load Patient
        //---------
        public static List<PatientData> LoadPatients() // Load all patients
        {
            // If the master data file doesn't exist, return an empty list.
            if (!File.Exists(filePath))
                return new List<PatientData>();

            string json = File.ReadAllText(filePath);
            // Deserialize the JSON string into the list of all PatientData records.
            return JsonSerializer.Deserialize<List<PatientData>>(json) ?? new List<PatientData>();
        }


        //----------
        //Save Patient's Details
        //---------
        public static void SavePatient(List<PatientData> patients) //Save to "patients.json"
        {
            // Serialize the updated list of all patients to a formatted JSON string.
            string json = JsonSerializer.Serialize(patients, new JsonSerializerOptions { WriteIndented = true });
            // Overwrite the master data file with the updated list.
            File.WriteAllText(filePath, json);
        }

        //----------
        //Generates the Patient's Code
        //---------
        public static string GenerateRandomCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            // Generates a random alphanumeric string of the specified length.
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        //----------
        //Create's Patient Own File
        //---------
        public static void CreatePatientFile(string code) //Create own Patient File
        {
            // Define the file path using the patient's unique code (e.g., pat_XXXXXX.json).
            string filePath = $"{code}.json";

            // Create the file only if it doesn’t exist
            if (!File.Exists(filePath))
            {
                // Creates an empty JSON file dedicated to storing this patient's appointments.
                File.WriteAllText(filePath, string.Empty);

            }
            else
            {
                Console.WriteLine($"{code}json already exists.");
            }
        }

        //----------
        //Patient's Unique Code
        //---------
        public static string GenerateUniquePatientCode()
        {
            // Note: This loads doctor data, implying it checks for uniqueness against existing doctor codes.
            List<DoctorData> doctors = DoctorDataBase.LoadDoctors();
            string newCode;
            bool codeExists;

            do
            {
                // Generate a new code prefixed with "pat_".
                newCode = "pat_" + GenerateRandomCode(6);
                // Checks if the generated code clashes with any existing doctor code.
                codeExists = doctors.Any(d => d.Code == newCode);
            }
            while (codeExists); // Repeat until the generated code is unique among doctor codes.

            return newCode;
        }
    }






    // -------------------------------------------
    // SECRETARY'S DATA CLASS
    // -------------------------------------------
    public class SecretaryData
    {
        public string? userName { get; set; }
        public string? password { get; set; }
        public string? doctorCode { get; set; }
        public string? doctorName { get; set; }
        public string? SecCode { get; set; }

        public string? DocNum { get; set; }

        public DateTime timeIn { get; set; }
        public DateTime timeOut { get; set; }
    }






    // -------------------------------------------
    // SECRETARY'S DATABASE CLASS
    // -------------------------------------------
    public class SecretaryDataBase
    {
        private static string filePath = "secretary.json";


        //----------
        //Load Secretaries
        //---------
        public static List<SecretaryData> LoadSecretary()
        {
            // If the master data file doesn't exist, return an empty list.
            if (!File.Exists(filePath))
                return new List<SecretaryData>();

            string json = File.ReadAllText(filePath);
            // Deserialize the JSON string into the list of all SecretaryData records.
            return JsonSerializer.Deserialize<List<SecretaryData>>(json) ?? new List<SecretaryData>();
        }


        //----------
        //Save Secretaries
        //---------
        public static void SaveSecretary(List<SecretaryData> secretary)
        {
            // Serialize the updated list of all secretaries to a formatted JSON string.
            string json = JsonSerializer.Serialize(secretary, new JsonSerializerOptions { WriteIndented = true });
            // Overwrite the master data file with the updated list.
            File.WriteAllText(filePath, json);
        }


        //----------
        //Generate Unique Code
        //---------
        public static string GenerateRandomCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            // Generates a random alphanumeric string of the specified length.
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        //----------
        //Delete Secretaries
        //---------
        public static void DeleteSecretary(string secCode) //Delete secretary account in the file
        {
            string file = "secretary.json";

            if (!File.Exists(file)) return;

            // Load all secretary data from the file.
            var list = JsonSerializer.Deserialize<List<SecretaryData>>(
                File.ReadAllText(file)
            ) ?? new List<SecretaryData>();

            // Remove the secretary whose SecCode matches the one provided.
            list.RemoveAll(s => s.SecCode == secCode);

            // Write the updated list back to the file.
            File.WriteAllText(
                file,
                JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true })
            );
        }


        //----------
        //Secretary's Unique Code
        //---------
        public static string GenerateUniqueSecCode()
        {
            List<SecretaryData> secretaries = SecretaryDataBase.LoadSecretary();
            string newCode;
            bool codeExists;

            do
            {
                // Generate a new code prefixed with "sec_".
                newCode = "sec_" + GenerateRandomCode(6);
                // Check if this generated code is already in use by another secretary.
                codeExists = secretaries.Any(d => d.SecCode == newCode);
            }
            while (codeExists); // Repeat until a unique code is found.

            return newCode;
        }
    }
}




