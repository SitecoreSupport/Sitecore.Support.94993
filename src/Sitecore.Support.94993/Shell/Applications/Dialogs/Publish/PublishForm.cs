using Sitecore.Configuration;
using Sitecore.Globalization;
using Sitecore.Jobs;
using Sitecore.Publishing;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Support.Shell.Applications.Dialogs.Publish
{
  public class PublishForm : Sitecore.Shell.Applications.Dialogs.Publish.PublishForm
  {
    public void CheckStatus()
    {
      Handle handle = Handle.Parse(base.JobHandle);
      if (!handle.IsLocal)
      {
        SheerResponse.Timer("CheckStatus", Settings.Publishing.PublishDialogPollingInterval);
      }
      else
      {
        PublishStatus status = PublishManager.GetStatus(handle);
        if (status == null)
        {
          throw new Exception("The publishing process was unexpectedly interrupted.");
        }
        if (status.Failed)
        {
          base.Active = "Retry";
          base.NextButton.Disabled = true;
          base.BackButton.Disabled = false;
          base.CancelButton.Disabled = false;
          base.ErrorText.Value = StringUtil.StringCollectionToString(status.Messages);
        }
        else
        {
          string str;
          if (status.State == JobState.Running)
          {
            object[] args = new object[6];
            args[0] = Translate.Text("Database:");
            string text = string.Empty;
            if ((status.CurrentTarget != null) && !string.IsNullOrWhiteSpace(status.CurrentTarget.Name))
            {
              text = status.CurrentTarget.Name;
            }
            args[1] = StringUtil.Capitalize(text);
            args[2] = Translate.Text("Language:");
            args[3] = status.CurrentLanguageMessage ?? string.Empty;
            args[4] = Translate.Text("Processed:");
            args[5] = status.Processed;
            str = string.Format("{0} {1}<br/><br/>{2} {3}<br/><br/>{4} {5}", args);
          }
          else if (status.State == JobState.Initializing)
          {
            str = Translate.Text("Initializing.");
          }
          else
          {
            str = Translate.Text("Queued.");
          }
          if (status.IsDone)
          {
            object[] parameters = new object[] { status.Processed.ToString() };
            base.Status.Text = Translate.Text("Items processed: {0}.", parameters);
            base.Active = "LastPage";
            base.BackButton.Disabled = true;
            string str3 = StringUtil.StringCollectionToString(status.Messages, "\n");
            if (!string.IsNullOrEmpty(str3))
            {
              base.ResultText.Value = str3;
            }
          }
          else
          {
            SheerResponse.SetInnerHtml("PublishingTarget", str);
            SheerResponse.Timer("CheckStatus", Settings.Publishing.PublishDialogPollingInterval);
          }
        }
      }
    }
  }
}