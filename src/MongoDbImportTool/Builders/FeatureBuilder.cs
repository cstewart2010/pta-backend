﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;

namespace MongoDbImportTool.Builders
{
    internal static class FeatureBuilder
    {
        private static readonly string FeaturesJson = $"{JsonHelper.CurrentDirectory}/json/features.min.json";
        private static readonly string LegendaryFeaturesJson = $"{JsonHelper.CurrentDirectory}/json/legendary_features.min.json";
        private static readonly string SkillsJson = $"{JsonHelper.CurrentDirectory}/json/skills.min.json";
        private static readonly string PassivesJson = $"{JsonHelper.CurrentDirectory}/json/passives.min.json";

        public static void AddFeatures()
        {
            DatabaseHelper.AddDocuments("Features", GetFeatures(FeaturesJson));
            DatabaseHelper.AddDocuments("LegendaryFeatures", GetFeatures(LegendaryFeaturesJson));
            DatabaseHelper.AddDocuments("Skills", GetFeatures(SkillsJson));
            DatabaseHelper.AddDocuments("Passives", GetFeatures(PassivesJson));
        }

        private static IEnumerable<FeatureModel> GetFeatures(string path)
        {
            foreach (var child in JsonHelper.GetToken(path))
            {
                yield return JsonHelper.BuildFeature(child);
            }
        }
    }
}
