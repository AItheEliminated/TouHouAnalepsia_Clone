using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouhouAnalepsia.Utils.Inits
{
	[AttributeUsage(AttributeTargets.Method)]
	public class InitOnBeforeInitialModuleScreenSetAsRootAttribute : AbstractInitAttribute
	{
	}
}
