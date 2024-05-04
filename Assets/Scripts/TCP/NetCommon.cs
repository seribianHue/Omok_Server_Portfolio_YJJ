using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public static class NetCommon
{
    public static string GetHostIP()
    {
        IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress ip in ipEntry.AddressList)
        {
            if(ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return string.Empty;
        //return ipEntry.AddressList[2].ToString();
    }
}
