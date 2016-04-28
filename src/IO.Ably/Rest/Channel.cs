using System;
using System.Collections.Generic;
using IO.Ably.Encryption;
using System.Threading.Tasks;

namespace IO.Ably.Rest
{
    /// <summary>
    /// The Ably Realtime service organises the traffic within any application into named channels.
    /// Channels are the "unit" of message distribution; clients attach to channels to subscribe to messages,
    /// and every message broadcast by the service is associated with a unique channel.
    /// A channel cannot be instantiated but needs to be created using the AblyRest.Channels.Get("channelname")
    /// </summary>
    public class RestChannel : IChannel
    {
        public string Name { get; private set; }
        private readonly AblyRest _ablyRest;
        public ChannelOptions Options { get; private set; }
        private readonly string _basePath;

        internal RestChannel(AblyRest ablyRest, string name,  ChannelOptions options)
        {
            Name = name;
            _ablyRest = ablyRest;
            SetOptions(options);
            _basePath = $"/channels/{name.EncodeUriPart()}";
        }

        internal void SetOptions(ChannelOptions options)
        {
            if (options == null)
            {
                Options = new ChannelOptions(false);
                return;
            }

            Options = new ChannelOptions(options.Encrypted, options.CipherParams);
        }

        /// <summary>
        /// Publish a message to the channel
        /// </summary>
        /// <param name="name">The event name of the message to publish</param>
        /// <param name="data">The message payload. Allowed payloads are string, objects and byte[]</param>
        public Task Publish(string name, object data)
        {
            var request = _ablyRest.CreatePostRequest(_basePath + "/messages", Options);

            request.PostData = new List<Message> { new Message(name, data)};
            return _ablyRest.ExecuteRequest(request);
        }

        public Task Publish(Message message)
        {
            return this.Publish(new[] {message});
        }

        /// <summary>
        /// Publish a list of messages to the channel
        /// </summary>
        /// <param name="messages">a list of messages</param>
        public Task Publish(IEnumerable<Message> messages)
        {
            var request = _ablyRest.CreatePostRequest(_basePath + "/messages", Options);
            request.PostData = messages;
            return _ablyRest.ExecuteRequest(request);
        }

        /// <summary>
        /// Obtain the set of members currently present for a channel
        /// </summary>
        /// <returns><see cref="PaginatedResource{T}"/> of the PresenseMessages</returns>
        public Task<PaginatedResource<PresenceMessage>> Presence()
        {
            var request = _ablyRest.CreateGetRequest(_basePath + "/presence", Options);
            return _ablyRest.ExecuteRequest<PaginatedResource<PresenceMessage>>(request);
        }

        /// <summary>
        /// Get the presence messages history for the channel
        /// </summary>
        /// <returns><see cref="PaginatedResource{PresenceMessage}"/></returns>
        public Task<PaginatedResource<PresenceMessage>> PresenceHistory()
        {
            var request = _ablyRest.CreateGetRequest(_basePath + "/presence", Options);
            return _ablyRest.ExecuteRequest<PaginatedResource<PresenceMessage>>(request);
        }

        /// <summary>
        /// Get the presence messages history for the channel by specifying a query
        /// </summary>
        /// <returns><see cref="PaginatedResource{PresenceMessage}"/></returns>
        public Task<PaginatedResource<PresenceMessage>> PresenceHistory(DataRequestQuery query)
        {
            var request = _ablyRest.CreateGetRequest(_basePath + "/presence", Options);
            request.AddQueryParameters(query.GetParameters());
            return _ablyRest.ExecuteRequest<PaginatedResource<PresenceMessage>>(request);
        }

        /// <summary>
        /// Return the message history of the channel
        /// </summary>
        /// <returns><see cref="PaginatedResource{T}"/> of Messages</returns>
        public Task<PaginatedResource<Message>> History()
        {
            return History(new DataRequestQuery());
        }

        /// <summary>
        /// Return the message history of the channel
        /// </summary>
        /// <param name="dataQuery"><see cref="DataRequestQuery"/></param>
        /// <returns><see cref="PaginatedResource{T}"/> of Messages</returns>
        public Task<PaginatedResource<Message>> History(DataRequestQuery dataQuery)
        {
            var query = dataQuery ?? new DataRequestQuery();

            query.Validate();

            var request = _ablyRest.CreateGetRequest(_basePath + "/messages", Options);

            request.AddQueryParameters(query.GetParameters());

            return _ablyRest.ExecuteRequest<PaginatedResource<Message>>(request);
        }
    }
}