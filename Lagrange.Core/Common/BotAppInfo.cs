#pragma warning disable CS8618

namespace Lagrange.Core.Common;

public class BotAppInfo
{
    public string Os { get; set; }
    
    public string VendorOs { get; set; }
    
    public string Kernel { get; set; }

    public string CurrentVersion { get; set; }
    
    public string PtVersion { get; set; }
    
    public int SsoVersion { get; set; }
    
    public string PackageName { get; set; }
    
    public byte[] ApkSignatureMd5 { get; set; }
    
    public WtLoginSdkInfo SdkInfo { get; set; }
    
    public int AppId { get; set; }
    
    public int SubAppId { get; set; }
    
    public int AppIdQrCode { get; set; }
    
    public ushort AppClientVersion { get; set; }
    
    public ushort NTLoginType { get; set; }

    private static readonly BotAppInfo Linux = new()
    {
        Os = "Linux",
        Kernel = "Linux",
        VendorOs = "linux",
        CurrentVersion = "3.2.15-30366",
        PtVersion = "2.0.0",
        SsoVersion = 19,
        PackageName = "com.tencent.qq",
        ApkSignatureMd5 = "com.tencent.qq"u8.ToArray(),
        SdkInfo = new WtLoginSdkInfo
        {
            SdkBuildTime = 0,
            SdkVersion = "nt.wtlogin.0.0.1",
            MiscBitMap = 12058620,
            SubSigMap = 0,
            MainSigMap = 169742560
        },
        AppId = 1600001615,
        SubAppId = 537258424,
        AppIdQrCode = 13697054,
        AppClientVersion = 30366,
        
        NTLoginType = 1
    };
    
    private static readonly BotAppInfo MacOs = new()
    {
        Os = "Mac",
        Kernel = "Darwin",
        VendorOs = "mac",
        CurrentVersion = "6.9.23-20139",
        PtVersion = "2.0.0",
        SsoVersion = 23,
        PackageName = "com.tencent.qq",
        ApkSignatureMd5 = "com.tencent.qq"u8.ToArray(),
        SdkInfo = new WtLoginSdkInfo
        {
            SdkBuildTime = 0,
            SdkVersion = "nt.wtlogin.0.0.1",
            MiscBitMap = 12058620,
            SubSigMap = 0,
            MainSigMap = 169742560
        },
        AppId = 1600001602,
        SubAppId = 537200848,
        AppIdQrCode = 537200848,
        AppClientVersion = 13172,
        
        NTLoginType = 5
    };
    
    private static readonly BotAppInfo Windows = new()
    {
        Os = "Windows",
        Kernel = "Windows_NT",
        VendorOs = "win32",
        CurrentVersion = "9.9.2-15962",
        PtVersion = "2.0.0",
        SsoVersion = 23,
        PackageName = "com.tencent.qq",
        ApkSignatureMd5 = "com.tencent.qq"u8.ToArray(),
        SdkInfo = new WtLoginSdkInfo
        {
            SdkBuildTime = 0,
            SdkVersion = "nt.wtlogin.0.0.1",
            MiscBitMap = 12058620,
            SubSigMap = 0,
            MainSigMap = 169742560,
        },
        AppId = 1600001604,
        SubAppId = 537138217,
        AppIdQrCode = 537138217,
        AppClientVersion = 13172,
        
        NTLoginType = 5
    };

    private static readonly BotAppInfo AndroidPhone = new()
    {
        Os = "Android",
        CurrentVersion = "9.1.60.045f5d19",
        PtVersion = "9.1.60",
        SsoVersion = 22,
        AppId = 16,
        SubAppId = 537275636,
        PackageName = "com.tencent.mobileqq",
        ApkSignatureMd5 = [0xA6, 0xB7, 0x45, 0xBF, 0x24, 0xA2, 0xC2, 0x77, 0x52, 0x77, 0x16, 0xF6, 0xF3, 0x6E, 0xB6, 0x8D],
        SdkInfo = new WtLoginSdkInfo
        {
            SdkBuildTime = 1702888273,
            SdkVersion = "6.0.0.2568",
            MiscBitMap = 150470524,
            SubSigMap = 66560,
            MainSigMap = 34869472
        },
        AppClientVersion = 0
    };
    
    private static readonly BotAppInfo AndroidPad = new()
    {
        Os = "Android",
        CurrentVersion = "9.1.60.045f5d19",
        PtVersion = "9.1.60",
        AppId = 16,
        SubAppId = 537275675,
        SsoVersion = 22,
        PackageName = "com.tencent.mobileqq",
        ApkSignatureMd5 = [0xA6, 0xB7, 0x45, 0xBF, 0x24, 0xA2, 0xC2, 0x77, 0x52, 0x77, 0x16, 0xF6, 0xF3, 0x6E, 0xB6, 0x8D],
        SdkInfo = new WtLoginSdkInfo
        {
            SdkBuildTime = 1702888273,
            SdkVersion = "6.0.0.2568",
            MiscBitMap = 150470524,
            SubSigMap = 66560,
            MainSigMap = 34869472,
        },
        AppClientVersion = 0
    };
    
    public static readonly Dictionary<Protocols, BotAppInfo> ProtocolToAppInfo = new()
    {
        { Protocols.Windows, Windows },
        { Protocols.Linux, Linux },
        { Protocols.MacOs, MacOs },
        { Protocols.AndroidPhone, AndroidPhone },
        { Protocols.AndroidPad, AndroidPad }
    };
}

public class WtLoginSdkInfo
{
    public uint SdkBuildTime { get; set; }
    
    public string SdkVersion { get; set; }
    
    public uint MiscBitMap { get; set; }
    
    public uint SubSigMap { get; set; }
    
    public uint MainSigMap { get; set; }
}