using System.Collections.Generic;

namespace SourceCodeIndexer.STAC.Notification
{
    internal class NotificationHandler : INotificationHandler
    {
        private class ProgressValue
        {
            internal readonly double MinValue;
            internal readonly double Range;

            internal ProgressValue(double minValue, double range)
            {
                MinValue = minValue;
                Range = range;
            }
        }

        private readonly INotificationHandler _handler;
        private readonly Dictionary<NotificationType, ProgressValue> _progressValueDictionary;

        internal NotificationHandler(INotificationHandler handler, bool applyStemming)
        {
            _handler = handler;

            int change = applyStemming ? 5 : 0;

            _progressValueDictionary = new Dictionary<NotificationType, ProgressValue>
            {
                {NotificationType.AnalyzingFile, new ProgressValue(0, 50 - change)},
                {NotificationType.IdentifyingToken, new ProgressValue(0, 50 - change)},
                {NotificationType.ReadingFileForIdentifiers, new ProgressValue(50 - change, 50 - change)},
                {NotificationType.Splitting, new ProgressValue(50 - change, 50 - change)},
                {NotificationType.Stemming, new ProgressValue(90, 10)}, // if stemming is not used this is never used and splitting ends in 100 (50, 50)
                {NotificationType.IndexingCompleted, new ProgressValue(100, 0)},
            };
        }

        /// <summary>
        /// All questions are returned true by defaul
        /// </summary>
        /// <param name="questionType">Notification message</param>
        /// <param name="additionalMessage">Additional message</param>
        /// <returns>Returns user handler response or true by default</returns>
        public bool GetYesNoAnswer(QuestionType questionType, string additionalMessage)
        {
            return _handler == null || _handler.GetYesNoAnswer(questionType, additionalMessage);
        }

        /// <summary>
        /// Updates status. Just notification
        /// </summary>
        /// <param name="notificationType">Notification type</param>
        /// <param name="valueCompleted">Completed value of total value.</param>
        /// <param name="totalValue">Total Value of this process.</param>
        /// <param name="message">Additional message</param>
        public void UpdateStatus(NotificationType notificationType, int valueCompleted, int totalValue, string message)
        {
            _handler?.UpdateStatus(notificationType, valueCompleted, totalValue, message);
            ProgressValue progressValue = _progressValueDictionary[notificationType];
            UpdateProgress(progressValue.MinValue + progressValue.Range * (valueCompleted/(double)totalValue));
        }

        /// <summary>
        /// Total percent completed at every status update
        /// </summary>
        /// <param name="percentCompleted">Total percent completed.</param>
        public void UpdateProgress(double percentCompleted)
        {
            _handler?.UpdateProgress(percentCompleted);
        }
    }
}
