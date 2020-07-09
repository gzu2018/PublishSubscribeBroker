using Nancy;
using Newtonsoft.Json;

namespace Broker
{
    public class RestAPIServer : NancyModule
    {
        public RestAPIServer()
        {
            CreateTopicRequest();
            RemoveTopicRequest();
            GetAllTopicNamesRequest();
            PublisherPublishTopicRequest();
            SubscriberSubscribeToTopicRequest();
            SubscriberUnsubscribeToTopicRequest();
            GetSubscriberActiveTopicsRequest();
        }

        private void CreateTopicRequest()
        {
            Post("/createTopic", param =>
            {
                var topicName = GetQueryValueFromKey("topicName");
                if (topicName.Length > 0 && Broker.AddTopic(topicName))
                    return GetSuccessJSON($"Topic {topicName} successfully added");
                return GetFailureJSON($"Topic {topicName} could not be added");
            });
        }

        private void RemoveTopicRequest()
        {
            Delete("/deleteTopic", param =>
            {
                var topicName = GetQueryValueFromKey("topicName");
                if (topicName.Length > 0 && Broker.RemoveTopic(topicName))
                    return GetSuccessJSON($"Topic {topicName} successfully removed");
                return GetFailureJSON($"Topic {topicName} could not be removed");
            });
        }

        private void GetAllTopicNamesRequest()
        {
            Get("/getAllTopicNames", param => GetSuccessJSON(Broker.GetTopicNamesAsString()));
        }

        private void PublisherPublishTopicRequest()
        {
            Post("/publishMessage", param =>
            {
                var topicName = GetQueryValueFromKey("topicName");
                var messageContent = GetQueryValueFromKey("messageContent");

                if (topicName.Length > 0 && messageContent.Length > 0 && Broker.SendMessage(topicName, messageContent))
                    return GetSuccessJSON($"Message to topic {topicName} was successfully published");
                return GetFailureJSON($"Message to topic {topicName} could not be published");
            });
        }

        private void SubscriberSubscribeToTopicRequest()
        {
            Post("/subscribeToTopic", param =>
            {
                var subscriberIDString = GetQueryValueFromKey("id");
                var topicName = GetQueryValueFromKey("topicName");

                int subscriberID;
                if (int.TryParse(subscriberIDString, out subscriberID))
                {
                    if (subscriberID >= 0 && topicName.Length > 0 && Broker.AddSubscriberToTopic(subscriberID, topicName))
                    {
                        return GetSuccessJSON($"Subscriber {subscriberID} successfully added to topic {topicName}");
                    }
                }

                return GetFailureJSON($"Subscriber {subscriberIDString} was not added to the topic {topicName}. (Perhaps an invalid ID?)");
            });
        }

        private void SubscriberUnsubscribeToTopicRequest()
        {
            Delete("/unsubscribeToTopic", param =>
            {
                var subscriberIDString = GetQueryValueFromKey("id");
                var topicName = GetQueryValueFromKey("topicName");

                int subscriberID;
                if (int.TryParse(subscriberIDString, out subscriberID))
                {
                    if (subscriberID >= 0 && topicName.Length > 0 && Broker.RemoveSubscriberFromTopic(subscriberID, topicName))
                    {
                        return GetSuccessJSON($"Subscriber {subscriberID} successfully removed from topic {topicName}");
                    }
                }

                return GetFailureJSON($"Subscriber {subscriberIDString} was not removed from the topic {topicName}. (Perhaps an invalid ID?)");
            });
        }

        private void GetSubscriberActiveTopicsRequest()
        {
            Get("/getSubscriberActiveTopics", param =>
            {
                var subscriberIDString = GetQueryValueFromKey("id");

                int subscriberID;
                if (int.TryParse(subscriberIDString, out subscriberID))
                {
                    return GetSuccessJSON(Broker.GetSubscriberActiveTopicsAsString(subscriberID));
                }

                return GetFailureJSON("Subscriber ID entered is not a valid integer");
            });
        }

        private string GetSuccessJSON(string successMsg)
        {
            var result = new { success = "true", message=successMsg};
            return JsonConvert.SerializeObject(result);
        }

        private string GetFailureJSON(string errorMsg)
        {
            var result = new { success = "false", message=errorMsg };
            return JsonConvert.SerializeObject(result);
        }

        private string GetQueryValueFromKey(string key)
        {
            return (string)this.Request.Query[key];
        }
    }
}
