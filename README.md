# Assignment2Automated_EmailApp
Automated Birthday Email Sender

This project is a .NET Core console application that sends automated birthday wishes emails to employees based on data from a CSV file. The application is configured to use your Outlook email for sending emails.

Prerequisites
Before you begin, make sure you have the following installed on your system:
.NET Core SDK

Follow these steps to clone and run the project:

1.Clone the repository to your local machine using the following command:
--git clone https://github.com/your-username/automated-birthday-email.git

2.Navigate to the project directory:
--cd Automated_Email_App

3.Set up your email credentials:
Open the program.cs file and replace the placeholders with your Outlook email and password.

4.Prepare the CSV file:
Place your CSV file containing employee data in the /bin/Debug/Data/Employee.csv directory of the project. Make sure the CSV file is named Employee.csv and follows the format as made in the csv file.

5.Build and run the application:
Run the following commands to build and run the application:
--dotnet build
--dotnet run

The application will read the CSV data and send birthday wishes emails to employees when their birthdays occur.

5.Schedule Email Sending
By default, the application sends email per day to the employee who has birthday on that day. If you want to send two emails on a specific date (e.g., December 24th), you can adjust the code in the Program.cs file.
