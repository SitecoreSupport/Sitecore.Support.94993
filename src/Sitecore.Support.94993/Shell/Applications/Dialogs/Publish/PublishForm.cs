using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Extensions;
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
      Handle handle = Handle.Parse(this.JobHandle);
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
          this.ErrorText.Value = StringUtil.StringCollectionToString(status.Messages);
        }
        else
        {
          string str;
          if (status.State == JobState.Running)
          {
            str = $"{Translate.Text("Database:")} {StringUtil.Capitalize(status.CurrentTarget.NullOr<Database, string>(db => db.Name))}<br/><br/>{Translate.Text("Language:")} {status.CurrentLanguageMessage ?? string.Empty}<br/><br/>{Translate.Text("Processed:")} {status.Processed}";
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
            this.Status.Text = Translate.Text("Items processed: {0}.", new object[] { status.Processed.ToString() });
            base.Active = "LastPage";
            base.BackButton.Disabled = true;
            string str2 = StringUtil.StringCollectionToString(status.Messages, "\n");
            if (!string.IsNullOrEmpty(str2))
            {
              this.ResultText.Value = str2;
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