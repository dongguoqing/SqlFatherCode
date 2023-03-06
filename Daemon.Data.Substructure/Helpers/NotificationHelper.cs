using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Data.Substructure.Helpers
{
    public class NotificationHelper
    {
        public static NotificationHelper Instance { get; } = new NotificationHelper();

        private NotificationHelper()
        {
        }

        public Action<Dictionary<Type, List<object>>> OnEntitiesAdded { get; set; }

        public Action<Dictionary<Type, List<object>>> OnEntitiesChanged { get; set; }

        public Action<Dictionary<Type, List<object>>> OnEntitiesDeleted { get; set; }
    }
}