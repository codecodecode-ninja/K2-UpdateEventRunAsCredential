using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCHC = SourceCode.Hosting.Client;
using SCWFM = SourceCode.Workflow.Management;

namespace K2_UpdateEventRunAsCredential
{
    class Program
    {
        /// <summary>
        /// This is a sample code to set or remove the run as credentials in K2 Workspace
        /// http://codecodecode.ninja/2015/07/setting-event-run-as-rights-via-api/ ‎
        /// 
        /// K2 Article:
        /// http://help.k2.com/onlinehelp/K2blackpearl/UserGuide/current/webframe.html#Specifying_Credentials_in_K2_Workspace_What_to_do.html
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            SCWFM.WorkflowManagementServer svr = null;
            try
            {
                // TODO. Set server name here. You need to run as a K2 Admin account
                svr = new SCWFM.WorkflowManagementServer("k2.myCoy.com", 5555);
                svr.Open();

                Console.WriteLine("Is Current User Admin: {0}", svr.IsCurrentUserAdmin().ToString());

                // TODO: Set process full path here
                SCWFM.ProcessSet procSet = svr.GetProcSet(@"TestEmailProj\Process1");
                SCWFM.Processes processes = svr.GetProcessVersions(procSet.ProcSetID);
                foreach (SCWFM.Process proc in processes)
                {
                    //NOTE: this will be looking for the current default process version only.
                    if (proc.DefaultVersion)
                    {
                        SCWFM.Activities activities = svr.GetProcActivities(proc.ProcID);
                        foreach (SCWFM.Activity activity in activities)
                        {
                            //TODO: set the activity name here. You can improvise here to loop from config files
                            if (activity.Name.Equals("DefaultActivity2", StringComparison.OrdinalIgnoreCase))
                            {
                                SCWFM.Events evtList = svr.GetActivityEvents(activity.ID);
                                foreach (SCWFM.Event evt in evtList)
                                {
                                    //TODO: set the event name you are looking for.
                                    if (evt.Name.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                                    {
                                        //TODO: Set the user's credential here
                                        bool result = svr.SetRunAsUser(@"myCoy\test1", "pass@word1", "K2", evt.Code, proc.ProcID);
                                        Console.WriteLine("Set user on {0}({1}), {2}({3}) = {4}", activity.Name, proc.ProcID, evt.Name, evt.Code.ToString(), result);

                                        //NOTE: use this to remove the credential and revert to service account
                                        //bool result = svr.DeleteRunAsUser(evt.Code, proc.ProcID);
                                        //Console.WriteLine("Reset to service acctount on {0}({1}), {2}({3}) = {4}", activity.Name, proc.ProcID, evt.Name, evt.Code.ToString(), result);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exeception: {0}", ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                if (svr.Connection != null && svr.Connection.IsConnected)
                {
                    svr.Connection.Dispose();
                }
            }
        }
    }
}
