using Nancy;
using Newtonsoft.Json;

namespace Broker
{
    public class RestApiServer : NancyModule
    {
        public RestApiServer()
        {
            EnableCors();
            InitializePublisherRequest();
            CreateTopicRequest();
            RemoveTopicRequest();
            GetAllTopicNamesRequest();
            PublisherPublishTopicRequest();
            SubscriberSubscribeToTopicRequest();
            SubscriberUnsubscribeToTopicRequest();
            GetPublisherActiveTopicsRequest();
            GetSubscriberActiveTopicsRequest();
            RemoveAllSubscriberTopicsByIDRequest();
        }

				// Most of the things I noticed in this file were that you can use the expression body instead of having to do a full method body. Also, some of the methods were able to be simplified using the ternary operator ?:

				private void EnableCors() => After.AddItemToEndOfPipeline(ctx => 
						ctx.Response.WithHeader("Access-Control-Allow-Origin", "*") // Set to allow all, change with implementation
								.WithHeader("Access-Control-Allow-Methods", "POST,GET,DELETE")
								.WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type, X-Requested-With")
								.WithHeader("Access-Control-Allow-Credentials", "true"));

				private void InitializePublisherRequest() => Get("/initializePublisher", param => GetSuccessJSONInt(Broker.AddNewPublisher()));

				private void CreateTopicRequest() => Post("/createTopic", param => {
						var topicName = GetQueryValueFromKey("topicName");

						return (int.TryParse(GetQueryValueFromKey("id"), out var publisherID) && topicName.Length > 0 && Broker.AddTopic(publisherID, topicName))
								? GetSuccessJSONMessage($"Topic {topicName} successfully added")
								: GetFailureJSONMessage($"Topic {topicName} could not be added");
				});

				private void RemoveTopicRequest() => Post("/deleteTopic", param => {
						var topicName = GetQueryValueFromKey("topicName");

						return (int.TryParse(GetQueryValueFromKey("id"), out var publisherID) && topicName.Length > 0 && Broker.RemoveTopic(publisherID, topicName))
								? GetSuccessJSONMessage($"Topic {topicName} successfully removed")
								: GetFailureJSONMessage($"Topic {topicName} could not be removed");
				});

				private void GetAllTopicNamesRequest() => Get("/getAllTopicNames", param => GetSuccessJSONArray(Broker.GetTopicNamesAsStringArray()));

				private void PublisherPublishTopicRequest() => Post("/publishMessage", param => {
						var topicName = GetQueryValueFromKey("topicName");
						var messageContent = GetQueryValueFromKey("messageContent");

						return (int.TryParse(GetQueryValueFromKey("id"), out var publisherID) && topicName.Length > 0 && messageContent.Length > 0 && Broker.SendMessage(publisherID, topicName, messageContent))
								? GetSuccessJSONMessage($"Message to topic {topicName} was successfully published")
								: GetFailureJSONMessage($"Message to topic {topicName} could not be published");
				});

				private void SubscriberSubscribeToTopicRequest() => Post("/subscribeToTopic", param => {
						var subscriberIDString = GetQueryValueFromKey("id");
						var topicName = GetQueryValueFromKey("topicName");

						return (int.TryParse(subscriberIDString, out var subscriberID) && subscriberID >= 0 && topicName.Length > 0 && Broker.AddSubscriberToTopic(subscriberID, topicName))
								? GetSuccessJSONMessage($"Subscriber {subscriberID} successfully added to topic {topicName}")
								: GetFailureJSONMessage($"Subscriber {subscriberIDString} was not added to the topic {topicName}. (Perhaps an invalid ID?)");
				});

				private void SubscriberUnsubscribeToTopicRequest() => Post("/unsubscribeToTopic", param => {
						var subscriberIDString = GetQueryValueFromKey("id");
						var topicName = GetQueryValueFromKey("topicName");

						return (int.TryParse(subscriberIDString, out var subscriberID) && subscriberID >= 0 && topicName.Length > 0 && Broker.RemoveSubscriberFromTopic(subscriberID, topicName))
								? GetSuccessJSONMessage($"Subscriber {subscriberID} successfully removed from topic {topicName}")
								: GetFailureJSONMessage($"Subscriber {subscriberIDString} was not removed from the topic {topicName}. (Perhaps an invalid ID?)");
				});

				private void GetPublisherActiveTopicsRequest() => Get("/getPublisherActiveTopics", param =>
						int.TryParse(GetQueryValueFromKey("id"), out var publisherID)
								? GetSuccessJSONArray(Broker.GetPublisherActiveTopicsAsStringArray(publisherID))
								: GetFailureJSONMessage("Publisher ID entered is not a valid integer"));

				private void GetSubscriberActiveTopicsRequest() => Get("/getSubscriberActiveTopics", param =>
						int.TryParse(GetQueryValueFromKey("id"), out var subscriberID)
								? GetSuccessJSONArray(Broker.GetSubscriberActiveTopicsAsStringArray(subscriberID))
								: GetFailureJSONMessage("Subscriber ID entered is not a valid integer"));

				private void RemoveAllSubscriberTopicsByIDRequest() => Post("/removeAllSubscriberActiveTopics", param =>
						int.TryParse(GetQueryValueFromKey("id"), out var publisherID)	// shouldn't this variable be subscriberID?
								? Broker.RemoveAllTopicsOwnedBySubscriber(publisherID)	// this method will ALWAYS return true
										? GetSuccessJSONMessage($"All topics owned by publisher {publisherID} successfully removed")
										: GetFailureJSONMessage($"Topics owned by publisher {publisherID} could not be removed. (Perhaps invalid ID?)")	// You'll never get here
								: GetFailureJSONMessage("Publisher ID entered is not a valid integer"));

				private string GetSuccessJSONMessage(string successMsg) => GetSerializedJsonString(true, successMsg);

        private string GetFailureJSONMessage(string errorMsg) => GetSerializedJsonString(false, errorMsg);

        private string GetSuccessJSONArray(string[] array) => GetSerializedJsonString(true, array);

				private string GetSuccessJSONInt( int integer ) => GetSerializedJsonString(true, integer);

				// This method can act as a factory of sorts that creates your serialized strings. Now you can add multiple success and failure methods without having to re-write the logic to create the serialized string
				private string GetSerializedJsonString(bool success, dynamic message) => JsonConvert.SerializeObject(new { success, message });

				private string GetQueryValueFromKey( string key ) => (string)Request.Query[key];
		}
}
