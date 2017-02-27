using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using RabbitMQ.Client;
using YmatouMQ.Common;
using YmatouMQ.Common.Extensions._Task;
using YmatouMQ.Common.MessageHandleContract;
using YmatouMQ.MessageScheduler;

namespace SubscribeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("依次输入qName,qos,slim");
            var par = Console.ReadLine().Split(new char[] { ',' }); ;
            var qname = par[0];
            var qos =ushort.Parse( par[1]);
            var slim = Convert.ToInt32(par[2]);
            var semaphoreSlim = new SemaphoreSlim(slim);
            const string EXCHANGE_NAME = "trading_postpay";
            var factory = new ConnectionFactory();
            factory.HostName = "rmqnode1";
            factory.Port = 800;
            using (var connection = factory.CreateConnection())
            {
                Console.WriteLine("conn success");               
                using (IModel channel = connection.CreateModel())
                {
                    Console.WriteLine("CreateModel success");
                    channel.BasicAcks += channel_BasicAcks;
                    channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Topic, true, false, null);

                    string queueName = "trading_postpay";

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (o, e) =>
                    {
                        var mid = Encoding.ASCII.GetString((byte[]) e.BasicProperties.Headers["msgid"]);
                        string data = Encoding.ASCII.GetString(e.Body);
                        Console.WriteLine("body:{0},id:{1}", data, mid);
                        if (mid == "mitest_A10_0")
                        {
                            Ack(mid, o, e);                            
                        }                        
                        //CallbackClient(e.Body, e.BasicProperties.Headers["uuid"].ToString(), semaphoreSlim);
                    };
                    if (qos > 0)
                        channel.BasicQos(0, qos, false);
                 
                    channel.QueueBind(queueName, EXCHANGE_NAME, "#.#");
                    string consumerTag = channel.BasicConsume(queueName, false,
                       "test_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), consumer);
                    Console.WriteLine("Listening press ENTER to quit");
                    Console.ReadLine();

                    channel.QueueUnbind(queueName, EXCHANGE_NAME, "#.#", null);
                   
                }
            }


            int workTh, ioTh;
            ThreadPool.GetAvailableThreads(out workTh, out ioTh);
            Console.WriteLine("work th  {0},io th {1}", workTh, ioTh);



            Console.Read();
        }

        static void channel_BasicAcks(object sender, BasicAckEventArgs e)
        {
            Console.WriteLine("ack {0},{1}", e.DeliveryTag,sender.ToString());
        }

        private static void Ack(string mid,object o, BasicDeliverEventArgs e)
        {
            (o as EventingBasicConsumer).Model.BasicAck(e.DeliveryTag, false);
            Console.WriteLine("id {0} ack success",mid);
        }

       

        private static async Task CallbackClient(byte[] messageBytes, string messageid,SemaphoreSlim slim)
        {
            await slim.WaitAsync().ConfigureAwait(false);
            //模拟URL请求
            var context = new MessageHandleContext<byte[]>(
                messageBytes, false, "trading", "trading_postpay", messageid, "mq");
            context.SetCallback("http://192.168.1.247:7770/OrderHandle/", 3000, "application/json", "POST");
            try
            {
                var result2 =
                    await
                        new MessageHandlerScheduler().TryRequestClientHandleMessage(context,
                            new YmatouMQNet4.Configuration.CallbackConfiguration
                            {
                                Url = "http://192.168.1.247:866/api/Values"
                            }).ConfigureAwait(false);
                Console.WriteLine("index {0},task id {1},thread id {2},add pool {3},ioth {4},workth {5},r {6}"
                    , 0
                    , Task.CurrentId
                    , Thread.CurrentThread.ManagedThreadId
                    , DateTime.Now
                    , 0
                    , 0
                    , result2.Result);
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                slim.Release();
            }
        }
    }
}
