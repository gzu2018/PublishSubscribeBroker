using System;
using System.Collections.Generic;
using System.Linq;

namespace Broker
{
    public static class Broker
    {
        private static List<Topic> _topicList = new List<Topic>();
        private static Dictionary<int, Subscriber> _subscriberMap = new Dictionary<int, Subscriber>();

        private static int _subscriberCount = 0;

        public static bool AddSubscriberToTopic(int subscriberID, string topicName)
        {
            try
            {
                var subscriber = GetSubscriberById(subscriberID);
                var topic = GetTopicFromString(topicName);

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

                topic.RemoveSubscriber(subscriber);

                return true;
            }
            catch (ArgumentNullException exp)
            {
                Console.WriteLine($"[ERR] {exp.Message}");
                return false;
            }
        }

        public static bool SendMessage(string topicName, string messageContent)
        {
            try
            {
                var topic = GetTopicFromString(topicName);
                topic.SendMessageToSubscribers(messageContent);
                return true;
            }
            catch (ArgumentNullException exp)
            {
                Console.WriteLine($"[ERR] {exp.Message}");
                return false;
            }
        }

        public static bool AddTopic(string topicName)
        {
            if (DoesTopicNameExist(topicName))
                return false;

            var topic = new Topic(topicName);
            _topicList.Add(topic);
            return true;
        }

        public static bool RemoveTopic(string topicName)
        {
            return _topicList.RemoveAll(topic => topic.Name.Equals(topicName, StringComparison.InvariantCultureIgnoreCase)) != 0;
        }

        public static bool AddSubscriberToMap(Subscriber sub)
        {
            int assignedID = _subscriberCount++;
            _subscriberMap[assignedID] = sub;
            return true;
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

        public static string GetTopicNamesAsString()
        {
            return String.Join(", ", GetTopicNamesAsStringArray());
        }

        public static string GetSubscriberActiveTopicsAsString(int id)
        {
            return String.Join(", ", GetSubscriberActiveTopicsAsStringArray(id));
        }

        public static void RemoveInactiveSubscriberConnections()
        {
            foreach (var disconnectedSubscriber in _subscriberMap.Where(entry => !entry.Value.IsSocketStillConnected()).ToList())
            {
                RemoveSubscriberFromMap(disconnectedSubscriber.Key);
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
    }
}
