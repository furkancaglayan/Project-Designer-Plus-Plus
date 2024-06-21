using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// A helper class to generate unique ids.
    /// </summary>
    public static class UniqueIdGenerator
    {
        private const string ValidCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private const int IdLength = 16;

        /// <summary>
        /// Generates and returns a 16 characters unique id.
        /// </summary>
        /// <returns></returns>
        public static string GenerateUniqueId()
        {
            StringBuilder builder = new StringBuilder();
            System.Random random = new System.Random();

            for (int i = 0; i < IdLength; i++)
            {
                int index = random.Next(ValidCharacters.Length);
                builder.Append(ValidCharacters[index]);
            }

            return builder.ToString();
        }
    }
}
