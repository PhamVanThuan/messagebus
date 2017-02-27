/**
 * Autogenerated by Thrift Compiler (0.9.2)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Thrift;
using Thrift.Collections;
using System.Runtime.Serialization;
using Thrift.Protocol;
using Thrift.Transport;

namespace MQMessageProtocols
{

  #if !SILVERLIGHT
  [Serializable]
  #endif
  public partial class MessageDto : TBase
  {
    private string _AppId;
    private string _code;
    private string _Ip;
    private string _MsgUniqueId;
    private byte[] _Body;

    public string AppId
    {
      get
      {
        return _AppId;
      }
      set
      {
        __isset.AppId = true;
        this._AppId = value;
      }
    }

    public string Code
    {
      get
      {
        return _code;
      }
      set
      {
        __isset.code = true;
        this._code = value;
      }
    }

    public string Ip
    {
      get
      {
        return _Ip;
      }
      set
      {
        __isset.Ip = true;
        this._Ip = value;
      }
    }

    public string MsgUniqueId
    {
      get
      {
        return _MsgUniqueId;
      }
      set
      {
        __isset.MsgUniqueId = true;
        this._MsgUniqueId = value;
      }
    }

    public byte[] Body
    {
      get
      {
        return _Body;
      }
      set
      {
        __isset.Body = true;
        this._Body = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool AppId;
      public bool code;
      public bool Ip;
      public bool MsgUniqueId;
      public bool Body;
    }

    public MessageDto() {
    }

    public void Read (TProtocol iprot)
    {
      TField field;
      iprot.ReadStructBegin();
      while (true)
      {
        field = iprot.ReadFieldBegin();
        if (field.Type == TType.Stop) { 
          break;
        }
        switch (field.ID)
        {
          case 1:
            if (field.Type == TType.String) {
              AppId = iprot.ReadString();
            } else { 
              TProtocolUtil.Skip(iprot, field.Type);
            }
            break;
          case 2:
            if (field.Type == TType.String) {
              Code = iprot.ReadString();
            } else { 
              TProtocolUtil.Skip(iprot, field.Type);
            }
            break;
          case 3:
            if (field.Type == TType.String) {
              Ip = iprot.ReadString();
            } else { 
              TProtocolUtil.Skip(iprot, field.Type);
            }
            break;
          case 4:
            if (field.Type == TType.String) {
              MsgUniqueId = iprot.ReadString();
            } else { 
              TProtocolUtil.Skip(iprot, field.Type);
            }
            break;
          case 5:
            if (field.Type == TType.String) {
              Body = iprot.ReadBinary();
            } else { 
              TProtocolUtil.Skip(iprot, field.Type);
            }
            break;
          default: 
            TProtocolUtil.Skip(iprot, field.Type);
            break;
        }
        iprot.ReadFieldEnd();
      }
      iprot.ReadStructEnd();
    }

    public void Write(TProtocol oprot) {
      TStruct struc = new TStruct("MessageDto");
      oprot.WriteStructBegin(struc);
      TField field = new TField();
      if (AppId != null && __isset.AppId) {
        field.Name = "AppId";
        field.Type = TType.String;
        field.ID = 1;
        oprot.WriteFieldBegin(field);
        oprot.WriteString(AppId);
        oprot.WriteFieldEnd();
      }
      if (Code != null && __isset.code) {
        field.Name = "code";
        field.Type = TType.String;
        field.ID = 2;
        oprot.WriteFieldBegin(field);
        oprot.WriteString(Code);
        oprot.WriteFieldEnd();
      }
      if (Ip != null && __isset.Ip) {
        field.Name = "Ip";
        field.Type = TType.String;
        field.ID = 3;
        oprot.WriteFieldBegin(field);
        oprot.WriteString(Ip);
        oprot.WriteFieldEnd();
      }
      if (MsgUniqueId != null && __isset.MsgUniqueId) {
        field.Name = "MsgUniqueId";
        field.Type = TType.String;
        field.ID = 4;
        oprot.WriteFieldBegin(field);
        oprot.WriteString(MsgUniqueId);
        oprot.WriteFieldEnd();
      }
      if (Body != null && __isset.Body) {
        field.Name = "Body";
        field.Type = TType.String;
        field.ID = 5;
        oprot.WriteFieldBegin(field);
        oprot.WriteBinary(Body);
        oprot.WriteFieldEnd();
      }
      oprot.WriteFieldStop();
      oprot.WriteStructEnd();
    }

    public override string ToString() {
      StringBuilder __sb = new StringBuilder("MessageDto(");
      bool __first = true;
      if (AppId != null && __isset.AppId) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("AppId: ");
        __sb.Append(AppId);
      }
      if (Code != null && __isset.code) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Code: ");
        __sb.Append(Code);
      }
      if (Ip != null && __isset.Ip) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Ip: ");
        __sb.Append(Ip);
      }
      if (MsgUniqueId != null && __isset.MsgUniqueId) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("MsgUniqueId: ");
        __sb.Append(MsgUniqueId);
      }
      if (Body != null && __isset.Body) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Body: ");
        __sb.Append(Body);
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}