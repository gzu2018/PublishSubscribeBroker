using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Broker
{
    public static class Broker
    {
        private static List<Topic> _topicList = new List<Topic>();
        private static Dictionary<int, Subscriber> _subscriberMap = new Dictionary<int, Subscriber>();

        private static int _publisherCount = 0;
        private static int _subscriberCount = 0;

        public static bool AddSubscriberToTopic(int subscriberID, string topicName)
        {
            try
            {
                var subscriber = GetSubscriberById(subscriberID);
                var topic = GetTopicFromString(topicName);

                if (topic.ContainsSubscriber(subscriber))
                    return false;

                topic.AddSubscriber(subscriber);

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

                if (!topic.ContainsSubscriber(subscriber))
                    return false;

                topic.RemoveSubscriber(subscriber);

                return true;
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

                if (topic.ID != publisherID)
                    return false;

                topic.SendMessageToSubscribers(messageContent);
                return true;
            }
            catch (ArgumentNullException exp)
            {
                Console.WriteLine($"[ERR] {exp.Message}");
                return false;
            }
        }

        public static bool AddTopic(int publisherID, string topicName)
        {
            if (DoesTopicNameExist(topicName) || publisherID < 0 || publisherID >= _publisherCount)
                return false;

            var topic = new Topic(publisherID, topicName);
            _topicList.Add(topic);
            return true;
        }

        public static bool RemoveTopic(int publisherID, string topicName)
        {
            return _topicList.RemoveAll(topic => topic.Name.Equals(topicName, StringComparison.InvariantCultureIgnoreCase) && topic.ID == publisherID) > 0;
        }

        public static int AddNewPublisher()
        {
            return _publisherCount++;
        }

        public static int AddSubscriberToMap(Subscriber sub)
        {
            var assignedID = _subscriberCount;
            _subscriberMap[assignedID] = sub;
            _subscriberCount++;
            return assignedID;
        }

        public static bool RemoveSubscriberFromMap(int id)
        {
            try
            {
                _subscriberMap.Remove(id);
                return true;
            }
            catch (ArgumentNullException exp)
            {
                Console.WriteLine($"[ERR] {exp.Message}");
                return false;
            }
        }
        
        public static string[] GetTopicNamesAsStringArray()
        {
            var topicNames = new string[_topicList.Count];
            var count = 0;
            _topicList.ForEach(topic => topicNames[count++] = topic.Name);
            return topicNames;
        }

        public static string[] GetPublisherActiveTopicsAsStringArray(int id)
        {
            if(id < 0 || id >= _publisherCount)
                return new string[0];

            var publisherTopics = new List<String>();

            foreach (var topic in _topicList)
            {
                if(topic.ID == id)
                    publisherTopics.Add(topic.Name);
            }

            return publisherTopics.ToArray();
        }

        public static string[] GetSubscriberActiveTopicsAsStringArray(int id)
        {
            try
            {
                var subscriber = GetSubscriberById(id);
                var subscribedTopics = new List<String>();

                foreach (var topic in _topicList)
                {
                    if (topic.ContainsSubscriber(subscriber))
                        subscribedTopics.Add(topic.Name);
                }

                return subscribedTopics.ToArray();
            }
            catch (ArgumentNullException exp)
            {
                Console.WriteLine($"[ERR] {exp.Message}");
                return new string[0];
            }
        }

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

        private static Topic GetTopicFromString(string topicName)
        {
            foreach (var topic in _topicList)
            {
                if (topic.Name == topicName)
                    return topic;
            }

            throw new ArgumentNullException("No Topic Found");
        }

        private static Subscriber GetSubscriberById(int id)
        {
            if (_subscriberMap.ContainsKey(id))
                return _subscriberMap[id];
            throw new ArgumentNullException("No Subscriber found");
        }

        private static bool DoesTopicNameExist(string topicName)
        {
            try
            {
                GetTopicFromString(topicName);
                return true;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        private static int RemoveInactiveSubscriberConnections()
        {
            var count = 0;
            foreach (var disconnectedSubscriber in _subscriberMap.Where(entry => !entry.Value.IsSocketStillConnected()).ToList())
            {
                RemoveSubscriberFromMap(disconnectedSubscriber.Key);
                count++;
            }

            return count;
        }
    }
}
