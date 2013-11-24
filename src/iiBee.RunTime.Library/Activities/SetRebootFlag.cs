/*  iiBee - Automation with reboots
    Copyright (C) 2013 AbraxasCSharp (@github)
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

namespace iiBee.RunTime.Library.Activities
{

    public sealed class SetRebootFlag : CodeActivity
    {
        public static volatile bool RebootPending = false;

        protected override void Execute(CodeActivityContext context)
        {
            RebootPending = true;
        }
    }
}
