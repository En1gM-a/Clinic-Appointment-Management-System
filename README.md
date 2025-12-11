# FinalProject: Clinic Management System (CMS)

## 🌟 Project Overview

**FinalProject** is a robust, console-based application designed to manage the core operations of a medical clinic, including user authentication, appointment scheduling, and patient record management. The system is built using C# and features a rich, interactive command-line interface with custom text centering and colored output.

## 🔑 Key Features

The application supports three distinct user roles: **Doctor**, **Patient**, and **Secretary**, each with a unique set of functionalities.

### General
* **Interactive Console UI:** Features custom ASCII art, centered text, and arrow-key navigation menus (`ShowMenuWithUICenter`, `ShowMenuWithPagesCenter`).
* **Account System:** Supports secure Login and Sign-up for three different account types.
* **Data Persistence:** Uses JSON files (`doctors.json`, `patients.json`, `secretary.json`) for data storage and separate `[userCode].json` files for individual appointment records.

### 👩‍⚕️ Doctor Features
* **Account Prefix:** `dr_` (e.g., `dr_smith`).
* **Appointment Management:** View all booked appointments (`ViewAppointments`), and use the `TAB` key to `Manage` appointments (CONFIRM or CANCEL).
* **Diagnosis:** Use the `V` key to record a `Diagnosis` for a patient after a consultation.
* **Scheduling:** Doctors can set their working `Time In` and `Time Out` schedules.
* **Secretary Management:** Can add and remove secretaries assigned to their account.

### 🧑 Patient Features
* **Account Prefix:** `pat_` (e.g., `pat_jane`).
* **Appointment Booking:** View available doctors (searchable by specialization and name) and book an appointment with a chosen doctor.
* **View Records:** Check the status and details of booked appointments, including the final `Diagnosis` from the doctor.

### 📝 Secretary Features
* **Account Prefix:** `sec_` (set by the Doctor).
* **Appointment Management:** View the assigned doctor's appointments and `Manage` (CONFIRM or CANCEL) appointments on behalf of the doctor.
* **Patient Registration:** Can quickly register new patient accounts (`SignUp`).

## 💻 Technology Stack

* **Language:** C#
* **Platform:** .NET (Console Application)
* **Data Storage:** JSON Files (`System.Text.Json` for serialization/deserialization).

## ▶️ Getting Started

### Prerequisites
* .NET SDK installed on your system.

### Installation and Setup
1.  **Clone the Repository:**
    ```bash
    git clone https://github.com/En1gM-a/Clinic-Appointment-Management-System
    cd FinalProject
    ```
2.  **Compile the Code:**
    ```bash
    dotnet build
    ```

### Running the Application

1.  **Execute the Program:**
    ```bash
    dotnet run
    ```
2.  **Log In/Sign Up:**
    * Use the main menu to choose **Log In** or **Sign Up**.
    * **Doctor Sign Up:** Requires an authorization key (`doctor12345`) (currently hard-codedhardcoded :/, could be auto-generated in future updates)
    * **Log In:** Enter the full username, including the prefix (e.g., `dr_sampleDoc`, `pat_samplePat`).

## ✍️ Contribution & Contact

If you have any questions or suggestions, please feel free to let me know.
---
