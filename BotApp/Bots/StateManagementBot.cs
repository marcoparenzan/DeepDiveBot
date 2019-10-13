using AdaptiveCards;
using BotApp.Services;
using BotConversation;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public class StateManagementBot : ActivityHandler
    {
        private IConfiguration configuration;
        private BotState conversationState;
        private AssetManager assetManager;
        private LuisNLP luisNLP;
        private BotConfiguration conversationConfiguration;

        public StateManagementBot(ConversationState conversationState, BotConversation.AssetManager assetManager, Services.LuisNLP luisNLP, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.conversationState = conversationState;
            this.assetManager = assetManager;
            this.luisNLP = luisNLP;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            conversationConfiguration = assetManager.BotConfiguration(turnContext.Activity.TopicName ?? configuration["DefaultTopicName"]);

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

            var culture = (activity.Locale ?? configuration["DefaultLocale"]);

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

            //
            //  let's do it!
            //

            // has response

            if (config.Response.IsNotEmpty())
            {
                textResponse = conversationConfiguration.Responses[config.Response][culture];
                //
                //  fake logic
                //  
                if (entitiesDict.ContainsKey("name")) textResponse = textResponse.Replace("[name]", entitiesDict["name"], System.StringComparison.InvariantCultureIgnoreCase);
                if (textResponse.Contains("{time}", System.StringComparison.InvariantCultureIgnoreCase)) textResponse = textResponse.Replace("{time}", DateTime.Now.ToString("HH:mm"), System.StringComparison.InvariantCultureIgnoreCase);
            }

            // change state

            if (config.NewState.IsNotEmpty())
            {
                conversationData.CurrentState = config.NewState;
            }

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
                    Attachments = new List<Attachment>()
                };
                
                // add adaptivecard if required
                if (config.AdaptiveCard.IsNotEmpty())
                {
                    var adaptiveCardAttachment = new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = assetManager.AdaptiveCards(config.AdaptiveCard),
                        Name = "AdaptiveCard"
                    };
                    activityResponse.Attachments.Add(adaptiveCardAttachment);
                }

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

