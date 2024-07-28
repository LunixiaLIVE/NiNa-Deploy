using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Runtime.CompilerServices;

namespace NiNa_Deploy
{
    public static class Utility
    {
        internal static bool TableLocked = false;
        internal static readonly String strVersion = "1.2";
        internal static int MaxThreads = 0;
        internal static int TotalThreadsCreated = 0;
        internal static int SleepTimeBetweenThreads = 0;
        internal static readonly String[] listStatus =
        {
            "Ready...",
            "Checking Connectivity...",
            "Checking Admin Shares...",
            "Checking WMI...",
            "Executing...",
            "Running..."
        };

        internal static DataTable RemoteProcessesResults = new();
        internal static DataTable GenerateRemoteProcessesTable()
        {
            DataTable dt = new();
            dt.Columns.Add(new DataColumn("Date", typeof(String)));
            dt.Columns.Add(new DataColumn("Time", typeof(String)));
            dt.Columns.Add(new DataColumn("RemoteHost", typeof(String)));
            dt.Columns.Add(new DataColumn("PingCode", typeof(String)));
            dt.Columns.Add(new DataColumn("ShareCode", typeof(String)));
            dt.Columns.Add(new DataColumn("WMICode", typeof(String)));
            dt.Columns.Add(new DataColumn("CMD", typeof(String)));
            dt.Columns.Add(new DataColumn("PID", typeof(String)));
            dt.Columns.Add(new DataColumn("ExitCode", typeof(String)));
            return dt;
        }
        internal static List<Thread> MonitoringThreadList = [];
        internal static List<Thread> RemoteThreadList = [];
    }

    public class WMI : IDisposable
    { 
        public void Dispose(){ Dispose(true); GC.SuppressFinalize(this); }
        protected bool Disposed { get; private set; }
        protected virtual void Dispose(bool disposing){ Disposed = true; }
        internal String TestConnection(String strComputer)
        {
            ConnectionOptions options = new()
            {
                Impersonation = ImpersonationLevel.Impersonate,
                EnablePrivileges = true,
                Timeout = TimeSpan.FromMilliseconds(Properties.Settings.Default.PingTimeout)
            };
            ManagementScope managementScope = new($"\\\\{strComputer}\\root\\cimv2", options);
            try
            {
                managementScope.Connect();
                if (managementScope.IsConnected){ return "Success"; } else { return "Failed"; }
            }
            catch(ManagementException e)
            {
                return e.Message.ToString();
            }
        }
    }
}
