using System;
using System.Collections;
using System.Linq;

namespace app.Dbf
{
	public delegate object Transform(object value);

	public class Transformations
	{
		public static Transform ValueFormat(string format)
		{
			return delegate (object value) {
				if (value == null)
					return value;
				if (value is IEnumerable)
					return String.Format(format, ToObjectArray((IEnumerable)value));
				else
					return String.Format(format, value);
			};
		}

		public static Transform RemoveNewLines()
		{
			return delegate (object value) {
				if (value == null || !(value is string))
					return value;
				return value.ToString().Replace(Environment.NewLine, " ");
			};
		}

		public static object[] ToObjectArray(IEnumerable enumerable)
		{
			return enumerable.Cast<object>().ToArray();
		}

		public static Transform ZeroIfNullOrEmpry()
		{
			return delegate (object value) {
				if (value == null || (value is string && value.ToString() == String.Empty))
					return 0;
				return value;
			};
		}

		public static Transform Const(params object[] constValue)
		{
			return delegate {
				if (constValue != null && constValue.Length == 1)
					return constValue[0];
				else
					return constValue;
			};
		}

		public static Transform DbNullIfEmptyString()
		{
			return delegate (object value) {
				if (value != null && value is string && value.ToString() == "")
					return DBNull.Value;
				return value;
			};
		}
	}
}