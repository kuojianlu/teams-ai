﻿
namespace Microsoft.Bot.Builder.M365.AI.Moderator
{
    public class OpenAIModeratorOptions
    {
        /// <summary>
        /// OpenAI API key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Which parts of the conversation to moderate.
        /// </summary>
        public ModerationType Moderate { get; set; }

        /// <summary>
        /// Optional. OpenAI organization.
        /// </summary>
        public string? Organization { get; set; }

        /// <summary>
        /// Optional. OpenAI endpoint.
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// The moderation model to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Create an instance of the OpenAIModeratorOptions class.
        /// </summary>
        /// <param name="apiKey">OpenAI API key.</param>
        /// <param name="moderate">Which parts of the conversation to moderate.</param>
        public OpenAIModeratorOptions(string apiKey, ModerationType moderate)
        {
            ApiKey = apiKey;
            Moderate = moderate;
        }
    }

    public enum ModerationType
    {
        Input,
        Output,
        Both
    };
}
