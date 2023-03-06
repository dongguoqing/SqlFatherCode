using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Daemon.Common.Const;
namespace Daemon.Common.Attribute
{
    public class QueryKeyWordAttribute : ModelBinderAttribute
    {
        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            Name = SystemConst.PREFIX_KEY_WORD + parameter.ParameterName;
            return base.GetBinding(parameter);
        }
    }
}
