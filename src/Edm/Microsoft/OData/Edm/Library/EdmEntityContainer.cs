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
using System.Linq;
using Microsoft.OData.Edm.Expressions;

namespace Microsoft.OData.Edm.Library
{
    /// <summary>
    /// Represents an EDM entity container.
    /// </summary>
    public class EdmEntityContainer : EdmElement, IEdmEntityContainer
    {
        private readonly string namespaceName;
        private readonly string name;
        private readonly List<IEdmEntityContainerElement> containerElements = new List<IEdmEntityContainerElement>();
        private readonly Dictionary<string, IEdmEntitySet> entitySetDictionary = new Dictionary<string, IEdmEntitySet>();
        private readonly Dictionary<string, IEdmSingleton> singletonDictionary = new Dictionary<string, IEdmSingleton>();
        private readonly Dictionary<string, object> operationImportDictionary = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmEntityContainer"/> class.
        /// </summary>
        /// <param name="namespaceName">Namespace of the entity container.</param>
        /// <param name="name">Name of the entity container.</param>
        public EdmEntityContainer(string namespaceName, string name)
        {
            EdmUtil.CheckArgumentNull(namespaceName, "namespaceName");
            EdmUtil.CheckArgumentNull(name, "name");

            this.namespaceName = namespaceName;
            this.name = name;
        }

        /// <summary>
        /// Gets a collection of the elements of this entity container.
        /// </summary>
        public IEnumerable<IEdmEntityContainerElement> Elements
        {
            get { return this.containerElements; }
        }

        /// <summary>
        /// Gets the namespace of this entity container.
        /// </summary>
        public string Namespace
        {
            get { return this.namespaceName; }
        }

        /// <summary>
        /// Gets the name of this entity container.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets the kind of this schema element.
        /// </summary>
        public EdmSchemaElementKind SchemaElementKind
        {
            get { return EdmSchemaElementKind.EntityContainer; }
        }
        
        /// <summary>
        /// Adds an entity container element to this entity container.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public void AddElement(IEdmEntityContainerElement element)
        {
            EdmUtil.CheckArgumentNull(element, "element");

            this.containerElements.Add(element);
            
            switch (element.ContainerElementKind)
            {
                case EdmContainerElementKind.EntitySet:
                    RegistrationHelper.AddElement((IEdmEntitySet)element, element.Name, this.entitySetDictionary, RegistrationHelper.CreateAmbiguousEntitySetBinding);
                    break;
                case EdmContainerElementKind.Singleton:
                    RegistrationHelper.AddElement((IEdmSingleton)element, element.Name, this.singletonDictionary, RegistrationHelper.CreateAmbiguousSingletonBinding);
                    break;
                case EdmContainerElementKind.ActionImport:
                case EdmContainerElementKind.FunctionImport:
                    RegistrationHelper.AddOperationImport((IEdmOperationImport)element, element.Name, this.operationImportDictionary);
                    break;
                case EdmContainerElementKind.None:
                    throw new InvalidOperationException(Edm.Strings.EdmEntityContainer_CannotUseElementWithTypeNone);
                default:
                    throw new InvalidOperationException(Edm.Strings.UnknownEnumVal_ContainerElementKind(element.ContainerElementKind));
            }
        }

        /// <summary>
        /// Creates and adds an entity set to this entity container.
        /// </summary>
        /// <param name="name">Name of the entity set.</param>
        /// <param name="elementType">The entity type of the elements in this entity set.</param>
        /// <returns>Created entity set.</returns>
        public virtual EdmEntitySet AddEntitySet(string name, IEdmEntityType elementType)
        {
            EdmEntitySet entitySet = new EdmEntitySet(this, name, elementType);
            this.AddElement(entitySet);
            return entitySet;
        }

        /// <summary>
        /// Creates and adds an singleton to this entity container.
        /// </summary>
        /// <param name="name">Name of the singleton.</param>
        /// <param name="entityType">The entity type of this singleton.</param>
        /// <returns>Created singleton.</returns>
        public virtual EdmSingleton AddSingleton(string name, IEdmEntityType entityType)
        {
            EdmSingleton singleton = new EdmSingleton(this, name, entityType);
            this.AddElement(singleton);
            return singleton;
        }

        /// <summary>
        /// Creates and adds a function import to this entity container.
        /// </summary>
        /// <param name="function">The function of the specified function import.</param>
        /// <returns>Created function import.</returns>
        public virtual EdmFunctionImport AddFunctionImport(IEdmFunction function)
        {
            EdmFunctionImport functionImport = new EdmFunctionImport(this, function.Name, function);
            this.AddElement(functionImport);
            return functionImport;
        }

        /// <summary>
        /// Creates and adds a function import to this entity container.
        /// </summary>
        /// <param name="name">Name of the function import.</param>
        /// <param name="function">The function of the specified function import.</param>
        /// <returns>Created function import.</returns>
        public virtual EdmFunctionImport AddFunctionImport(string name, IEdmFunction function)
        {
            EdmFunctionImport functionImport = new EdmFunctionImport(this, name, function);
            this.AddElement(functionImport);
            return functionImport;
        }

        /// <summary>
        /// Creates and adds a function import to this entity container.
        /// </summary>
        /// <param name="name">Name of the function import.</param>
        /// <param name="function">The function of the specified function import.</param>
        /// <param name="entitySet">An entity set containing entities returned by this function import. 
        /// The two expression kinds supported are <see cref="IEdmEntitySetReferenceExpression"/> and <see cref="IEdmPathExpression"/>.</param>
        /// <returns>Created function import.</returns>
        public virtual EdmFunctionImport AddFunctionImport(string name, IEdmFunction function, IEdmExpression entitySet)
        {
            EdmFunctionImport functionImport = new EdmFunctionImport(this, name, function, entitySet, false /*includeInServiceDocument*/);
            this.AddElement(functionImport);
            return functionImport;
        }

        /// <summary>
        /// Creates and adds a function import to this entity container.
        /// </summary>
        /// <param name="name">Name of the function import.</param>
        /// <param name="function">The function of the specified function import.</param>
        /// <param name="entitySet">An entity set containing entities returned by this function import. 
        /// The two expression kinds supported are <see cref="IEdmEntitySetReferenceExpression"/> and <see cref="IEdmPathExpression"/>.</param>
        /// <param name="includeInServiceDocument">A value indicating whether this function import will be in the service document.</param>
        /// <returns>Created operation import.</returns>
        public virtual EdmOperationImport AddFunctionImport(string name, IEdmFunction function, IEdmExpression entitySet, bool includeInServiceDocument)
        {
            EdmOperationImport functionImport = new EdmFunctionImport(this, name, function, entitySet, includeInServiceDocument);
            this.AddElement(functionImport);
            return functionImport;
        }

        /// <summary>
        /// Creates and adds an action import to this entity container.
        /// </summary>
        /// <param name="name">Name of the action import.</param>
        /// <param name="action">Action that the action import is importing to the container.</param>
        /// <param name="entitySet">An entity set containing entities returned by this action import. 
        /// The two expression kinds supported are <see cref="IEdmEntitySetReferenceExpression"/> and <see cref="IEdmPathExpression"/>.</param>
        /// <returns>Created action import.</returns>
        public virtual EdmActionImport AddActionImport(string name, IEdmAction action, IEdmExpression entitySet)
        {
            EdmActionImport actionImport = new EdmActionImport(this, name, action, entitySet);
            this.AddElement(actionImport);
            return actionImport;
        }

        /// <summary>
        /// Creates and adds an action import to this entity container.
        /// </summary>
        /// <param name="action">Action that the action import is importing to the container.</param>
        /// <returns>Created action import.</returns>
        public virtual EdmActionImport AddActionImport(IEdmAction action)
        {
            EdmActionImport actionImport = new EdmActionImport(this, action.Name, action, null);
            this.AddElement(actionImport);
            return actionImport;
        }

        /// <summary>
        /// Creates and adds an action import to this entity container.
        /// </summary>
        /// <param name="name">Name of the action import.</param>
        /// <param name="action">Action that the action import is importing to the container.</param>
        /// <returns>Created action import.</returns>
        public virtual EdmActionImport AddActionImport(string name, IEdmAction action)
        {
            EdmActionImport actionImport = new EdmActionImport(this, name, action, null);
            this.AddElement(actionImport);
            return actionImport;
        }

        /// <summary>
        /// Searches for an entity set with the given name in this entity container and returns null if no such set exists.
        /// </summary>
        /// <param name="setName">The name of the element being found.</param>
        /// <returns>The requested element, or null if the element does not exist.</returns>
        public virtual IEdmEntitySet FindEntitySet(string setName)
        {
            if (!string.IsNullOrEmpty(setName))
            {
                IEdmEntitySet element;
                return this.entitySetDictionary.TryGetValue(setName, out element) ? element : null;
            }

            return null;
        }

        /// <summary>
        /// Searches for a singleton with the given name in this entity container and returns null if no such singleton exists.
        /// </summary>
        /// <param name="singletonName">The name of the singleton to search.</param>
        /// <returns>The requested singleton, or null if the singleton does not exist.</returns>
        public virtual IEdmSingleton FindSingleton(string singletonName)
        {
            IEdmSingleton element;
            return this.singletonDictionary.TryGetValue(singletonName, out element) ? element : null;
        }

        /// <summary>
        /// Searches for operation imports with the given name in this entity container and returns null if no such operation import exists.
        /// </summary>
        /// <param name="operationName">The name of the operation to find.</param>
        /// <returns>A group of the requested operation imports, or an empty enumerable if no such operation import exists.</returns>
        public IEnumerable<IEdmOperationImport> FindOperationImports(string operationName)
        {
            object element;
            if (this.operationImportDictionary.TryGetValue(operationName, out element))
            {
                List<IEdmOperationImport> listElement = element as List<IEdmOperationImport>;
                if (listElement != null)
                {
                    return listElement;
                }

                return new IEdmOperationImport[] { (IEdmOperationImport)element };
            }

            return Enumerable.Empty<IEdmOperationImport>();
        }
    }
}
