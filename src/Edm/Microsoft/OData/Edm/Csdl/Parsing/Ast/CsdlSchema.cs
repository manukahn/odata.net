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

using System;
using System.Collections.Generic;

namespace Microsoft.OData.Edm.Csdl.Parsing.Ast
{
    /// <summary>
    /// Represents a CSDL schema.
    /// </summary>
    internal class CsdlSchema : CsdlElementWithDocumentation
    {
        private readonly List<CsdlStructuredType> structuredTypes;
        private readonly List<CsdlEnumType> enumTypes;
        private readonly List<CsdlOperation> operations;
        private readonly List<CsdlTerm> terms;
        private readonly List<CsdlEntityContainer> entityContainers;
        private readonly List<CsdlAnnotations> outOfLineAnnotations;
        private readonly List<CsdlTypeDefinition> typeDefinitions;

        private readonly string alias;
        private readonly string namespaceName;
        private readonly Version version;

        public CsdlSchema(
            string namespaceName,
            string alias,
            Version version,
            IEnumerable<CsdlStructuredType> structuredTypes,
            IEnumerable<CsdlEnumType> enumTypes,
            IEnumerable<CsdlOperation> operations,
            IEnumerable<CsdlTerm> terms,
            IEnumerable<CsdlEntityContainer> entityContainers,
            IEnumerable<CsdlAnnotations> outOfLineAnnotations,
            IEnumerable<CsdlTypeDefinition> typeDefinitions,
            CsdlDocumentation documentation,
            CsdlLocation location)
            : base(documentation, location)
        {
            this.alias = alias;
            this.namespaceName = namespaceName;
            this.version = version;
            this.structuredTypes = new List<CsdlStructuredType>(structuredTypes);
            this.enumTypes = new List<CsdlEnumType>(enumTypes);
            this.operations = new List<CsdlOperation>(operations);
            this.terms = new List<CsdlTerm>(terms);
            this.entityContainers = new List<CsdlEntityContainer>(entityContainers);
            this.outOfLineAnnotations = new List<CsdlAnnotations>(outOfLineAnnotations);
            this.typeDefinitions = new List<CsdlTypeDefinition>(typeDefinitions);
        }

        public IEnumerable<CsdlStructuredType> StructuredTypes
        {
            get { return this.structuredTypes; }
        }

        public IEnumerable<CsdlEnumType> EnumTypes
        {
            get { return this.enumTypes; }
        }

        public IEnumerable<CsdlOperation> Operations
        {
            get { return this.operations; }
        }

        public IEnumerable<CsdlTerm> Terms
        {
            get { return this.terms; }
        }

        public IEnumerable<CsdlEntityContainer> EntityContainers
        {
            get { return this.entityContainers; }
        }

        public IEnumerable<CsdlAnnotations> OutOfLineAnnotations
        {
            get { return this.outOfLineAnnotations; }
        }

        public IEnumerable<CsdlTypeDefinition> TypeDefinitions
        {
            get { return this.typeDefinitions; }
        }

        public string Alias
        {
            get { return this.alias; }
        }

        public string Namespace
        {
            get { return this.namespaceName; }
        }

        public Version Version
        {
            get { return this.version; }
        }
    }
}
