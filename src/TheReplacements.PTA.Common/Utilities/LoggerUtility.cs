using System;
using TheReplacements.PTA.Common.Enums;

namespace TheReplacements.PTA.Common.Utilities
{
    internal static class LoggerUtility
    {
        public static void Debug(
            MongoCollection affectedCollection,
            object record)
        {
            LogMessage
            (
                LogLevel.Debug,
                affectedCollection,
                record
            );
        }

        public static void Info(
            MongoCollection affectedCollection,
            object record)
        {
            LogMessage
            (
                LogLevel.Info,
                affectedCollection,
                record
            );
        }

        public static void Warn(
            MongoCollection affectedCollection,
            object record)
        {
            LogMessage
            (
                LogLevel.Warn,
                affectedCollection,
                record
            );
        }

        public static void Error(
            MongoCollection affectedCollection,
            object record)
        {
            LogMessage
            (
                LogLevel.Error,
                affectedCollection,
                record
            );
        }

        private static void LogMessage(
            LogLevel level,
            MongoCollection affectedCollection,
            object record)
        {
            MongoCollectionHelper.Logs.InsertOne(new Models.LoggerModel
            {
                AffectedCollection = affectedCollection,
                LogLevel = level,
                Timestamp = DateTime.Now,
                Message = record.ToString()
            });
        }
    }
}
