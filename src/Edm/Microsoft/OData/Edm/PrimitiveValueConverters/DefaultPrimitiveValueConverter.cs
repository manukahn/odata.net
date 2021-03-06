//   OData .NET Libraries
//   Copyright (c) Microsoft Corporation. All rights reserved.  
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

namespace Microsoft.OData.Edm.PrimitiveValueConverters
{
    using System;
    using System.Globalization;

    /// <summary>
    /// The default implementation of primitive value converter for unsigned ints, which:
    ///     converts UInt16 to Int32,
    ///     converts UInt32 to Int64,
    ///     converts UInt64 to Decimal.
    /// </summary>
    internal class DefaultPrimitiveValueConverter : IPrimitiveValueConverter
    {
        internal static readonly IPrimitiveValueConverter Instance = new DefaultPrimitiveValueConverter();

        private DefaultPrimitiveValueConverter()
        {
        }

        public object ConvertToUnderlyingType(object value)
        {
            var type = value.GetType();
            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.UInt16:
                    return Convert.ToInt32(value, CultureInfo.InvariantCulture);
                case TypeCode.UInt32:
                    return Convert.ToInt64(value, CultureInfo.InvariantCulture);
                case TypeCode.UInt64:
                    return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            }

            return value;
        }

        public object ConvertFromUnderlyingType(object value)
        {
            var type = value.GetType();
            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Int32:
                    return Convert.ToUInt16(value, CultureInfo.InvariantCulture);
                case TypeCode.Int64:
                    return Convert.ToUInt32(value, CultureInfo.InvariantCulture);
                case TypeCode.Decimal:
                    return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
            }

            return value;
        }
    }
}
