using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using Dna.Ecommerce.LiveIntegration.Addin;
using Dynamicweb.Core.Helpers;
using Dynamicweb.Mailing;
using Dynamicweb.Rendering;
using Constants = Dna.Ecommerce.LiveIntegration.Configuration.Constants;

namespace Dna.Ecommerce.LiveIntegration.Logging
{
  public class Logger
  {
    private static readonly object SyncLock = new object();
    private static readonly CultureInfo CultureInfo = new CultureInfo("en-US");
    private readonly string _logFile;

    private Logger()
    {
      if (HttpContext.Current != null)
      {
        _logFile = HttpContext.Current.Server.MapPath("/Files/System/Log/LiveIntegration/" + Constants.AddInName + ".log");
      }
    }

    public static Logger Instance { get; } = new Logger();

    private bool LogConnectionErrors => Settings.Instance.LogConnectionErrors;

    private bool LogResponseErrors => Settings.Instance.LogResponseErrors;

    private bool LogGeneralErrors => Settings.Instance.LogGeneralErrors;

    private bool LogDebugInfo => Settings.Instance.LogDebugInfo;

    public void Log(ErrorLevel errorLevel, string logLine)
    {
      if (!string.IsNullOrEmpty(_logFile))
      {
        bool isAllowedAddToLog = IsAllowedAddToLog(errorLevel);
        lock (SyncLock)
        {
          if (isAllowedAddToLog)
          {
            MoveToHistoryFile();
            logLine = string.Format("{0}: {1}: {2}{3}", DateTime.Now.ToString(CultureInfo), errorLevel, logLine, Environment.NewLine);
            try
            {
              TextFileHelper.WriteTextFile(logLine, _logFile, true, System.Text.Encoding.UTF8);
            }
            catch (IOException)
            {
              Thread.Sleep(500);
              TextFileHelper.WriteTextFile(logLine, _logFile, true, System.Text.Encoding.UTF8);
            }
            catch
            {
              var fileName = HttpContext.Current.Server.MapPath(string.Format("/Files/System/Log/LiveIntegration/{0}.log", Guid.NewGuid()));
              TextFileHelper.WriteTextFile(logLine, fileName, false, System.Text.Encoding.UTF8);
            }
          }
        }
        if (errorLevel != ErrorLevel.DebugInfo && errorLevel != ErrorLevel.EmailSend)
        {
          SendLog(logLine, isAllowedAddToLog);
        }
      }
    }

    private void MoveToHistoryFile()
    {
      var fi = new FileInfo(_logFile);
      try
      {
        int maxSize = Settings.Instance.LogMaxSize;
        if (maxSize > 100)
        {
          maxSize = 100;
        }
        if (!fi.Exists || fi.Length < maxSize * 1024 * 1024)
        {
          return;
        }
        var fileName = Path.GetFileNameWithoutExtension(_logFile);
        var newFileName = string.Format("{0}-{1:yyyyMMddHHmmss}{2}", fileName, DateTime.Now, fi.Extension);
        var newLocation = Path.Combine(fi.DirectoryName, newFileName);
        File.Move(_logFile, newLocation);
      }
      catch
      {
        var logLine = string.Format("{0}: {1}: Cannot truncate log file.{2}", DateTime.Now.ToString(CultureInfo), ErrorLevel.Error, Environment.NewLine);
        TextFileHelper.WriteTextFile(logLine, _logFile, true, System.Text.Encoding.UTF8);
      }
    }

    private void SendLog(string lastError, bool isLastErrorInLog)
    {
      string frequencySettings = Settings.Instance.NotificationSendingFrequency;
      if (string.IsNullOrEmpty(frequencySettings))
      {
        return;
      }
      var frequency = Helpers.GetEnumValueFromString(frequencySettings, NotificationFrequency.Never);
      if (frequency == NotificationFrequency.Never)
      {
        return;
      }
      // Get last time when the email was sent
      DateTime lastTimeSend = Settings.LastNotificationEmailSent;
      bool emailSent = false;
      if (lastTimeSend == DateTime.MinValue)
      {
        emailSent = SendMail(lastError);
      }
      else
      {
        //send email if the frequency interval already passed
        if (DateTime.Now.Subtract(lastTimeSend) >= TimeSpan.FromMinutes((double)frequency))
        {
          if (!isLastErrorInLog)
          {
            emailSent = SendMail(GetLastLogData() + lastError);
          }
          else
          {
            emailSent = SendMail(GetLastLogData());
          }
        }
      }
      if (emailSent)
      {
        Log(ErrorLevel.EmailSend, "Send e-mail with errors");//used for getting the last errors appeared for the future email
        Settings.LastNotificationEmailSent = DateTime.Now;
      }
    }

    /// <summary>
    /// Gets appeared errors to send from the last email send
    /// </summary>
    /// <returns></returns>
    public string GetLastLogData()
    {
      string result = "";

      lock (SyncLock)
      {
        foreach (var line in File.ReadLines(_logFile, System.Text.Encoding.UTF8).Reverse())
        {
          if (line.Contains(ErrorLevel.DebugInfo.ToString()))
          {
            continue;
          }
          else if (!line.Contains(ErrorLevel.EmailSend.ToString()))
          {
            result += line + "<br>";
          }
          else
          {
            break;
          }
        }
      }
      return result;
    }

    /// <summary>
    /// Send mail according to configuration
    /// </summary>
    /// <param name="message">error/success message</param>
    public static bool SendMail(string message)
    {
      bool result = false;
      string notificationTemplate = Settings.Instance.NotificationTemplate;
      string notificationEmail = Settings.Instance.NotificationEmail;

      if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(notificationTemplate) && StringHelper.IsValidEmailAddress(notificationEmail))
      {
        Template templateInstance = new Template(notificationTemplate);
        templateInstance.SetTag("Ecom:LiveIntegration.AddInName", Constants.AddInName);
        templateInstance.SetTag("Ecom:LiveIntegration.ErrorMessage", message);

        string notificationEmailSubject = Settings.Instance.NotificationEmailSubject;
        string notificationEmailSenderEmail = Settings.Instance.NotificationEmailSenderEmail;
        string notificationEmailSenderName = Settings.Instance.NotificationEmailSenderName;

        var mail = new System.Net.Mail.MailMessage
        {
          IsBodyHtml = true,
          Subject = notificationEmailSubject,
          SubjectEncoding = System.Text.Encoding.UTF8,
          From = new System.Net.Mail.MailAddress(notificationEmailSenderEmail, notificationEmailSenderName, System.Text.Encoding.UTF8)
        };
        //Set parameters for MailMessage
        mail.To.Add(notificationEmail);
        mail.BodyEncoding = System.Text.Encoding.UTF8;
        //Render Template and set as Body
        mail.Body = templateInstance.Output();
        //Send mail
        EmailHandler.Send(mail);
        result = true;
      }
      return result;
    }

    private bool IsAllowedAddToLog(ErrorLevel level)
    {
      bool result = false;
      switch (level)
      {
        case ErrorLevel.ConnectionError:
          result = LogConnectionErrors;
          break;

        case ErrorLevel.ResponseError:
          result = LogResponseErrors;
          break;

        case ErrorLevel.Error:
          result = LogGeneralErrors;
          break;

        case ErrorLevel.DebugInfo:
          result = LogDebugInfo;
          break;

        case ErrorLevel.EmailSend:
          result = true;
          break;
      }
      return result;
    }
  }
}