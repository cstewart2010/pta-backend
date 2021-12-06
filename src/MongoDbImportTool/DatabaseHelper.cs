using System;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Interfaces;
using TheReplacement.PTA.Common.Utilities;

namespace MongoDbImportTool
{
    internal static class DatabaseHelper
    {
        public static void AddDocuments<TDocument>(
            string collectionName,
            IEnumerable<TDocument> documents) where TDocument : INamed
        {
            DexUtility.AddStaticDocuments(collectionName, documents, Console.Out);
        }
    }
}
