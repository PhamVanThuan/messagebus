using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common.Extensions;
using YmatouMQ.Common.MessageHandleContract;
using YmatouMQMessageMongodb.AppService;
using YmatouMQMessageMongodb.Domain.IRepository;
using YmatouMQMessageMongodb.Domain.Module;
using YmatouMQMessageMongodb.Domain.Specifications;
using YmatouMQMessageMongodb.Repository;
using YmatouMQNet4.Core;

namespace YmatouMQTest
{
    [TestClass]
    public class MongoTest
    {
        [TestMethod]
        public async Task Insert()
        {
           await new MessageAppService_TimerBatch().BatchAddMessageAsync(new List<MQMessage> 
                   { 
                   {new MQMessage("test2","liguo","0.0.0.0",Guid.NewGuid().ToString ("N"),new {a=1},null)},
                   }, "test2", "liguo"
              );
            Assert.IsTrue(true);
        }
        [TestMethod]
        public async Task Insert_Message()
        {
            MessageAppService_TimerBatch appService = new MessageAppService_TimerBatch();

            await appService.BatchAddMessageAsync(new List<MQMessage> 
            {
                {new MQMessage("test2","liguo","0.0.0.1",Guid.NewGuid ().ToString ("N"),"{\"CellNumber\":\"18621651640\",\"Message\":\"test2\",\"Sign\":\"【洋码头】\",\"MessageId\":\"150616164327822-18621651640-411273\",\"MessageSupply\":5}",null)},
                {new MQMessage("test2","liguo","0.0.0.1",Guid.NewGuid ().ToString ("N"),"{\"CellNumber\":\"18621651640\",\"Message\":\"test2\",\"Sign\":\"【洋码头】\",\"MessageId\":\"150616164327822-18621651640-411273\",\"MessageSupply\":5}",null)},
                {new MQMessage("test2","liguo","0.0.0.1",Guid.NewGuid ().ToString ("N"),"{\"CellNumber\":\"18621651640\",\"Message\":\"test2\",\"Sign\":\"【洋码头】\",\"MessageId\":\"150616164327822-18621651640-411273\",\"MessageSupply\":5}",null)},
                {new MQMessage("test2","liguo","0.0.0.1",Guid.NewGuid ().ToString ("N"),"{\"CellNumber\":\"18621651640\",\"Message\":\"test2\",\"Sign\":\"【洋码头】\",\"MessageId\":\"150616164327822-18621651640-411273\",\"MessageSupply\":5}",null)},
                {new MQMessage("test2","liguo","0.0.0.1",Guid.NewGuid ().ToString ("N"),"{\"CellNumber\":\"18621651640\",\"Message\":\"test2\",\"Sign\":\"【洋码头】\",\"MessageId\":\"150616164327822-18621651640-411273\",\"MessageSupply\":5}",null)},
                {new MQMessage("test2","liguo","0.0.0.1",Guid.NewGuid ().ToString ("N"),"{\"CellNumber\":\"18621651640\",\"Message\":\"test2\",\"Sign\":\"【洋码头】\",\"MessageId\":\"150616164327822-18621651640-411273\",\"MessageSupply\":5}",null)},
                {new MQMessage("test2","liguo","0.0.0.1",Guid.NewGuid ().ToString ("N"),"{\"CellNumber\":\"18621651640\",\"Message\":\"test2\",\"Sign\":\"【洋码头】\",\"MessageId\":\"150616164327822-18621651640-411273\",\"MessageSupply\":5}",null)}
            }, "test2", "liguo");

            Assert.IsTrue(true);
        }
        [TestMethod]
        public async Task Insert_Message_status()
        {
            await MessageHandleStatusAppService_Batch.Instance.SaveMessageStatusAsync(new MQMessageStatus("7b4d01f8cf9d47f5b03089f33a13082e", MessagePublishStatus.PushOk, "test2", "", null));
            Assert.IsTrue(true);
        }
        [TestMethod]
        public async Task WriteMongodbAsync()
        {
            IMessageStatusRepository statusRepo = new MessageStatusRepository();
            await statusRepo.TryAddAsync(new MQMessageStatus(Guid.NewGuid().ToString(), MessagePublishStatus.PushOk, "A", "test", null)
                ,"test001", "test1", TimeSpan.FromMilliseconds(300)); ;
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void WriteMongodb()
        {
            IMessageStatusRepository statusRepo = new MessageStatusRepository();
            var list = new List<MQMessageStatus>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new MQMessageStatus(Guid.NewGuid().ToString(), MessagePublishStatus.PushOk, "A", "test", null));
            }
            statusRepo.BatchAdd(list,null, "test1", "test");
            Assert.IsTrue(true);
        }        
        [TestMethod]
        public void UpdateMessageStatus_NotExists()
        {
            var list=new List<MessagePushStatus2>();
            list.Add(new MessagePushStatus2 { UuId = "135a052e84394b34a0cc557f1cba7a13", AppId = "trading", Code = "trading_postpay" });
            list.Add(new MessagePushStatus2 { UuId = "2fc0961beea6453fba274b2748268662", AppId = "trading", Code = "trading_postpay" });
            list.Add(new MessagePushStatus2 { UuId = "ae38ad6a8f3a49d68a9bb5e2692adb46", AppId = "trading", Code = "trading_postpay" });
            list.Add(new MessagePushStatus2 { UuId = "6e13bddbd712467c9533f68ac7f26b27", AppId = "trading", Code = "trading_postpay" });
            list.Add(new MessagePushStatus2 { UuId = "2e203c4afdd244e797b34499e889c079", AppId = "trading", Code = "trading_postpay" });
            var result = MessageAppService.TryUpdateMessagePushStatus(list);
            Assert.AreEqual(5, result.Count());
        }
        [TestMethod]
        public void UpdateMessageStatus_PartialExists()
        {
            var list = new List<MessagePushStatus2>();
            list.Add(new MessagePushStatus2 { UuId = "a2c6b5c9d0ec4471a8366cf7bf117bb7", AppId = "trading", Code = "trading_postpay" });
            list.Add(new MessagePushStatus2 { UuId = "0ae0594b2ff24ef1bb61e54cb34f0589", AppId = "trading", Code = "trading_postpay" });
            list.Add(new MessagePushStatus2 { UuId = "05563d07372940afa63618fa656e87d1", AppId = "trading", Code = "trading_postpay" });
            list.Add(new MessagePushStatus2 { UuId = "ddf8fe2f459e4d319830c56c5b2d811b", AppId = "trading", Code = "trading_postpay" });
            list.Add(new MessagePushStatus2 { UuId = "2e203c4afdd244e797b34499e889c079", AppId = "trading", Code = "trading_postpay" });
            var result = MessageAppService.TryUpdateMessagePushStatus(list);
            Assert.AreEqual(1, result.Count());
        }
        [TestMethod]
        public void Match_AwaitRetryMessage()
        {
            var query = RetryMessageSpecifications.Match_AwaitRetryMessage(TimeSpan.Parse("00:05:00"));
            Console.WriteLine(query);
        }

        [TestMethod]
        public void FindAllCollections()
        {
            IMessageRepository mesageRepository = new MQMessageRepository();
            var collections = mesageRepository.FindAllCollections("MQ_Message_trading_201608");
            collections.EachAction(c => Console.WriteLine(c));
        }
    }
}
