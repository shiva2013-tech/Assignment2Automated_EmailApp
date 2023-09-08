using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using CsvHelper.Configuration.Attributes;
using Hangfire;
using Hangfire.MemoryStorage;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Application Starts.");
        // Initialize Hangfire with MemoryStorage
        GlobalConfiguration.Configuration
            .UseMemoryStorage();

        // Schedule the email sending task
        RecurringJob.AddOrUpdate(() => CallScheduler("Birthday"), "0 2 * * *"); // Cron expression for 2:00 AM every day
        RecurringJob.AddOrUpdate(() => CallScheduler("ChristamsDay"), "0 4 24 12 *"); // Cron expression for 4:00 AM on December 24th every year


        // Start Hangfire background processing
        BackgroundJobServer server = new BackgroundJobServer();

        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();

        //CallScheduler(null);
        //// Schedule the email sending task to run every day at a specific time
        //var scheduledTime = new TimeSpan(16, 13, 0); // 8:00 AM
        //var currentTime = DateTime.Now.TimeOfDay;
        //var delay = scheduledTime > currentTime
        //    ? (int)(scheduledTime - currentTime).TotalMilliseconds
        //    : (int)(scheduledTime.Add(new TimeSpan(24, 0, 0)) - currentTime).TotalMilliseconds;

        //var timer = new Timer(CallScheduler, null, delay, TimeSpan.FromHours(24).Milliseconds); // Repeat every 24 hours




    }

    public static async Task CallScheduler(string events)
    {
        var recipients = GetRecipients(); // Load recipients from your data source

        switch (events)
        {
            case "Birthday":
                DateTime currentDate = DateTime.Now;
                List<Employee> employeeWithTodayDOB = recipients
                .Where(e =>
                    e.DoB.Day == currentDate.Day &&
                    e.DoB.Month == currentDate.Month)
                .ToList();
                recipients = employeeWithTodayDOB;
                break;

            case "ChristamsDay":
                break;

            default:
                // Code to execute if none of the cases match
                break;
        }


        var batchSize = 100; // Choose an appropriate batch size
        List<List<Employee>> batches = Batches(recipients, batchSize);

        var smtpClient = new SmtpClient("smtp-mail.outlook.com")
        {
            Credentials = new NetworkCredential("***************@outlook.com", "Password"),
            EnableSsl = true,
            Port = 587
        };

        foreach (var batch in batches)
        {
            await SendEmailAsync(smtpClient, batch, events);
        }
    }


    static List<Employee> GetRecipients()
    {
        string csvFilePath = @"..\Data\Employee.csv";
        var employees = new List<Employee>();

        using (var reader = new StreamReader(csvFilePath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            // Use a custom TypeConverter for the BirthDate property
            csv.Context.TypeConverterOptionsCache.AddOptions<Employee>(new TypeConverterOptions
            {
                Formats = new[] { "dd-MM-yyyy" }
            });

            employees = csv.GetRecords<Employee>().ToList();
        }
        // Return a list of Employee
        return employees;
    }

    static List<List<Employee>> Batches(List<Employee> recipients, int batchSize)
    {
        var batches = new List<List<Employee>>();

        for (int i = 0; i < recipients.Count; i += batchSize)
        {
            var batch = recipients
                .GetRange(i, Math.Min(batchSize, recipients.Count - i))
                .ToList();

            batches.Add(batch);
        }

        return batches;
    }

    static async Task SendEmailAsync(SmtpClient smtpClient, List<Employee> recipients, string events)
    {
        foreach (var recipient in recipients)
        {

            //var mailMessage = new MailMessage
            //{
            //    From = new MailAddress("your_email@example.com"),
            //    Subject = "Birthday Wishes",
            //    Body = "Happy Birthday!",
            //    IsBodyHtml = false
            //};
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("**********@outlook.com", "TestCompany");
            switch (events)
            {
                case "Birthday":


                    mailMessage.Subject = "Happy Birthday!";
                    mailMessage.Body = $"Dear {recipient.Name},\n\nHappy Birthday!\n\n" +
                                       $"On behalf of the entire [Company Name] team, we would like to wish you a very happy birthday! " +
                                       $"\n\nBirthdays are special occasions, and today, we want to take a moment to celebrate you. Your dedication, hard work, and positive attitude are not only appreciated but also inspire us all." +
                                       $"\n\nWe hope this day is filled with joy, laughter, and cherished moments with your loved ones. May it mark the beginning of a new year filled with even greater achievements and opportunities." +
                                       $"\n\nWarmest wishes," +
                                       $"\n\n[Company Name]\n\n[Company Contact Information].";// Replace some particular information according to you
                    break;

                case "ChristamsDay":
                    mailMessage.Subject = "Happy Christams Day!";
                    mailMessage.Body = $"Dear {recipient.Name},\n\nHappy Christams Day!\n\n" +
                                       $"Wishing you a joyful and blessed Christmas filled with love, peace, and happiness." +
                                       $"\n\nMay this festive season bring warmth and cheer to your heart and home." +
                                       $"\n\nWarmest wishes," +
                                       $"\n\n[Company Name]\n\n[Company Contact Information].";// Replace some particular information according to you
                    break;

                default:
                    // Code to execute if none of the cases match
                    break;
            }

            mailMessage.To.Add(recipient.Email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"Sent to: {recipient.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending to {recipient.Email}: {ex.Message}");

                // Log the error to a text file
                LogErrorToFile($"Error sending to {recipient.Email}: {ex.Message}");
            }
        }
    }

    static void LogErrorToFile(string errorMessage)
    {
        
        string logFilePath = @"..\Logs\error.log"; ; // Specify the path to your log file

        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {errorMessage}");
            }
        }
        catch (Exception)
        {
            // Handle any exceptions that may occur while writing to the log file
        }
    }
}
class Employee
{
    [Name("EmpId")]
    public int Id { get; set; }

    [Name("Name")]
    public string Name { get; set; }

    [Name("Email")]
    public string Email { get; set; }

    [Name("DoB")]
    [Format("dd-MM-yyyy")]
    public DateTime DoB { get; set; }
}
