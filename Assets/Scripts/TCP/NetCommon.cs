using System.Net;

public static class NetCommon
{
    public static string GetHostIP()
    {
        IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
        return ipEntry.AddressList[1].ToString();
    }
}
