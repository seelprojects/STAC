namespace SourceCodeIndexer.STAC.Notification
{
    public interface INotificationHandler
    {
        /// <summary>
        /// Gets yes or no answer
        /// </summary>
        /// <param name="questionType">Question type</param>
        /// <param name="message">Additional message</param>
        /// <returns>True or false response</returns>
        bool GetYesNoAnswer(QuestionType questionType, string message);

        /// <summary>
        /// Updates status. Just notification
        /// </summary>
        /// <param name="notificationType">Notification type</param>
        /// <param name="valueCompleted">Completed value of total value.</param>
        /// <param name="totalValue">Total Value of this process.</param>
        /// <param name="message">Additional message</param>
        void UpdateStatus(NotificationType notificationType, int valueCompleted, int totalValue, string message);

        /// <summary>
        /// Total percent completed at every status update
        /// </summary>
        /// <param name="percentCompleted">Total percent completed.</param>
        void UpdateProgress(double percentCompleted);

        /// <summary>
        /// To pass any text to UI
        /// </summary>
        /// <param name="text">Text to be passed</param>
        void UpdateStatus(string text);
    }
}
