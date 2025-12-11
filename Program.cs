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
    // ConsoleHelper CLASS
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

            // FIX: Use trimmed length so the visible text centers properly
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
            int selected = 0;

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

                if (key.Key == ConsoleKey.UpArrow)
                    selected = (selected - 1 + options.Count) % options.Count;
                else if (key.Key == ConsoleKey.DownArrow)
                    selected = (selected + 1) % options.Count;
                else if (key.Key == ConsoleKey.Enter)
                    return selected;
                else if (key.Key == ConsoleKey.Escape)
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

                if (key.Key == ConsoleKey.UpArrow)
                {
                    selected--;
                    if (selected < start) selected = end - 1;
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    selected++;
                    if (selected >= end) selected = start;
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    if (page < totalPages - 1)
                    {
                        page++;
                        selected = page * itemsPerPage;
                    }
                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    if (page > 0)
                    {
                        page--;
                        selected = page * itemsPerPage;
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    return selected;
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    return -1; // Back option
                }
                else if(key.Key == ConsoleKey.Q)
                {
                    return -10; //Search
                }
            }
        }
    }


    //-------------------------
    //MAIN
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

                        string prompt = "Enter username: ";
                        int padding = (Console.WindowWidth - prompt.Length - 20) / 2;
                        // subtract extra space for user input (~20 chars)
                        Console.SetCursorPosition(Math.Max(0, padding), Console.CursorTop);
                        Console.Write(prompt);
                        string user = Console.ReadLine()!;

                        // Center password prompt
                        string passwordPrompt = "Enter password: ";
                        int passwordPadding = (Console.WindowWidth - passwordPrompt.Length - 20) / 2;
                        Console.SetCursorPosition(Math.Max(0, padding), Console.CursorTop);
                        Console.Write(prompt); ;
                        string pass = Console.ReadLine()!;

                        if (user.StartsWith("dr_", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var doctors = DoctorDataBase.LoadDoctors();

                                var doctor = doctors.FirstOrDefault(d =>
                                    d.UserName?.Equals(user, StringComparison.OrdinalIgnoreCase) == true &&
                                    d.Password == pass);

                                if (doctor != null)
                                {
                                    
                                    helper.CenterText($"\n\nWelcome, {doctor.UserName}!", ConsoleColor.Green);
                                    Console.ReadKey(true);

                                    Doctor docAcc = new Doctor
                                    {
                                        User = doctor.UserName,
                                        Pass = doctor.Password,
                                        conNum = doctor.Contact,
                                        Hosp = doctor.Hospital ?? "",
                                        Spec = doctor.Specialization,
                                        Code = doctor.Code
                                    };

                                    try { docAcc.Menu(); } catch { }
                                }
                                else
                                {
                                    helper.CenterText("No Doctor account found or wrong password!", ConsoleColor.Red);
                                }
                            }
                            catch (Exception ex)
                            {
                                helper.CenterText($"ERROR loading doctor accounts:\n{ex.Message}", ConsoleColor.Red);
                            }
                        }
                
                        else if (user.StartsWith("pat_", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var patients = PatientDataBase.LoadPatients();

                                var patient = patients.FirstOrDefault(p =>
                                    p.UserName?.Equals(user, StringComparison.OrdinalIgnoreCase) == true &&
                                    p.Password == pass);

                                if (patient != null)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    helper.CenterText($"\n\nWelcome, {patient.UserName}!", ConsoleColor.Green);
                                    Console.ResetColor();
                                    Console.ReadKey(true);

                                    Patient patAcc = new Patient
                                    {
                                        User = patient.UserName,
                                        Pass = patient.Password,
                                        ConNum = patient.ConNum,
                                        medHis = string.Join("|", patient.MedHistory),
                                        Age = patient.AgePatient,
                                        Code = patient.Code
                                    };

                                    try { patAcc.Menu(); } catch { }
                                }
                                else
                                {
                                    helper.CenterText("No Patient account found or wrong password!", ConsoleColor.Red);
                                }
                            }
                            catch (Exception ex)
                            {
                                helper.CenterText($"ERROR loading patient accounts:\n{ex.Message}", ConsoleColor.Red);
                            }
                        }
                        else if (user.StartsWith("sec_", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var secretaryList = SecretaryDataBase.LoadSecretary();

                                var secretaryAcc = secretaryList.FirstOrDefault(s =>
                                    s.userName?.Equals(user, StringComparison.OrdinalIgnoreCase) == true &&
                                    s.password == pass);

                                if (secretaryAcc != null)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    helper.CenterText($"\n\nWelcome, {secretaryAcc.userName}!",ConsoleColor.Green);
                                    Console.ResetColor();
                                    Console.ReadKey(true);

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

                                    try { secAcc.Menu(); } catch { }
                                }
                                else
                                {
                                    helper.CenterText("No Secretary account found or wrong password!", ConsoleColor.Red);
                                }
                            }
                            catch (Exception ex)
                            {
                                helper.CenterText($"ERROR loading secretary accounts:\n{ex.Message}", ConsoleColor.Red);
                            }
                        }
                        else
                        {
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
                        int ch = helper.ShowMenuWithUICenter(SignUp);

                        switch (ch)
                        {
                            case -1:
                                break;

                            case 0:
                                doc.SignUp();
                                break;

                            case 1:
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
    // Main Account CLASS
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
    // For Doctor and Secretary to Appointments CLASS
    // -------------------------------------------
    public interface ViewPatientDetails
    {
        void ViewAppointments();
    }


    // -------------------------------------------
    // Doctor Account CLASS
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
        //MANAGE (CANCEL / CONFIRM
        //-------------------------
        private void Manage(ConsoleHelper helper, List<AppointmentData> appointments, List<AppointmentData> displayedAppointments, string filePath)
        {
            helper.CenterText("Manage Appointment", ConsoleColor.Yellow);
            helper.CenterText("───────────────────────────────────────────────────────────", ConsoleColor.Yellow);

            Console.Write("\nEnter appointment number to manage: ");
            if (int.TryParse(Console.ReadLine(), out int selected) && selected > 0 && selected <= displayedAppointments.Count)
            {
                var selectedApp = displayedAppointments[selected - 1];
                var originalApp = appointments.First(a => a.TimeStart == selectedApp.TimeStart && a.DoctorUser == selectedApp.DoctorUser); 

                Console.Write("\nEnter action [CONFIRM/CANCEL]: ");
                string action = Console.ReadLine()!.Trim().ToUpper();

                if (action == "CONFIRM")
                {
                    originalApp.StatusDoc = "CONFIRMED";
                    helper.CenterText("Appointment confirmed successfully!", ConsoleColor.Green);
                }
                else if (action == "CANCEL")
                {
                    Console.Write("Enter reason for cancellation: ");
                    string reason = Console.ReadLine()!.Trim();

                    if (string.IsNullOrEmpty(reason))
                    {
                        helper.CenterText("Cancellation reason cannot be empty!", ConsoleColor.Red);
                        Console.ReadKey(true);
                        return;
                    }
                    originalApp.StatusDoc = "CANCELLED";
                    originalApp.StatusSec = "CANCELLED";
                    originalApp.Diagnosis = $"CANCELLED: {reason} [DOCTOR]";
                    helper.CenterText("Appointment cancelled successfully!", ConsoleColor.Red);
                }
                else
                {
                    helper.CenterText("Invalid action!", ConsoleColor.Red);
                    Console.ReadKey(true);
                    return;
                }

                // Save to doctor's file
                File.WriteAllText(filePath, JsonSerializer.Serialize(appointments, new JsonSerializerOptions { WriteIndented = true }));

                // Save to patient's file
                string patientFile = $"{selectedApp.PatCode}.json";
                if (File.Exists(patientFile))
                {
                    string patData = File.ReadAllText(patientFile);
                    if (!string.IsNullOrWhiteSpace(patData))
                    {
                        var patAppointments = JsonSerializer.Deserialize<List<AppointmentData>>(patData);
                        var matching = patAppointments?.FirstOrDefault(x => x.TimeStart == selectedApp.TimeStart && x.DoctorUser == selectedApp.DoctorUser);
                        if (matching != null)
                        {
                            matching.StatusDoc = originalApp.StatusDoc;
                            matching.StatusSec = originalApp.StatusSec;
                            matching.Diagnosis = originalApp.Diagnosis;
                            File.WriteAllText(patientFile, JsonSerializer.Serialize(patAppointments, new JsonSerializerOptions { WriteIndented = true }));
                        }
                    }
                }

                helper.CenterText("Changes saved successfully!", ConsoleColor.Yellow);
                Console.ReadKey(true);
            }
            else
            {
                helper.CenterText("Invalid appointment number!", ConsoleColor.Red);
                Console.ReadKey(true);
            }
        }


        //-------------------------
        //DOCTOR GIVES DIAGNOSIS
        //-------------------------
        private void Diagnosis (ConsoleHelper helper, List<AppointmentData> appointments, List<AppointmentData> displayedAppointments, string filePath)
        {
            Console.Write("\nEnter appointment number to diagnose: ");
            if (int.TryParse(Console.ReadLine(), out int selected) && selected > 0 && selected <= displayedAppointments.Count)
            {
                var selectedApp = displayedAppointments[selected - 1];
                var originalApp = appointments.First(a => a.TimeStart == selectedApp.TimeStart && a.DoctorUser == selectedApp.DoctorUser);

                if (originalApp.StatusDoc == "CANCELLED" || originalApp.StatusSec == "CANCELLED")
                {
                    helper.CenterText("The PATIENT or YOU cancelled the appointment...\nUnable to give Diagnosis", ConsoleColor.Red);
                    Console.ReadKey(true);
                    return;
                }

                Console.Write("Enter Diagnosis: ");
                string diagnosis = Console.ReadLine()!;
                originalApp.Diagnosis = diagnosis;

                helper.CenterText("Patient Diagnosed Successfully!", ConsoleColor.DarkGreen);


                // Save updated appointments to doctor's file
                File.WriteAllText(filePath, JsonSerializer.Serialize(appointments, new JsonSerializerOptions { WriteIndented = true }));

                // Also update patient's file
                string patientFile = $"{selectedApp.PatCode}.json";
                if (File.Exists(patientFile))
                {
                    string patData = File.ReadAllText(patientFile);
                    if (!string.IsNullOrWhiteSpace(patData))
                    {
                        var patAppointments = JsonSerializer.Deserialize<List<AppointmentData>>(patData);
                        var matching = patAppointments?.FirstOrDefault(x => x.TimeStart == selectedApp.TimeStart && x.DoctorUser == selectedApp.DoctorUser);
                        if (matching != null)
                        {
                            matching.Diagnosis = selectedApp.Diagnosis;
                            File.WriteAllText(patientFile, JsonSerializer.Serialize(patAppointments, new JsonSerializerOptions { WriteIndented = true }));
                        }
                    }
                }

                helper.CenterText("Changes saved successfully!", ConsoleColor.Yellow);
            }
        }


        //-------------------------
        //SEARCH APPOINTMENTS
        //-------------------------
        private List<AppointmentData> SearchAppointments(ConsoleHelper helper, List<AppointmentData> allAppointments)
        {
            Console.Clear();
            helper.printClinicName();
            helper.CenterText("Search Appointments", ConsoleColor.Yellow);
            helper.CenterText("────────────────────────────────────────────", ConsoleColor.Yellow);

            Console.Write("\nEnter search term: ");
            string searchTerm = Console.ReadLine()!.Trim().ToUpper();

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
                return allAppointments;
            }

            return matches; // displayedAppointments will show filtered list
        }



        //-------------------------
        //FOR VIEWING PATIENT WHO BOOKED THE DOCTOR
        //-------------------------
        public virtual void ViewAppointments()
        {
            ConsoleHelper helper = new ConsoleHelper();
            string filePath = $"{Code}.json"; // Doctor's file path

            // Load appointments once
            List<AppointmentData> appointments = new List<AppointmentData>();
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(jsonData))
                    appointments = JsonSerializer.Deserialize<List<AppointmentData>>(jsonData) ?? new List<AppointmentData>();
            }
            else
            {
                Console.Clear();
                helper.printClinicName();
                helper.CenterText("File not found!", ConsoleColor.Red);
                Console.ReadKey(true);
                return;
            }

            if (appointments.Count == 0)
            {
                Console.Clear();
                helper.printClinicName();
                helper.CenterText("No Appointments Scheduled.", ConsoleColor.Yellow);
                Console.ReadKey(true);
                return;
            }

            // Main interactive loop
            bool exit = false;
            List<AppointmentData> displayedAppointments = appointments;
            while (!exit)
            {
                Console.Clear();
                helper.printClinicName();
                helper.CenterText($"{User} Appointments", ConsoleColor.Yellow);
                helper.CenterText("────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────", ConsoleColor.Yellow);

                
                // Display all appointments
                int count = 1;
                foreach (var a in displayedAppointments)
                {
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

                helper.CenterText("[Enter to Select] [TAB to Manage Appointment] " +
                    "[V to Give Diagnosis] [Q to Search] [ESC to Go Back]", ConsoleColor.Yellow);

                // Wait for key press
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Tab:  // Manage appointment
                        Manage(helper, appointments, displayedAppointments, filePath);
                        break;

                    case ConsoleKey.V:    // Give Diagnosis
                        Diagnosis(helper, appointments, displayedAppointments, filePath);
                        break;

                    case ConsoleKey.Q:    // Search appointments
                       
                        displayedAppointments = SearchAppointments(helper, appointments);
                        break;

                    case ConsoleKey.Escape:
                        exit = true;
                        break;
                }
            }
        }

        //-------------------------
        //REMOVE SECRETARY ACCOUNT
        //-------------------------
        private void SearchAndRemoveSecretary()
        {
            ConsoleHelper helper = new ConsoleHelper();

            // Load all secretaries
            var allSecs = SecretaryDataBase.LoadSecretary();

            // Secretaries belonging to this doctor only
            var mySecs = allSecs.Where(s => s.doctorCode == Code).ToList();

            if (mySecs.Count == 0)
            {
                helper.CenterText("You have no secretaries to remove.", ConsoleColor.Yellow);
                Console.ReadKey(true);
                return;
            }

            // Ask for search keyword
            Console.Clear();
            helper.printClinicName();
            helper.CenterText("Enter part of the secretary name to search:");
            helper.CenterText("(Example: 'james')");

            Console.Write("\nSearch: ");
            string keyword = Console.ReadLine()?.Trim().ToLower() ?? "";

            if (string.IsNullOrEmpty(keyword))
                return;

            // Filter by partial match
            var filtered = mySecs
                .Where(s => s.userName?.ToLower().Contains(keyword) == true)
                .ToList();

            if (filtered.Count == 0)
            {
                helper.CenterText("No secretary found with that name.", ConsoleColor.Yellow);
                Console.ReadKey(true);
                return;
            }

            // Build menu list
            List<string> options = filtered
                .Select(s => $"{s.userName} | Code: {s.SecCode}")
                .ToList();

            options.Add("Back");

            // Show selection menu
            int choice = helper.ShowMenuWithPagesCenter(options, "Select secretary to remove:");

            if (choice == -1 || choice == options.Count - 1)
                return; // Back or ESC

            var selected = filtered[choice];

            // Confirmation
            Console.Clear();
            helper.printClinicName();
            helper.CenterText("Are you sure you want to delete:", ConsoleColor.Yellow);
            helper.CenterText($"{selected.userName}", ConsoleColor.Red);
            helper.CenterText("[Y] Yes   [N] No", ConsoleColor.Yellow);

            var key = Console.ReadKey(true).Key;

            if (key != ConsoleKey.Y)
                return;

            // Perform deletion
            SecretaryDataBase.DeleteSecretary(selected.SecCode ?? "");

            helper.CenterText("Secretary removed successfully!", ConsoleColor.Green);
            Console.ReadKey(true);
        }

        //-------------------------
        //MAIN MENU FOR DOCTOR
        //-------------------------
        public override void Menu()
        {
            while (true)
            {

                string timeIn = "NOT SET";
                string timeOut = "NOT SET";

                string filePath = "doctors.json";

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    List<DoctorData> doctors = JsonSerializer.Deserialize<List<DoctorData>>(json) ?? new List<DoctorData>();

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

                string header =
                    $"Doctor : {User}\n" +
                    $"Specialization: {fieldSpec}\n" +
                    $"Hospital: {Hosp}\n" +
                    $"Contact Number: {conNum}\n" +
                    $"Time In: {timeIn}\n" +
                    $"Time Out: {timeOut}\n";

                


                int choice = helper.ShowMenuWithUICenter(DocMenu, header);

                Console.Clear();
                helper.printClinicName();
                

                switch (choice)
                {
                    //-------------------------
                    //VIEW APPOINTMENTS
                    //-------------------------
                    case 0: // Viewing of Patients who appointed
                        ViewAppointments();
                        Console.ReadKey(true);
                        break;

                    //-------------------------
                    //DOCTOR CHOOSE THEIR OWN S
                    //-------------------------
                    case 1: //Doctor choose their own sched

                        Console.Write("Enter start time (yyyy MM dd HH:mm): ");
                        string startInput = Console.ReadLine()!;

                        Console.Write("Enter end time (yyyy MM dd HH:mm): ");
                        string endInput = Console.ReadLine()!;

                        if (DateTime.TryParse(startInput, out DateTime start) &&
                            DateTime.TryParse(endInput, out DateTime end))
                        {
                            DateTime tomorrow = DateTime.Now.Date.AddDays(1);


                            if (start.Date != tomorrow || end.Date != tomorrow)
                            {
                                helper.CenterText("You can only schedule appointments for tomorrow!!", ConsoleColor.Red);
                            }
                            else if (end <= start)
                            {
                                
                                helper.CenterText("End time must be after start time.\n", ConsoleColor.Red);
                                
                            }
                            else
                            {
                                sched.StartSched = start;
                                sched.EndSched = end;
                                hasSched = true;

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
                            helper.CenterText("Invalid format! Please use yyyy MM dd HH:mm format!!", ConsoleColor.Yellow);
                        }

                        Console.ReadKey(true);
                        break;

                    case 2: //Doctor add their secretary
                        
                        Console.Clear();
                        helper.printClinicName();
                        Console.WriteLine();
                        helper.CenterText("==== Secretary Registration ====", ConsoleColor.Yellow);
                        Console.WriteLine();

                        var secretary = SecretaryDataBase.LoadSecretary();

                        Console.Write("Enter username: ");
                        string username = Console.ReadLine()!;

                        username = $"sec_{username}";

                        bool usernameExist = secretary.Any(s => s.userName?.Equals(username, StringComparison.OrdinalIgnoreCase) == true);

                        if(usernameExist)
                        {
                            helper.CenterText($"{username} already exists!! Please choose another one!!", ConsoleColor.Red);
                            helper.CenterText("\nPress any key to return...");
                            Console.ReadKey(true);
                            return;
                        }

                        Console.Write("Enter password: ");
                        string password = Console.ReadLine() ?? "";

                        Console.Write("Confirm password: ");
                        string confirm = Console.ReadLine() ?? "";

                        if (confirm == password)
                        {
                            
                            string file = "doctors.json";
                            DateTime docStart = DateTime.MinValue;
                            DateTime docEnd = DateTime.MinValue;

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

                            SecretaryData newSec = new SecretaryData
                            {
                                userName = username,
                                password = password,
                                doctorCode = Code ?? "",
                                doctorName = User ?? "",
                                DocNum = conNum ?? "",
                                SecCode = SecretaryDataBase.GenerateUniqueSecCode(),
                                timeIn = docStart,   
                                timeOut = docEnd
                            };

                            secretary.Add(newSec);

                            SecretaryDataBase.SaveSecretary(secretary);

                            Console.Clear();
                            helper.printClinicName();

                            Console.WriteLine("\n");
                            helper.CenterText($"Successfully Registered {username}!!", ConsoleColor.DarkGreen);
                            Console.WriteLine("\n");

                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Password doesn't match!! Enter again");
                            Console.ResetColor();
                            return;
                        }

                        Console.ReadKey(true);
                        return;
                    case 3: //Search and Renmove Secretary
                        SearchAndRemoveSecretary();
                        Console.ReadKey(true);
                        break;
                    case 4: //Log Out
                        helper.CenterText("\nLogging out...", ConsoleColor.DarkGreen);
                        Console.ReadKey(true);
                        return;
                }
            }
        }

        public override void SignUp()
        {
            Console.Clear();
            helper.printClinicName();
            Console.WriteLine();
            helper.CenterText("==== Doctor Registration ====", ConsoleColor.Yellow);
            Console.WriteLine();
            Console.Write("Enter authorization key: ");
            string key = Console.ReadLine() ?? "";
            if (key == authKey)
            {

                var doctors = DoctorDataBase.LoadDoctors();

                try
                {
                    Console.Write("Enter username: ");
                    string username = Console.ReadLine() ?? "";

                    if (string.IsNullOrWhiteSpace(username))
                    {
                        helper.CenterText("Password cannot be empty!", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        return;
                    }

                    username = $"dr_{username}";
                    bool usernameExist = doctors.Any(d => d.UserName?.Equals(username, StringComparison.OrdinalIgnoreCase) == true);

                    if (usernameExist)
                    {
                        helper.CenterText($"{username} already exists!! Please choose another one!!", ConsoleColor.Red);
                        helper.CenterText("\nPress any key to return...");
                        Console.ReadKey(true);
                        return;
                    }

                    UserName = username;

                    Console.Write("Enter password: ");
                    Password = Console.ReadLine() ?? "";

                    if (string.IsNullOrWhiteSpace(Password))
                    {
                        helper.CenterText("Password cannot be empty!", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        return;
                    }

                    Console.Write("Confirm password: ");
                    string confirm = Console.ReadLine() ?? "";

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

                        Console.Clear();
                        helper.printClinicName();
                        string head = "Choose Specialization";
                        int specilization = helper.ShowMenuWithPagesCenter(DoctorSpecializations, head);
                        string spec = DoctorSpecializations[specilization];

                        DoctorData newDoc = new DoctorData
                        {
                            UserName = UserName,
                            Password = Password,
                            Contact = Contact,
                            Hospital = Hosp,
                            Specialization = spec,
                            Code = DoctorDataBase.GenerateUniqueDoctorCode()
                        };


                        doctors.Add(newDoc);
                        DoctorDataBase.SaveDoctors(doctors);

                        DoctorDataBase.CreateDoctorFile(newDoc.Code);


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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Password doesn't match!! Enter again");
                        Console.ResetColor();
                        return;
                    }
                }

                catch(Exception ex)
                {
                    Console.Clear();
                    helper.printClinicName();
                    helper.CenterText("An unexpected error occurred!", ConsoleColor.Red);
                    helper.CenterText(ex.Message, ConsoleColor.DarkRed);
                    Console.ReadKey(true);
                }
                


            }
            else
            {
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

        public bool IsTimeTaken(List<AppointmentData> appointments, DateTime start, DateTime end)
        {
            foreach (var app in appointments)
            {
                // Check if time overlaps (collision detection)
                bool overlap =
                    (start < app.TimeEnd && end > app.TimeStart);

                if (overlap)
                    return true;
            }

            return false;
        }

        private List<AppointmentData> Search(ConsoleHelper helper, List<AppointmentData> allAppointments)
        {
            Console.Clear();
            helper.printClinicName();
            helper.CenterText("Search Appointments", ConsoleColor.Yellow);
            helper.CenterText("────────────────────────────────────────────", ConsoleColor.Yellow);

            Console.Write("\nEnter search term: ");
            string searchTerm = Console.ReadLine()!.Trim().ToUpper();

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
                return allAppointments;
            }

            return matches; 
        }
       
        public override void Menu() // PATIENT MENU
        {
            while (true)
            {

                string header =
                    $"Patient : {User}\n" +
                    $"Age: {age}\n" +
                    $"Contact Number: {ConNum}\n" +
                    $"Patiend Code: {Code}\n" +
                    $"Medical History: {medicalHistory}\n";

                int choice = helper.ShowMenuWithUICenter(PatientMenu, header); //UI

                Console.Clear();
                helper.printClinicName();
                

                switch (choice)
                {

                   
                    case 0: // Viewing of Doctor patient appointed
                        Console.WriteLine("\n");
                        helper.CenterText("---- Booked Appointment ----", ConsoleColor.Yellow);
                        Console.WriteLine("\n");

                        string filePath = $"{Code}.json"; // your file path

                        if (File.Exists(filePath))
                        {
                            string jsonData = File.ReadAllText(filePath);

                            if (string.IsNullOrWhiteSpace(jsonData))
                            {
                                helper.CenterText("No Appointments Scheduled.", ConsoleColor.Yellow);
                                Console.ReadKey(true);
                                break;
                            }

                            List<AppointmentData> appointments = JsonSerializer.Deserialize<List<AppointmentData>>(jsonData) ?? new List<AppointmentData>();
                            int count = 1;
                            // Display all info
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
                            Console.WriteLine("File not found!");
                        }

                        Console.ReadKey(true);
                        

                        break;

                    case 1: // Choose doctor

                        List<DoctorData> doctors = DoctorDataBase.LoadDoctors();
                        List<DoctorData> availableDoctors = new List<DoctorData>();
                        DoctorData chosenDoctor;
                        // Build display list
                        List<string> doctorOptions = new List<string>();

                        foreach (var doctor in doctors) // Write the details of all doctors in the List
                        {
                            if (!string.IsNullOrEmpty(doctor.timeIn) && !string.IsNullOrEmpty(doctor.timeOut))
                            {
                                availableDoctors.Add(doctor);
                                doctorOptions.Add($"{doctor.UserName} | {doctor.Hospital} | {doctor.Specialization}" +
                                    $" | Time: {doctor.timeIn} - {doctor.timeOut}");
                                
                            }
                        }

                        if (availableDoctors.Count == 0)
                        {
                            helper.CenterText("No doctors available. Check again later.", ConsoleColor.Yellow);
                            Console.ReadKey(true);
                            break;
                        }

                        
                        string doctorHeader =
                            "Available Doctors\n";

                        
                        int selectedIndex = helper.ShowMenuWithPagesCenter(doctorOptions, doctorHeader);

                        List<DoctorData> searchedOptions = new List<DoctorData>();

                        if (selectedIndex == -1)
                        {
                            break;
                        } else if (selectedIndex == -10)
                        {
                            Console.Clear();
                            helper.printClinicName();
                            Console.Write("Search doctors (name, hospital, specialization, time): ");
                            string query = Console.ReadLine()?.ToLower() ?? "";

                            // Filter available doctors
                            searchedOptions = availableDoctors
                                .Where(d =>
                                    (d.UserName?.ToLower().Contains(query) ?? false) ||
                                    (d.Hospital?.ToLower().Contains(query) ?? false) ||
                                    (d.Specialization?.ToLower().Contains(query) ?? false) ||
                                    (d.timeIn?.ToLower().Contains(query) ?? false) ||
                                    (d.timeOut?.ToLower().Contains(query) ?? false))
                                .ToList();

                            if (searchedOptions.Count == 0)
                            {
                                helper.CenterText("No doctors matched your search.", ConsoleColor.Red);
                                Console.ReadKey(true);
                                searchedOptions = new List<DoctorData>(availableDoctors); // reset list
                            }

                            // Convert filtered list to strings for display
                            List<string> searchedStrings = searchedOptions
                                .Select(d => $"{d.UserName} | {d.Hospital} | {d.Specialization} | Time: {d.timeIn} - {d.timeOut}")
                                .ToList();

                            // Show menu with searched options
                            int searchIndex = helper.ShowMenuWithPagesCenter(searchedStrings, doctorHeader);

                         
                            if (searchIndex == -1) continue; // ESC pressed

                            chosenDoctor = searchedOptions[searchIndex];

                            // Proceed with appointment booking for chosenDoctor
                        }
                        else
                        {
                                chosenDoctor = availableDoctors[selectedIndex];
                        }

                            

                        Console.Clear();
                        helper.printClinicName();
                        Console.WriteLine($"\nYou selected: {chosenDoctor.UserName} ({chosenDoctor.Specialization}) ({chosenDoctor.timeIn} - {chosenDoctor.timeOut})\n");

                       
                        while(true)
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

                        Console.Write("Enter start time (yyyy MM dd HH:mm): ");
                        if (!DateTime.TryParse(Console.ReadLine(), out DateTime start))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Invalid start time format!");
                            Console.ResetColor();
                            Console.ReadKey(true);
                            break;
                        }

                        Console.Write("Enter end time (yyyy MM dd HH:mm): ");
                        if (!DateTime.TryParse(Console.ReadLine(), out DateTime end))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Invalid end time format!");
                            Console.ResetColor();
                            Console.ReadKey(true);
                            break;
                        }

                        string startString = start.ToString();
                        string endString = end.ToString();

                        DateTime dutyStart = DateTime.Parse(chosenDoctor.timeIn ?? "");
                        DateTime dutyEnd = DateTime.Parse(chosenDoctor.timeOut ?? "");

                        if (start < dutyStart)
                        {
                            helper.CenterText($"Doctor starts at {dutyStart:yyyy MMMM dd HH:mm}", ConsoleColor.Red);
                            Console.ReadKey(true);
                            break;
                        }

                        
                        if (end > dutyEnd)
                        {
                            helper.CenterText($"Doctor is only available until {dutyEnd:yyyy MMMM dd HH:mm}", ConsoleColor.Red);
                            Console.ReadKey(true);
                            break;
                        }

                        // 3. End must be after start
                        if (end <= start)
                        {
                            helper.CenterText("End time must be after start time!", ConsoleColor.Red);
                            Console.ReadKey(true);
                            break;
                        }

                        string doctorFile = $"{chosenDoctor.Code}.json";
                        List<AppointmentData> doctorAppointments = new List<AppointmentData>();

                        if (File.Exists(doctorFile))
                        {
                            string docJson = File.ReadAllText(doctorFile);

                            if (!string.IsNullOrWhiteSpace(docJson))
                            {
                                doctorAppointments = JsonSerializer.Deserialize<List<AppointmentData>>(docJson)
                                                     ?? new List<AppointmentData>();
                            }
                        }

                        if (IsTimeTaken(doctorAppointments, start, end))
                        {
                            helper.CenterText("Time already taken. Please choose another timeslot", ConsoleColor.Red);
                            Console.ReadKey(true);
                            break;
                        }

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

                        doctorAppointments.Add(record);

                        File.WriteAllText(
                            doctorFile,
                            JsonSerializer.Serialize(doctorAppointments, new JsonSerializerOptions { WriteIndented = true })
                        );


                        // === SAVE TO PATIENT FILE ===
                        string patientFile = $"{Code}.json";
                        List<AppointmentData> patientAppointments = new List<AppointmentData>();

                        if (File.Exists(patientFile))
                        {
                            string patJson = File.ReadAllText(patientFile);

                            // Only deserialize if file is not empty or whitespace
                            if (!string.IsNullOrWhiteSpace(patJson))
                            {
                                patientAppointments = JsonSerializer.Deserialize<List<AppointmentData>>(patJson)
                                                     ?? new List<AppointmentData>();
                            }
                        }

                        // Add new appointment
                        patientAppointments.Add(record);

                        // Save back to file
                        File.WriteAllText(
                            patientFile,
                            JsonSerializer.Serialize(patientAppointments, new JsonSerializerOptions { WriteIndented = true })
                        );


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
                        return;
                }
            }
        }

        public override void SignUp()
        {
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

                username = $"pat_{username}";

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
                if (confirm == Password)
                {
                    Console.Write("Enter age: ");
                    AgePatient = int.Parse(Console.ReadLine() ?? "");

                    if (AgePatient == null)
                    {
                        helper.CenterText("Age cannot be empty!", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        return;
                    }

                    if (AgePatient <= 0)
                    {
                        helper.CenterText("Invalid age! Enter a positive number.", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        return;
                    }

                    Console.Write("Enter Medical History (Separate with spaces): ");
                    MedHistory = Console.ReadLine()!.Split(' ');

                    if (MedHistory == null)
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

                    PatientData newPat = new PatientData
                    {
                        UserName = UserName,
                        Password = Password,
                        ConNum = ConNum,
                        MedHistory = MedHistory,
                        AgePatient = AgePatient,
                        Code = PatientDataBase.GenerateUniquePatientCode()
                    };

                    User = UserName;
                    MedHis = MedHistory;
                    AgePat = AgePatient;
                    Code = newPat.Code;


                    this.Code = newPat.Code;

                    patients.Add(newPat);
                    PatientDataBase.SavePatient(patients);
                    PatientDataBase.CreatePatientFile(newPat.Code);

                    Console.Clear();
                    helper.printClinicName();

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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Password do not match!!Enter again!!");
                    Console.ResetColor();
                    Console.ReadKey(true);
                    return;
                }
            }
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
        List<string> SecMenu = new List<string> { "Check doctor's Appointment", "Register Patient", "Log Out" };
        public override void Menu()
        {
            while (true) // Keep showing menu until Log Out
            {
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
        private void Manage(ConsoleHelper helper, List<AppointmentData> appointments, List<AppointmentData> displayedAppointments, string filePath)
        {
            helper.CenterText("Manage Appointment", ConsoleColor.Yellow);
            helper.CenterText("───────────────────────────────────────────────────────────", ConsoleColor.Yellow);

            Console.Write("\nEnter appointment number to manage: ");
            if (int.TryParse(Console.ReadLine(), out int selected) && selected > 0 && selected <= displayedAppointments.Count)
            {
                var selectedApp = displayedAppointments[selected - 1];
                var originalApp = appointments.First(a => a.TimeStart == selectedApp.TimeStart && a.DoctorUser == selectedApp.DoctorUser); // <-- FIXED

                Console.Write("\nEnter action [CONFIRM/CANCEL]: ");
                string action = Console.ReadLine()!.Trim().ToUpper();

                if (action == "CONFIRM")
                {
                    originalApp.StatusSec = "CONFIRMED";
                    helper.CenterText("Appointment confirmed successfully!", ConsoleColor.Green);
                }
                else if (action == "CANCEL")
                {
                    Console.Write("Enter reason for cancellation: ");
                    string reason = Console.ReadLine()!.Trim();
                    originalApp.StatusSec = "CANCELLED";
                    originalApp.StatusDoc = "CANCELLED";
                    originalApp.Diagnosis = $"CANCELLED: {reason}";
                    helper.CenterText("Appointment cancelled successfully!", ConsoleColor.Red);
                }
                else
                {
                    helper.CenterText("Invalid action!", ConsoleColor.Red);
                    Console.ReadKey(true);
                    return;
                }

                // Save updated appointments to doctor's file
                File.WriteAllText(filePath, JsonSerializer.Serialize(appointments, new JsonSerializerOptions { WriteIndented = true }));

                // Existing patient file logic stays as it was
                string patientFile = $"{selectedApp.PatCode}.json";
                if (File.Exists(patientFile))
                {
                    string patData = File.ReadAllText(patientFile);
                    if (!string.IsNullOrWhiteSpace(patData))
                    {
                        var patAppointments = JsonSerializer.Deserialize<List<AppointmentData>>(patData);
                        var matching = patAppointments?.FirstOrDefault(x => x.TimeStart == selectedApp.TimeStart && x.DoctorUser == selectedApp.DoctorUser);
                        if (matching != null)
                        {
                            matching.StatusSec = originalApp.StatusSec;
                            matching.StatusDoc = originalApp.StatusDoc;
                            matching.Diagnosis = originalApp.Diagnosis;
                            File.WriteAllText(patientFile, JsonSerializer.Serialize(patAppointments, new JsonSerializerOptions { WriteIndented = true }));
                        }
                    }
                }

                helper.CenterText("Changes saved successfully!", ConsoleColor.Yellow);
                Console.ReadKey(true);
            }
            else
            {
                helper.CenterText("Invalid appointment number!", ConsoleColor.Red);
                Console.ReadKey(true);
            }
        }

        

        private List<AppointmentData> SearchAppointments(ConsoleHelper helper, List<AppointmentData> allAppointments)
        {
            Console.Clear();
            helper.printClinicName();
            helper.CenterText("Search Appointments", ConsoleColor.Yellow);
            helper.CenterText("────────────────────────────────────────────", ConsoleColor.Yellow);

            Console.Write("\nEnter search term: ");
            string searchTerm = Console.ReadLine()!.Trim().ToUpper();

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
                return allAppointments;
            }

            return matches; // displayedAppointments will show filtered list
        }
        public virtual void ViewAppointments()
        {
            Console.Clear();
            ConsoleHelper helper = new ConsoleHelper();
            helper.printClinicName();

            helper.CenterText($"{Docname} Appointments", ConsoleColor.Yellow);
            helper.CenterText("────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────", ConsoleColor.Yellow);

            string filePath = $"{Doccode}.json"; // Doctor's file path

            List<AppointmentData> appointments = new List<AppointmentData>();
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(jsonData))
                    appointments = JsonSerializer.Deserialize<List<AppointmentData>>(jsonData) ?? new List<AppointmentData>();
            }
            else
            {
                Console.Clear();
                helper.printClinicName();
                helper.CenterText("File not found!", ConsoleColor.Red);
                Console.ReadKey(true);
                return;
            }

            if (appointments.Count == 0)
            {
                Console.Clear();
                helper.printClinicName();
                helper.CenterText("No Appointments Scheduled.", ConsoleColor.Yellow);
                Console.ReadKey(true);
                return;
            }

            // Main interactive loop
            bool exit = false;
            List<AppointmentData> displayedAppointments = appointments;
            while (!exit)
            {
                Console.Clear();
                helper.printClinicName();
                helper.CenterText($"{User} Appointments", ConsoleColor.Yellow);
                helper.CenterText("────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────", ConsoleColor.Yellow);

                // Display all appointments
                int count = 1;
                foreach (var a in displayedAppointments)
                {
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

                // Wait for key press
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Tab:  // Manage appointment
                        Manage(helper, appointments, displayedAppointments, filePath);
                        break;

                    case ConsoleKey.Q:    // Search appointments

                        displayedAppointments = SearchAppointments(helper, appointments);
                        break;

                    case ConsoleKey.Escape:
                        exit = true;
                        break;
                }
            }
        }

        public override void SignUp()
        {
            Patient forPat = new Patient();

            
            forPat.SignUp();

            
            if (forPat.MedHistory == null || forPat.MedHistory.Length == 0)
            {
                forPat.MedHistory = new string[] { "None" }; // or leave as empty array
                return;
            }

            forPat.Age = forPat.AgePatient;
            forPat.medHis = string.Join(" | ", forPat.MedHistory);
            forPat.User = forPat.UserName;


            forPat.Menu();
        }

        
    }

    // -------------------------------------------
    // For Schedule CLASS
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
    // For Appointing Appointment CLASS
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

    public class DoctorDataBase
    {
        

        private static string filePath = "doctors.json";

        public static List<DoctorData> LoadDoctors()
        {
            if (!File.Exists(filePath))
                return new List<DoctorData>();

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<DoctorData>>(json) ?? new List<DoctorData>();
        }

        public static void SaveDoctors(List<DoctorData> doctors)
        {
            string json = JsonSerializer.Serialize(doctors, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public static void CreateDoctorFile(string code)
        {
            string filePath = $"{code}.json";

            // Create the file only if it doesn’t exist
            if (!File.Exists(filePath))
            {
                // Create an empty JSON file
                File.WriteAllText(filePath, string.Empty);

            }
            else
            {
                Console.WriteLine($"{code}json already exists.");
            }
        }

        public static string GenerateRandomCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateUniqueDoctorCode()
        {
            List<DoctorData> doctors = DoctorDataBase.LoadDoctors();
            string newCode;
            bool codeExists;

            do
            {
                newCode = "dr_" + GenerateRandomCode(6); // e.g. DR-8QJ29A
                codeExists = doctors.Any(d => d.Code == newCode);
            }
            while (codeExists);

            return newCode;
        }

        public static void UpdateDoctorTime(string username, string timeIn, string timeOut) //Update the file to add timein timeout
        {

            ConsoleHelper helper = new ConsoleHelper();

            string filePath = "doctors.json"; // correct filename

            if (!File.Exists(filePath))
            {
                Console.WriteLine("doctors.json not found!");
                return;
            }

            // Load the data file
            string json = File.ReadAllText(filePath);
            List<DoctorData> doctors = JsonSerializer.Deserialize<List<DoctorData>>(json) ?? new List<DoctorData>();

            // Find the matching doctor
            var doctor = doctors.FirstOrDefault(d =>
                d.UserName?.Equals(username, StringComparison.OrdinalIgnoreCase) == true);

            if (doctor != null)
            {
                // Update time fields
                doctor.timeIn = timeIn;
                doctor.timeOut = timeOut;

                // Save updated list
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

   

    public class PatientData
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? ConNum { get; set; }
        public string[] MedHistory { get; set; } = Array.Empty<string>();
        public int? AgePatient { get; set; }
        public string? Code { get; set; }
       
    }

    public class PatientDataBase
    {
        private static string filePath = "patients.json";

        public static List<PatientData> LoadPatients() // Load all patients
        {
            if (!File.Exists(filePath))
                return new List<PatientData>();

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<PatientData>>(json) ?? new List<PatientData>();
        }

        public static void SavePatient(List<PatientData> patients) //Save to "patients.json"
        {
            string json = JsonSerializer.Serialize(patients, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public static string GenerateRandomCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void CreatePatientFile(string code) //Create own Patient File
        {
            string filePath = $"{code}.json";

            // Create the file only if it doesn’t exist
            if (!File.Exists(filePath))
            {
                // Create an empty JSON file
                File.WriteAllText(filePath, string.Empty);
                
            }
            else
            {
                Console.WriteLine($"{code}json already exists.");
            }
        }

        public static string GenerateUniquePatientCode()
        {
            List<DoctorData> doctors = DoctorDataBase.LoadDoctors();
            string newCode;
            bool codeExists;

            do
            {
                newCode = "pat_" + GenerateRandomCode(6); // e.g. DR-8QJ29A
                codeExists = doctors.Any(d => d.Code == newCode);
            }
            while (codeExists);

            return newCode;
        }
    }

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

    public class SecretaryDataBase
    {
        private static string filePath = "secretary.json";

        public static List<SecretaryData> LoadSecretary()
        {
            if (!File.Exists(filePath))
                return new List<SecretaryData>();

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<SecretaryData>>(json) ?? new List<SecretaryData>();
        }

        public static void SaveSecretary(List<SecretaryData> secretary)
        {
            string json = JsonSerializer.Serialize(secretary, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public static string GenerateRandomCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void DeleteSecretary(string secCode) //Delete secretary account in the file
        {
            string file = "secretary.json";

            if (!File.Exists(file)) return;

            var list = JsonSerializer.Deserialize<List<SecretaryData>>(
                File.ReadAllText(file)
            ) ?? new List<SecretaryData>();

            list.RemoveAll(s => s.SecCode == secCode); // Remove the secretary whose SecCode matches the one provided.

            File.WriteAllText( // Write the updated list back to the file.
                file,
                JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true })
            );
        }

        public static string GenerateUniqueSecCode()
        {
            List<SecretaryData> doctors = SecretaryDataBase.LoadSecretary();
            string newCode;
            bool codeExists;

            do
            {
                newCode = "sec_" + GenerateRandomCode(6); // e.g. DR-8QJ29A
                codeExists = doctors.Any(d => d.SecCode == newCode);
            }
            while (codeExists);

            return newCode;
        }
    }
}




