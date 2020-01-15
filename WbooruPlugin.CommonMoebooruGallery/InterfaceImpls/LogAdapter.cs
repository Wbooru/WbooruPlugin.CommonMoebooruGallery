using GeneralizableMoebooruAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wbooru;

namespace WbooruPlugin.CommonMoebooruGallery.InterfaceImpls
{
    public class LogAdapter : ILog
    {
        public static ILog Default { get; } = new LogAdapter();

        public void Debug(string message, [CallerMemberName] string method = "unknown method") => Log.Debug(message, method);

        public void Error(string message, [CallerMemberName] string method = "unknown method") => Log.Error(message, method);

        public void Info(string message, [CallerMemberName] string method = "unknown method") => Log.Info(message, method);

        public void Warn(string message, [CallerMemberName] string method = "unknown method") => Log.Warn(message, method);
    }
}
