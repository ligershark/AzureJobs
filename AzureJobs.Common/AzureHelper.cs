using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureJobs.Common {
    /// <summary>
    /// This method will look for the following indicators as running as an
    /// azure webjob:
    ///     1. appSetting AzCommon.IsWebJob
    ///     2. Env var named WEBJOBS_NAME that is not empty
    /// </summary>
    public class AzureHelper {
        public bool IsRunningAsWebJob() {

            bool isRunningAsWebJob = false;

            string isWjAppSetting = ConfigurationManager.AppSettings.Get("AzCommon.IsWebJob") as string;
            if (!string.IsNullOrEmpty(isWjAppSetting)) {
                isRunningAsWebJob = bool.Parse(isWjAppSetting);
            }
            else {
                // see if there is a env var WEBJOBS_NAME
                string envVarValue = Environment.GetEnvironmentVariable("WEBJOBS_NAME");
                if (!string.IsNullOrEmpty(envVarValue)) {
                    isRunningAsWebJob = true;
                }
            }

            return isRunningAsWebJob;
        }
    }
}
