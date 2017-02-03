using System;
using Newtonsoft.Json.Linq;

namespace JsonLD.Entities
{
    /// <summary>
    /// Null pattern for <see cref="IContextProvider"/>
    /// </summary>
    public class NullContextProvider : IContextProvider
    {
        /// <summary>
        /// Gets the expanded context for a give serialized type..
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <returns>null context</returns>
        public JToken GetContext(Type modelType)
        {
            return null;
        }
    }
}
