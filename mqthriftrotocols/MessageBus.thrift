namespace * MessageProtocols

struct MessageDto {
  1: string AppId;
  2: string code;
  3: string Ip;
  4: string MsgUniqueId;
  5: binary Body;
}

service MQMessageBus {
  /**
   * Prints test bus application.
   * @param string val
   */
   string testString(1: string val),

  /**
   * Prints publish message to rabbitmq bus.
   * @param MessageDto dto
   */
   Response publish(1: MessageDto dto)
}

struct Response {
  1: string message,
  2: i32 code
}