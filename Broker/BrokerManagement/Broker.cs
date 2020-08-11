using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Broker
{
    public static class Broker
    {
        private static readonly List<Topic> _topicList = new List<Topic>();
        private static readonly Dictionary<int, Subscriber> _subscriberMap = new Dictionary<int, Subscriber>();

        private static int _publisherCount = 0;
        private static int _subscriberCount = 0;

        public static bool AddSubscriberToTopic(int subscriberID, string topicName)
        {
            try
            {
                var subscriber = GetSubscriberById(subscriberID);
                var topic = GetTopicFromString(topicName);

								// If you converted the list to a set, then you could just add the new item without checking if it already contained it, as a set won't allow duplicate items
                if (topic.SubscriberList.Contains(subscriber)) return false;

                topic.SubscriberList.Add(subscriber);

                return true;
            }
            catch (ArgumentNullException exp)
            {
                Console.WriteLine($"[ERR] {exp.Message}");
                return false;
            }
        }

        public static bool RemoveSubscriberFromTopic(int subscriberID, string topicName)
        {
            try
            {
								var subscriber = GetSubscriberById(subscriberID);
                var topic = GetTopicFromString(topicName);

                return topic.SubscriberList.Remove(subscriber);	// list.Remove will return false if the item does not exist in the list, so you don't need to add the manual check to see if the item is in the list

								// You could one-line this try section if you wanted
								//return GetTopicFromString(topicName).RemoveSubscriber(GetSubscriberById(subscriberID));

						}
            catch (ArgumentNullException exp)
            {
                Console.WriteLine($"[ERR] {exp.Message}");
                return false;
            }
        }

        public static bool SendMessage(int publisherID, string topicName, string messageContent)
        {
            try
            {
                var topic = GetTopicFromString(topicName);

                if (topic.ID != publisherID) return false;

                return topic.SendMessageToSubscribers(messageContent); // You weren't handling this SendMessage... method returning false because of a socket exception
            }
            catch (ArgumentNullException exp)
            {
                Console.WriteLine($"[ERR] {exp.Message}");
                return false;
            }
        }

        public static bool AddTopic(int publisherID, string topicName)
        {
            if (DoesTopicNameExist(topicName) || publisherID < 0 || publisherID >= _publisherCount) return false;

            _topicList.Add(new Topic(publisherID, topicName));
            return true;
        }

				public static bool RemoveTopic( int publisherID, string topicName ) => _topicList.RemoveAll(topic => topic.Name.Equals(topicName, StringComparison.InvariantCultureIgnoreCase) && topic.ID == publisherID) > 0;

				public static int AddNewPublisher() => _publisherCount++;

				public static int AddSubscriberToMap(Subscriber sub)
        {
            _subscriberMap[_subscriberCount] = sub;
            return _subscriberCount++;
        }
        
				// This can be one-lined. A lot of the IEnumerables can be converted into other IEnumberables
        public static string[] GetTopicNamesAsStringArray() => _topicList.Select(topic => topic.Name).ToArray();

				// LINQ methods are very useful for creating new IEnumerables from others (e.g. converting a list to an array)
        public static string[] GetPublisherActiveTopicsAsStringArray(int id) => (id < 0 || id >= _publisherCount) ? new string[0] : _topicList.Where(topic => topic.ID == id).Select(topic => topic.Name).ToArray();

				// int.TryParse would return false, so even if id was null, it wouldn't make it to this method
        public static string[] GetSubscriberActiveTopicsAsStringArray(int id) => _topicList.Where(topic => topic.SubscriberList.Contains(GetSubscriberById(id))).Select(topic => topic.Name).ToArray();

        public static bool RemoveAllTopicsOwnedBySubscriber(int id)
        {
						_topicList.RemoveAll(topic => topic.ID == id);
            return true;
        }

				// Why is this method async if you're not going to await anything? The async portion will run, but the program execution won't wait until it's finished if you don't await it
        public static async Task PeriodicallyRemoveInactiveSubscribersTask(int delayInSeconds, bool writeToConsole)
        {
            while (true)
            {
                Task.Run(() =>
                {
                    var removedConnectionCount = RemoveInactiveSubscriberConnections();
                    if(writeToConsole)
                        Console.WriteLine($"{removedConnectionCount} inactive subscribers have been removed");
                });
                await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
            }
        }

				// list.First will either return the first instance found, or throw an exception if it doesn't find one. You could also use FirstOrDefault, which would return the default value of the type if one isn't in the list
        private static Topic GetTopicFromString(string topicName) => _topicList.First(topic => topic.Name == topicName);

				// ArgumentNullException is thrown when the argument is null, not when you didn't find an item in your collection. If you try accessing the map directly like this, it will throw it's own exception.
        private static Subscriber GetSubscriberById(int id) => _subscriberMap[id];

        private static bool DoesTopicNameExist(string topicName)
        {
            try
            {
                GetTopicFromString(topicName);
                return true;
            }
            catch (InvalidOperationException)	// This is the exception thrown by the .First() method if no item matches the predicate, or if the list is empty
            {
                return false;
            }
        }

        private static int RemoveInactiveSubscriberConnections()
        {
            var count = 0;
            _subscriberMap.Where(entry => !entry.Value.IsSocketStillConnected()).ToList().ForEach(disconnectedSubscriber =>	// I prefer the LINQ methods like this, but that's just personal preference
						{
                _subscriberMap.Remove(disconnectedSubscriber.Key); // You're method to remove this was redundant and unnecessary. You already have access to the map and its methods. You don't need to recreate them to use them.
                count++;
            });

            return count;
        }
    }
}
