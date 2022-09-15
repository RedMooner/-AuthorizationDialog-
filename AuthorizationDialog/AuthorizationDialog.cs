using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows;

namespace AuthorizationDialogLib
{
    public class AuthorizationDialog
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }
        [DllImport("credui.dll", CharSet = CharSet.Unicode)]
        private static extern uint CredUIPromptForWindowsCredentials(ref CREDUI_INFO notUsedHere,
          int authError,
          ref uint authPackage,
          IntPtr InAuthBuffer,
          uint InAuthBufferSize,
          out IntPtr refOutAuthBuffer,
          out uint refOutAuthBufferSize,
          ref bool fSave,
          uint flags);

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern bool CredUnPackAuthenticationBuffer(int dwFlags, IntPtr pAuthBuffer, uint cbAuthBuffer,
            StringBuilder pszUserName, ref int pcchMaxUserName, StringBuilder pszDomainName,
            ref int pcchMaxDomainame, StringBuilder pszPassword, ref int pcchMaxPassword
            );

        [DllImport("ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr ptr);

        public delegate void ErrorMessageEventHandler(string message);
        public event ErrorMessageEventHandler ErrorMessage;
        private string _login = "admin";
        private string _password = "admin";

        public string Login
        {
            get { return _login; }
            set { _login = value; }
        }
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        private Window _currentWindow;
        public AuthorizationDialog(Window window)
        {
            _currentWindow = window;
        }
        public enum DialogResults
        {
            OK,
            Error
        }
        public DialogResults DialogResult = DialogResults.Error;
        public bool ShowDialog(string title,string caption)
        {
            if (CheckCredentials(title, caption))
                DialogResult = DialogResults.OK;
            else
                DialogResult = DialogResults.Error;
            return true;
        }
    
        private bool CheckCredentials(string title,string caption)
        {
            bool save = false;
            int errorcode = 0;
            uint dialogReturn;
            uint authPackage = 0;
            IntPtr outCredBuffer;
            uint outCredSize;

            CREDUI_INFO credui = new CREDUI_INFO();
            credui.cbSize = Marshal.SizeOf(credui);
            credui.pszCaptionText = "Авторизация";
            credui.pszMessageText = "Введите логин и пароль";
            credui.hwndParent = new WindowInteropHelper(_currentWindow).Handle;

            //Show dialog
            dialogReturn = CredUIPromptForWindowsCredentials(ref credui,
            errorcode, ref authPackage, (IntPtr)0, 0, out outCredBuffer, out outCredSize, ref save,
            0x1 /*CREDUIWIN_GENERIC*/);

            if (dialogReturn != 0) return false; //Cancel pressed

            var usernameBuf = new StringBuilder(100);
            var passwordBuf = new StringBuilder(100);
            var domainBuf = new StringBuilder(100);

            int maxUserName = 100;
            int maxDomain = 100;
            int maxPassword = 100;

            //Validate credentials
            if (CredUnPackAuthenticationBuffer(0, outCredBuffer, outCredSize, usernameBuf,
                ref maxUserName, domainBuf, ref maxDomain, passwordBuf, ref maxPassword))
            {
                CoTaskMemFree(outCredBuffer);

                using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
                {
                    bool valid = false;
                    try
                    {
                        if (usernameBuf.ToString() == Login && passwordBuf.ToString() == Password) valid = true;
                    }
                    catch (System.DirectoryServices.AccountManagement.PrincipalOperationException ex)
                    {
                        if (ErrorMessage != null) ErrorMessage("Error: " + ex.Message);
                        valid = false;
                    }
                    return valid;
                }
            }
            else throw new ApplicationException("CredUnPackAuthenticationBuffer failed");
        }
    }
}
