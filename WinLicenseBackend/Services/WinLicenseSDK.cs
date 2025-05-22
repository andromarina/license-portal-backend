using System.Runtime.InteropServices;
using System.Text;

namespace WinLicenseBackend.Services
{
    /****************************************************************************** 
   /* Required structures for WinLicense SDK functions
   /******************************************************************************/

    [StructLayout(LayoutKind.Sequential)]
    public class SystemTime
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class sLicenseFeatures
    {
        public int cb;
        public int NumDays;
        public int NumExec;
        public SystemTime ExpDate;
        public int CountryId;
        public int Runtime;
        public int GlobalMinutes;
        public SystemTime InstallDate;
        public int NetInstances;
        public int EmbedLicenseInfoInKey;
        public int EmbedCreationDate;
    }

    /****************************************************************************** 
    /* Class: WinlicenseSDK
    /*
    /* Description: Wrapper for the Winlicense SDK APIs
    /*
    /******************************************************************************/

    public class WinlicenseSDK
    {
        const string WINLICENSE_DLL = "WinlicenseSDK.dll";
       // const string WINLICENSE_DLL = "WinlicenseSDK64.dll";

        [DllImport(WINLICENSE_DLL, EntryPoint = "WLGenPassword", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenPassword(string PassHash, string Name, StringBuilder PasswordBuffer);

        [DllImport(WINLICENSE_DLL, EntryPoint = "WLGenTrialExtensionFileKey", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenTrialExtensionFileKey(string TrialHash, int Level, int NumDays, int NumExec, int NewDate, int NumMinutes, int TimeRuntime, byte[] BufferOut);

        [DllImport(WINLICENSE_DLL, EntryPoint = "WLGenLicenseFileKey", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseFileKey(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
                             int NumDays, int NumExec, SystemTime NewDate, int CountryId, int Runtime, int GlobalTime, byte[] LicenseBuffer);

        [DllImport(WINLICENSE_DLL, CharSet = CharSet.Unicode, EntryPoint = "WLGenLicenseFileKeyW", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseFileKeyW(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
                             int NumDays, int NumExec, SystemTime NewDate, int CountryId, int Runtime, int GlobalTime, byte[] LicenseBuffer);

        [DllImport(WINLICENSE_DLL, EntryPoint = "WLGenLicenseRegistryKey", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseRegistryKey(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            int NumDays, int NumExec, SystemTime NewDate, int CountryId, int Runtime, int GlobalTime, string RegName, string RegValueName, byte[] LicenseBuffer);

        [DllImport(WINLICENSE_DLL, CharSet = CharSet.Unicode, EntryPoint = "WLGenLicenseRegistryKeyW", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseRegistryKeyW(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            int NumDays, int NumExec, SystemTime NewDate, int CountryId, int Runtime, int GlobalTime, string RegName, string RegValueName, byte[] LicenseBuffer);

        [DllImport(WINLICENSE_DLL, EntryPoint = "WLGenLicenseTextKey", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseTextKey(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            int NumDays, int NumExec, SystemTime NewDate, int CountryId, int Runtime, int GlobalTime, StringBuilder LicenseBuffer);

        [DllImport(WINLICENSE_DLL, CharSet = CharSet.Unicode, EntryPoint = "WLGenLicenseTextKeyW", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseTextKeyW(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            int NumDays, int NumExec, SystemTime NewDate, int CountryId, int Runtime, int GlobalTime, StringBuilder LicenseBuffer);

        [DllImport(WINLICENSE_DLL, EntryPoint = "WLGenLicenseSmartKey", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseSmartKey(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            int NumDays, int NumExec, SystemTime NewDate, StringBuilder LicenseBuffer);

        [DllImport(WINLICENSE_DLL, CharSet = CharSet.Unicode, EntryPoint = "WLGenLicenseSmartKeyW", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseSmartKeyW(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            int NumDays, int NumExec, SystemTime NewDate, StringBuilder LicenseBuffer);

        [DllImport(WINLICENSE_DLL, EntryPoint = "WLGenLicenseFileKeyEx", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseFileKeyEx(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            sLicenseFeatures LicenseFeatures, byte[] LicenseBuffer);

        [DllImport(WINLICENSE_DLL, CharSet = CharSet.Unicode, EntryPoint = "WLGenLicenseFileKeyExW", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseFileKeyExW(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            sLicenseFeatures LicenseFeatures, byte[] LicenseBuffer);

        [DllImport(WINLICENSE_DLL, EntryPoint = "WLGenLicenseTextKeyEx", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseTextKeyEx(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            sLicenseFeatures LicenseFeatures, StringBuilder LicenseBuffer);

        [DllImport(WINLICENSE_DLL, CharSet = CharSet.Unicode, EntryPoint = "WLGenLicenseTextKeyExW", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseTextKeyExW(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            sLicenseFeatures LicenseFeatures, StringBuilder LicenseBuffer);

        [DllImport(WINLICENSE_DLL, EntryPoint = "WLGenLicenseRegistryKeyEx", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseRegistryKey(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            sLicenseFeatures LicenseFeatures, string RegName, string RegValueName, byte[] LicenseBuffer);

        [DllImport(WINLICENSE_DLL, CharSet = CharSet.Unicode, EntryPoint = "WLGenLicenseRegistryKeyExW", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseRegistryKeyW(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            sLicenseFeatures LicenseFeatures, string RegName, string RegValueName, byte[] LicenseBuffer);

        [DllImport(WINLICENSE_DLL, EntryPoint = "WLGenLicenseDynSmartKey", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseDynSmartKey(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            sLicenseFeatures LicenseFeatures, StringBuilder LicenseBuffer);

        [DllImport(WINLICENSE_DLL, CharSet = CharSet.Unicode, EntryPoint = "WLGenLicenseDynSmartKeyW", CallingConvention = CallingConvention.StdCall)]
        public static extern int WLGenLicenseDynSmartKeyW(string LicenseHash, string UserName, string Organization, string CustomData, string MachineID,
            sLicenseFeatures LicenseFeatures, StringBuilder LicenseBuffer);
    }
}
