using Nancy;
using Newtonsoft.Json;

namespace Broker
{
    public class RestAPIServer : NancyModule
    {
        public RestAPIServer()
        {
            EnableCORS();
            CreateTopicRequest();
            RemoveTopicRequest();
            GetAllTopicNamesRequest();
            PublisherPublishTopicRequest();
            SubscriberSubscribeToTopicRequest();
            SubscriberUnsubscribeToTopicRequest();
            GetSubscriberActiveTopicsRequest();
        }

        private void EnableCORS()
        {
            After.AddItemToEndOfPipeline((ctx) =>
            {
                ctx.Response.WithHeader("Access-Control-Allow-Origin", "*") // Change with implementation
                    .WithHeader("Access-Control-Allow-Methods", "POST,GET,DELETE")
                    .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type, X-Requested-With")
                    .WithHeader("Access-Control-Allow-Credentials", "true");
            });
        }

        private void CreateTopicRequest()
        {
            Post("/createTopic", param =>
            {
                var topicName = GetQueryValueFromKey("topicName");
                if (topicName.Length > 0 && Broker.AddTopic(topicName))
                    return GetSuccessJSONMessage($"Topic {topicName} successfully added");
                return GetFailureJSONMessage($"Topic {topicName} could not be added");
            });
        }

        private void RemoveTopicRequest()
        {
            Post("/deleteTopic", param =>
            {
                var topicName = GetQueryValueFromKey("topicName");
                if (topicName.Length > 0 && Broker.RemoveTopic(topicName))
                    return GetSuccessJSONMessage($"Topic {topicName} successfully removed");
                return GetFailureJSONMessage($"Topic {topicName} could not be removed");
            });
        }

        private void GetAllTopicNamesRequest()
        {
            Get("/getAllTopicNames", param => GetSuccessJSONArray(Broker.GetTopicNamesAsStringArray()));
        }

        private void PublisherPublishTopicRequest()
        {
            Post("/publishMessage", param =>
            {
                var topicName = GetQueryValueFromKey("topicName");
                var messageContent = GetQueryValueFromKey("messageContent");

                if (topicName.Length > 0 && messageContent.Length > 0 && Broker.SendMessage(topicName, messageContent))
                    return GetSuccessJSONMessage($"Message to topic {topicName} was successfully published");
                return GetFailureJSONMessage($"Message to topic {topicName} could not be published");
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
                        return GetSuccessJSONMessage($"Subscriber {subscriberID} successfully added to topic {topicName}");
                    }
                }

                return GetFailureJSONMessage($"Subscriber {subscriberIDString} was not added to the topic {topicName}. (Perhaps an invalid ID?)");
            });
        }

        private void SubscriberUnsubscribeToTopicRequest()
        {
            Post("/unsubscribeToTopic", param =>
            {
                var subscriberIDString = GetQueryValueFromKey("id");
                var topicName = GetQueryValueFromKey("topicName");

                int subscriberID;
                if (int.TryParse(subscriberIDString, out subscriberID))
                {
                    if (subscriberID >= 0 && topicName.Length > 0 && Broker.RemoveSubscriberFromTopic(subscriberID, topicName))
                    {
                        return GetSuccessJSONMessage($"Subscriber {subscriberID} successfully removed from topic {topicName}");
                    }
                }

                return GetFailureJSONMessage($"Subscriber {subscriberIDString} was not removed from the topic {topicName}. (Perhaps an invalid ID?)");
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
                    return GetSuccessJSONArray(Broker.GetSubscriberActiveTopicsAsStringArray(subscriberID));
                }

                return GetFailureJSONMessage("Subscriber ID entered is not a valid integer");
            });
        }

        private string GetSuccessJSONMessage(string successMsg)
        {
            var result = new { success = true, message=successMsg};
            return JsonConvert.SerializeObject(result);
        }

        private string GetFailureJSONMessage(string errorMsg)
        {
            var result = new { success = false, message=errorMsg };
            return JsonConvert.SerializeObject(result);
        }

        private string GetSuccessJSONArray(string[] array)
        {
            var result = new { success = true, message = array };
            return JsonConvert.SerializeObject(result);
        }

        private string GetQueryValueFromKey(string key)
        {
            return (string)this.Request.Query[key];
        }
    }
}
