using NLog;
using System;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iiBee.RunTime.WorkflowHandling.Extensions
{
    public class LogParticipant : TrackingParticipant
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        private void AddLog(TraceLevel level, string msg)
        {
            switch (level)
            {
                case TraceLevel.Error:
                    log.Error(msg);
                    break;
                case TraceLevel.Info:
                    log.Info(msg);
                    break;
                case TraceLevel.Off:
                    break;
                case TraceLevel.Verbose:
                    log.Trace(msg);
                    break;
                case TraceLevel.Warning:
                    log.Warn(msg);
                    break;
                default:
                    break;
            }
        }

        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            log.Trace("Type: {0} Level: {1}, RecordNumber: {2}", record.GetType().Name, record.Level, record.RecordNumber);

            WorkflowInstanceRecord instance = record as WorkflowInstanceRecord;
            if (instance != null)
                log.Trace(" InstanceID: {0} State: {1}", instance.InstanceId, instance.State);

            BookmarkResumptionRecord bookmark = record as BookmarkResumptionRecord;
            if (bookmark != null)
                log.Trace(" Bookmark {0} resumed", bookmark.BookmarkName);

            ActivityStateRecord activity = record as ActivityStateRecord;
            if (activity != null)
            {
                IDictionary<string, object> variables = activity.Variables;
                StringBuilder s = new StringBuilder();

                if (variables.Count > 0)
                {
                    s.AppendLine(" Variables:");
                    foreach (KeyValuePair<string, object> v in variables)
                    {
                        s.AppendLine(string.Format("  {0} Value: [{1}]", v.Key, v.Value));
                    }
                }
                log.Trace(" Activity: {0} State: {1} {2}", activity.Activity.Name, activity.State, s.ToString());
            }

            CustomTrackingRecord user = record as CustomTrackingRecord;
            if ((user != null) && (user.Data.Count > 0))
            {
                log.Trace(" User Data: {0}", user.Name);
                foreach (string data in user.Data.Keys)
                {
                    log.Trace("  {0} : {1}", data, user.Data[data]);
                }
            }
        }
    }
}
