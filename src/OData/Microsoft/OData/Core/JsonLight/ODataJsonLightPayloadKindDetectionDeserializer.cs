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

namespace Microsoft.OData.Core.JsonLight
{
    #region Namespaces
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
#if ODATALIB_ASYNC
    using System.Threading.Tasks;
#endif
    using Microsoft.OData.Core.Json;
    #endregion Namespaces

    /// <summary>
    /// OData JsonLight deserializer for detecting the payload kind of a JsonLight payload.
    /// </summary>
    internal sealed class ODataJsonLightPayloadKindDetectionDeserializer : ODataJsonLightPropertyAndValueDeserializer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonLightInputContext">The JsonLight input context to read from.</param>
        internal ODataJsonLightPayloadKindDetectionDeserializer(ODataJsonLightInputContext jsonLightInputContext)
            : base(jsonLightInputContext)
        {
        }

        /// <summary>
        /// Detects the payload kind(s).
        /// </summary>
        /// <param name="detectionInfo">Additional information available for the payload kind detection.</param>
        /// <returns>An enumerable of zero, one or more payload kinds that were detected from looking at the payload in the message stream.</returns>
        internal IEnumerable<ODataPayloadKind> DetectPayloadKind(ODataPayloadKindDetectionInfo detectionInfo)
        {
            Debug.Assert(detectionInfo != null, "detectionInfo != null");
            Debug.Assert(this.ReadingResponse, "Payload kind detection is only supported in responses.");

            // prevent the buffering JSON reader from detecting in-stream errors - we read the error ourselves
            this.JsonReader.DisableInStreamErrorDetection = true;

            try
            {
                this.ReadPayloadStart(
                    ODataPayloadKind.Unsupported,
                    /*duplicatePropertyNamesChecker*/null,
                    /*isReadingNestedPayload*/false,
                    /*allowEmptyPayload*/false);
                return this.DetectPayloadKindImplementation(detectionInfo);
            }
            catch (ODataException)
            {
                // If we are not able to read the payload in the expected JSON format/structure
                // return no detected payload kind below.
                return Enumerable.Empty<ODataPayloadKind>();
            }
            finally
            {
                this.JsonReader.DisableInStreamErrorDetection = false;
            }
        }

#if ODATALIB_ASYNC
        /// <summary>
        /// Detects the payload kind(s).
        /// </summary>
        /// <param name="detectionInfo">Additional information available for the payload kind detection.</param>
        /// <returns>A task which returns an enumerable of zero, one or more payload kinds that were detected from looking at the payload in the message stream.</returns>
        internal Task<IEnumerable<ODataPayloadKind>> DetectPayloadKindAsync(ODataPayloadKindDetectionInfo detectionInfo)
        {
            Debug.Assert(detectionInfo != null, "detectionInfo != null");
            Debug.Assert(this.ReadingResponse, "Payload kind detection is only supported in responses.");

            // prevent the buffering JSON reader from detecting in-stream errors - we read the error ourselves
            this.JsonReader.DisableInStreamErrorDetection = true;

            return this.ReadPayloadStartAsync(
                ODataPayloadKind.Unsupported,
                /*duplicatePropertyNamesChecker*/null,
                /*isReadingNestedPayload*/false,
                /*allowEmptyPayload*/false)

                .FollowOnSuccessWith(t =>
                    {
                        return this.DetectPayloadKindImplementation(detectionInfo);
                    })

                .FollowOnFaultAndCatchExceptionWith<IEnumerable<ODataPayloadKind>, ODataException>(t =>
                    {
                        // If we are not able to read the payload in the expected JSON format/structure
                        // return no detected payload kind below.
                        return Enumerable.Empty<ODataPayloadKind>();
                    })

                .FollowAlwaysWith(t =>
                    {
                        this.JsonReader.DisableInStreamErrorDetection = false;
                    });
        }
#endif

        /// <summary>
        /// Detects the payload kind(s).
        /// </summary>
        /// <param name="detectionInfo">Additional information available for the payload kind detection.</param>
        /// <returns>An enumerable of zero, one or more payload kinds that were detected from looking at the payload in the message stream.</returns>
        private IEnumerable<ODataPayloadKind> DetectPayloadKindImplementation(ODataPayloadKindDetectionInfo detectionInfo)
        {
            Debug.Assert(detectionInfo != null, "detectionInfo != null");
            Debug.Assert(this.JsonReader.DisableInStreamErrorDetection, "The in-stream error detection should be disabled for payload kind detection.");

            this.AssertJsonCondition(JsonNodeType.Property, JsonNodeType.EndObject);

            // If we found a context URI and parsed it, look at the detected payload kind and return it.
            if (this.ContextUriParseResult != null)
            {
                // Store the parsed context URI on the input context so we can avoid parsing it again.
                detectionInfo.SetPayloadKindDetectionFormatState(new ODataJsonLightPayloadKindDetectionState(this.ContextUriParseResult));

                return this.ContextUriParseResult.DetectedPayloadKinds;
            }

            // Otherwise this is a payload without context URI and we have to start sniffing; only error payloads
            // don't have a context URI so check for a single 'error' property (ignoring custom annotations).
            ODataError error = null;
            while (this.JsonReader.NodeType == JsonNodeType.Property)
            {
                string propertyName = this.JsonReader.ReadPropertyName();
                string annotatedPropertyName, annotationName;
                if (!ODataJsonLightDeserializer.TryParsePropertyAnnotation(propertyName, out annotatedPropertyName, out annotationName))
                {
                    if (ODataJsonLightReaderUtils.IsAnnotationProperty(propertyName))
                    {
                        if (propertyName != null && propertyName.StartsWith(JsonLightConstants.ODataPropertyAnnotationSeparatorChar + JsonLightConstants.ODataAnnotationNamespacePrefix, System.StringComparison.Ordinal))
                        {
                            // Any @odata.* instance annotations are not allowed for errors.
                            return Enumerable.Empty<ODataPayloadKind>();
                        }
                        else
                        {
                            // Skip custom instance annotations
                            this.JsonReader.SkipValue();
                        }
                    }
                    else
                    {
                        if (string.CompareOrdinal(JsonLightConstants.ODataErrorPropertyName, propertyName) == 0)
                        {
                            // If we find multiple errors or an invalid error value, this is not an error payload.
                            if (error != null || !this.JsonReader.StartBufferingAndTryToReadInStreamErrorPropertyValue(out error))
                            {
                                return Enumerable.Empty<ODataPayloadKind>();
                            }

                            // At this point we successfully read the first error property. 
                            // Skip the error value and check whether there are more properties.
                            this.JsonReader.SkipValue();
                        }
                        else
                        {
                            // if it contains non-annotation property, it is not an error payload.
                            return Enumerable.Empty<ODataPayloadKind>();
                        }
                    }
                }
                else
                {
                    // Property annotation
                    return Enumerable.Empty<ODataPayloadKind>();
                }
            }

            // If we got here without finding a context URI or an error payload, we don't know what this is.
            if (error == null)
            {
                return Enumerable.Empty<ODataPayloadKind>();
            }

            return new ODataPayloadKind[] { ODataPayloadKind.Error };
        }
    }
}
