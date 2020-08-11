using System.Collections.Generic;

namespace Broker
{
    public class Topic
    {
        public string Name { get; }
        public int ID { get; }
				public List<Subscriber> SubscriberList { get; } = new List<Subscriber>();

				public Topic(int ownerID, string topicName)
        {
            Name = topicName;
            ID = ownerID;
				}

				// Unless there's a specific reason you want the list to be private, making it public readonly would be better in this case since these methods are redundant.
				//	You're not doing any validation or anything that would warrant creating these extra methods. You're literally just creating methods that do what the list can already do itself.

				//public void AddSubscriber( Subscriber sub ) => SubscriberList.Add(sub);

				//public bool RemoveSubscriber( Subscriber sub ) => SubscriberList.Remove(sub);

				//public bool ContainsSubscriber( Subscriber sub ) => SubscriberList.Contains(sub);

				public bool SendMessageToSubscribers(string messageContent)
        {
						SubscriberList.ForEach(sub => sub.SendMessage($"[{Name}]: {messageContent}")); // I personally prefer the LINQ methods. They're a bit cleaner
            return true;
            //catch (SocketException exception)		// You'll never hit this catch block because you're handling the exception in your SendMessage method
            //{
            //    Console.WriteLine($"[ERR] {exception.Message}");
            //    return false;
            //}
        }
    }
}
