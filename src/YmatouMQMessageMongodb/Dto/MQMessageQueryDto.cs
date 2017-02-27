using System;

namespace YmatouMQMessageMongodb.Dto
{
    public class MQMessageQueryDto
    {
        public string AppId { get; set; }
        public string Message { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }
        public bool Status { get; set; }
    }
}