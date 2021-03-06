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

namespace Microsoft.OData.Edm.Csdl.CsdlSemantics
{
    internal class UnresolvedValueTerm : UnresolvedVocabularyTerm, IEdmValueTerm
    {
        private readonly UnresolvedValueTermTypeReference type = new UnresolvedValueTermTypeReference();
        private readonly string appliesTo = null;
        private readonly string defaultValue = null;

        public UnresolvedValueTerm(string qualifiedName)
            : base(qualifiedName)
        {
        }

        public override EdmSchemaElementKind SchemaElementKind
        {
            get { return EdmSchemaElementKind.ValueTerm; }
        }

        public override EdmTermKind TermKind
        {
            get { return EdmTermKind.Value; }
        }

        public IEdmTypeReference Type
        {
            get { return this.type; }
        }

        public string AppliesTo
        {
            get { return this.appliesTo; }
        }

        public string DefaultValue
        {
            get { return this.defaultValue; }
        }

        private class UnresolvedValueTermTypeReference : IEdmTypeReference
        {
            private readonly UnresolvedValueTermType definition = new UnresolvedValueTermType();

            public bool IsNullable
            {
                get { return false; }
            }

            public IEdmType Definition
            {
                get { return this.definition; }
            }

            private class UnresolvedValueTermType : IEdmType
            {
                public EdmTypeKind TypeKind
                {
                    get { return EdmTypeKind.None; }
                }
            }
        }
    }
}
