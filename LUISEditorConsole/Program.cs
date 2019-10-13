using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LUISEditorConsole
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var credentials = new ApiKeyServiceClientCredentials("");
            var client = new LUISAuthoringClient(credentials);
            client.Endpoint = "https://westus.api.cognitive.microsoft.com/";

            var configuration = JsonConvert.DeserializeObject<BotConversation.BotConfiguration>(File.ReadAllText("../../../../Data/conversation.json"));
            var desiredIntents = configuration.Intents.Keys.ToArray();

            var apps = await client.Apps.ListAsync();

            foreach (var app in apps)
            {
                var key = configuration.Apps[app.Name].Key;
                var entitiesFound = new List<string>();

                var existingIntents = (await client.Model.ListIntentsAsync(app.Id.Value, app.ActiveVersion)).ToDictionary(xx => xx.Name, xx => xx.Id);
                foreach (var intentToAdd in desiredIntents.Except(existingIntents.Keys))
                {
                    await client.Model.AddIntentAsync(app.Id.Value, app.ActiveVersion, new ModelCreateObject
                    {
                        Name = intentToAdd
                    });
                    foreach (var utterance in configuration.Intents[intentToAdd][key])
                    {
                        await client.EnsureUtterance(app, intentToAdd, utterance, entitiesFound);
                    }
                }
                foreach (var intentToRemove in existingIntents.Keys.Except(desiredIntents))
                {
                    if (intentToRemove == "None") continue;
                    await client.Model.DeleteIntentAsync(app.Id.Value, app.ActiveVersion, existingIntents[intentToRemove]);
                }
                foreach (var intentToUpdate in existingIntents.Keys.Intersect(desiredIntents))
                {
                    // nothing to do
                    foreach (var utterance in configuration.Intents[intentToUpdate][key])
                    {
                        await client.EnsureUtterance(app, intentToUpdate, utterance, entitiesFound);
                    }
                }

                await client.Train.TrainVersionAsync(app.Id.Value, app.ActiveVersion);
                //await client.Apps.PublishAsync(app.Id.Value, new ApplicationPublishObject {
                //    VersionId = app.ActiveVersion,
                //    IsStaging = false
                //});
            }
        }

        static Regex entityInfoRegex = new Regex(@"\[(?<entityInfo>[a-z0-9]+\:[a-z0-9]+)\]", RegexOptions.IgnoreCase);

        static async Task EnsureUtterance(this LUISAuthoringClient client, ApplicationInfoResponse app, string intent, string utterance, List<string> entitiesFound)
        {
            var elos = new List<EntityLabelObject>();

            var effectiveUtterance = utterance;
            while (true)
            {
                var entityInfoCaptured = entityInfoRegex.Match(effectiveUtterance);
                if (!entityInfoCaptured.Success) break;

                var entityInfo = entityInfoCaptured.Groups["entityInfo"].Value.Split(':');
                var entityValue = entityInfo[0];
                var entityName = entityInfo[1];
                if (!entitiesFound.Contains(entityName))
                {
                    // brute force insert (throws ex if exists)
                    try
                    {
                        await client.Model.AddEntityAsync(app.Id.Value, app.ActiveVersion, new ModelCreateObject
                        {
                            Name = entityName
                        });
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        entitiesFound.Add(entityName);
                    }
                }

                var elo = new EntityLabelObject
                {
                    EntityName = entityName,
                    StartCharIndex = entityInfoCaptured.Index,
                    EndCharIndex = entityInfoCaptured.Index + entityValue.Length - 1
                };
                elos.Add(elo);

                effectiveUtterance = effectiveUtterance.Replace(entityInfoCaptured.Groups[0].Value, entityValue);
            }

            await client.Examples.AddAsync(app.Id.Value, app.ActiveVersion, new ExampleLabelObject
            {
                Text = effectiveUtterance,
                IntentName = intent,
                EntityLabels = elos
            });
        }
    }
}
