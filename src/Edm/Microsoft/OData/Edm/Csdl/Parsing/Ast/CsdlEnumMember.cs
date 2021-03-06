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

namespace Microsoft.OData.Edm.Csdl.Parsing.Ast
{
    /// <summary>
    /// Represents a CSDL enumeration type member.
    /// </summary>
    internal class CsdlEnumMember : CsdlNamedElement
    {
        public CsdlEnumMember(string name, long? value, CsdlDocumentation documentation, CsdlLocation location)
            : base(name, documentation, location)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the underlying type value of the member.
        /// Value can be null only during deserialization of the declaring enumeration type.
        /// When the type's deserialization is complete, all its members get their values assigned.
        /// </summary>
        public long? Value
        {
            get;
            set;
        }
    }
}
