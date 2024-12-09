using System;

namespace Threads
{
    public class ActionCommand : BaseCommand
    {
        public Action action;

        public ActionCommand()
        {
            type = CommandReturnType.ACTION;
        }
    }
}