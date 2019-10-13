using AdaptiveCards;
using BotWpfClient.CognitiveServices;
using BotWpfClient.ViewModels;
using BotWpfClient.Views;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BotWpfClient
{
    static class App
    {
        [STAThread]
        public static void Main()
        {
            var configuration = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json")
                   .Build();

            var locale = configuration["Locale"];
            var topicName = configuration["TopicName"];

            var view = new MainWindow();
            var viewModel = new MainViewModel(configuration);
            view.DataContext = viewModel;

            var bot = new ChannelAccount() { Id = "You", Name = topicName };
            var user = new ChannelAccount() { Id = "Me", Name = "Me" };
            var client = new DirectLineClient(configuration["DirectLineKey"]);
            var conversation = client.Conversations.StartConversationAsync().Result;

            viewModel.SendAction = async () => {
                if (viewModel.Text.IsEmpty()) return;

                var activity = new Activity {
                    Locale = locale,
                    TopicName = topicName,
                    Type = ActivityTypes.Message,
                    From = user,
                    Recipient = bot,
                    Text = viewModel.Text
                };
                await client.Conversations.PostActivityAsync(conversation.ConversationId, activity);
                type($"{user.Name}>{viewModel.Text}");
                viewModel.Text = string.Empty;
            };

            void type(string text = null)
            {
                view.Dispatcher.Invoke(() => {
                    viewModel.Type(text);
                });
            };

            var sm = new SpeechManager(configuration);
            sm.Start(locale, async text =>
            {
                var activity = new Activity
                {
                    Locale = locale,
                    TopicName = topicName,
                    Type = ActivityTypes.Message,
                    From = user,
                    Recipient = bot,
                    Speak = text
                };
                await client.Conversations.PostActivityAsync(conversation.ConversationId, activity);
                type($"{user.Name}>{activity.Speak}");
            },
            bot.Name,
            awakeningHandler: async awakening => {
                view.Dispatcher.Invoke(() => {
                    viewModel.Awakening = awakening;
                });
            });

            var t2s = new TextToSpeechClient(configuration);

            async Task speak(string text)
            {
                await sm.Text(text);
            }

            var pull = Task.Factory.StartNew(async () => {

                var watermark = string.Empty;
                var activityIds = new HashSet<string>();
                while (true)
                {
                    var set = await client.Conversations.GetActivitiesAsync(conversation.ConversationId, watermark);
                    if (set.Activities.Count == 0)
                    {
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(set.Watermark)) watermark = set.Watermark;
                    foreach (var activity in set.Activities)
                    {
                        if (activityIds.Contains(activity.Id)) continue;
                        if (activity.From.Id == user.Id) continue;
                        activityIds.Add(activity.Id);

                        switch (activity.Type)
                        {
                            case "endOfConversation":
                                //await this.Stop();
                                break;
                            case "message":
                                if (activity.Attachments != null)
                                {
                                    if (activity.Attachments.Any(xx => xx.ContentType == AdaptiveCard.ContentType))
                                    {
                                        var json = activity.Attachments.First(xx => xx.ContentType == AdaptiveCard.ContentType).Content.ToString();
                                        var result = AdaptiveCard.FromJson(json);
                                        if (result != null)
                                        {
                                            viewModel.AdaptiveCard(result.Card);
                                        }
                                        else
                                        { 
                                        }
                                    }
                                }
                                if (activity.Speak.IsNotEmpty())
                                {
                                    await speak(activity.Speak);
                                    type(activity.Speak);
                                }
                                if (activity.Text.IsNotEmpty())
                                {
                                    type(activity.Text);
                                }

                                break;
                            default:
                                type($"{activity.Type}:{bot.Name}>{activity.Text}");
                                break;
                        }
                    }
                }
            });

            var app = new Application();
            view.Show();
            app.Run(view);
        }
    }
}
