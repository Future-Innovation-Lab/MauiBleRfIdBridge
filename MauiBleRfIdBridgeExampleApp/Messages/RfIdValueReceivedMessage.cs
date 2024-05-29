using CommunityToolkit.Mvvm.Messaging.Messages;
using MauiBleRfIdBridgeExampleApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiBleRfIdBridgeExampleApp.Messages
{
    public class RfIdValueReceivedMessage : ValueChangedMessage<RfIdTagData>
    {
        public RfIdValueReceivedMessage(RfIdTagData message) : base(message)
        {
        }
    }
}
