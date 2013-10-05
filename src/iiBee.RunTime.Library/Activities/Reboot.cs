using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

namespace iiBee.RunTime.Library.Activities
{
    /// <summary>
    /// Activity to reboot System and continue on last execution spot.
    /// </summary>
    public sealed class Reboot : NativeActivity
    {
        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            context.CreateBookmark("RebootPending", new BookmarkCallback(OnBookmarkCallback));
        }

        private void OnBookmarkCallback(NativeActivityContext context, Bookmark bookmark, object val)
        {
            Console.WriteLine("Reboot Bookmark set");
        }
    }
}
