using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;

namespace HRC.Common.Security
{
    public class WindowsSecurity
    {
        public static string GetWindowsName()
        {
            if (System.Security.Principal.WindowsIdentity.GetCurrent() != null)
            {
                return System.Security.Principal.WindowsIdentity.GetCurrent().Name;                
            }
            return string.Empty;
        }

        public static bool CheckWindowsAuthentication(string login, string password)
        {            
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
            {
                if (context.ValidateCredentials(login, password))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
