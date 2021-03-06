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

using Microsoft.OData.Edm.Expressions;

namespace Microsoft.OData.Edm.Library
{
    /// <summary>
    /// Represents an EDM function import.
    /// </summary>
    public class EdmFunctionImport : EdmOperationImport, IEdmFunctionImport
    {
        private static readonly string FunctionArgumentNullParameterName = "function";

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmFunctionImport"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="name">The name.</param>
        /// <param name="function">The function.</param>
        public EdmFunctionImport(IEdmEntityContainer container, string name, IEdmFunction function)
            : this(container, name, function, null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmFunctionImport"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="name">The name.</param>
        /// <param name="function">The function.</param>
        /// <param name="entitySetExpression">The entity set expression.</param>
        /// <param name="includeInServiceDocument">The value indicates if the function is to be include in the service document or not.</param>
        public EdmFunctionImport(IEdmEntityContainer container, string name, IEdmFunction function, IEdmExpression entitySetExpression, bool includeInServiceDocument)
            : base(container, function, name, entitySetExpression)
        {
            EdmUtil.CheckArgumentNull(function, "function");

            this.Function = function;
            this.IncludeInServiceDocument = includeInServiceDocument;
        }

        /// <summary>
        /// Gets the function that defines the function import.
        /// </summary>
        public IEdmFunction Function { get; private set; }

        /// <summary>
        /// Gets the kind of this operation, which is always FunctionImport.
        /// </summary>
        public override EdmContainerElementKind ContainerElementKind
        {
            get { return EdmContainerElementKind.FunctionImport; }
        }

        /// <summary>
        /// Gets a value indicating whether [include in service document].
        /// </summary>
        public bool IncludeInServiceDocument { get; private set; }

        /// <summary>
        /// Operations the name of the argument null parameter.
        /// </summary>
        /// <returns>Returns the name of the operation from this import.</returns>
        protected override string OperationArgumentNullParameterName()
        {
            return FunctionArgumentNullParameterName;
        }
    }
}
