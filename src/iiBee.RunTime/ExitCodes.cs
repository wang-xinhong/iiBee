
namespace iiBee.RunTime
{
    public sealed class ExitCodes
    {
        /// <summary>
        /// Use this exitcode if application has starten without arguments and application has nothing todo.
        /// </summary>
        public const int HaveDoneNothing = 0;

        /// <summary>
        /// Use this exitcode if application has stopped execution of workflow to execute an system reboot.
        /// </summary>
        public const int ClosedForReboot = -5;

        /// <summary>
        /// Use this exitcode if application has stopped execution of workflow and finished all his work.
        /// </summary>
        public const int FinishedSuccessfully = 1;

        /// <summary>
        /// Use this exitcode if applilcation has stopped because of an unhandeled exception.
        /// </summary>
        public const int ApplicationError = -9;
    }
}
