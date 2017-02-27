using System;

namespace YmatouMQNet4.Configuration
{
    public struct _UpdateConnectionInfo
    {
        public ConnectionInfo Conn { get; set; }
        public CfgUpdateType UpType { get; set; }
        public string AppId { get; set; }
        public string Code { get; set; }
        public uint ConnNum { get; set; }
    }
}
