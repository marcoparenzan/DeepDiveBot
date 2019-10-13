using AdaptiveCards;
using AdaptiveCards.Rendering.Wpf;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BotWpfClient.ViewModels
{
    public class MainViewModel: BaseViewModel<MainViewModel>
    {
        private IConfiguration configuration;

        public MainViewModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public ObservableCollection<string> Items { get; } = new ObservableCollection<string>();

        public int ItemsSelectedIndex => Items.Count - 1;

        public void Type(string text = null)
        {
            Items.Add(text ?? Text);
            Notify(nameof(ItemsSelectedIndex));
            PageName = "Conversation";
            }

        public Action SendAction { get; set; }

        public RelayCommand Send => new RelayCommand(()=> {

            SendAction();

        });

        private string pageName = "Conversation";
        public string PageName
        {
            get => pageName;
            set => SetProperty(ref pageName, value);
        }

        private string text;
        public string Text
        {
            get => text;
            set
            {
                SetProperty(ref text, value);
                PageName = "Conversation";
            }
        }

        private bool awakening;

        public bool Awakening
        {
            get => awakening;
            set => SetProperty(ref awakening, value);
        }

        private FrameworkElement card;

        public FrameworkElement Card
        {
            get => card;
            set
            {
                SetProperty(ref card, value);
                PageName = "AdaptiveCard";
            }
        }

        static AdaptiveCardRenderer renderer = new AdaptiveCardRenderer();

        public void AdaptiveCard(AdaptiveCard card)
        {
            Dispatch(vm =>
            {
                try
                {
                    var result = renderer.RenderCard(card);
                    result.OnAction += (sender, e) =>
                    {
                        Execute(e.Action);
                    };
                    Card = result.FrameworkElement;
                    _ = Task.Factory.StartNew(async () =>
                    {
                        await Task.Delay(10000);
                        Card = null;
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace);
                }
            });
        }

        private static void Execute(AdaptiveAction action)
        {
            if (action is AdaptiveOpenUrlAction openUrlAction)
            {
                try
                {
                    Process.Start(openUrlAction.Url.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace);
                }
            }
            //else if (action is AdaptiveSubmitAction submitAction)
            //{
            //    var inputs = sender.UserInputs.AsJson();
            //    inputs.Merge(submitAction.Data);
            //    MessageBox.Show("Show", JsonConvert.SerializeObject(inputs, Formatting.Indented));
            //}
        }
    }
}
