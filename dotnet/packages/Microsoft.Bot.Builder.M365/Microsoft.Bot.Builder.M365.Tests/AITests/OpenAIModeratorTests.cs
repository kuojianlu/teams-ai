﻿using Castle.Core.Logging;
using Microsoft.Bot.Builder.M365.AI.Action;
using Microsoft.Bot.Builder.M365.AI;
using Microsoft.Bot.Builder.M365.AI.Moderator;
using Microsoft.Bot.Builder.M365.AI.Planner;
using Microsoft.Bot.Builder.M365.AI.Prompt;
using Microsoft.Bot.Builder.M365.Exceptions;
using Microsoft.Bot.Builder.M365.OpenAI;
using Microsoft.Bot.Schema;
using Moq;
using System.Reflection;

namespace Microsoft.Bot.Builder.M365.Tests.AITests
{
    public class OpenAIModeratorTests
    {
        [Fact]
        public async void Test_ReviewPrompt_ThrowsException()
        {
            // Arrange
            var apiKey = "randomApiKey";

            var botAdapterMock = new Mock<BotAdapter>();
            // TODO: when TurnState is implemented, get the user input
            var activity = new Activity()
            {
                Text = "input",
            };
            var turnContext = new TurnContext(botAdapterMock.Object, activity);
            var turnStateMock = new Mock<TurnState>();
            var promptTemplate = new PromptTemplate(
                "prompt",
                new PromptTemplateConfiguration
                {
                    Completion =
                    {
                        MaxTokens = 2000,
                        Temperature = 0.2,
                        TopP = 0.5,
                    }
                }
            );

            var clientMock = new Mock<OpenAIClient>(It.IsAny<OpenAIClientOptions>(), It.IsAny<ILogger>(), It.IsAny<HttpClient>());
            var exception = new OpenAIClientException("Exception Message");
            clientMock.Setup(client => client.ExecuteTextModeration(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(exception);

            var options = new OpenAIModeratorOptions(apiKey, ModerationType.Both);
            var moderator = new OpenAIModerator<TurnState>(options);
            moderator.GetType().GetField("_client", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(moderator, clientMock.Object);

            // Act
            var result = await Assert.ThrowsAsync<OpenAIClientException>(async () => await moderator.ReviewPrompt(turnContext, turnStateMock.Object, promptTemplate));

            // Assert
            Assert.Equal("Exception Message", result.Message);
        }

        [Theory]
        [InlineData(ModerationType.Input)]
        [InlineData(ModerationType.Output)]
        [InlineData(ModerationType.Both)]
        public async void Test_ReviewPrompt_Flagged(ModerationType moderate)
        {
            // Arrange
            var apiKey = "randomApiKey";

            var botAdapterMock = new Mock<BotAdapter>();
            // TODO: when TurnState is implemented, get the user input
            var activity = new Activity()
            {
                Text = "input",
            };
            var turnContext = new TurnContext(botAdapterMock.Object, activity);
            var turnStateMock = new Mock<TurnState>();
            var promptTemplate = new PromptTemplate(
                "prompt",
                new PromptTemplateConfiguration
                {
                    Completion =
                    {
                        MaxTokens = 2000,
                        Temperature = 0.2,
                        TopP = 0.5,
                    }
                }
            );

            var clientMock = new Mock<OpenAIClient>(It.IsAny<OpenAIClientOptions>(), It.IsAny<ILogger>(), It.IsAny<HttpClient>());
            var response = new ModerationResponse()
            {
                Id = "Id",
                Model = "Model",
                Results = new List<ModerationResult>()
                {
                    new ModerationResult()
                    {
                        Flagged = true,
                        CategoriesFlagged = new ModerationCategoriesFlagged()
                        {
                            Hate = false,
                            HateThreatening = false,
                            SelfHarm = false,
                            Sexual = false,
                            SexualMinors = false,
                            Violence = true,
                            ViolenceGraphic = false,
                        },
                        CategoryScores = new ModerationCategoryScores()
                        {
                            Hate = 0,
                            HateThreatening = 0,
                            SelfHarm = 0,
                            Sexual = 0,
                            SexualMinors = 0,
                            Violence = 0.9,
                            ViolenceGraphic = 0,
                        }
                    }
                }
            };
            clientMock.Setup(client => client.ExecuteTextModeration(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response);

            var options = new OpenAIModeratorOptions(apiKey, moderate);
            var moderator = new OpenAIModerator<TurnState>(options);
            moderator.GetType().GetField("_client", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(moderator, clientMock.Object);

            // Act
            var result = await moderator.ReviewPrompt(turnContext, turnStateMock.Object, promptTemplate);

            // Assert
            if (moderate == ModerationType.Input || moderate == ModerationType.Both)
            {
                Assert.NotNull(result);
                Assert.Equal(AITypes.DoCommand, result.Commands[0].Type);
                Assert.Equal(DefaultActionTypes.FlaggedInputActionName, ((PredictedDoCommand)result.Commands[0]).Action);
                Assert.NotNull(((PredictedDoCommand)result.Commands[0]).Entities);
                Assert.True(((PredictedDoCommand)result.Commands[0]).Entities.ContainsKey("Result"));
                Assert.StrictEqual(response.Results[0], ((PredictedDoCommand)result.Commands[0]).Entities.GetValueOrDefault("Result"));
            }
            else
            {
                Assert.Null(result);
            }
        }

        [Fact]
        public async void Test_ReviewPlan_ThrowsException()
        {
            // Arrange
            var apiKey = "randomApiKey";

            var turnContextMock = new Mock<ITurnContext>();
            var turnStateMock = new Mock<TurnState>();
            var plan = new Plan(new List<IPredictedCommand>()
            {
                new PredictedDoCommand("action"),
                new PredictedSayCommand("response"),
            });

            var clientMock = new Mock<OpenAIClient>(It.IsAny<OpenAIClientOptions>(), It.IsAny<ILogger>(), It.IsAny<HttpClient>());
            var exception = new OpenAIClientException("Exception Message");
            clientMock.Setup(client => client.ExecuteTextModeration(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(exception);

            var options = new OpenAIModeratorOptions(apiKey, ModerationType.Both);
            var moderator = new OpenAIModerator<TurnState>(options);
            moderator.GetType().GetField("_client", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(moderator, clientMock.Object);

            // Act
            var result = await Assert.ThrowsAsync<OpenAIClientException>(async () => await moderator.ReviewPlan(turnContextMock.Object, turnStateMock.Object, plan));

            // Assert
            Assert.Equal("Exception Message", result.Message);
        }

        [Theory]
        [InlineData(ModerationType.Input)]
        [InlineData(ModerationType.Output)]
        [InlineData(ModerationType.Both)]
        public async void Test_ReviewPlan_Flagged(ModerationType moderate)
        {
            // Arrange
            var apiKey = "randomApiKey";

            var turnContextMock = new Mock<ITurnContext>();
            var turnStateMock = new Mock<TurnState>();
            var plan = new Plan(new List<IPredictedCommand>()
            {
                new PredictedDoCommand("action"),
                new PredictedSayCommand("response"),
            });

            var clientMock = new Mock<OpenAIClient>(It.IsAny<OpenAIClientOptions>(), It.IsAny<ILogger>(), It.IsAny<HttpClient>());
            var response = new ModerationResponse()
            {
                Id = "Id",
                Model = "Model",
                Results = new List<ModerationResult>()
                {
                    new ModerationResult()
                    {
                        Flagged = true,
                        CategoriesFlagged = new ModerationCategoriesFlagged()
                        {
                            Hate = false,
                            HateThreatening = false,
                            SelfHarm = false,
                            Sexual = false,
                            SexualMinors = false,
                            Violence = true,
                            ViolenceGraphic = false,
                        },
                        CategoryScores = new ModerationCategoryScores()
                        {
                            Hate = 0,
                            HateThreatening = 0,
                            SelfHarm = 0,
                            Sexual = 0,
                            SexualMinors = 0,
                            Violence = 0.9,
                            ViolenceGraphic = 0,
                        }
                    }
                }
            };
            clientMock.Setup(client => client.ExecuteTextModeration(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response);

            var options = new OpenAIModeratorOptions(apiKey, moderate);
            var moderator = new OpenAIModerator<TurnState>(options);
            moderator.GetType().GetField("_client", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(moderator, clientMock.Object);

            // Act
            var result = await moderator.ReviewPlan(turnContextMock.Object, turnStateMock.Object, plan);

            // Assert
            if (moderate == ModerationType.Output || moderate == ModerationType.Both)
            {
                Assert.NotNull(result);
                Assert.Equal(AITypes.DoCommand, result.Commands[0].Type);
                Assert.Equal(DefaultActionTypes.FlaggedOutputActionName, ((PredictedDoCommand)result.Commands[0]).Action);
                Assert.NotNull(((PredictedDoCommand)result.Commands[0]).Entities);
                Assert.True(((PredictedDoCommand)result.Commands[0]).Entities.ContainsKey("Result"));
                Assert.StrictEqual(response.Results[0], ((PredictedDoCommand)result.Commands[0]).Entities.GetValueOrDefault("Result"));
            }
            else
            {
                Assert.StrictEqual(plan, result);
            }
        }
    }
}