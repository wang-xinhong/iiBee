/*  iiBee - Automation with reboots
    Copyright (C) 2013 AbraxasCSharp (@github)
 */ 

namespace iiBee.RunTime.WorkflowHandling
{
    public enum ExitReaction
    {
        Reboot,
        Finished,
        ErrorLoadingFromInstanceStore,
        ErrorExecuting
    }
}
