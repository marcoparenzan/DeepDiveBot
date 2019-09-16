// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BotApp.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public class StateManagementBot : ActivityHandler
    {
        private BotState conversationState;
        private BotConversation.Configuration conversationConfiguration;
        private LuisNLP luisNLP;

        public StateManagementBot(ConversationState conversationState, BotConversation.Configuration conversationConfiguration, Services.LuisNLP luisNLP)
        {
            this.conversationState = conversationState;
            this.conversationConfiguration = conversationConfiguration;
            this.luisNLP = luisNLP;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            //await turnContext.SendActivityAsync("Welcome to State Bot Sample. Type anything to get started.");
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Get the state properties from the turn context.

            var conversationStateAccessors =  conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData {
                CurrentState = "greetings"
            });

            var activity = turnContext.Activity;

            var culture = (activity.Locale ?? "it-IT");

            var ok = true;

            var isSpeak = activity.Speak.IsNotEmpty();
            var text = isSpeak ? activity.Speak: activity.Text;

            // exit if empty
            if (text.IsEmpty())
            {
                return;
            }

            var textResponse = string.Empty;

            //
            //  evaluate conversation
            //
            var (intent, entities) = await luisNLP.Resolve(culture, text);
            var entitiesDict = entities.ToDictionary(xx => xx.name, xx => xx.value);
            var config = conversationConfiguration.Conversation[conversationData.CurrentState][intent];
            textResponse = conversationConfiguration.Responses[config.Response][culture];
            //if (entitiesDict.ContainsKey("name")) textResponse = textResponse.Replace("[name]", entitiesDict["name"], System.StringComparison.InvariantCultureIgnoreCase);
            //if (textResponse.Contains("{time}", System.StringComparison.InvariantCultureIgnoreCase)) textResponse = textResponse.Replace("{time}", DateTime.Now.ToString("HH:mm"), System.StringComparison.InvariantCultureIgnoreCase);
            conversationData.CurrentState = config.NewState;

            //
            //  response
            //

            Activity activityResponse = default;

            if (ok)
            {
                activityResponse = new Activity
                {
                    Type = ActivityTypes.Message,
                    From = activity.Recipient,
                    Recipient = activity.From,
                };
                if (isSpeak)
                    activityResponse.Speak = textResponse;
                else
                    activityResponse.Text = textResponse;
            }
            else
            {
                activityResponse = new Activity
                {
                    Type = ActivityTypes.Message,
                    From = activity.Recipient,
                    Recipient = activity.From,
                    Text = "ERRORE"
                };

            }

            await turnContext.SendActivityAsync(activityResponse);
        }
    }
}

