/*
Author: liyonghelpme
Email: 233242872@qq.com
*/
using MyLib;
using Google.ProtocolBuffers;

namespace KBEngine
{
    using UnityEngine; 
    using System; 
    
    //using MessageID = System.UInt16;
    //using MessageLength = System.UInt32;

    public class MessageReader
    {
        enum READ_STATE
        {
            //READ_STATE_FLAG = 0,
            READ_STATE_MSGLEN = 1,
            READ_STATE_FLOWID = 2,
            READ_STATE_MODULEID = 3,
            READ_STATE_MSGID = 4,
            //READ_STATE_RESPONSE_TIME = 5,
            READ_STATE_RESPONSE_FLAG =6,
            READ_STATE_BODY = 7,
        }
        
        private byte msgid = 0;
        private ushort msglen = 0;
        byte flag;
        byte flowId;
        byte moduleId;
        byte responseFlag;
        public MessageHandler msgHandle = null;
        public IMainLoop mainLoop;
        /*
         * Response Packet Format
         * 
         * 0xcc   byte
         * length int
         * flowId int
         * moduleId byte
         * messageId short
         * responseTime int
         * responseFlag byte
         * protobuffer 
         */ 
        private uint expectSize = 1;
        private READ_STATE state = READ_STATE.READ_STATE_MSGLEN;
        private MemoryStream stream = new MemoryStream();
        
        public MessageReader()
        {
            expectSize = 2;
            state = READ_STATE.READ_STATE_MSGLEN;
        }

        public void process(byte[] datas, uint length, ThreadSafeDic flowHandler)
        {
            //Debug.LogError("process receive Data " + length + " state " + state+" expect "+expectSize);
            uint totallen = 0;
            while (length > 0 && expectSize > 0)
            {
                if (state == READ_STATE.READ_STATE_MSGLEN)
                {
                    if (length >= expectSize)
                    {
                        Array.Copy(datas, totallen, stream.data(), stream.wpos, expectSize);
                        totallen += expectSize;
                        stream.wpos += (int)expectSize;
                        length -= expectSize;
                        
                        msglen = stream.readUint16();
                        stream.clear();
                        
                        state = READ_STATE.READ_STATE_FLOWID;
                        expectSize = 1;
                    } else
                    {
                        Array.Copy(datas, totallen, stream.data(), stream.wpos, length);
                        stream.wpos += (int)length;
                        expectSize -= length;
                        break;
                    }
                } else if (state == READ_STATE.READ_STATE_FLOWID)
                {
                    if (length >= expectSize)
                    {
                        Array.Copy(datas, totallen, stream.data(), stream.wpos, expectSize);
                        totallen += expectSize;
                        stream.wpos += (int)expectSize;
                        length -= expectSize;
                        
                        flowId = stream.readUint8();
                        stream.clear();
                        
                        state = READ_STATE.READ_STATE_MODULEID;
                        expectSize = 1;
                    } else
                    {
                        Array.Copy(datas, totallen, stream.data(), stream.wpos, length);
                        stream.wpos += (int)length;
                        expectSize -= length;
                        break;
                    }
                    
                } else if (state == READ_STATE.READ_STATE_MODULEID)
                {
                    if (length >= expectSize)
                    {
                        Array.Copy(datas, totallen, stream.data(), stream.wpos, expectSize);
                        totallen += expectSize;
                        stream.wpos += (int)expectSize;
                        length -= expectSize;
                        
                        moduleId = stream.readUint8();
                        stream.clear();
                        
                        state = READ_STATE.READ_STATE_MSGID;
                        expectSize = 1;
                    } else
                    {
                        Array.Copy(datas, totallen, stream.data(), stream.wpos, length);
                        stream.wpos += (int)length;
                        expectSize -= length;
                        break;
                    }
                } else if (state == READ_STATE.READ_STATE_MSGID)
                {
                    if (length >= expectSize)
                    {
                        Array.Copy(datas, totallen, stream.data(), stream.wpos, expectSize);
                        totallen += expectSize;
                        stream.wpos += (int)expectSize;
                        length -= expectSize;
                        
                        msgid = stream.readUint8();
                        stream.clear();
                        
                        state = READ_STATE.READ_STATE_RESPONSE_FLAG;
                        expectSize = 1;
                    } else
                    {
                        Array.Copy(datas, totallen, stream.data(), stream.wpos, length);
                        stream.wpos += (int)length;
                        expectSize -= length;
                        break;
                    }
                }
                else if (state == READ_STATE.READ_STATE_RESPONSE_FLAG)
                {
                    if (length >= expectSize)
                    {
                        Array.Copy(datas, totallen, stream.data(), stream.wpos, expectSize);
                        totallen += expectSize;
                        stream.wpos += (int)expectSize;
                        length -= expectSize;
                        
                        responseFlag = stream.readUint8();
                        stream.clear();
                        
                        state = READ_STATE.READ_STATE_BODY;
                        //expectSize = msglen - 4 - 1 - 2 - 4 - 2;
                        expectSize = (uint)(msglen - 1 - 1 - 1 - 1);//flowId moduleId msgId 
                    } else
                    {
                        Array.Copy(datas, totallen, stream.data(), stream.wpos, length);
                        stream.wpos += (int)length;
                        expectSize -= length;
                        break;
                    }
                }

                /*
                 * body Can be empty
                 */ 
                if (state == READ_STATE.READ_STATE_BODY)
                {
                    //Debug.LogError("body expect BodySize:"+length+" expSize "+expectSize);
                    if (length >= expectSize)
                    {
                        Array.Copy(datas, totallen, stream.data(), stream.wpos, expectSize);
                        totallen += expectSize;
                        stream.wpos += (int)expectSize;
                        length -= expectSize;
                        /*
                         * No Handler Or PushMessage  forward To IPacketHandler 
                         * Call Who's RPC Method Or Register Many RPC Method to Handle It ?
                         * [PushHandler]
                         * void GCPushSpriteInfo(Packet packet) {
                         * }
                         * 
                         * PacketHandler namespace
                         * IPacketHandler---->GCPushSpriteInfo
                         */ 

                        IMessageLite pbmsg = KBEngine.Message.handlePB(moduleId, msgid, stream);
                        //Debug.LogError("expect msgType: "+pbmsg.GetType());
                        Packet p = new Packet(msglen, flowId, moduleId, msgid, responseFlag, pbmsg);
                        var fullName = pbmsg.GetType().FullName;
                        mainLoop.queueInLoop(() =>
                        {
                            Log.Net("ReadPacket: "+p.protoBody.ToString());
                        });

                        MessageHandler handler = null;
                        if (flowHandler == null)
                        {
                            handler = msgHandle;
                        } else if (flowHandler.Contain(flowId))
                        {
                            handler = flowHandler.Get(flowId);
                            flowHandler.Remove(flowId);
                            if (handler == null)
                            {
                                Debug.LogError("FlowHandlerIsNull: "+flowId);
                            }
                        }else {
                            handler = msgHandle;
                        }

                        //Debug.LogError("HandlerIs: "+flowId+" h "+handler);
                        if (fullName.Contains("Push"))
                        {
                            //Log.Net("MessageReader Handler PushMessage");
                            if (mainLoop != null)
                            {
                                mainLoop.queueInLoop(delegate
                                {
                                    var handlerName = fullName.Replace("MyLib", "PacketHandler");
                                    var tp = Type.GetType(handlerName);
                                    if (tp == null)
                                    {
                                        Debug.LogError("PushMessage noHandler " + handlerName);
                                    } else
                                    {
                                        //Debug.Log("Handler Push Message here "+handlerName);
                                        var ph = (PacketHandler.IPacketHandler)Activator.CreateInstance(tp);
                                        ph.HandlePacket(p);
                                    }

                                });
                            }
                        } else if (handler != null)
                        {
                            mainLoop.queueInLoop(()=>{
                                handler(p);
                            });

                        } else
                        {
                            Debug.LogError("MessageReader::process No handler for flow Message " + msgid + " " + flowId + " " + pbmsg.GetType() + " " + pbmsg);
                        }

                        stream.clear();
                        state = READ_STATE.READ_STATE_MSGLEN;
                        expectSize = 2;
                    } else
                    {
                        Array.Copy(datas, totallen, stream.data(), stream.wpos, length);
                        stream.wpos += (int)length;
                        expectSize -= length;
                        break;
                    } 
                }
                
            }

            if (responseFlag != 0)
            {
                Debug.LogError("MessageReader:: read Error Packet " + responseFlag);
            }

            //Log.Net("current state after " + state + " msglen " + msglen + " " + length);
            //Log.Net("MessageReader::  prop  flag" + flag + "  msglen " + msglen + " flowId " + flowId + " moduleId " + moduleId + " msgid " + msgid + " responseTime " + responseTime + " responseFlag " + responseFlag + " expectSize " + expectSize);
        }
        
    }
} 
