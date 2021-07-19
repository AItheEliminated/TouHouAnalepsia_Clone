using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouhouAnalepsia.Utils.Inits
{
	public class AbstractInitAttribute : Attribute
	{
		private static readonly Type[] _empty = new Type[0];

		private Type[] _after;

		public Type[] After
		{
			set
			{
				if (value == null)
					throw new ArgumentNullException("After");
				_after = value;
			}
			get
			{
				return _after == null ? _empty : _after;
			}
		}
	}
}
