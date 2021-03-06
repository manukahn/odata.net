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

namespace Microsoft.OData.Core.UriParser.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.OData.Core.Metadata;
    using Microsoft.OData.Core.UriParser.Parsers;
    using Microsoft.OData.Core.UriParser.Semantic;
    using Microsoft.OData.Core.UriParser.TreeNodeKinds;
    using Microsoft.OData.Edm;

    /// <summary>
    /// Class for resolving different kinds of Uri parsing context.
    /// </summary>
    public class ODataUriResolver
    {
        /// <summary>
        /// Instance for <see cref="ODataUriResolver"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Resolver is immutable")]
        internal static readonly ODataUriResolver Default = new ODataUriResolver();

        /// <summary>
        /// Whether to enable case insensitive for the resolver.
        /// </summary>
        /// <remarks>
        /// All extensions should look at this property and keep case sensitive behaviour consistent.
        /// </remarks>
        public bool EnableCaseInsensitive { get; set; }

        /// <summary>
        /// Promote the left and right operand types
        /// </summary>
        /// <param name="binaryOperatorKind">the operator kind</param>
        /// <param name="leftNode">the left operand</param>
        /// <param name="rightNode">the right operand</param>
        /// <param name="typeReference">type reference for the result BinaryOperatorNode.</param>
        public virtual void PromoteBinaryOperandTypes(
               BinaryOperatorKind binaryOperatorKind,
               ref SingleValueNode leftNode,
               ref SingleValueNode rightNode,
               out IEdmTypeReference typeReference)
        {
            typeReference = null;
            BinaryOperatorBinder.PromoteOperandTypes(binaryOperatorKind, ref leftNode, ref rightNode);
        }

        /// <summary>
        /// Resolve navigation source from model.
        /// </summary>
        /// <param name="model">The model to be used.</param>
        /// <param name="identifier">The identifier to be resolved.</param>
        /// <returns>The resolved navigation source.</returns>
        public virtual IEdmNavigationSource ResolveNavigationSource(IEdmModel model, string identifier)
        {
            if (EnableCaseInsensitive)
            {
                var result = model.EntityContainer.Elements.OfType<IEdmNavigationSource>()
                    .Where(source => string.Equals(identifier, source.Name, StringComparison.OrdinalIgnoreCase)).ToList();

                if (result.Count == 1)
                {
                    return result.Single();
                }
                else if (result.Count > 1)
                {
                    // TODO: fix loc strings.
                    throw new ODataException("More than one navigation sources match the name '" + identifier + "' were found in model.");
                }
            }

            return model.FindDeclaredNavigationSource(identifier);
        }

        /// <summary>
        /// Resolve property from property name
        /// </summary>
        /// <param name="type">The declaring type.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The resolved <see cref="IEdmProperty"/></returns>
        public virtual IEdmProperty ResolveProperty(IEdmStructuredType type, string propertyName)
        {
            if (EnableCaseInsensitive)
            {
                var result = type.Properties()
                .Where(_ => string.Equals(propertyName, _.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();

                if (result.Count == 1)
                {
                    return result.Single();
                }
                else if (result.Count > 1)
                {
                    throw new ODataException(Strings.UriParserMetadata_MultiplePropertiesFound(propertyName, type.ODataFullName()));
                }
            }

            return type.FindProperty(propertyName);
        }

        /// <summary>
        /// Resolve type name from model.
        /// </summary>
        /// <param name="model">The model to be used.</param>
        /// <param name="typeName">The type name to be resolved.</param>
        /// <returns>Resolved type.</returns>
        [SuppressMessage("DataWeb.Usage", "AC0003:MethodCallNotAllowed", Justification = "IEdmModel.FindType is allowed here and all other places should call this method to get to the type.")]
        public virtual IEdmSchemaType ResolveType(IEdmModel model, string typeName)
        {
            if (EnableCaseInsensitive)
            {
                var result = model.SchemaElements.OfType<IEdmSchemaType>()
               .Where(_ => string.Equals(typeName, _.FullName(), StringComparison.OrdinalIgnoreCase))
               .ToList();

                if (result.Count == 1)
                {
                    return result.Single();
                }
                else if (result.Count > 1)
                {
                    // TODO: fix loc strings.
                    throw new ODataException("More than one type match the name '" + typeName + "' were found in model.");
                }
            }

            return model.FindType(typeName);
        }

        /// <summary>
        /// Resolve bound operations based on name.
        /// </summary>
        /// <param name="model">The model to be used.</param>
        /// <param name="identifier">The operation name.</param>
        /// <param name="bindingType">The type operation was binding to.</param>
        /// <returns>Resolved operation list.</returns>
        public virtual IEnumerable<IEdmOperation> ResolveBoundOperations(IEdmModel model, string identifier, IEdmType bindingType)
        {
            if (EnableCaseInsensitive)
            {
                return model.SchemaElements.OfType<IEdmOperation>()
                    .Where(operation => string.Equals(
                            identifier,
                            operation.FullName(),
                            this.EnableCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
                    && operation.IsBound && operation.Parameters.Any()
                    && operation.HasEquivalentBindingType(bindingType));
            }

            return model.FindBoundOperations(identifier, bindingType);
        }

        /// <summary>
        /// Resolve operation imports with certain name.
        /// </summary>
        /// <param name="model">The model to search.</param>
        /// <param name="identifier">The qualified name of the operation import which may or may not include the container name.</param>
        /// <returns>All operation imports that can be found by the specified name, returns an empty enumerable if no operation import exists.</returns>
        public virtual IEnumerable<IEdmOperationImport> ResolveOperationImports(IEdmModel model, string identifier)
        {
            if (EnableCaseInsensitive)
            {
                return model.EntityContainer.Elements.OfType<IEdmOperationImport>()
                    .Where(source => string.Equals(identifier, source.Name, StringComparison.OrdinalIgnoreCase));
            }

            return model.FindDeclaredOperationImports(identifier);
        }

        /// <summary>
        /// Resolve operation's parameters.
        /// </summary>
        /// <param name="operation">Current operation for parameters.</param>
        /// <param name="input">A dictionary the paramenter list.</param>
        /// <returns>A dictionary containing resolved parameters.</returns>
        public virtual IDictionary<IEdmOperationParameter, SingleValueNode> ResolveOperationParameters(IEdmOperation operation, IDictionary<string, SingleValueNode> input)
        {
            Dictionary<IEdmOperationParameter, SingleValueNode> result = new Dictionary<IEdmOperationParameter, SingleValueNode>(EqualityComparer<IEdmOperationParameter>.Default);
            foreach (var item in input)
            {
                IEdmOperationParameter functionParameter = null;
                if (EnableCaseInsensitive)
                {
                    var list = operation.Parameters.Where(parameter => string.Equals(item.Key, parameter.Name, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (list.Count == 1)
                    {
                        functionParameter = list.Single();
                    }
                    else if (list.Count > 1)
                    {
                        // TODO: fix loc strings.
                        throw new ODataException("More than one parameter match the name '" + item.Key + "' were found.");
                    }
                }
                else
                {
                    functionParameter = operation.FindParameter(item.Key);
                }

                // ensure parameter name existis
                if (functionParameter == null)
                {
                    throw new ODataException(Strings.ODataParameterWriterCore_ParameterNameNotFoundInOperation(item.Key, operation.Name));
                }

                result.Add(functionParameter, item.Value);
            }

            return result;
        }

        /// <summary>
        /// Resolve keys for certain entity set, this function would be called when key is specified as positional values. E.g. EntitySet('key')
        /// </summary>
        /// <param name="type">Type for current entityset.</param>
        /// <param name="positionalValues">The list of positional values.</param>
        /// <param name="convertFunc">The convert function to be used for value converting.</param>
        /// <returns>The resolved key list.</returns>
        public virtual IEnumerable<KeyValuePair<string, object>> ResolveKeys(IEdmEntityType type, IList<string> positionalValues, Func<IEdmTypeReference, string, object> convertFunc)
        {
            var keyProperties = type.Key().ToList();
            var keyPairList = new List<KeyValuePair<string, object>>(positionalValues.Count);

            for (int i = 0; i < keyProperties.Count; i++)
            {
                string valueText = positionalValues[i];
                IEdmProperty keyProperty = keyProperties[i];
                object convertedValue = convertFunc(keyProperty.Type, valueText);
                if (convertedValue == null)
                {
                    throw ExceptionUtil.CreateSyntaxError();
                }

                keyPairList.Add(new KeyValuePair<string, object>(keyProperty.Name, convertedValue));
            }

            return keyPairList;
        }

        /// <summary>
        /// Resolve keys for certain entity set, this function would be called when key is specified as name value pairs. E.g. EntitySet(ID='key')
        /// </summary>
        /// <param name="type">Type for current entityset.</param>
        /// <param name="namedValues">The dictionary of name value pairs.</param>
        /// <param name="convertFunc">The convert function to be used for value converting.</param>
        /// <returns>The resolved key list.</returns>
        public virtual IEnumerable<KeyValuePair<string, object>> ResolveKeys(IEdmEntityType type, IDictionary<string, string> namedValues, Func<IEdmTypeReference, string, object> convertFunc)
        {
            var convertedPairs = new Dictionary<string, object>(StringComparer.Ordinal);
            var keyProperties = type.Key().ToList();

            foreach (IEdmStructuralProperty property in keyProperties)
            {
                string valueText;

                if (EnableCaseInsensitive)
                {
                    var list = namedValues.Keys.Where(key => string.Equals(property.Name, key, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (list.Count > 1)
                    {
                        // TODO: fix loc strings.
                        throw new ODataException("More than one key match the name '" + property.Name + "' were found.");
                    }
                    else if (list.Count == 0)
                    {
                        throw ExceptionUtil.CreateSyntaxError();
                    }

                    valueText = namedValues[list.Single()];
                }
                else if (!namedValues.TryGetValue(property.Name, out valueText))
                {
                    throw ExceptionUtil.CreateSyntaxError();
                }

                //// Debug.Assert(property.Type.IsPrimitive() || property.Type.IsTypeDefinition(), "Keys can only be primitive or type definition");
                object convertedValue = convertFunc(property.Type, valueText);
                if (convertedValue == null)
                {
                    throw ExceptionUtil.CreateSyntaxError();
                }

                convertedPairs[property.Name] = convertedValue;
            }

            return convertedPairs;
        }
    }
}
